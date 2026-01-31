using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// place_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class PlaceMasterRow
{
    public int place_id;
    public string place_name;
    public int region_id;
    public int map_x;
    public int map_y;
    public int? arrival_event;
}

/// <summary>
/// place_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class PlaceMasterModel
{
    public static List<PlaceMasterRow> Rows { get; private set; } = new List<PlaceMasterRow>();

    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("place_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<PlaceMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("PlaceMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
