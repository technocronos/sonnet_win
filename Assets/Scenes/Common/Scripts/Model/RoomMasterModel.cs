using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// room_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class RoomMasterRow
{
    public int room_id;
    public string background;
    public string structure;
    public string overlayer1;
    public string overlayer2;
    public string cover;
    public string mats;
    public string structure_head;
    public string structure_left;
    public string structure_right;
    public string structure_foot;
}

/// <summary>
/// room_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class RoomMasterModel
{
    public static List<RoomMasterRow> Rows { get; private set; } = new List<RoomMasterRow>();

    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("room_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<RoomMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("RoomMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
