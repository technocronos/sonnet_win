using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// help_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class HelpMasterRow
{
    public string help_id;
    public string help_title;
    public string help_body;
    public int unlock_level;
    public int sort_order;
}

/// <summary>
/// help_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class HelpMasterModel
{
    public static List<HelpMasterRow> Rows { get; private set; } = new List<HelpMasterRow>();

    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("help_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<HelpMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("HelpMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
