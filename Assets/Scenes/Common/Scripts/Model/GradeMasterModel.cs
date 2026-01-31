using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// grade_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class GradeMasterRow
{
    public int grade_id;
    public string grade_name;
    public int? raise_border;
    public int? abase_border;
    public int battle_reward;
    public int? dtech_id;
}

/// <summary>
/// grade_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class GradeMasterModel
{
    public static List<GradeMasterRow> Rows { get; private set; } = new List<GradeMasterRow>();

    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("grade_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<GradeMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("GradeMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
