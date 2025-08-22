using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleFuncs
{

    private static string cardSide { set; get; }
    private static int cutValue { set; get; }

    private static Dictionary<string, float> Types = new Dictionary<string, float>();

    public static int type { get; set; }

    // 
    // 変数 side で指定された側の攻撃カードの種類を決定する。
    // 変数 side は "P" か "E" で指定する。
    // 決定した種類は変数 type に格納する。
    public static void Card(string side)
    {
        // 前回呼び出し時と異なる側である場合は必要な値を取得する。
        if (cardSide != side)
        {

            cardSide = side;

            // 3属性の攻撃力をそれぞれ取り出すとともに、一番低い攻撃力を変数 cutValue に取得。    
            cutValue = 65535;
            for (int i = 1; i <= 3; i++)
            {
                string varName = "type" + i + "Val";
                Types[varName] = (float)(BattleBehaviour.Instance.att[i + side] * 1.0);

                if (cutValue > Types[varName]) cutValue = (int)Types[varName];
            }

            // [一番低い攻撃力 - カード出現率マージン]を足きりラインとする
            // カード出現率マージンは [4 * Lv*3%増]。
            cutValue -= 4 * (int)(1.0 + (BattleBehaviour.Instance.lv[side] / 100 * 3));

            // 足きりラインでカットして3属性の値を再計算。
            Types["type1Val"] -= cutValue;
            Types["type2Val"] -= cutValue;
            Types["type3Val"] -= cutValue;

            // 3属性の値合計の19%のラインを取得。
            double lowestLine = (Types["type1Val"] + Types["type2Val"] + Types["type3Val"]) * 0.19;

            // そのラインを下回っている値はラインまで引き上げる。
            if (Types["type1Val"] < lowestLine) Types["type1Val"] = (int)lowestLine;
            if (Types["type2Val"] < lowestLine) Types["type2Val"] = (int)lowestLine;
            if (Types["type3Val"] < lowestLine) Types["type3Val"] = (int)lowestLine;
        }

        // [その属性の値 / 3属性合計] をその属性カードの出現率とする。
        // 上のラインの処理で、(だいたいだけど)最大でも62%、最低でも19%になる。
        // 乱数を取得して、属性を決定、変数 type に格納する。
        float randValue = BattleBehaviour.Instance.randomEx.Value();

        float rand = randValue * (Types["type1Val"] + Types["type2Val"] + Types["type3Val"]);
        type = (rand < Types["type1Val"]) ? 1 : ((rand < Types["type1Val"] + Types["type2Val"]) ? 2 : 3);

        // デバック表示。
        //trace("type1" add side add ": " add type1Val);
        //trace("type2" add side add ": " add type2Val);
        //trace("type3" add side add ": " add type3Val);
        //trace("rand:" add rand add " type:" add type);

    }


    //
    // ダメージの計算をする。
    // 引数)
    //     att          攻撃力
    //     def          防御力
    //     level        攻撃側レベル
    //     attTact      攻撃側戦術。リベンジの場合は-1をセットする。
    //     defTact      防衛側戦術。リベンジの場合は-1をセットする。
    // 戻り値)
    //     damage       ダメージ値。
    public static double CalcDam(double att, double def, int level, int attTact, int defTact)
    {

        jsonBattle battle = BattleBehaviour.Instance.battle;

        // 数字スケールを調整。だいたい att30 vs def30 の差が 5 になるくらい
        att *= 0.70;
        def *= 0.55;

        // att-defでダメージを単純算出。
        double damage = att - def;
        //trace("pre-damage" add damage);

        // 想定通常ダメージを取得。
        //     攻撃Lv = 攻撃側Lv  OR  リベンジの場合は両者Lvの平均
        //     想定素パラ =   30 x (1 + (攻撃Lv - 1) x 0.03)
        //     想定武器パラ = 攻撃Lv * 7/8 * 0.8
        //     想定ダメ = (想定素パラ + 想定武器パラ) * (0.80 - 0.65)
        // という計算式。
        int attLv = (attTact == -1) ? (battle.LvP + battle.LvE) / 2 : level;
        double normNaked = 30 * (1 + (attLv - 1) * 0.03);
        double normWeapon = attLv * 7 / 8 * 0.8;
        double normDam = (normNaked + normWeapon) * 0.15;

        // ダメージの想定上限と想定下限を取得。
        double uppLimit = normDam * 1.8;
        double lowLimit = normDam * 0.3;

        // 想定限界を超えている場合は、超えている分を1/4する。
        if (uppLimit < damage) damage = uppLimit + (damage - uppLimit) / 4;
        if (lowLimit > damage) damage = lowLimit - (lowLimit - damage) / 4;

        // 強攻補正を取得。攻撃側が"強攻"か"ユニゾン"ならダメージの30%。
        // ただし1を切らないようにする。
        double attRev;
        double defRev;
        double absRev;
        if (attTact == 1 || attTact == 0)
        {
            attRev = damage * 0.30;
            if (attRev < 1.0) attRev = 1.0;
        }
        else
        {
            attRev = 0.0;
        }

        // 慎重補正を取得。防衛側が"戦略的慎重"か"ユニゾン"ならダメージの20%。
        // ただし1を切らないようにする。
        if ((defTact == 2 && attTact != 0) || defTact == 0)
        {
            defRev = damage * 0.20;
            if (defRev < 1.0) defRev = 1.0;
        }
        else
        {
            defRev = 0.0;
        }

        // 吸収補正を取得。攻撃側が"強行"以外、防衛側が"吸収"ならダメージの30%。
        // ただし1を切らないようにする。
        if (defTact == 3 && attTact != 1)
        {
            absRev = damage * 0.30;
            if (absRev < 1.0) absRev = 1.0;
        }
        else
        {
            absRev = 0.0;
        }

        // 補正を適用
        damage = damage + attRev - defRev + absRev;

        // ここでマイナスを補正
        if (damage < 0) damage = 0;
        //trace("rev-damage" add damage);

        // ランダムで±10%する。
        float randValue = BattleBehaviour.Instance.randomEx.Value();
        damage *= 1.0 + (randValue * 2 - 1.0) * 0.05;

        // リベンジの場合はダメージを15%減
        if (attTact == -1) damage *= 0.85;

        // 最後に四捨五入して整数にする。
        damage = (int)(Mathf.Round((float)damage));

        // ただし、ダメージ0の場合は1/3 の確率で 1 に補正。
        if (damage == 0)
        {
            randValue = BattleBehaviour.Instance.randomEx.Value();
            damage = (randValue < 0.33) ? 1 : 0;
        }

        // デバック用
        //trace("att: " add att);
        //trace("def: " add def);
        //trace("level: " add level);
        //trace("normDam: " add normDam add " (" add normNaked add " + " add normWeapon add ")");
        //trace("uppLimit: " add uppLimit add ", lowLimit: " add lowLimit);
        //trace("attTact: " add attTact add ", defTact: " add defTact);
        //trace("attRev: " add attRev add ", defRev: " add defRev add ", absRev: " add absRev);
        //trace("damage: " add damage);

        /*
        fscommand2("JavaScript", "showmsg", "att=" add att);
        fscommand2("JavaScript", "showmsg", "def=" add def);
        fscommand2("JavaScript", "showmsg", "level=" add level);
        fscommand2("JavaScript", "showmsg", "normDam=" add normDam add " (" add normNaked add " + " add normWeapon add ")");
        fscommand2("JavaScript", "showmsg", "uppLimit=" add uppLimit add ", lowLimit: " add lowLimit);
        fscommand2("JavaScript", "showmsg", "attTact=" add attTact add ", defTact: " add defTact);
        fscommand2("JavaScript", "showmsg", "attRev=" add attRev add ", defRev: " add defRev add ", absRev: " add absRev);
        fscommand2("JavaScript", "showmsg", "damage=" add damage);
        */

        return damage;


    }

}
