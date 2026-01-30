using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// text_master テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class TextMasterRow
{
    public int text_id;
    public string symbol;
    public string ja;
    public string en;
    public string category;
    public string characount;
    public string image_name;
    public string string_table_id;
    public int? sort;
    public string create_at;
}

/// <summary>
/// text_master マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class TextMasterModel
{
    public static List<TextMasterRow> Rows { get; private set; } = new List<TextMasterRow>();

    /// <summary>
    /// マスターデータから text_master をロード
    /// </summary>
    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null) return;
        if (!masterData.masters.TryGetValue("text_master", out var raw)) return;

        try
        {
            JToken token;
            if (raw is JToken t) { token = t; }
            else { token = JToken.Parse(raw.ToString()); }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<TextMasterRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("TextMasterModel.LoadFromMasterData parse error: " + e.Message);
        }
    }

    /// <summary>
    /// シンボルからテキストを取得（多言語対応）
    /// </summary>
    /// <param name="symbol">シンボル（例: "text_log_body_123"）</param>
    /// <param name="lang">言語（0=日本語, 1=英語など）</param>
    /// <returns>テキスト。見つからない場合は空文字列</returns>
    public static string GetText(string symbol, int lang = 0)
    {
        var row = Rows.Find(r => r.symbol == symbol);
        if (row == null)
            return "";

        // 言語に応じて返す（lang=0なら日本語、lang=1なら英語）
        if (lang == 0)
            return row.ja ?? "";
        else
            return row.en ?? row.ja ?? "";
    }

    /// <summary>
    /// name_idからキャラクター名を取得
    /// </summary>
    /// <param name="nameId">name_id（character_info.name_id）</param>
    /// <param name="lang">言語（0=日本語, 1=英語など）</param>
    /// <returns>キャラクター名。見つからない場合は空文字列</returns>
    public static string GetCharacterName(int nameId, int lang = 0)
    {
        string symbol = "text_log_body_" + nameId;
        return GetText(symbol, lang);
    }
}
