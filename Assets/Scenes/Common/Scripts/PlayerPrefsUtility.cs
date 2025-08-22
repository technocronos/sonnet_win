//  PlayerPrefsUtility.cs
//  http://kan-kikuchi.hatenablog.com/entry/PlayerPrefsUtility
//
//  Created by kan kikuchi on 2015.10.22.

using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

/// <summary>
/// PlayerPrefsに関する便利クラス
/// </summary>
public static class PlayerPrefsUtility {

    //=================================================================================
    //共通
    //=================================================================================

    //keyを暗号化
    private static string EncryptKey(string key){
        return key;
    }

    //valueを暗号化
    private static string EncryptValue(string value){
        return value;
    }

    //valueを複合化
    private static string DecryptValue(string value){
        return value;
    }

    //指定したkeyで保存されている値を複合化して読み込む
    private static string LoadAndDecryptValue(string key){
        string str = PlayerPrefs.GetString (EncryptKey(key), "");
        if(string.IsNullOrEmpty(str)){
            return "";
        }
        return DecryptValue (str);
    }

    //=================================================================================
    //保存
    //=================================================================================

    public static void Save(string key , int value){
        PlayerPrefs.SetString (EncryptKey(key), EncryptValue(value.ToString()) );
    }

    public static void Save(string key , float value){
        PlayerPrefs.SetString (EncryptKey(key), EncryptValue(value.ToString()) );
    }

    public static void Save(string key , string value){
        PlayerPrefs.SetString (EncryptKey(key), EncryptValue(value) );
    }

    public static void Save(string key, bool value){
        //そのままでは保存出来ないので指定したkeyが空かどうかで判断
        if(value){
            PlayerPrefs.SetString (key, key);
        }
        else{
            PlayerPrefs.SetString (key, "");
        }
    }

    /// <summary>
    /// リストを保存
    /// </summary>
    public static void SaveList<T>(string key , List<T> value){
        string serizlizedList = Serialize<List<T>> (value);
        PlayerPrefs.SetString (EncryptKey(key), EncryptValue(serizlizedList));
    }

    /// <summary>
    /// ディクショナリーを保存
    /// </summary>
    public static void SaveDict<Key, Value>(string key , Dictionary<Key, Value> value){
        string serizlizedDict = Serialize<Dictionary<Key, Value>> (value);
        PlayerPrefs.SetString (EncryptKey(key), EncryptValue(serizlizedDict));
    }

    //=================================================================================
    //読み込み
    //=================================================================================

    public static int Load(string key, int defaultValue){
        string valueStr = LoadAndDecryptValue (key);
        if(string.IsNullOrEmpty(valueStr)){
            return defaultValue;
        }

        return int.Parse (valueStr);
    }

    public static float Load(string key, float defaultValue){
        string valueStr = LoadAndDecryptValue (key);
        if(string.IsNullOrEmpty(valueStr)){
            return defaultValue;
        }

        return float.Parse (valueStr);
    }

    public static string Load(string key, string defaultValue){
        string valueStr = LoadAndDecryptValue (key);
        if(string.IsNullOrEmpty(valueStr)){
            return defaultValue;
        }

        return valueStr;
    }

    /// <summary>
    /// リストを読み込み
    /// </summary>
    public static List<T> LoadList<T> (string key){
        //keyがある時だけ読み込む
        if (PlayerPrefs.HasKey (EncryptKey(key))) {
            string serizlizedList = LoadAndDecryptValue (key);
            return Deserialize<List<T>> (serizlizedList);
        }

        return new List<T> ();
    }

    /// <summary>
    /// ディクショナリーを読み込み
    /// </summary>
    public static Dictionary<Key, Value> LoadDict<Key, Value> (string key){
        //keyがある時だけ読み込む
        if (PlayerPrefs.HasKey (EncryptKey(key))) {
            string serizlizedDict = LoadAndDecryptValue (key);
            return Deserialize<Dictionary<Key, Value>> (serizlizedDict);
        }

        return new Dictionary<Key, Value> ();
    }

    //=================================================================================
    //シリアライズ、デシリアライズ
    //=================================================================================

    private static string Serialize<T> (T obj){
        BinaryFormatter binaryFormatter = new BinaryFormatter ();
        MemoryStream    memoryStream    = new MemoryStream ();
        binaryFormatter.Serialize (memoryStream , obj);
        return Convert.ToBase64String (memoryStream   .GetBuffer ());
    }

    private static T Deserialize<T> (string str){
        BinaryFormatter binaryFormatter = new BinaryFormatter ();
        MemoryStream    memoryStream    = new MemoryStream (Convert.FromBase64String (str));
        return (T)binaryFormatter.Deserialize (memoryStream);
    }
}