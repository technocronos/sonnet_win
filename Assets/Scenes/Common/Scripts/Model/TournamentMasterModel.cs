using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// tournament_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class TournamentMasterRow
{
    public int tournament_id;
    public string tournament_name;
    public string open_date;
    public string close_date;
}

/// <summary>
/// tournament_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class TournamentMasterModel
{
    public static List<TournamentMasterRow> Rows { get; private set; } = new List<TournamentMasterRow>();

    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("tournament_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<TournamentMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("TournamentMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}
