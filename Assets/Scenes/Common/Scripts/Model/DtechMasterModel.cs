using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// dtech_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class DtechMasterRow
{
    public int dtech_id;
    public string dtech_name;
    public string dtech_desc;
    public int invoke_rate;
    public int code_id;
    public string value1;
    public string value2;
    public string value3;
    public int graphic_id;
}

/// <summary>
/// dtech_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class DtechMasterModel
{
    public static List<DtechMasterRow> Rows { get; private set; } = new List<DtechMasterRow>();

    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("dtech_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<DtechMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("DtechMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
