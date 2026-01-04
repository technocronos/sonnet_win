using UnityEditor;
using UnityEngine;

public class CreateAssetBundles
{
    public const string ORG_PATH = "Assets/AssetBundle/";
    
    public static string [] AssetBundleNames;
    
    
    [MenuItem("MyMenu/Build AssetBundles/StandaloneWindows")]
    static void BuildAllAssetBundlesStandaloneWindows()
    {
        BuildPipeline.BuildAssetBundles("Assets/StreamingAssets/StandaloneWindows", BuildAssetBundleOptions.ChunkBasedCompression,BuildTarget.StandaloneWindows);
    }
    
    [MenuItem("MyMenu/Build AssetBundles/Android")]
    static void BuildAllAssetBundlesAndroid()
    {
        BuildPipeline.BuildAssetBundles("Assets/StreamingAssets/Android", BuildAssetBundleOptions.ChunkBasedCompression,BuildTarget.Android);
    }
    
    [MenuItem("MyMenu/Build AssetBundles/iOS")]
    static void BuildAllAssetBundlesiOS()
    {
        BuildPipeline.BuildAssetBundles("Assets/StreamingAssets/IOS", BuildAssetBundleOptions.ChunkBasedCompression,BuildTarget.iOS);
    }
    
    [MenuItem ("MyMenu/Names/Test")]
    static void SetNamesTKPR ()
    {
        string [] guids = AssetDatabase.FindAssets ("t:texture2d", new string [] { ORG_PATH + "Test" });
        for (int i = 0; i < guids.Length; i++) {
            string path = AssetDatabase.GUIDToAssetPath (guids [i]);
            char[] separator = new char[] {'/'};
            string[] splitted = path.Split(separator);
            string fileName = splitted[splitted.Length - 1];
            separator = new char[] {'.'};
            splitted = fileName.Split(separator);
            separator = new char[] {'_'};
            string[] splitted2 = splitted[0].Split(separator);
            NameAssetBundle (path , splitted2[0]);
        }

        Debug.Log("WordフォルダのAssetBundlネームを設定しました");
    }
    /*
    [MenuItem ("MyMenu/Names/Treasure")]
    static void SetNamesTKUT ()
    {
        string [] guids = AssetDatabase.FindAssets ("t:texture2d", new string [] { ORG_PATH + "Treasure" });
        for (int i = 0; i < guids.Length; i++) {
            string path = AssetDatabase.GUIDToAssetPath (guids [i]);
            char[] separator = new char[] {'/'};
            string[] splitted = path.Split(separator);
            string fileName = splitted[splitted.Length - 1];
            separator = new char[] {'.'};
            splitted = fileName.Split(separator);
            //NameAssetBundle (path , splitted[0]);
            NameAssetBundle (path , "treasure");
        }

        Debug.Log("TreasureフォルダのAssetBundlネームを設定しました");
    }
    */
    /*
    [MenuItem ("MyMenu/Names/Texture2D")]
    static void SetNamesBG ()
    {
        string [] guids = AssetDatabase.FindAssets ("t:texture2d", new string [] { ORG_PATH + "Texture2D" });
        for (int i = 0; i < guids.Length; i++) {
            string path = AssetDatabase.GUIDToAssetPath (guids [i]);
            char[] separator = new char[] {'/'};
            string[] splitted = path.Split(separator);
            string fileName = splitted[splitted.Length - 1];
            separator = new char[] {'.'};
            splitted = fileName.Split(separator);
            NameAssetBundle (path , splitted[0]);
        }
        
        Debug.Log("Texture2DフォルダのAssetBundlネームを設定しました");
    }
    
    [MenuItem ("MyMenu/Names/Prefab")]
    static void SetNamesChara ()
    {
        string [] guids = AssetDatabase.FindAssets ("t:prefab", new string [] { ORG_PATH + "Prefab" });
        Debug.Log(guids.Length);
        for (int i = 0; i < guids.Length; i++) {
            string path = AssetDatabase.GUIDToAssetPath (guids [i]);
            char[] separator = new char[] {'/'};
            string[] splitted = path.Split(separator);
            string fileName = splitted[splitted.Length - 1];
            separator = new char[] {'.'};
            splitted = fileName.Split(separator);
            NameAssetBundle (path , splitted[0]);
        }
        
        Debug.Log("PrefabフォルダのAssetBundlネームを設定しました");
    }
    
    [MenuItem ("MyMenu/Names/AudioClip")]
    static void SetNamesAudio ()
    {
        string [] guids = AssetDatabase.FindAssets ("t:AudioClip", new string [] { ORG_PATH + "AudioClip" });
        Debug.Log(guids.Length);
        for (int i = 0; i < guids.Length; i++) {
            string path = AssetDatabase.GUIDToAssetPath (guids [i]);
            char[] separator = new char[] {'/'};
            string[] splitted = path.Split(separator);
            string fileName = splitted[splitted.Length - 1];
            separator = new char[] {'.'};
            splitted = fileName.Split(separator);
            NameAssetBundle (path , splitted[0]);
        }
        
        Debug.Log("AudioClipフォルダのAssetBundlネームを設定しました");
    }
    */
    /// <summary>
    /// AssetBundle名を名付けます.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="abName"></param>
    static void NameAssetBundle (string path, string abName = "")
    {
        AssetImporter importer = AssetImporter.GetAtPath (path);
        importer.assetBundleName = abName;
        if (abName == "") {
            importer.assetBundleVariant = "";
        } else {
            importer.assetBundleVariant = "unity3d";
        }
    }
}
