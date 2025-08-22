using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosePhaseBehaviour : MonoBehaviour
{
    public string mode { get; set; }
    bool retry { get; set; } = false;
    bool btn_flg { get; set; } = false;


    public static ClosePhaseBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static ClosePhaseBehaviour instance;


    // 
    // 終了フェーズの制御を行う。
    private void Start()
    {
        instance = this;
    }

    //
    // 終了フェーズを開始する。
    // 変数modeにはバトル結果が格納されている。
    public void ClosePhaseStart()
    {
        Dictionary<string, string> launcher = new Dictionary<string, string>();
        BattleBehaviour Battle = BattleBehaviour.Instance;

        // 終了処理後のPOSTで送信する内容をセット。
        launcher["result"] = mode;
        launcher["code"] = Battle.battle.validationCode;
        launcher["time"] = InfoBehaviour.Instance.turnValue.ToString();

        launcher["hpP"] = HpGaugeBehaviour.Instance.HpInfo["P"].value.ToString();
        launcher["hpE"] = HpGaugeBehaviour.Instance.HpInfo["E"].value.ToString();

        launcher["tactP0"] = Battle.statTact["P0"].ToString();
        launcher["tactP1"] = Battle.statTact["P1"].ToString();
        launcher["tactP2"] = Battle.statTact["P2"].ToString();
        launcher["tactP3"] = Battle.statTact["P3"].ToString();
        launcher["tactE0"] = Battle.statTact["E0"].ToString();
        launcher["tactE1"] = Battle.statTact["E1"].ToString();
        launcher["tactE2"] = Battle.statTact["E2"].ToString();
        launcher["tactE3"] = Battle.statTact["E3"].ToString();
        launcher["nattCntP"] = Battle.stat["N"].attCnt["P"].ToString();
        launcher["nattCntE"] = Battle.stat["N"].attCnt["E"].ToString();
        launcher["nhitCntP"] = Battle.stat["N"].hitCnt["P"].ToString();
        launcher["nhitCntE"] = Battle.stat["N"].hitCnt["E"].ToString();
        launcher["ndamP"] = Battle.stat["N"].dam["P"].ToString();
        launcher["ndamE"] = Battle.stat["N"].dam["E"].ToString();
        launcher["revCntP"] = Battle.statRevCnt["P"].ToString();
        launcher["revCntE"] = Battle.statRevCnt["E"].ToString();
        launcher["rattCntP"] = Battle.stat["R"].attCnt["P"].ToString();
        launcher["rattCntE"] = Battle.stat["R"].attCnt["E"].ToString();
        launcher["rhitCntP"] = Battle.stat["R"].hitCnt["P"].ToString();
        launcher["rhitCntE"] = Battle.stat["R"].hitCnt["E"].ToString();
        launcher["rdamP"] = Battle.stat["R"].dam["P"].ToString();
        launcher["rdamE"] = Battle.stat["R"].dam["E"].ToString();
        launcher["odamP"] = Battle.stat["O"].dam["P"].ToString();
        launcher["odamE"] = Battle.stat["O"].dam["E"].ToString();

        string text = "";

        // ナビゲータのセリフをセット。
        if (mode == "lose" && Battle.battle.continueError > 0)
        {
            if (Battle.battle.continueError == 2)
            {
                text = retry ? Utility.getText("BATTLE_NAV_FIN_TIMEOUT") : Utility.getText("TEXT_NAV_CONTINUE_COUNT_LIMIT").Replace("{0}", (Battle.battle.CONTINUE_COUNT_LIMIT + 2).ToString());
            }
            else if (Battle.battle.continueError == 3)
            {
                text = retry ? Utility.getText("BATTLE_NAV_FIN_TIMEOUT") : Utility.getText("TEXT_NAV_CONTINUE_NOT_USE");
            }
            else
            {
                text = retry ? Utility.getText("BATTLE_NAV_FIN_TIMEOUT") : BattleBehaviour.Instance.getNaviSpeak(mode);
            }
            NaviBehaviour.Instance.setText(text);
        }
        else
        {
            text = retry ? Utility.getText("BATTLE_NAV_FIN_TIMEOUT") : BattleBehaviour.Instance.getNaviSpeak(mode);
        }

        NaviBehaviour.Instance.setText(text);
        NaviBehaviour.Instance.setSide("P");
        NaviBehaviour.Instance.Show(1);

        // 解説のメッセージを変更＆点滅を停止。
        PreterBehaviour.Instance.Visible(true);
        PreterBehaviour.Instance.setText(Utility.getText("TEXT_MESSAGE_BATTLE_WAIT"));
        PreterBehaviour.Instance.setPos("center");
        PreterBehaviour.Instance.PlayAnim("norm");

        // POSTで画面遷移。
        // サーバへの確認リクエストを出したあと、そのレスポンスがあるまで待機する。
        APIConnectManager.Instance.BattleResult(Battle.battle.battle_id, null, 0, launcher, onSend);

    }


    public jsonBattleResult launcher { get; set; }

    //
    // 終了のボタンが押された後gotoするラベル。
    void onSend(string json)
    {
        Debug.Log("ClosePhaseBehaviour onSend run..");

        launcher = this.JsonToClass(json);

        // レスポンスがあったなら...
        if (launcher.result != "")
        {

            //fscommand2("JavaScript", "showmsg", "launcher.response=" add launcher.response);

            string text = "";
            string navi_text = "";

            // エラーコードが返っている場合はその表示を行う。
            switch (launcher.result)
            {

                // エラーないならresultへ
                case "ok":

                    //PlayerPrefをクリア
                    BattleBehaviour.Instance.ClearStat();

                    this.gotoResult();
                    break;

                // エラーがある場合はその内容を表示。
                case "-1":
                case "0":
                case "1":
                case "2":
                    text = Utility.getText("TEXT_ERROR");
                    navi_text = Utility.getText("API_ERROR_BattleResult_" + launcher.result);
                    break;
                default:
                    text = Utility.getText("TEXT_ERROR");
                    navi_text = Utility.getText("BATTLE_NAV_ERROR");
                    break;
            }

            if (text != "")
                PreterBehaviour.Instance.setText(text);
            if (navi_text != "")
                NaviBehaviour.Instance.setText(text);
        }
    }


    public void tutorialEnd()
    {
        NaviBehaviour.Instance.setText(BattleBehaviour.Instance.battle.navSerif_end);
        NaviBehaviour.Instance.setSide("P");
        NaviBehaviour.Instance.Show(1);

        // 解説に決定ボタンを押すように促すメッセージをセット＆点滅。
        PreterBehaviour.Instance.Visible(true);
        PreterBehaviour.Instance.setText(Utility.getText("BATTLE_STR_PUSH_BUTTON_MESSAGE"));
        PreterBehaviour.Instance.setPos("center");
        PreterBehaviour.Instance.PlayAnim("blink");

        BattleBehaviour.Instance.nextSeq = "tutorialend";
        BattleBehaviour.Instance.TouchPanel.SetActive(true);
    }

    void gotoResult()
    {
        // 解説に決定ボタンを押すように促すメッセージをセット＆点滅。
        PreterBehaviour.Instance.Visible(true);
        PreterBehaviour.Instance.setText(Utility.getText("BATTLE_STR_PUSH_BUTTON_MESSAGE"));
        PreterBehaviour.Instance.setPos("center");
        PreterBehaviour.Instance.PlayAnim("blink");

        BattleBehaviour.Instance.nextSeq = "result";
        BattleBehaviour.Instance.TouchPanel.SetActive(true);

    }

    /*
     * jsonで受け取った情報をjsonBattleResultクラスに格納する
     */
    public jsonBattleResult JsonToClass(string json)
    {
        //API結果受け取り
        jsonBattleResult BattleResult = JsonUtility.FromJson<jsonBattleResult>(json);

        Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        foreach (KeyValuePair<string, object> keyvalue in jsonDict)
        {
            if (keyvalue.Key == "current")
            {
                Dictionary<string, object> jsonDictCurrent = JsonConvert.DeserializeObject<Dictionary<string, object>>(keyvalue.Value.ToString());
                foreach (KeyValuePair<string, object> keyvaluecurrent in jsonDictCurrent)
                {
                    if (keyvaluecurrent.Key == "exp")
                    {
                        if (keyvaluecurrent.Value != null)
                            BattleResult.current.exp = JsonConvert.DeserializeObject<Dictionary<string, int>>(keyvaluecurrent.Value.ToString());
                    }
                }
            }
            else if (keyvalue.Key == "battleresult")
            {
                Dictionary<string, object> jsonDictBattleResult = JsonConvert.DeserializeObject<Dictionary<string, object>>(keyvalue.Value.ToString());
                foreach (KeyValuePair<string, object> keyvaluebattleresult in jsonDictBattleResult)
                {
                    if (keyvaluebattleresult.Key == "equip")
                    {
                        parseResultEquip(ref BattleResult.battleresult.equip, keyvaluebattleresult.Value);
                    }
                }
            }
            else if (keyvalue.Key == "ready")
            {
                Dictionary<string, object> jsonDictReady = JsonConvert.DeserializeObject<Dictionary<string, object>>(keyvalue.Value.ToString());
                foreach (KeyValuePair<string, object> keyvalueready in jsonDictReady)
                {
                    if (keyvalueready.Key == "equip")
                    {
                        parseEqp(ref BattleResult.ready.equip, keyvalueready.Value);
                    }
                }
            }
            else if (keyvalue.Key == "chara")
            {
                Dictionary<string, object> jsonDictChara = JsonConvert.DeserializeObject<Dictionary<string, object>>(keyvalue.Value.ToString());
                foreach (KeyValuePair<string, object> keyvaluechara in jsonDictChara)
                {
                    if (keyvaluechara.Key == "equip")
                    {
                        parseEqp(ref BattleResult.chara.equip, keyvaluechara.Value);
                    }
                }
            }
            else if (keyvalue.Key == "capture")
            {
                if (keyvalue.Value != null)
                {
                    Dictionary<string, object> jsonDictCapture = JsonConvert.DeserializeObject<Dictionary<string, object>>(keyvalue.Value.ToString());
                    foreach (KeyValuePair<string, object> keyvaluecapture in jsonDictCapture)
                    {
                        if (keyvaluecapture.Key == "equip")
                        {
                            parseEqp(ref BattleResult.capture.equip, keyvaluecapture.Value);
                        }
                    }
                }
            }
            else if (keyvalue.Key == "battle")
            {
                Dictionary<string, object> jsonDictBattle = JsonConvert.DeserializeObject<Dictionary<string, object>>(keyvalue.Value.ToString());
                foreach (KeyValuePair<string, object> keyvaluebattle in jsonDictBattle)
                {
                    if (keyvaluebattle.Key == "bias_ready")
                    {
                        Dictionary<string, object> jsonDictBattleBiasReady = JsonConvert.DeserializeObject<Dictionary<string, object>>(keyvaluebattle.Value.ToString());
                        foreach (KeyValuePair<string, object> keyvaluebattlebiasready in jsonDictBattleBiasReady)
                        {
                            if (keyvaluebattlebiasready.Key == "equip")
                            {
                                parseEqp(ref BattleResult.battle.bias_ready.equip, keyvaluebattlebiasready.Value);
                            }
                        }
                    }
                    else if (keyvaluebattle.Key == "rival_ready")
                    {
                        Dictionary<string, object> jsonDictBattleRivalReady = JsonConvert.DeserializeObject<Dictionary<string, object>>(keyvaluebattle.Value.ToString());
                        foreach (KeyValuePair<string, object> keyvaluebattlerivalready in jsonDictBattleRivalReady)
                        {
                            if (keyvaluebattlerivalready.Key == "equip")
                            {
                                parseEqp(ref BattleResult.battle.rival_ready.equip, keyvaluebattlerivalready.Value);
                            }
                        }
                    }
                    else if (keyvaluebattle.Key == "bias_result")
                    {
                        Dictionary<string, object> jsonDictBattleBiasResult = JsonConvert.DeserializeObject<Dictionary<string, object>>(keyvaluebattle.Value.ToString());
                        foreach (KeyValuePair<string, object> keyvaluebattlebiasresult in jsonDictBattleBiasResult)
                        {
                            if (keyvaluebattlebiasresult.Key == "equip")
                            {
                                parseResultEquip(ref BattleResult.battle.bias_result.equip, keyvaluebattlebiasresult.Value);
                            }
                        }
                    }
                    else if (keyvaluebattle.Key == "rival_result")
                    {
                        Dictionary<string, object> jsonDictBattleRivalResult = JsonConvert.DeserializeObject<Dictionary<string, object>>(keyvaluebattle.Value.ToString());
                        foreach (KeyValuePair<string, object> keyvaluebattlerivalresult in jsonDictBattleRivalResult)
                        {
                            if (keyvaluebattlerivalresult.Key == "equip")
                            {
                                parseResultEquip(ref BattleResult.battle.rival_result.equip, keyvaluebattlerivalresult.Value);
                            }
                        }
                    }
                }
            }
        }

        return BattleResult;
    }

    void parseResultEquip(ref jsonResultEquip resultequip, object value)
    {
        resultequip = new jsonResultEquip();

        if (value != null)
        {
            Dictionary<string, object> jsonDictEquip = JsonConvert.DeserializeObject<Dictionary<string, object>>(value.ToString());
            foreach (KeyValuePair<string, object> keyvalueeqp in jsonDictEquip)
            {
                if (keyvalueeqp.Key == "before" && keyvalueeqp.Value != null)
                    parseEqp(ref resultequip.before, keyvalueeqp.Value);
                else if (keyvalueeqp.Key == "after" && keyvalueeqp.Value != null)
                    parseEqp(ref resultequip.after, keyvalueeqp.Value);
            }
        }
    }

    void parseEqp(ref Dictionary<int, jsonEquip> equip, object value)
    {
        equip = new Dictionary<int, jsonEquip>();

        try
        {
            Dictionary<int, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<int, object>>(value.ToString());
            for (int i = 1; i <= 4; i++)
            {
                if (jsonDict.ContainsKey(i))
                {
                    jsonEquip jsoneqplist = new jsonEquip();

                    jsoneqplist = JsonUtility.FromJson<jsonEquip>(jsonDict[i].ToString());

                    equip.Add(i, jsoneqplist);
                }
                else
                {
                    equip.Add(i, new jsonEquip());
                }

            }
            /*
            foreach (KeyValuePair<string, object> keyvalue in jsonDict)
            {
                jsonEquip jsoneqplist = new jsonEquip();

                jsoneqplist = JsonUtility.FromJson<jsonEquip>(keyvalue.Value.ToString());

                equip.Add(int.Parse(keyvalue.Key), jsoneqplist);
            }
            */
        }
        catch (Exception e)
        {
            for (int i = 1; i <= 4; i++)
            {

                equip.Add(i, new jsonEquip());

            }
        }
    }

}
