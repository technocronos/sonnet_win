using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// drama_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class DramaMasterRow
{
    public int drama_id;
    public string flow;
    public string bg1_path;
    public string bg2_path;
    public string bg3_path;
}

/// <summary>
/// drama_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class DramaMasterModel
{
    public static List<DramaMasterRow> Rows { get; private set; } = new List<DramaMasterRow>();

    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("drama_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<DramaMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("DramaMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
