using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// condition_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class ConditionMasterRow
{
    public int value_type;
    public int owner_id;
    public int sequence;
    public int flag_group;
    public int flag_id;
    public int? flag_group2;
    public int? flag_id2;
    public int go_value;
    public string flavor_text;
}

/// <summary>
/// condition_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class ConditionMasterModel
{
    public static List<ConditionMasterRow> Rows { get; private set; } = new List<ConditionMasterRow>();

    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("condition_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<ConditionMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("ConditionMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
