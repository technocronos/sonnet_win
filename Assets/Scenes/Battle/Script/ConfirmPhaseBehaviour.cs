using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmPhaseBehaviour : MonoBehaviour
{
    public jsonBattleConfirm transmitter { get; set; }

    public static ConfirmPhaseBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static ConfirmPhaseBehaviour instance;

    private void Start()
    {
        instance = this;
    }

    public void Init()
    {
        instance = this;
        // 解説に決定ボタンを押すように促すメッセージをセット＆点滅。
        PreterBehaviour.Instance.Visible(true);
        PreterBehaviour.Instance.setText(Utility.getText("TEXT_MESSAGE_BATTLE_TAP_START"));
        PreterBehaviour.Instance.PreterAnim.SetBool("PreterBlink", true);
    }

    // 
    // ユーザがなにかボタンを押したらcallされるラベル。
    public void Transmit()
    {
        // サーバに確認リクエストを投げる。
        Dictionary<string, string> transUrl = new Dictionary<string, string>();
        transUrl = Utility.ParseUrl(BattleBehaviour.Instance.battle.urlOnConfirm);

        //APIをたたく
        APIConnectManager.Instance.BattleConfirm(transUrl["action"], int.Parse(transUrl["battleId"]), transUrl["code"], onSend);

    }

    void onSend(string json)
    {
        //API結果受け取り
        transmitter = JsonUtility.FromJson<jsonBattleConfirm>(json);

        // 解説ウィンドウのメッセージ変更＆点滅を停止。
        BattleBehaviour.Instance.PreterText.text = Utility.getText("TEXT_MESSAGE_BATTLE_DATA_LOADING");
        BattleBehaviour.Instance.PreterAnim.SetBool("PreterBlink", false);

        //
        // サーバへの確認リクエストを出したあと、そのレスポンスがあるまで待機する。

        // レスポンスがあったなら...
        if (transmitter.result != "")
        {
            // エラーコードが返っている場合はその表示を行う。
            switch (transmitter.result)
            {
                // エラーないなら何もしない
                case "ok":
                    break;

                // エラーがある場合はその内容を表示。
                case "error":
                    if (transmitter.err_code == "already_start")
                    {
                        BattleBehaviour.Instance.PreterText.text = Utility.getText("API_ERROR_BattleConfirm_" + transmitter.err_code);
                    }
                    else if (transmitter.err_code == "notfoune")
                    {
                        BattleBehaviour.Instance.PreterText.text = Utility.getText("API_ERROR_BattleConfirm_" + transmitter.err_code);
                    }
                    else if (transmitter.err_code == "inviled_code")
                    {
                        BattleBehaviour.Instance.PreterText.text = Utility.getText("API_ERROR_BattleConfirm_" + transmitter.err_code);
                    }
                    else if (transmitter.err_code == "not_my_battle")
                    {
                        BattleBehaviour.Instance.PreterText.text = Utility.getText("API_ERROR_BattleConfirm_" + transmitter.err_code);
                    }
                    else if (transmitter.err_code == "consume_pt")
                    {
                        BattleBehaviour.Instance.PreterText.text = Utility.getText("API_ERROR_BattleConfirm_" + transmitter.err_code);
                    }
                    else
                    {
                        BattleBehaviour.Instance.PreterText.text = Utility.getText("API_ERROR_OTHER").Replace("{0}", transmitter.err_code);
                    }

                    NaviBehaviour.Instance.Visible(true);
                    NaviBehaviour.Instance.setText(Utility.getText("BATTLE_NAV_ERROR"));

                    break;
                case "retry":
                    //リレイザーで戻ってきたような場合を想定

                    //javascript側でrandomSeedを更新する。でないとコンティニューしても毎回敵が同じ動きをしてしまう。
                    //サーバに格納されてるのとずれてしまうがまあいいや・・
                    BattleBehaviour.Instance.battle.randomSeed = Random.Range(1, 65535);

                    transmitter.result = "ok";

                    break;
            }
        }

        BattleBehaviour.Instance.ConfEnd();

    }
}
