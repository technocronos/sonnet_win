using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// monster_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class MonsterMasterRow
{
    public int character_id;
    public int category;
    public string monster_no;
    public int rare_level;
    public int appearance_area;
    public string habitat;
    public string flavor_text;
}

/// <summary>
/// monster_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class MonsterMasterModel
{
    public static List<MonsterMasterRow> Rows { get; private set; } = new List<MonsterMasterRow>();

    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("monster_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<MonsterMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("MonsterMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
