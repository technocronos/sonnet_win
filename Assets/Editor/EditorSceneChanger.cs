using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

/// <summary>
/// シーンを開く拡張ウィンドウ.
/// </summary>
public class EditorSceneChanger : EditorWindow
{
    private static GUIStyle guiStyle;

    private static string[] paths;

    // スクロールの座標.
	private Vector2 _scrollPos;

	/// <summary>
	/// シーンを開く拡張ウィンドウを開く.
	/// </summary>
    [MenuItem ("MyMenu/Scene/Scenes")]
	public static void Init()
	{
        GetWindow<EditorSceneChanger> ("Scenes");
	}

	/// <summary>
	/// シーンを開く拡張ウィンドウを表示する.
	/// </summary>
	private void OnGUI()
	{
        //1度だけ取得.
        if (guiStyle == null)
        {
			GUISkin guiSkin;
#if UNITY_IOS
			guiSkin = AssetDatabase.LoadAssetAtPath<GUISkin> ("Assets/GUISkin/EditorGUISkin.guiskin");
#else
			guiSkin = AssetDatabase.LoadAssetAtPath<GUISkin> ("Assets/GUISkin/EditorGUISkin.guiskin");
#endif
            
            guiStyle = guiSkin.GetStyle ("button");
        }

        //1度だけ取得.
        if (paths == null)
        {
            string[] guidPaths = AssetDatabase.FindAssets ("t:Scene", new string[] { "Assets/Scenes" });
            paths = new string[guidPaths.Length];
            for (int i = 0; i < guidPaths.Length; i++)
            {
                paths[i] = AssetDatabase.GUIDToAssetPath (guidPaths[i]);
            }
        }

		EditorGUILayout.BeginVertical();

        _scrollPos = EditorGUILayout.BeginScrollView (_scrollPos, GUILayout.Height (position.height));

        for (int i = 0; i < paths.Length; i++)
        {
            string path = paths[i];
            string sceneName = path.Substring (path.LastIndexOf ("/") + 1);
            if (GUILayout.Button (sceneName, guiStyle))
            {
                if (EditorApplication.SaveCurrentSceneIfUserWantsTo ())
                {
                    EditorApplication.OpenScene (path);
                }
            }
        }

		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();
	}
	
	/// <summary>
	/// <para>指定したディレクトリ内の指定した</para>
	/// <para>いずれかの拡張子を持つファイル名 (パスを含む) を返します</para>
	/// </summary>
	public static string[] GetFiles
	( 
		string          path       , 
		params string[] extensions 
	)
	{
		return Directory
				.GetFiles( path, "*.*" )
				.Where( c => extensions.Any( extension => c.EndsWith( extension ) ) )
				.ToArray()
			;
	}
    
	/// <summary>
	/// <para>指定したディレクトリの中から、</para>
	/// <para>指定したいずれかの拡張子を持ち、</para>
	/// <para>サブディレクトリを検索するかどうかを決定する</para>
	/// <para>値を持つファイル名 (パスを含む) を返します</para>
	/// </summary>
	public static string[] GetFiles
	( 
		string          path         , 
		SearchOption    searchOption , 
		params string[] extensions 
	)
	{
		return Directory
				.GetFiles( path, "*.*", searchOption )
				.Where( c => extensions.Any( extension => c.EndsWith( extension ) ) )
				.ToArray()
			;
	}
}