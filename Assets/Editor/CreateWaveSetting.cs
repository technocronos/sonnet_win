using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

public class MyMenuSetting : EditorWindow
{

    [MenuItem("MyMenu/PlayerPrefs全削除")]
    static void DeleteUserId()
    {
        PlayerPrefs.DeleteAll();
    }
    
    [MenuItem("MyMenu/PlayerPrefsキャッシュ全削除")]
    static void PlayerPrefsDelete()
    {
        PlayerPrefs.DeleteAll();
        Delete(Application.persistentDataPath);
        Delete(Application.temporaryCachePath);
    }
    
    //Assetsディレクトリ以下にあるTestディレクトリを削除
    /// <summary>
    /// 指定したディレクトリとその中身を全て削除する
    /// </summary>
    public static void Delete(string targetDirectoryPath){
        if (!Directory.Exists (targetDirectoryPath)) {
            return;
        }

        Debug.Log(targetDirectoryPath　+　"フォルダの中を空にします");
        //ディレクトリ以外の全ファイルを削除
        string[] filePaths = Directory.GetFiles(targetDirectoryPath);
        foreach (string filePath in filePaths){
            File.SetAttributes(filePath, FileAttributes.Normal);
            File.Delete(filePath);
        }

        //ディレクトリの中のディレクトリも再帰的に削除
        string[] directoryPaths = Directory.GetDirectories(targetDirectoryPath);
        foreach (string directoryPath in directoryPaths){
            Delete(directoryPath);
        }

        //中が空になったらディレクトリ自身も削除
        Directory.Delete(targetDirectoryPath, false);
    }

    /// <summary>
    /// メイン画面からPlayします.
    /// </summary>
    [MenuItem ("MyMenu/Scene/MainPlay")]
    static void PlayTitle ()
    {
        Change ("Assets/Scenes/Main/Main.unity");
        EditorApplication.isPlaying = true;
    }
	
    static void Change (string scene)
    {
        bool isCancel = EditorApplication.SaveCurrentSceneIfUserWantsTo ();
        if (!isCancel) return;

        EditorApplication.OpenScene (scene);
    }
}
