using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// equippable_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class EquippableMasterRow
{
    public string race;
    public int mount_id;
    public int item_id;
    public int equippable_level;
}

/// <summary>
/// equippable_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class EquippableMasterModel
{
    public static List<EquippableMasterRow> Rows { get; private set; } = new List<EquippableMasterRow>();

    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("equippable_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<EquippableMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("EquippableMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
