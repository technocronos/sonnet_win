using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// gacha_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class GachaMasterRow
{
    public int gacha_id;
    public string gacha_name;
    public int price;
    public int? price_bulk;
    public int? freeticket_item_id;
    public int? freeticket_count;
    public string caption;
    public string flavor_text;
    public int? unlock_level;
    public int? gacha_kind;
    public int? sp_flg;
    public int? wk_flg;
    public int? close_flg;
}

/// <summary>
/// gacha_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class GachaMasterModel
{
    public static List<GachaMasterRow> Rows { get; private set; } = new List<GachaMasterRow>();

    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("gacha_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<GachaMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("GachaMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
