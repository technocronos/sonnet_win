using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// character_info テーブルの1行を表すモデルクラス（スキーマ準拠）
/// </summary>
[Serializable]
public class CharacterInfoRow
{
    // スキーマ:
    //  character_id   int(11)       NOT NULL AUTO_INCREMENT
    //  user_id        int(11)       NOT NULL
    //  entry          char(3)       NOT NULL
    //  race           char(3)       NOT NULL
    //  name_id        int(11)       NOT NULL
    //  graphic_id     int(11)       NOT NULL
    //  grade_id       smallint(6)   NOT NULL
    //  grade_pt       smallint(6)   NOT NULL DEFAULT 0
    //  sally_sphere   int(11)                DEFAULT NULL
    //  exp            int(11)       NOT NULL DEFAULT 0
    //  param_seed     smallint(6)   NOT NULL DEFAULT 0
    //  hp             decimal(9,3)  NOT NULL
    //  hp_max         decimal(9,3)  NOT NULL
    //  attack1        smallint(6)   NOT NULL
    //  attack2        smallint(6)   NOT NULL
    //  attack3        smallint(6)   NOT NULL
    //  defence1       smallint(6)   NOT NULL
    //  defence2       smallint(6)   NOT NULL
    //  defence3       smallint(6)   NOT NULL
    //  defenceX       smallint(6)   NOT NULL DEFAULT 0
    //  speed          smallint(6)   NOT NULL
    //  death_count    smallint(6)   NOT NULL DEFAULT 0
    //  last_affected  datetime      NOT NULL
    //  create_at      timestamp     NOT NULL DEFAULT current_timestamp()

    public int character_id;
    public int user_id;
    public string entry;
    public string race;
    public int name_id;
    public int graphic_id;
    public int grade_id;
    public int grade_pt;
    public int? sally_sphere;
    public int exp;
    public int param_seed;

    public float hp;
    public float hp_max;
    public int attack1;
    public int attack2;
    public int attack3;
    public int defence1;
    public int defence2;
    public int defence3;
    public int defenceX;
    public int speed;
    public int death_count;
    public string last_affected;
    public string create_at;
}

/// <summary>
/// character_info マスタを Unity 側で保持するための静的モデル
/// </summary>
public static class CharacterInfoModel
{
    /// <summary>character_info 全行（MasterData API では character_id &lt; 0 のモンスターのみ）</summary>
    public static List<CharacterInfoRow> Rows { get; private set; } = new List<CharacterInfoRow>();

    /// <summary>
    /// MasterData API の json を受け取り、character_info 部分だけをパースして保存する
    /// </summary>
    public static void LoadFromMasterData(jsonMasterData masterData)
    {
        Rows.Clear();

        if (masterData == null || masterData.masters == null)
        {
            return;
        }

        if (!masterData.masters.TryGetValue("character_info", out var raw))
        {
            // character_info が含まれていない
            return;
        }

        try
        {
            // masters の value は object 型なので、一度 JToken 経由で配列として解釈する
            JToken token;

            if (raw is JToken t)
            {
                token = t;
            }
            else
            {
                // 文字列 or 匿名オブジェクトとして入っている場合もあるので文字列化してパース
                token = JToken.Parse(raw.ToString());
            }

            if (token.Type == JTokenType.Array)
            {
                Rows = token.ToObject<List<CharacterInfoRow>>();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("CharacterInfoModel.LoadFromMasterData parse error: " + e.Message);
        }
    }
}

