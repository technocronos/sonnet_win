using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

public class ChangeEnvironment
{

    // ?????????
    [MenuItem("Environment/Develop")]
    static void Develop()
    {
        var symbols = GetSymbols();

        Predicate<string> predicate = FindStr;
        symbols.RemoveAll(predicate);

        symbols.Add("DEVELOP");
        symbols.Remove("RELEASE");
        SetSymbols(symbols);
    }

    // ?????????
    [MenuItem("Environment/Release")]
    static void Release()
    {
        var symbols = GetSymbols();

        Predicate<string> predicate = FindStr;
        symbols.RemoveAll(predicate);

        symbols.Add("RELEASE");
        symbols.Remove("DEVELOP");
        SetSymbols(symbols);
    }

    private static bool FindStr(string str)
    {
        if (str == "DEVELOP" || str == "RELEASE")
            return true;
        else
            return false;
    }

    // ??????????????????
    static List<string> GetSymbols()
    {
        return PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';').ToList();
    }

    // ????????????
    static void SetSymbols(List<string> symbols)
    {
        var symbolStr = string.Empty;
        symbols.ForEach(s => symbolStr += s + ";");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbolStr);
    }
}