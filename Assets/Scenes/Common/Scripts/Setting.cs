using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Common.Scripts
{
    public class Settings
    {
        public static string APP_BANDLE_ID = "jp.technocronos.sonnet";

        public static string PREF_KEY_HIS_USER_ID = "PREF_KEY_HIS_USER_ID";
        public static string LANGUAGE_SELECTED = "LANGUAGE_SELECTED";
        public static string LANGUAGE_SELECTED_KEY = "LANGUAGE_SELECTED_KEY";
        public static string SUPPORT_MAIL_ADDRESS = "sonnet.userhelp @gmail.com";
        public static string TUTORIAL_GLOBALMAP = "TUTORIAL_GLOBALMAP";
        public static string TUTORIAL_HISPAGE = "TUTORIAL_HISPAGE";
        public static string TUTORIAL_READY = "TUTORIAL_READY";
        public static string TUTORIAL_SPHERE_NEW = "TUTORIAL_SPHERE_NEW";
        public static string TUTORIAL_SPHERE_ATT = "TUTORIAL_SPHERE_ATT";
        public static string TUTORIAL_SPHERE_ITM = "TUTORIAL_SPHERE_ITM";
        public static string AF_INVITE_KEY = "deep_link_sub1";
        public static float PAR_FRAME = 0.01f;

        public static string EASYMODE_SPHEREID = "EASYMODE_SPHEREID";
        public static string EASYMODE_SPHERE = "EASYMODE_SPHERE";

        public static string RESOURCE_HASH = "RESOURCE_HASH";

        public static string CHANNELID_AP_RECV = "AP_RECV";
        public static int IOSID_AP_RECV = 1;
        public static string CHANNELID_BP_RECV = "BP_RECV";
        public static int IOSID_BP_RECV = 2;

        public static string YOUR_ONESIGNAL_APP_ID = "645981aa-d521-4766-819e-2b57668f998d";

        public static string AP_DEVKEY = "rokG9uJaSGjzDqqJ9n98i4";
        public static string AP_APPID = "1372485938";

        public static string SPHERE_FLAG_KEY = "SPHERE_FLAG_KEY";

#if RELEASE
        public const string Environment = "本番環境";
        public static string Domain = "https";
        public static string Host = "native.sonnet.crns-game.net";
        public static string RsourceHost = "crns-games.win";
        public const bool IsDevelop = false;
#else
        public const string Environment = "開発環境";
        public static string Domain = "http";
        public static string Host = "localhost:8080";
        public static string RsourceHost = "test.native.sonnet.crns-game.net";
        public const bool IsDevelop = true;
#endif

        public static string ver = "1";

        public static string IOS_APP_ID = "1372485938";
    }

    //https://qiita.com/sugasaki/items/ea5eec093ad7934abd5c
    //C# > enumに文字列を割り当てる。
    /// <summary>
    /// Enumに文字列を付加するためのAttributeクラス
    /// </summary>
    public class StringValueAttribute : Attribute
    {
        /// <summary>
        /// Holds the stringvalue for a value in an enum.
        /// </summary>
        public string StringValue { get; protected set; }

        /// <summary>
        /// Constructor used to init a StringValue Attribute
        /// </summary>
        /// <param name="value"></param>
        public StringValueAttribute(string value)
        {
            this.StringValue = value;
        }
    }

    public static class CommonAttribute
    {

        /// <summary>
        /// Will get the string value for a given enums value, this will
        /// only work if you assign the StringValue attribute to
        /// the items in your enum.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetStringValue(this Enum value)
        {
            // Get the type
            Type type = value.GetType();

            // Get fieldinfo for this type
            System.Reflection.FieldInfo fieldInfo = type.GetField(value.ToString());

            //範囲外の値チェック
            if (fieldInfo == null) return null;

            StringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

            // Return the first if there was a match.
            return attribs.Length > 0 ? attribs[0].StringValue : null;

        }
    }
}
