using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// square_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class SquareMasterRow
{
    public int square_id;
    public int cost;
    public int cost_aquatic;
    public int cost_amphibia;
    public string category;
}

/// <summary>
/// square_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class SquareMasterModel
{
    public static List<SquareMasterRow> Rows { get; private set; } = new List<SquareMasterRow>();

    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("square_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<SquareMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("SquareMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
