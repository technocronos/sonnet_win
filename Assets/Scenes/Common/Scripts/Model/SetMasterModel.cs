using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// set_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class SetMasterRow
{
    public int set_id;
    public string set_name;
    public string set_text;
    public int? rear_id;
}

/// <summary>
/// set_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class SetMasterModel
{
    public static List<SetMasterRow> Rows { get; private set; } = new List<SetMasterRow>();

    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("set_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<SetMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("SetMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
