using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class StartUp
{

#if UNITY_EDITOR

    static StartUp()
    {
#if RELEASE
        PlayerSettings.keystorePass = "ME2U537M";
        PlayerSettings.keyaliasPass = "ME2U537M";
#else
        PlayerSettings.Android.keystorePass = "android";
        PlayerSettings.Android.keyaliasPass = "android";
#endif
    }

#endif
}
