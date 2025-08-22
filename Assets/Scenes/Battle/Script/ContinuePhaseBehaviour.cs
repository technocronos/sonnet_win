using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuePhaseBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    public static ContinuePhaseBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static ContinuePhaseBehaviour instance;

    private void Start()
    {
        instance = this;
    }

    public void ContinuePhaseStart()
    {
        Dictionary<string, string> transmitter = new Dictionary<string, string>();

        // サーバに確認リクエストを投げる。
        transmitter["turn"] = InfoBehaviour.Instance.TextTurn.text;
        transmitter["hpP"] = HpGaugeBehaviour.Instance.HpInfo["P"].value.ToString();
        transmitter["hpE"] = HpGaugeBehaviour.Instance.HpInfo["E"].value.ToString();
        transmitter["starP"] = MainPhaseBehaviour.Instance.starStoreP.GetComponent<StarStoreBehaviour>().value.ToString();
        transmitter["starE"] = MainPhaseBehaviour.Instance.starStoreE.GetComponent<StarStoreBehaviour>().value.ToString();
        transmitter["ndamP"] = BattleBehaviour.Instance.stat["N"].dam["P"].ToString(); // プレイヤーが通常攻撃によって与えたダメージ
        transmitter["rdamP"] = BattleBehaviour.Instance.stat["R"].dam["P"].ToString(); // プレイヤーがリベンジ攻撃によって与えたダメージ
        transmitter["odamP"] = BattleBehaviour.Instance.stat["O"].dam["P"].ToString(); // プレイヤーがその他攻撃によって与えたダメージ

        // 解説ウィンドウのメッセージ変更＆点滅を停止。
        PreterBehaviour.Instance.setText(Utility.getText("TEXT_MESSAGE_BATTLE_DATA_LOADING"));
        PreterBehaviour.Instance.PlayAnim("Norm");
        PreterBehaviour.Instance.Visible(true);

        int BattleId = BattleBehaviour.Instance.Param.battleId;
        string validationCode = BattleBehaviour.Instance.battle.validationCode;

        //APIをたたく
        APIConnectManager.Instance.BattleContinue(BattleId, validationCode, transmitter, onResponse);

    }

    void onResponse(string json)
    {

        jsonBattleContinue response = JsonUtility.FromJson<jsonBattleContinue>(json);

        //確認メッセージボックスは非表示
        ContinueConfirmBehaviour.Instance.gameObject.SetActive(false);

        // エラーコードが返っている場合はその表示を行う。
        switch (response.result)
        {

            // エラーないならrecoverをgotoして復旧処理
            case "ok":
                Recover();
                break;

            // エラーがある場合はその内容を表示。
            case "error_no_item":
                string text = Utility.getText("API_ERROR_BattleContinue_error_no_item");
                PreterBehaviour.Instance.setText(text);
                PreterBehaviour.Instance.PlayAnim("Norm");
                PreterBehaviour.Instance.Visible(true);

                NaviBehaviour.Instance.setText(Utility.getText("BATTLE_NAV_ALD_START"));
                NaviBehaviour.Instance.setSide("P");
                NaviBehaviour.Instance.Show(1);
                break;
            default:
                PreterBehaviour.Instance.setText("エラー: " + response.result);
                PreterBehaviour.Instance.PlayAnim("Norm");
                PreterBehaviour.Instance.Visible(true);

                NaviBehaviour.Instance.setText(Utility.getText("BATTLE_NAV_ERROR"));
                NaviBehaviour.Instance.setSide("P");
                NaviBehaviour.Instance.Show(1);
                break;
        }
    }

    //ゲーム内コンティニュー時の処理
    void Recover()
    {
        jsonBattle battle = BattleBehaviour.Instance.battle;
        // 解説を非表示に。
        PreterBehaviour.Instance.Visible(false);

        //PlayerPrefをクリア
        BattleBehaviour.Instance.ClearStat();

        //表示を更新、初期化
        InfoBehaviour.Instance.turnValue = 0;
        InfoBehaviour.Instance.TurnRefresh();
        HpGaugeBehaviour.Instance.HpInfo["P"].value = BattleBehaviour.Instance.battle.hpMaxP;

        // 統計値を初期化
        //ゲーム内コンティニューの場合はプレイヤーが与えたダメージ以外は
        //一旦リセットしておく必要がある。
        battle.statTactP0 = 0;     // プレイヤーが「ユニゾン」した回数
        battle.statTactP1 = 0;     // プレイヤーが「強攻」を選択した回数
        battle.statTactP2 = 0;     // プレイヤーが「慎重」を選択した回数
        battle.statTactP3 = 0;     // プレイヤーが「吸収」を選択した回数
        battle.statTactE0 = 0;     // 同、相手側
        battle.statTactE1 = 0;     // 
        battle.statTactE2 = 0;     // 
        battle.statTactE3 = 0;     // 
        battle.statNattCntP = 0;   // プレイヤーが通常攻撃を繰り出した回数
        battle.statNattCntE = 0;   // 同、相手側
        battle.statNhitCntP = 0;   // プレイヤーが通常攻撃を当てた回数
        battle.statNhitCntE = 0;   // 同、相手側
        battle.statNdamP = BattleBehaviour.Instance.stat["N"].dam["P"];      // プレイヤーが通常攻撃によって与えたダメージ
        battle.statNdamE = 0;      // 同、相手側
        battle.statRevCntP = 0;    // プレイヤーがリベンジを発動した回数
        battle.statRevCntE = 0;    // 同、相手側
        battle.statRattCntP = 0;   // プレイヤーがリベンジ攻撃を繰り出した回数
        battle.statRattCntE = 0;   // 同、相手側
        battle.statRhitCntP = 0;   // プレイヤーがリベンジ攻撃を当てた回数
        battle.statRhitCntE = 0;   // 同、相手側
        battle.statRdamP = BattleBehaviour.Instance.stat["R"].dam["P"];      // プレイヤーがリベンジ攻撃によって与えたダメージ
        battle.statRdamE = 0;      // 同、相手側
        battle.statOdamP = BattleBehaviour.Instance.stat["O"].dam["P"];      // プレイヤーがその他攻撃によって与えたダメージ
        battle.statOdamE = 0;      // 同、相手側

        //コンティニューアイテム数を減らす。
        battle.continueItemCnt--;
        battle.continue_count++;

        //javascript側でrandomSeedを更新する。でないとコンティニューしても毎回敵が同じ動きをしてしまう。
        //サーバに格納されてるのとずれてしまうがまあいいや・・
        battle.randomSeed = Random.Range(1, 65535);

        //統計値を初期化する
        BattleBehaviour.Instance.initStat();

        if (battle.continue_count > battle.CONTINUE_COUNT_LIMIT)
            battle.continueError = 2;

        //スターも0に初期化する。
        MainPhaseBehaviour.Instance.starStoreP.GetComponent<StarStoreBehaviour>().starTypes = new Dictionary<int, int>();
        MainPhaseBehaviour.Instance.starStoreP.GetComponent<StarStoreBehaviour>().value = 0;
        MainPhaseBehaviour.Instance.starStoreP.GetComponent<StarStoreBehaviour>().setText(0);

        MainPhaseBehaviour.Instance.starStoreE.GetComponent<StarStoreBehaviour>().starTypes = new Dictionary<int, int>();
        MainPhaseBehaviour.Instance.starStoreE.GetComponent<StarStoreBehaviour>().value = 0;
        MainPhaseBehaviour.Instance.starStoreE.GetComponent<StarStoreBehaviour>().setText(0);

        //またBGM再スタート。再スタートなのでタップイベントを介さないでそのまま鳴る。
        AudioManager.Instance.PlayBGM(battle.bgm_sound, AudioManager.BGM_VOLUME_DEFULT);

        //ｱﾊﾞﾀｰ再登場
        StartCoroutine(MainPhaseBehaviour.Instance.Avatar["P"].PlayAnim("AvatarRebirth"));

        // ナビゲータのセリフをセット。
        NaviBehaviour.Instance.setText(Utility.getText("BATTLE_NAV_RECOVER"));
        NaviBehaviour.Instance.setSide("P");
        NaviBehaviour.Instance.Show(1);

        // 親のmainラベルをcall
        BattleBehaviour.Instance.Main();
    }

}
