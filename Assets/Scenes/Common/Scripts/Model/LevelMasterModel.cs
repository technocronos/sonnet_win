using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// level_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class LevelMasterRow
{
    public string race;
    public int level;
    public int exp;
    public int param_growth;
    public int auto_growth;
    public int member_limit;
}

/// <summary>
/// level_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class LevelMasterModel
{
    public static List<LevelMasterRow> Rows { get; private set; } = new List<LevelMasterRow>();

    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("level_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<LevelMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("LevelMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
