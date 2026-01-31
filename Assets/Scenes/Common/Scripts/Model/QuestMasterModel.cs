using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// quest_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class QuestMasterRow
{
    public int quest_id;
    public string quest_name;
    public int place_id;
    public string type;
    public int consume_pt;
    public int penalty_pt;
    public int content_id;
    public int upper_level;
    public int repeatable;
    public string preferred_level;
    public int sort_order;
    public string flavor_text;
    public string start_date;
    public string end_date;
    public int? gacha_id;
}

/// <summary>
/// quest_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class QuestMasterModel
{
    public static List<QuestMasterRow> Rows { get; private set; } = new List<QuestMasterRow>();

    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("quest_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<QuestMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("QuestMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
