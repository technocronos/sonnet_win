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
        PlayerSettings.keystorePass = "android";
        PlayerSettings.keyaliasPass = "android";
#endif
    }

#endif
}
