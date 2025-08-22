using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class PostXcodeBuild
{
    [PostProcessBuild]
    public static void SetXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget != BuildTarget.iOS) return;

        var plistPath = pathToBuiltProject + "/Info.plist";
        var plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        var rootDict = plist.root;
        // ここに記載したKey-ValueがXcodeのinfo.plistに反映されます
        rootDict.SetString("NSAdvertisingAttributionReportEndpoint", "https://appsflyer-skadnetwork.com/");

        File.WriteAllText(plistPath, plist.WriteToString());

        processForiOS(pathToBuiltProject);
    }

    static void processForiOS(string path)
    {
        string pbxPath = PBXProject.GetPBXProjectPath(path);
        PBXProject pbx = new PBXProject();
        pbx.ReadFromString(File.ReadAllText(pbxPath));
        string target = pbx.GetUnityMainTargetGuid();
        pbx.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
        target = pbx.GetUnityFrameworkTargetGuid();
        pbx.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
        File.WriteAllText(pbxPath, pbx.WriteToString());
    }
}