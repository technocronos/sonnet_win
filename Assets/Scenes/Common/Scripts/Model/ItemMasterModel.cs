using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// item_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class ItemMasterRow
{
    // スキーマ:
    //  item_id         int(11)       NOT NULL DEFAULT 0
    //  item_name       varchar(150)  NOT NULL DEFAULT ''
    //  category        char(3)       NOT NULL DEFAULT ''
    //  present_flg     tinyint(4)    NOT NULL DEFAULT 1
    //  flavor_text     varchar(150)  NOT NULL DEFAULT ''
    //  durability      smallint(6)   NOT NULL DEFAULT 0
    //  item_type       tinyint(4)    NOT NULL DEFAULT 0
    //  item_limitation smallint(6)   NOT NULL DEFAULT 0
    //  item_value      smallint(6)   NOT NULL DEFAULT 0
    //  item_spread     tinyint(4)    NOT NULL DEFAULT 0
    //  item_vfx        tinyint(4)    NOT NULL DEFAULT 0
    //  item_flags      smallint(6)   NOT NULL DEFAULT 0
    //  set_id          smallint(6)            DEFAULT NULL
    //  rear_level      tinyint(4)             DEFAULT NULL

    public int item_id;
    public string item_name;
    public string category;
    public int present_flg;
    public string flavor_text;
    public int durability;
    public int item_type;
    public int item_limitation;
    public int item_value;
    public int item_spread;
    public int item_vfx;
    public int item_flags;
    public int? set_id;
    public int? rear_level;
}

/// <summary>
/// item_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class ItemMasterModel
{
    /// <summary>item_master 全行</summary>
    public static List<ItemMasterRow> Rows { get; private set; } = new List<ItemMasterRow>();

    /// <summary>
    /// MasterData API の json を受け取り、item_master 部分だけをパースして保存する
    /// </summary>
    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null)
        {
            return;
        }

        if (!masterData.masters.TryGetValue("item_master", out var raw))
        {
            // item_master が含まれていない
            return;
        }

        try
        {
            // masters の value は object 型なので、一度 JToken 経由で配列として解釈する
            JToken token;

            if (raw is JToken t)
            {
                token = t;
            }
            else
            {
                // 文字列 or 匿名オブジェクトとして入っている場合もあるので文字列化してパース
                token = JToken.Parse(raw.ToString());
            }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<ItemMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("ItemMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
