using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using System.Collections.ObjectModel;
using Scenes.Common.Scripts;
using System.Text.RegularExpressions;

/// <summary>
/// ユーティリティクラス。
/// </summary>
public static class Utility
{

    private static DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    // DateTimeからUnixTimeへ変換
    public static long GetUnixTime(DateTime dateTime)
    {
        return (long)(dateTime.ToUniversalTime() - UnixEpoch.ToUniversalTime()).TotalMilliseconds;
    }

    // UnixTimeからDateTimeへ変換
    public static DateTime GetDateTime(long unixTime)
    {
        DateTime utc = UnixEpoch.AddSeconds(unixTime);

        // タイムゾーンを指定してUTCからJSTへ変換
        TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(utc);
        return UnixEpoch.AddSeconds(unixTime + offset.TotalSeconds);

    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * 日付をあとどれくらいか出す。
     */
    public static string compareDate(string expire_at)
    {
        string expire = "";
        string f = "yyyy-MM-dd HH:mm:ss";
        //通信で帰って来たものが"2017-11-06 09%3A54%3A15"みたくエンコードされてる場合があるため緊急処置としてデコードする。
        expire_at = WWW.UnEscapeURL(expire_at);

        //今日の日時を表示
        DateTime date1 = DateTime.ParseExact(expire_at, f, null);
        DateTime date2 = DateTime.Now;
        TimeSpan diff = date1 - date2;

        var diffResult = diff.TotalMilliseconds / 86400000;//1日は86400000ミリ秒

        if (diffResult < 1)
        {
            //1日以下の場合は時間で返す
            diffResult = diff.TotalMilliseconds / 3600000;
            if (diffResult < 1)
            {
                //あと1時間以下の場合は分で返す
                diffResult = diff.TotalMilliseconds / 60000;
                expire = Utility.getText("TEXT_MINUTE").Replace("{0}", Math.Floor(diffResult + 1).ToString());
            }
            else
            {
                expire = Utility.getText("TEXT_HOUR").Replace("{0}", Math.Floor(diffResult + 1).ToString()); 
            }
        }
        else
        {
            expire = Utility.getText("TEXT_DAY").Replace("{0}", Math.Floor(diffResult + 1).ToString());
        }

        return expire;
    }

    /// <summary>
    ///  byteからMBへ変換します。
    /// </summary>
    public static float ByteToMB(long _byteSize)
    {
        long KB = (_byteSize + 1023) / 1024;
        float MB = (float)(KB + 1023) / (float)1024;

        return MB;
    }

    /// <summary>
    ///  byteからKBへ変換します。
    /// </summary>
    public static long ByteToKB(long _byteSize)
    {
        long KB = (_byteSize + 1023) / 1024;
        return KB;
    }

    /// <summary>
    /// フロートの秒数をmsに変換します
    /// </summary>
    public static int ToMilliseconds(this float _second)
    {
        return Mathf.RoundToInt(_second * 1000f);
    }

    /// <summary>
    ///  桁数を取得します。
    /// </summary>
    public static int GetDigit(int _value)
    {
        return (_value == 0) ? 1 : (int)Mathf.Log10(_value) + 1;
    }

    public static string getHistoryText(jsonHistory history)
    {
        var text = "";

        jsonConstants constants;
        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        if (history.deleted_at != "")
        {
            text = Utility.getText("TEXT_DELETED");
        }
        else if (history.type == constants.History_Log.TYPE_BATTLE_CHALLENGE || history.type == constants.History_Log.TYPE_BATTLE_DEFENCE)
        {
            if (history.battle != null)
            {
                /* 誰と戦ったのか */
                if (history.type == 1)
                    text = Utility.getText("TEXT_CHARANGE").Replace("{0}", history.battle.rival_character_name);
                else
                    text = Utility.getText("TEXT_CHARANGED").Replace("{0}", history.battle.rival_character_name);

                /* バトルサマリ */
                text += Utility.getText("TEXT_RESULT") + "：" + Utility.getText("TEXT_" + history.battle.bias_status);

                if (history.battle.tournament_id == constants.Tournament_Master.TOUR_MAIN)
                {
                    text +=  Utility.getText("TEXT_GRADE_POINT") + "：" + history.battle.bias_result.gain.grade;
                }
                /* コメント */
                /*
                if (history.battle.comment)
                {
                    text += AppUtil.nl2br(history.battle.comment);
                }
                */
            }
            else
            {
                text = Utility.getText("TEXT_BATTLEINFO_DELETED");
            }
        }
        else if (history.type == constants.History_Log.TYPE_CHANGE_GRADE)
        {

            if (history.ref2_value > 0)
                text = Utility.getText("TEXT_HISTORY_GRADE_UP").Replace("{0}", history.player_name).Replace("{1}", history.grade.grade_name);
            else
                text = Utility.getText("TEXT_HISTORY_GRADE_DOWN").Replace("{0}", history.player_name).Replace("{1}", history.grade.grade_name);

        }
        else if (history.type == constants.History_Log.TYPE_LEVEL_UP)
        {
            text = Utility.getText("TEXT_HISTORY_LEVEL_UP").Replace("{0}", history.player_name).Replace("{1}", history.ref2_value.ToString());
        }
        else if (history.type == constants.History_Log.TYPE_EFFECT_TIMEUP)
        {
            text = Utility.getText("TEXT_HISTORY_ITEM_EFFECT_END").Replace("{0}", history.player_name).Replace("{1}", history.effect_name);
        }
        /*
        else if (history.type == constants.History_Log.TYPE_INVITE_SUCCESS)
        {
            if (history.invited)
            {
                text = history.invited.short_name + "さんがｹﾞｰﾑ招待に応じてくれました。特典ｹﾞｯﾄ!";
            }
            else
            {
                text = "友だち招待に応じたため特典ｹﾞｯﾄ!";
            }
        }
        */
        else if (history.type == constants.History_Log.TYPE_PRESENTED)
        {
            //廃止
            //text = history.giver.short_name + "さんから" + history.item.item_name + "をﾌﾟﾚｾﾞﾝﾄしてもらいました";

        }
        else if (history.type == constants.History_Log.TYPE_QUEST_FIN)
        {
            //廃止
        }
        else if (history.type == constants.History_Log.TYPE_ITEM_BREAK)
        {
            text = Utility.getText("TEXT_HISTORY_ITEM_BREAK").Replace("{0}", history.item.item_name);
        }
        else if (history.type == constants.History_Log.TYPE_ITEM_LVUP)
        {
            text = Utility.getText("TEXT_HISTORY_ITEM_LEVEL_UP").Replace("{0}", history.item.item_name).Replace("{1}", history.ref2_value.ToString());
        }
        else if (history.type == constants.History_Log.TYPE_WEEKLY_HIGHER)
        {
            text = Utility.getText("TEXT_HISTORY_WEEKLY_RANK_RESULT").Replace("{0}", history.ref1_value.ToString()).Replace("{1}", history.item.item_name);
        }
        else if (history.type == constants.History_Log.TYPE_CAPTURE)
        {
            text = Utility.getText("TEXT_HISTORY_MONSTER_GET").Replace("{0}", history.rare_name).Replace("{1}", history.monster.monster_name);
        }
        else if (history.type == constants.History_Log.TYPE_ADMIRED)
        {
            //廃止
        }
        else if (history.type == constants.History_Log.TYPE_REPLIED)
        {
            //廃止
        }
        else if (history.type == constants.History_Log.TYPE_COMMENT)
        {
            //廃止
        }
        else if (history.type == constants.History_Log.TYPE_QUEST_FIN2)
        {
            text = Utility.getText("TEXT_HISTORY_QUEST_RESULT_" + history.summary.result).Replace("{0}", history.summary.quest_name);

            if (history.summary.attain_stair > 0)
                text += Utility.getText("TEXT_HISTORY_QUEST_RESULT_ATTAIN_STAIR").Replace("{0}", history.summary.attain_stair.ToString());

        }
        else if (history.type == constants.History_Log.TYPE_TEAM_BATTLE)
        {
            //廃止
        }

        return text;
    }



    //--------------------------------------------------------------------------------
    // 引数に渡したオブジェクトをディープコピーしたオブジェクトを生成して返す
    // ジェネリックメソッド版
    //--------------------------------------------------------------------------------
    public static T DeepCopy<T>(T target)
    {
        T result;
        BinaryFormatter b = new BinaryFormatter();
        MemoryStream mem = new MemoryStream();

        try
        {
            b.Serialize(mem, target);
            mem.Position = 0;
            result = (T)b.Deserialize(mem);
        }
        finally
        {
            mem.Close();
        }

        return result;
    }
    // 拡張メソッド版
    public static object DeepCopy(this object target)
    {
        object result;
        BinaryFormatter b = new BinaryFormatter();
        MemoryStream mem = new MemoryStream();

        try
        {
            b.Serialize(mem, target);
            mem.Position = 0;
            result = b.Deserialize(mem);
        }
        finally
        {
            mem.Close();
        }

        return result;
    }

    public static Dictionary<string, string> ParseUrl(string url)
    {
        //scene=Home&id=1000 のように&で連結する。sceneは遷移するシーン名で必須パラメータ。
        string[] _t = url.Split(new char[] { '&' });
        Dictionary<string, string> Urls = new Dictionary<string, string>();

        //分解してDictionaryに格納する
        foreach (string s in _t)
        {
            Urls[s.Split('=')[0]] = s.Split('=')[1];
        }

        return Urls;
    }

    /// <summary>
    /// イメージをaddressableから返す
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
    public static Sprite getAssetImage(string filepath)
    {
        string name = filepath.Replace("Image/", "");

        //存在確認
        var exists = Addressables.LoadResourceLocationsAsync(name);
        var file = exists.WaitForCompletion();

        if (file.Count == 0) return null;

        var op = Addressables.LoadAssetAsync<Sprite>(name);
        Sprite sprite = op.WaitForCompletion();

        return sprite;
    }

    //-----------------------------------------------------------------------------------------------------
    /**
     * アイテムのアイコンURLを返す
     *
     */
    public static string getItemIconURL(int item_id)
    {
        var icon_url = item_id.ToString("D5");
        //汎用攻撃アイコンを使用する場合は "att"。
        if (3000 <= item_id && item_id <= 3999)
        {
            icon_url = "att";
        }
        return "Image/Item/" + icon_url;
    }

    //-----------------------------------------------------------------------------------------------------
    /**
     * ガチャのアイコンURLを返す
     *
     */
    public static string getGachaBannarURL(int gacha_id, string size = null)
    {
        var icon_url = gacha_id.ToString("D5");
        return "img/gacha/" + icon_url + size;
    }

    /// <summary>
    /// 消費アイテムの効果を表示するテンプレート
    ///
    /// パラメータ)
    /// item 以下のキーを含む配列。
    /// item_type
    /// item_value
    /// item_limitation
    /// </summary>
    public static string ItemEffects(jsonEquip item)
    {

        string effects = "";
        jsonConstants constants = APIConnectManager.Instance.constants;

        if (item.item_type == constants.Item_Master.ITEM_RECV_HP)
        {
            effects = Utility.getText("ITEMEFFECT_RECV_HP1").Replace("{0}", item.item_value.ToString());
            effects += "\n";
            effects += Utility.getText("ITEMEFFECT_RECV_HP2").Replace("{0}", item.item_limitation.ToString());
            effects += "\n";
            effects += Utility.getText("ITEMEFFECT_RECV_HP3").Replace("{0}", (item.item_spread + 1).ToString());
        }
        else if (item.item_type == constants.Item_Master.ITEM_RECV_AP)
        {
            effects = Utility.getText("ITEMEFFECT_RECV_AP").Replace("{0}", item.item_value.ToString());
        }
        else if (item.item_type == constants.Item_Master.ITEM_RECV_MP)
        {
            effects = Utility.getText("ITEMEFFECT_RECV_MP").Replace("{0}", item.item_value.ToString()); 
        }
        else if (item.item_type == constants.Item_Master.ITEM_INCR_PARAM)
        {
            effects = Utility.getText("ITEMEFFECT_INCR_PARAM").Replace("{0}", item.item_value.ToString()).Replace("{1}", item.item_limitation.ToString()); 
        }
        else if (item.item_type == constants.Item_Master.ITEM_DECR_PARAM)
        {
            effects = Utility.getText("ITEMEFFECT_DECR_PARAM").Replace("{0}", item.item_value.ToString()).Replace("{1}", (item.item_value * constants.Character_Info.HP_SCALE).ToString()).Replace("{2}", (item.item_value * 8).ToString());
        }
        else if (item.item_type == constants.Item_Master.ITEM_INCR_EXP)
        {
            effects = Utility.getText("ITEMEFFECT_INCR_EXP").Replace("{0}", item.item_limitation.ToString()).Replace("{1}", item.item_value.ToString());
            effects += "\n";
            effects += Utility.getText("ITEMEFFECT_NOTICE_MENTE");
        }
        else if (item.item_type == constants.Item_Master.ITEM_REPAIRE)
        {
            effects = Utility.getText("ITEMEFFECT_REPAIRE").Replace("{0}", item.item_value.ToString());             
        }
        else if (item.item_type == constants.Item_Master.ITEM_TACT_ATT)
        {
            effects = Utility.getText("ITEMEFFECT_TACT_ATT1").Replace("{0}", item.item_value.ToString());
            effects += "\n";
            effects += Utility.getText("ITEMEFFECT_TACT_ATT2").Replace("{0}", item.item_limitation.ToString()); 
            effects += "\n";
            effects += Utility.getText("ITEMEFFECT_TACT_ATT3").Replace("{0}", (item.item_spread + 1).ToString());
        }
        else if (item.item_type == constants.Item_Master.ITEM_ATTRACT)
        {
            if (item.item_value == 2)
                effects = Utility.getText("ITEMEFFECT_ATTRACT2").Replace("{0}", item.item_limitation.ToString());
            else
                effects = Utility.getText("ITEMEFFECT_ATTRACT1").Replace("{0}", item.item_limitation.ToString());

            effects += "\n";
            effects += Utility.getText("ITEMEFFECT_NOTICE_MENTE");
        }
        else if (item.item_type == constants.Item_Master.ITEM_DTECH_UPPER)
        {
            if (item.item_value == 2)
                effects = Utility.getText("ITEMEFFECT_DTECH_UPPER2").Replace("{0}", item.item_limitation.ToString()).Replace("{1}", constants.Item_Master.ITEM_DTECH_UPPER_INVOKE.ToString());
            else
                effects = Utility.getText("ITEMEFFECT_DTECH_UPPER1").Replace("{0}", item.item_limitation.ToString()).Replace("{1}", constants.Item_Master.ITEM_DTECH_UPPER_INVOKE.ToString());

            effects += "\n";
            effects += Utility.getText("ITEMEFFECT_NOTICE_MENTE");
            effects += "\n";
            effects += Utility.getText("ITEMEFFECT_NOTICE_GRADE");
        }
        else if (item.item_type == constants.Item_Master.ITEM_CONTINUE_BATTLE)
        {
            effects = Utility.getText("ITEMEFFECT_NOTICE_NO_CONTENUE") +"\n";
            effects += Utility.getText("ITEMEFFECT_NOTICE_CONTENUE_COUNT") + "\n";
            effects += Utility.getText("ITEMEFFECT_NOTICE_CONTENUE_STAR") + "\n";
        }

        return effects;
    }

    /// <summary>
    /// RGB を 0 ～ 255 で指定したカラー値を取得
    /// </summary>
    /// <param name="r">赤</param>
    /// <param name="g">緑</param>
    /// <param name="b">青</param>
    public static Color Rgb(int r, int g, int b)
    {
        return new Color((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f);
    }
    /// <summary>
    /// カラーを #RRGGBB の形で取得
    /// </summary>
    /// <param name="hexrgb">16進数のカラー値 RRGGBB</param>
    public static Color Hex(int hexrgb)
    {
        int r = (hexrgb >> 16) & 0xff;
        int g = (hexrgb >> 8) & 0xff;
        int b = hexrgb & 0xff;
        return Rgb(r, g, b);
    }

    public static string getText(string symbol)
    {
        return TextMasterModel.GetText(symbol);
    }

    public static string getStatusIcon(int status, string patarn = "")
    {
        int _lang = PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        string iconname = "Image/quest_status_" + status + patarn;

        if (_lang == 0)
            return iconname;
        else
            return iconname + "_en";
    }

    public static string getVCoinAmount(float amount)
    {

        return decimal.Parse(amount.ToString(), System.Globalization.NumberStyles.AllowExponent | System.Globalization.NumberStyles.AllowDecimalPoint).ToString();
    }

}
