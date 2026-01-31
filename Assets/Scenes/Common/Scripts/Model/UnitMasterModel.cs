using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// unit_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class UnitMasterRow
{
    public int character_id;
    public string mobility;
    public int move_pow;
    public int battle_brain;
    public int? dtech1_id;
    public int reward_exp;
    public int reward_gold;
    public int? normal_drop;
    public int? rare_drop;
    public int? srare_drop;
}

/// <summary>
/// unit_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class UnitMasterModel
{
    public static List<UnitMasterRow> Rows { get; private set; } = new List<UnitMasterRow>();

    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("unit_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<UnitMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("UnitMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
