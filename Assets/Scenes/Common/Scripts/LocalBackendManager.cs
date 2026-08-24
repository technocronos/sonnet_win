using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Scenes.Common.Scripts
{
    public static class LocalBackendSettings
    {
        public const string DatabaseHost = "127.0.0.1";
        public const int DatabasePort = 13306;
        public const string ApiHost = "127.0.0.1";
        public const int ApiPort = 18080;
        public const string DatabaseUser = "sonnet_mas";
        public const string DatabasePassword = "HHil0OjGZTXv0JEh6IN5";

        public static string ApiAuthority => ApiHost + ":" + ApiPort;
        public static string ApiBaseUrl => "http://" + ApiAuthority + "/";
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [DefaultExecutionOrder(-10000)]
    public sealed class LocalBackendManager : MonoBehaviour
    {
        private const int StartupTimeoutSeconds = 90;
        private const string RuntimeFolderName = "SonnetOfWizard";
        private static LocalBackendManager instance;
        private static bool finished;
        private static string startupError;

        private Process mariadbProcess;
        private Process phpProcess;
        private Mutex instanceMutex;
        private string installRoot;
        private string userRoot;
        private string databaseRoot;
        private string logsRoot;
        private string configRoot;
        private string runtimeRoot;
        private string runtimeWebappRoot;
        private bool isShuttingDown;

        public static bool IsFinished => finished;
        public static bool IsReady => finished && string.IsNullOrEmpty(startupError);
        public static string StartupError => startupError;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateBeforeMainScene()
        {
            if (instance != null)
                return;

            var gameObject = new GameObject(nameof(LocalBackendManager));
            DontDestroyOnLoad(gameObject);
            instance = gameObject.AddComponent<LocalBackendManager>();
        }

        private async void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            try
            {
                await StartBackendAsync();
                UnityEngine.Debug.Log("Local backend is ready: " + LocalBackendSettings.ApiBaseUrl);
            }
            catch (Exception exception)
            {
                startupError = exception.Message;
                UnityEngine.Debug.LogError("Local backend startup failed: " + exception);
            }
            finally
            {
                finished = true;
            }
        }

        private async Task StartBackendAsync()
        {
            installRoot = Path.Combine(Application.streamingAssetsPath, "Backend");
            userRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), RuntimeFolderName);
            databaseRoot = Path.Combine(userRoot, "Database");
            logsRoot = Path.Combine(userRoot, "Logs");
            configRoot = Path.Combine(userRoot, "Config");
            runtimeRoot = Path.Combine(userRoot, "Runtime");
            runtimeWebappRoot = Path.Combine(runtimeRoot, "webapp");

            ValidateInstallLayout();
            Directory.CreateDirectory(logsRoot);
            Directory.CreateDirectory(configRoot);
            Directory.CreateDirectory(runtimeRoot);
            EnsureRuntimeWebapp();

            instanceMutex = new Mutex(false, "Local\\SonnetOfWizard.LocalBackend");
            if (!instanceMutex.WaitOne(0, false))
                throw new InvalidOperationException("Another Sonnet local backend instance is already running.");

            var initializedMarker = Path.Combine(configRoot, "database.initialized");
            if (!File.Exists(initializedMarker))
            {
                if (Directory.Exists(databaseRoot) && Directory.EnumerateFileSystemEntries(databaseRoot).GetEnumerator().MoveNext())
                    throw new InvalidOperationException("Database exists without an initialization marker. It was not overwritten.");

                Directory.CreateDirectory(databaseRoot);
                await RunAndRequireSuccessAsync(MariaInstallPath(), "--datadir=" + Quote(databaseRoot), "mariadb-install");
            }

            mariadbProcess = StartLoggedProcess(
                MariaServerPath(),
                "--datadir=" + Quote(databaseRoot) +
                " --port=" + LocalBackendSettings.DatabasePort +
                " --bind-address=" + LocalBackendSettings.DatabaseHost +
                " --console",
                databaseRoot,
                "mariadb");

            await WaitForDatabaseAsync();

            if (!File.Exists(initializedMarker))
            {
                await RunMariaSqlAsync("CREATE DATABASE IF NOT EXISTS sonnet_1; CREATE DATABASE IF NOT EXISTS sonnet_m; " +
                    "CREATE USER IF NOT EXISTS '" + LocalBackendSettings.DatabaseUser + "'@'" + LocalBackendSettings.DatabaseHost + "' IDENTIFIED BY '" + LocalBackendSettings.DatabasePassword + "'; " +
                    "GRANT ALL PRIVILEGES ON sonnet_1.* TO '" + LocalBackendSettings.DatabaseUser + "'@'" + LocalBackendSettings.DatabaseHost + "'; " +
                    "GRANT ALL PRIVILEGES ON sonnet_m.* TO '" + LocalBackendSettings.DatabaseUser + "'@'" + LocalBackendSettings.DatabaseHost + "'; FLUSH PRIVILEGES;");
                await ImportDatabaseAsync("sonnet_1");
                await ImportDatabaseAsync("sonnet_m");
                WriteInitializationMarker(initializedMarker);
            }

            var phpIniPath = WritePhpIni();
            phpProcess = StartLoggedProcess(
                PhpPath(),
                "-c " + Quote(phpIniPath) + " -S " + LocalBackendSettings.ApiAuthority + " -t " + Quote(Path.Combine(runtimeWebappRoot, "htdocs")),
                Path.Combine(runtimeWebappRoot, "htdocs"),
                "php",
                new[] { "SONNET_DB_HOST=" + LocalBackendSettings.DatabaseHost, "SONNET_DB_PORT=" + LocalBackendSettings.DatabasePort });

            await WaitForHttpAsync();
        }

        private void ValidateInstallLayout()
        {
            RequireFile(PhpPath());
            RequireFile(MariaServerPath());
            RequireFile(MariaAdminPath());
            RequireFile(MariaClientPath());
            RequireFile(MariaInstallPath());
            RequireFile(Path.Combine(installRoot, "initial_db", "sonnet_1.sql"));
            RequireFile(Path.Combine(installRoot, "initial_db", "sonnet_m.sql"));
            if (!Directory.Exists(Path.Combine(installRoot, "webapp", "htdocs")))
                throw new FileNotFoundException("Backend webapp/htdocs is missing.");
        }

        private void EnsureRuntimeWebapp()
        {
            if (Directory.Exists(runtimeWebappRoot))
            {
                MakeWritable(runtimeWebappRoot);
                Directory.CreateDirectory(Path.Combine(runtimeWebappRoot, "var", "cache", "mojavi"));
                Directory.CreateDirectory(Path.Combine(runtimeWebappRoot, "var", "logs"));
                return;
            }

            CopyDirectory(Path.Combine(installRoot, "webapp"), runtimeWebappRoot);
            MakeWritable(runtimeWebappRoot);
            Directory.CreateDirectory(Path.Combine(runtimeWebappRoot, "var", "cache", "mojavi"));
            Directory.CreateDirectory(Path.Combine(runtimeWebappRoot, "var", "logs"));
        }

        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (var file in Directory.GetFiles(source))
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), false);
            foreach (var directory in Directory.GetDirectories(source))
                CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
        }

        private static void MakeWritable(string root)
        {
            foreach (var directory in Directory.GetDirectories(root, "*", SearchOption.AllDirectories))
            {
                var info = new DirectoryInfo(directory);
                info.Attributes &= ~FileAttributes.ReadOnly;
            }
            foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
            {
                var info = new FileInfo(file);
                info.Attributes &= ~FileAttributes.ReadOnly;
            }
        }

        private async Task WaitForDatabaseAsync()
        {
            await WaitUntilAsync(async () =>
            {
                var result = await RunProcessAsync(MariaAdminPath(), "--protocol=tcp --host=" + LocalBackendSettings.DatabaseHost + " --port=" + LocalBackendSettings.DatabasePort + " --user=root ping", databaseRoot, null, null, "mariadb-admin");
                return result.ExitCode == 0;
            }, "MariaDB");
        }

        private async Task WaitForHttpAsync()
        {
            await WaitUntilAsync(() => Task.Run(() =>
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(LocalBackendSettings.ApiBaseUrl + "?module=Api&action=Login&opensocial_owner_id=0&ver=1&lang=0");
                    request.Timeout = 2000;
                    request.ReadWriteTimeout = 2000;
                    request.UserAgent = "Mozilla/5.0 (Linux; Android 10)";
                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var stream = new StreamReader(response.GetResponseStream()))
                        return (int)response.StatusCode == 200 && stream.ReadToEnd().IndexOf("\"result\":\"ok\"", StringComparison.OrdinalIgnoreCase) >= 0;
                }
                catch (WebException)
                {
                    return false;
                }
            }), "PHP HTTP server");
        }

        private async Task WaitUntilAsync(Func<Task<bool>> probe, string serviceName)
        {
            var deadline = DateTime.UtcNow.AddSeconds(StartupTimeoutSeconds);
            while (DateTime.UtcNow < deadline)
            {
                if (await probe())
                    return;

                await Task.Delay(500);
            }

            throw new TimeoutException(serviceName + " did not become ready within " + StartupTimeoutSeconds + " seconds.");
        }

        private async Task ImportDatabaseAsync(string databaseName)
        {
            var dumpPath = Path.Combine(installRoot, "initial_db", databaseName + ".sql");
            var result = await RunProcessAsync(MariaClientPath(), "--protocol=tcp --host=" + LocalBackendSettings.DatabaseHost + " --port=" + LocalBackendSettings.DatabasePort + " --user=root " + databaseName, databaseRoot, null, dumpPath, "import-" + databaseName);
            if (result.ExitCode != 0)
                throw new InvalidOperationException("Initial import failed for " + databaseName + ". See logs.");
        }

        private async Task RunMariaSqlAsync(string sql)
        {
            var result = await RunProcessAsync(MariaClientPath(), "--protocol=tcp --host=" + LocalBackendSettings.DatabaseHost + " --port=" + LocalBackendSettings.DatabasePort + " --user=root --execute=" + Quote(sql), databaseRoot, null, null, "mariadb-bootstrap");
            if (result.ExitCode != 0)
                throw new InvalidOperationException("MariaDB bootstrap SQL failed. See logs.");
        }

        private async Task RunAndRequireSuccessAsync(string fileName, string arguments, string logName)
        {
            var result = await RunProcessAsync(fileName, arguments, installRoot, null, null, logName);
            if (result.ExitCode != 0)
                throw new InvalidOperationException(logName + " failed. See logs.");
        }

        private Process StartLoggedProcess(string fileName, string arguments, string workingDirectory, string logName, string[] environment = null)
        {
            var startInfo = CreateStartInfo(fileName, arguments, workingDirectory, environment);
            var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            AttachLog(process, logName);
            if (!process.Start())
                throw new InvalidOperationException("Could not start " + logName + ".");
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return process;
        }

        private async Task<ProcessResult> RunProcessAsync(string fileName, string arguments, string workingDirectory, string[] environment, string stdinFile, string logName)
        {
            var startInfo = CreateStartInfo(fileName, arguments, workingDirectory, environment);
            startInfo.RedirectStandardInput = stdinFile != null;
            var process = new Process { StartInfo = startInfo };
            AttachLog(process, logName);
            if (!process.Start())
                throw new InvalidOperationException("Could not start " + logName + ".");

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            if (stdinFile != null)
            {
                using (var source = File.OpenRead(stdinFile))
                    await source.CopyToAsync(process.StandardInput.BaseStream);
                process.StandardInput.Close();
            }

            await Task.Run(() => process.WaitForExit());
            return new ProcessResult { ExitCode = process.ExitCode };
        }

        private ProcessStartInfo CreateStartInfo(string fileName, string arguments, string workingDirectory, string[] environment)
        {
            var startInfo = new ProcessStartInfo(fileName, arguments)
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            if (environment != null)
            {
                foreach (var item in environment)
                {
                    var separator = item.IndexOf('=');
                    startInfo.EnvironmentVariables[item.Substring(0, separator)] = item.Substring(separator + 1);
                }
            }
            return startInfo;
        }

        private void AttachLog(Process process, string logName)
        {
            var path = Path.Combine(logsRoot, logName + ".log");
            process.OutputDataReceived += (_, args) => AppendLog(path, args.Data);
            process.ErrorDataReceived += (_, args) => AppendLog(path, args.Data);
        }

        private static void AppendLog(string path, string line)
        {
            if (line == null)
                return;
            File.AppendAllText(path, DateTime.Now.ToString("O") + " " + line + Environment.NewLine, Encoding.UTF8);
        }

        private string WritePhpIni()
        {
            var phpRoot = Path.Combine(installRoot, "php");
            var phpIniPath = Path.Combine(configRoot, "php.ini");
            var ini = "[PHP]" + Environment.NewLine +
                      "extension_dir=\"" + Path.Combine(phpRoot, "ext").Replace("\\", "/") + "\"" + Environment.NewLine +
                      "short_open_tag=On" + Environment.NewLine +
                      "date.timezone=Asia/Tokyo" + Environment.NewLine +
                      "memory_limit=512M" + Environment.NewLine +
                      "max_execution_time=120" + Environment.NewLine +
                      "extension=php_pdo_mysql.dll" + Environment.NewLine +
                      "extension=php_mysqli.dll" + Environment.NewLine +
                      "extension=php_mbstring.dll" + Environment.NewLine +
                      "extension=php_curl.dll" + Environment.NewLine +
                      "extension=php_gd2.dll" + Environment.NewLine;
            File.WriteAllText(phpIniPath, ini, new UTF8Encoding(false));
            return phpIniPath;
        }

        private void WriteInitializationMarker(string markerPath)
        {
            var temporaryPath = markerPath + ".tmp";
            File.WriteAllText(temporaryPath, "initialized=" + DateTime.UtcNow.ToString("O"), new UTF8Encoding(false));
            File.Move(temporaryPath, markerPath);
        }

        private string PhpPath() => Path.Combine(installRoot, "php", "php.exe");
        private string MariaServerPath() => Path.Combine(installRoot, "mariadb", "bin", "mariadbd.exe");
        private string MariaAdminPath() => Path.Combine(installRoot, "mariadb", "bin", "mariadb-admin.exe");
        private string MariaClientPath() => Path.Combine(installRoot, "mariadb", "bin", "mariadb.exe");
        private string MariaInstallPath() => Path.Combine(installRoot, "mariadb", "bin", "mariadb-install-db.exe");
        private static string Quote(string value) => "\"" + value.Replace("\"", "\\\"") + "\"";
        private static void RequireFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Required local backend file is missing.", path);
        }

        private void OnApplicationQuit()
        {
            Shutdown();
        }

        private void OnDestroy()
        {
            if (instance == this)
                Shutdown();
        }

        private void Shutdown()
        {
            if (isShuttingDown)
                return;
            isShuttingDown = true;

            if (phpProcess != null && !phpProcess.HasExited)
            {
                phpProcess.CloseMainWindow();
                if (!phpProcess.WaitForExit(3000))
                    phpProcess.Kill();
            }

            if (mariadbProcess != null && !mariadbProcess.HasExited)
            {
                try
                {
                    RunProcessAsync(MariaAdminPath(), "--protocol=tcp --host=" + LocalBackendSettings.DatabaseHost + " --port=" + LocalBackendSettings.DatabasePort + " --user=root shutdown", databaseRoot, null, null, "mariadb-shutdown").GetAwaiter().GetResult();
                    mariadbProcess.WaitForExit(5000);
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogWarning("MariaDB normal shutdown failed: " + exception.Message);
                }
            }

            if (instanceMutex != null)
            {
                instanceMutex.ReleaseMutex();
                instanceMutex.Dispose();
                instanceMutex = null;
            }
        }

        private sealed class ProcessResult
        {
            public int ExitCode;
        }
    }
#else
    public static class LocalBackendManager
    {
        public static bool IsFinished => true;
        public static bool IsReady => true;
        public static string StartupError => null;
    }
#endif
}
