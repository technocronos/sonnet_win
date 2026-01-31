using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// item_level_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class ItemLevelMasterRow
{
    public int item_id;
    public int level;
    public int evolution;
    public int exp;
    public int attack1;
    public int attack2;
    public int attack3;
    public int defence1;
    public int defence2;
    public int defence3;
    public int speed;
    public int defenceX;
}

/// <summary>
/// item_level_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class ItemLevelMasterModel
{
    public static List<ItemLevelMasterRow> Rows { get; private set; } = new List<ItemLevelMasterRow>();

    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("item_level_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<ItemLevelMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("ItemLevelMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
