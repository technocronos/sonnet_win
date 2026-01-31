using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// mount_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class MountMasterRow
{
    public string race;
    public int mount_id;
    public string mount_name;
    public int default_id;
    public int slot_no;
    public int sort_order;
}

/// <summary>
/// mount_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class MountMasterModel
{
    public static List<MountMasterRow> Rows { get; private set; } = new List<MountMasterRow>();

    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("mount_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<MountMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("MountMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
