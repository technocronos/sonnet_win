using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Scenes.Common.Scripts;
using MyScene;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using Steamworks;
using Newtonsoft.Json;

public class APIConnectManager : EventDispatcher
{

    public delegate void EventCallback(string json);

    private EventCallback _eventCallback = null;
    public GameObject connectObj;
    private int _retryCount = 0;
    private static APIConnectManager mInstance;

    public jsonLogin login;
    public jsonRegist regist;
    public jsonConstants constants;
    public jsonMasterData masterData;

    //コルーチンを途中で完全に停止させるため変数にいれる
    private IEnumerator _routine = null;

    public static APIConnectManager Instance
    {
        get
        {
            if (mInstance == null)
            {
                GameObject go = new GameObject("APIConnectManager");
                mInstance = go.AddComponent<APIConnectManager>();
                DontDestroyOnLoad(go);
            }

            return mInstance;
        }
    }

    private void Awake()
    {
        if (mInstance == null)
        {
            mInstance = this;
        }
    }

    public void SteamLogin(EventCallback eventCallback)
    {
        Debug.Log("Steamログインを開始します");

        if (SteamManager.Initialized)
        {
            string playerName = SteamFriends.GetPersonaName();
            CSteamID steamId = SteamUser.GetSteamID();

            SaveLoadManager.Instance.UserID = steamId.ToString();

            //コールバック
            eventCallback?.Invoke(SaveLoadManager.Instance.UserID);

        }
        else
        {
            //ログイン失敗
            GameObject MessageCanvas = Main.Instance.MessageView;

            var strtbl = LocalizationSettings.StringDatabase.GetTable("StringTable");

            MessageCanvas.SetActive(true);
            MessageCanvas.GetComponent<MessageBehaviour>().Open(string.Format(strtbl.GetEntry("error_firebase_login").Value, Settings.SUPPORT_MAIL_ADDRESS), false);

        }
    }

    public void Login(EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        //なりすまし
        //string userid = "EgNQxZo5wUW82U9xG44uAHUxmYI3";
        string userid = SaveLoadManager.Instance.UserID;

        string param = "?module=Api&action=Login&opensocial_owner_id=" + userid + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        Connect(param);
        //StartCoroutine(_routine);
    }

    public void Regist(string name, string inviterId, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "index.php?module=Api&action=Regist&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        WWWForm form = new WWWForm();
        form.AddField("name", name);

        //招待の場合
        if (!String.IsNullOrEmpty(inviterId))
        {
            form.AddField("inviterId", inviterId);

#if UNITY_ANDROID
            if (!String.IsNullOrEmpty(SystemInfo.deviceUniqueIdentifier))
                form.AddField("deviceId", SystemInfo.deviceUniqueIdentifier);
#endif
        }

        Connect(param, form);
    }

    public void Inherit(string inherit_code, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=Inherit&inherit_code=" + inherit_code + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        Connect(param);
        //StartCoroutine(_routine);
    }

    public void Home(EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=HomeSummary&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        Connect(param);
        //StartCoroutine(_routine);
    }

    public void Info(int page, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "index.php?module=Api&action=Notice&page=" + page + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        Connect(param);
        //StartCoroutine(_routine);
    }

    public void tutorial(string Param, EventCallback eventCallback)
    {
        if (Param != string.Empty) Param = "&" + Param;

        this._eventCallback = eventCallback;
        string param = "?module=Api&action=Tutorial" + Param + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        Connect(param);
        //StartCoroutine(_routine);
    }

    public void terminable(int questId, int sphereId, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=Terminable&questId=" + questId + "&sphereId=" + sphereId + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        Connect(param);
        //StartCoroutine(_routine);
    }

    public void StartDushCampain(EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=StartDushCampain" + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        Connect(param);
        //StartCoroutine(_routine);
    }

    public void QuestDrama(int questId, int placeId, bool end, string code, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=QuestDrama&questId=" + questId + "&placeId=" + placeId + "&code=" + code + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        if (end)
        {
            param += "&end=1";
        }
        Connect(param);
        //StartCoroutine(_routine);
    }

    public void FieldDrama(int sphereId, int end, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=FieldDrama&sphereId=" + sphereId + "&end=" + end + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }

    public void showdrama(int DramaId, string endTo, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=ShowDrama&dramaId=" + DramaId + "&endTo=" + endTo + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        Connect(param);
        //StartCoroutine(_routine);
    }

    public void QuestList(EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=QuestList&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        Connect(param);
        //StartCoroutine(_routine);
    }

    public void Ready(int questId, int placeId, int consume_pt, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=Ready&questId=" + questId + "&placeId=" + placeId + "&consume_pt=" + consume_pt + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        Connect(param);
        //StartCoroutine(_routine);
    }

    public void ReadyEnd(int questId, int placeId, int consume_pt, Dictionary<string, int> slot, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=Ready&questId=" + questId + "&placeId=" + placeId + "&consume_pt=" + consume_pt + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        WWWForm form = new WWWForm();
        foreach (KeyValuePair<string, int> item in slot)
        {
            form.AddField(item.Key, item.Value);
        }
        Connect(param, form);
        //StartCoroutine(_routine);
    }

    public void Sphere(int sphereId, string reopen, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=Sphere&id=" + sphereId + "&reopen=" + reopen + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        Connect(param);
        //StartCoroutine(_routine);
    }

    public void SphereCommand(int sphereId, string code, int rev, Dictionary<string, string> _VarVal, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=SphereCommand&id=" + sphereId + "&code=" + code + "&rev=" + rev + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        foreach (KeyValuePair<string, string> VarVal in _VarVal)
        {
            param += "&" + VarVal.Key + "=" + VarVal.Value;
        }

        Connect(param);
        //StartCoroutine(_routine);
    }


    /// <summary>
    /// スフィアのアイテムリストを取得する
    /// </summary>
    /// <param name="sphereId">スフィアID</param>
    /// <param name="code">validation_code</param>
    /// <param name="eventCallback"></param>
    public void SphereItemList(int sphereId, string code, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=SphereItemList&id=" + sphereId + "&code=" + code + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        Connect(param);
        //StartCoroutine(_routine);
    }


    public void FieldReopen(string giveup, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "index.php?module=Api&action=FieldReopen&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        if (giveup != null)
        {
            WWWForm form = new WWWForm();
            form.AddField("giveup", giveup);
            Connect(param, form);
        }
        else
        {
            Connect(param);
        }

        //StartCoroutine(_routine);
    }

    /// <summary>
    /// スフィアの終了情報を取得する
    /// </summary>
    /// <param name="sphereId">スフィアID</param>
    public void FieldEnd(int sphereId, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=FieldEnd&sphereId=" + sphereId + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        Connect(param);
        //StartCoroutine(_routine);
    }

    public void Battle(int battleId, string firstscene, int repaireId, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=Battle&battleId=" + battleId + "&firstscene=" + firstscene + "&repaireId=" + repaireId + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        Connect(param);
        //StartCoroutine(_routine);
    }

    public void TutorialBattle(string result, string from, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=TutorialBattle&from=" + from + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        if (result != null)
        {
            WWWForm form = new WWWForm();
            form.AddField("result", result);
            Connect(param, form);
        }
        else
        {
            Connect(param);
        }
        //StartCoroutine(_routine);
    }

    public void BattleConfirm(string action, int battleId, string code, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=" + action + "&battleId=" + battleId + "&code=" + code + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        Connect(param);
        //StartCoroutine(_routine);
    }

    public void BattleContinue(int battleId, string code, Dictionary<string, string> transmitter, EventCallback eventCallback)
    {
        //loadVariables(/:urlOnContinue  add "%26hpP%3D" add hpP add "%26hpE%3D" add hpE add "%26turn%3D" add time add "%26ndamP%3D" add ndamP add "%26rdamP%3D" add rdamP add "%26odamP%3D" add odamP, "");
        string query = "";
        foreach (KeyValuePair<string, string> keyvalue in transmitter)
        {
            query += "&" + keyvalue.Key + "=" + keyvalue.Value;
        }

        this._eventCallback = eventCallback;
        string param = "?module=Api&action=BattleContinue&battleId=" + battleId + "&code=" + code + query + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        Connect(param);
        //StartCoroutine(_routine);
    }

    public void BattleBuyItem(int battleId, string code, int item_id, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "index.php?module=Api&action=BattleBuyItem&battleId=" + battleId + "&code=" + code + "&item_id=" + item_id + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="battleId"></param>
    /// <param name="side">cha or def</param>
    /// <param name="launcher"></param>
    /// <param name="eventCallback"></param>
    public void BattleResult(int battleId, string side, int repaireId, Dictionary<string, string> launcher, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "index.php?module=Api&action=BattleResult&battleId=" + battleId + "&side=" + side + "&repaireId=" + repaireId + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        if (launcher != null)
        {
            WWWForm form = new WWWForm();
            foreach (KeyValuePair<string, string> item in launcher)
            {
                form.AddField(item.Key, item.Value);
            }
            Connect(param, form);
        }
        else
        {
            Connect(param);
        }
        //StartCoroutine(_routine);
    }

    public void Status(EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=Status&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        Connect(param);
        //StartCoroutine(_routine);
    }

    public void ParamUp(string CharaId, string add_att1, string add_att2, string add_att3, string add_def1, string add_def2, string add_def3, string add_spd, string add_hp, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=ParamUp&attack1=" + add_att1 + "&attack2=" + add_att2 + "&attack3=" + add_att3 + "&defence1=" + add_def1 + "&defence2=" + add_def2 + "&defence3=" + add_def3 + "&speed=" + add_spd + "&hp_max=" + add_hp + " &oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
        Connect(param);
        //StartCoroutine(_routine);
    }
    public void Suggest(string type, string targetId, string mode, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=Suggest&type=" + type + "&targetId=" + targetId + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        if (mode != null)
        {
            WWWForm form = new WWWForm();
            form.AddField("mode", mode);
            Connect(param, form);
        }
        else
        {
            Connect(param);
        }

        //StartCoroutine(_routine);
    }

    public void Shop(int buy, string cat, string currency, int num, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=Shop&buy=" + buy + "&cat=" + cat + "&currency=" + currency + "&num=" + num + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }

    public void UserItem(int user_item_id, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=UserItem&user_item_id=" + user_item_id + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }


    public void ShopList(string cat, string currency, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=ShopList&cat=" + cat + "&currency=" + currency + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }

    public void Gacha(EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=Gacha&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }

    public void GachaPlay(int gachaId, string go, int count, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=GachaPlay&go=" + go + "&gachaId=" + gachaId + "&count=" + count + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }

    public void GachaResult(string dataId, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;
        string param = "?module=Api&action=GachaResult&dataId=" + dataId + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }

    public void GachaLineup(int gachaId, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=GachaLineup" + "&gachaId=" + gachaId + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }

    public void EquipChange(int charaId, string func, int change, int synth, int mountId, int base_id, int source_id, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=EquipChange&charaId=" + charaId + "&func=" + func + "&change=" + change + "&synth=" + synth + "&mountId=" + mountId + "&base_id=" + base_id + "&source_id=" + source_id + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }

    public void SyncGetPrice(int base_id, int source_id, bool evol, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=EquipChange" + "&base_id=" + base_id + "&source_id=" + source_id + "&evol=" + evol + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }


    public void EquipSync(int charaId, int useritemId, int mountId, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=EquipChange&charaId=" + charaId + "&synth=" + useritemId + "&mountId=" + mountId + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }

    public void EquipEvol(int charaId, int useritemId, int mountId, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=EquipChange&charaId=" + charaId + "&evolution=" + useritemId + "&mountId=" + mountId + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }


    public void EquipList(EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=EquipList&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }

    public void GradeList(EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=GradeList&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }

    public void GradeUser(int gradeId, int page, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=GradeUser" + "&gradeId=" + gradeId + "&page=" + page + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }


    public void MemberList(int userId, int page, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=MemberList" + "&userId=" + userId + "&page=" + page + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }

    public void MemberSearch(EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=MemberSearch" + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }

    public void ApproachList(string side, int page, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=ApproachList&side=" + side + "&page=" + page + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }

    /// <summary>
    /// 申請に対するアクションをする
    /// </summary>
    /// <param name="approach_id"></param>
    /// <param name="act">accept, reject, cancel, clear</param>
    /// <param name="eventCallback"></param>
    public void ApproachAct(int approach_id, string act, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=ApproachList" + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        WWWForm form = new WWWForm();

        form.AddField("approach_id", approach_id.ToString());
        form.AddField(act, 1);

        Connect(param, form);

    }

    public void Approach(int companionId, string approach, string dissolve, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=Approach" + "&companionId=" + companionId + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        WWWForm form = new WWWForm();

        if (approach != null)
        {
            form.AddField("approach", approach);

            Connect(param, form);
        }
        else if (dissolve != null)
        {
            form.AddField("dissolve", dissolve);

            Connect(param, form);
        }
        else
        {
            Connect(param);
        }
    }

    public void HistoryList(int userId, string category, string type, int page, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=HistoryList" + "&userId=" + userId + "&category=" + category + "&type=" + type + "&page=" + page + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }


    public void BattleHistory(int charaId, int tourId, string side, int page, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=BattleHistory" + "&charaId=" + charaId + "&tourId=" + tourId + "&side=" + side + "&page=" + page + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }


    public void ItemUseFire(int uitemId, int targetId, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=ItemUseFire" + "&uitemId=" + uitemId + "&targetId=" + targetId + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }

    public void ItemExchange(int uitemId, int targetId, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=ItemExchange" + "&uitemId=" + uitemId + "&targetId=" + targetId + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }

    public void HelpList(string id, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=HelpList" + "&id=" + id + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }



    public void VcoinSend(float amount, string address, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=VcoinSend" + "&amount=" + amount + "&address=" + address + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }

    public void VcoinLog(EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=VcoinLog" + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }

    public void VcoinList(EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=VcoinList" + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }

    public void Discard(int uitemId, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=Discard" + "&uitemId=" + uitemId + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }

    public void RivalList(EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=RivalList" + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);

        //StartCoroutine(_routine);
    }


    public void BattleRanking(string type, int count, int page, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=BattleRanking" + "&type=" + type + "&count=" + count + "&page=" + page + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }

    public void HisPage(int userId, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=HisPage" + "&userId=" + userId + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }

    public void RivalConfirm(int rivalId, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=BattleConfirm" + "&rivalId=" + rivalId + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }

    public void RivalBattle(int rivalId, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=BattleConfirm" + "&rivalId=" + rivalId + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        WWWForm form = new WWWForm();
        form.AddField("doBattle", 1);

        Connect(param, form);
    }

    public void MonsterList(int category, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=MonsterList" + "&category=" + category + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }

    /// <summary>
    /// マスターデータを取得する
    /// </summary>
    public void MasterData(EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=MasterData" + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }

    public void RaidDungeon(EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=RaidDungeon" + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }

    public void RaidMonsterList(int date, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=RaidMonsterList" + "&date=" + date + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }

    public void RaidRanking(int raid_dungeon_id, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=RaidRanking" + "&raid_dungeon_id=" + raid_dungeon_id + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }

    public void Appsflyer(string platform_uid, Dictionary<string, object> conversionData, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=Appsflyer" + "&platform_uid=" + platform_uid + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        WWWForm form = new WWWForm();
        foreach (KeyValuePair<string, object> item in conversionData)
        {
            form.AddField(item.Key, (item.Value == null) ? "" : item.Value.ToString());
        }

        Connect(param, form);
    }

    public void Invite(int inviterId, int recipientId, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=Invite" + "&inviterId=" + inviterId + "&recipientId=" + recipientId + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }

    public void AddrRegist(string address, EventCallback eventCallback)
    {
        this._eventCallback = eventCallback;

        string param = "?module=Api&action=AddrRegist" + "&address=" + address + "&oauth=" + login.oauth + "&ver=" + Settings.ver + "&lang=" + PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        Connect(param);
    }

    byte[] bytes = new byte[2000];

    /// <summary>
    /// 通信を行う
    /// </summary>
    /// <param name="param">パラメータ</param>
    /// <param name="formData">ポストデータ</param>
    /// <returns></returns>
    private async void Connect(string param, WWWForm formData = null)
    {

        if (connectObj != null)
        {
            connectObj.SetActive(true);
        }

        string url = Settings.Domain + "://" + Settings.Host + "/" + param;

        Debug.Log(url);

        UnityWebRequest request;
        if (formData == null)
        {
            request = UnityWebRequest.Get(url);
        }
        else
        {
            request = UnityWebRequest.Post(url, formData);
        }


        try
        {
            //UnityWebRequestにバッファをセット
            request.downloadHandler = new DownloadHandlerBuffer();

#if UNITY_EDITOR
            if (SystemInfo.operatingSystem.Contains("iOS") || SystemInfo.operatingSystem.Contains("Mac OS"))
            {
                request.SetRequestHeader("user-agent", @"Mozilla/5.0 (iPhone; CPU iPhone OS 11_2_5 like Mac OS X) AppleWebKit/604.5.2 (KHTML, like Gecko) Version/11.0 Mobile/15D5046b Safari/604.1");
            }
            else
            {
                request.SetRequestHeader("user-agent", @"Android");
            }
#elif UNITY_IPHONE
        request.SetRequestHeader("user-agent", @"" + SystemInfo.operatingSystem + "/" + UnityEngine.iOS.Device.generation.ToString() + "/" + SystemInfo.deviceModel);
#elif UNITY_ANDROID
        request.SetRequestHeader("user-agent", @"" + SystemInfo.operatingSystem + "/" + SystemInfo.deviceModel);
#else
            if (SystemInfo.operatingSystem.Contains("iOS") || SystemInfo.operatingSystem.Contains("Mac OS"))
            {
                request.SetRequestHeader("user-agent", @"Mozilla/5.0 (iPhone; CPU iPhone OS 11_2_5 like Mac OS X) AppleWebKit/604.5.2 (KHTML, like Gecko) Version/11.0 Mobile/15D5046b Safari/604.1");
            }
            else
            {
                request.SetRequestHeader("user-agent", @"Android");
            }

#endif


            await request.SendWebRequest();

            ConnectEnd(request);
        }
        finally
        {
            request.Dispose();
        }
    }

    private void ConnectEnd(UnityWebRequest request)
    {
        Debug.Log("ConnectEnd run..");

        var strtbl = LocalizationSettings.StringDatabase.GetTable("StringTable");

        GameObject MessageCanvas = Main.Instance.MessageView;
        // 通信エラーチェック
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
            MessageCanvas.SetActive(true);
            MessageCanvas.GetComponent<MessageBehaviour>().Open(string.Format(strtbl.GetEntry("error_connection").Value, request.error), false);
        }
        else
        {
            DownloadHandler handler = request.downloadHandler;

            if (request.responseCode == 200)
            {
                // UTF8文字列として取得する
                string text = handler.text;
                Debug.Log(text);
                if (connectObj != null)
                {
                    connectObj.SetActive(false);
                }

                CommonError jsonInfo = null;

                try
                {
                    jsonInfo = JsonUtility.FromJson<CommonError>(text);
                }
                catch (ArgumentException e)
                {
                    //それ以外のexeptionはそのまま出力
                    MessageCanvas.SetActive(true);
                    MessageCanvas.GetComponent<MessageBehaviour>().Open(string.Format(strtbl.GetEntry("error_unknown").Value, Settings.SUPPORT_MAIL_ADDRESS), false);
                }

                if (jsonInfo.result == "error")
                {
                    if (jsonInfo.err_code.Equals("maintenance"))
                    {
                        MessageCanvas.SetActive(true);
                        MessageCanvas.GetComponent<MessageBehaviour>().Open(strtbl.GetEntry("error_maintenance").Value, false);
                    }
                    else if (jsonInfo.err_code.Equals("error_no_regist_access"))
                    {
                        MessageCanvas.SetActive(true);
                        MessageCanvas.GetComponent<MessageBehaviour>().Open(strtbl.GetEntry("error_invalid_access").Value, false);
                    }
                    else if (jsonInfo.err_code.Equals("error_unmatch_ver"))
                    {
                        MessageCanvas.SetActive(true);
                        MessageCanvas.GetComponent<MessageBehaviour>().Open(strtbl.GetEntry("error_unmatch_ver").Value, true);
                    }
                    else if (jsonInfo.err_code.Equals("error_session_expire"))
                    {
                        MessageCanvas.SetActive(true);
                        MessageCanvas.GetComponent<MessageBehaviour>().Open(strtbl.GetEntry("error_session_expire").Value, false);
                    }
                    else if (jsonInfo.err_code.Equals("error_unsupported_carrier"))
                    {
                        MessageCanvas.SetActive(true);
                        MessageCanvas.GetComponent<MessageBehaviour>().Open(strtbl.GetEntry("error_unsupported_carrier").Value, false);
                    }
                    else
                    {
                        //上記以外のエラーは個別に画面で処理されたい
                        _eventCallback?.Invoke(text);
                    }
                }
                else
                {
                    //callback
                    _eventCallback?.Invoke(text);
                }
            }
        }
    }
}

[Serializable]
public class CommonError
{
    /*
　   * 結果コード
　   * ok 正常 error 不正
    */
    public string result;
    /*
     * 共通エラーコード
    */
    public string err_code;

    //リソースハッシュ
    public string resource_hash;
}

[Serializable]
public class jsonLogin
{
    /*
　   * 結果コード
　   * ok 正常 error 不正
    */
    public string result;
    public string err_code;

    /*
     *  登録状態判定
     *  0 未登録
     *  1 登録済み
     *  2 エラー等で、ユーザレコードは出来てるのに、キャラクターレコードが出来てない
     *  3 まだチュートリアル中
     */
    public int regist;

    public int tutorial_step;

    public string nextscene;

    public int dramaId;

    //セッションID。今後のAPIリクエストは常にこのパラメータ付与が必要。
    public string oauth;

    public jsonConstants constants;

    public bool appsflyer;

    public jsonTextMaster[] text_master;

}

[Serializable]
public class jsonTextMaster
{
    public int text_id;
    public string symbol;
    public string ja;
    public string en;
    public string create_at;
}

[Serializable]
public class jsonConstants
{
    public int ACTION_PT_MAX;
    public float ACTION_PT_RECOVERY;
    public int MATCH_PT_MAX;
    public float MATCH_PT_RECOVERY;
    public float HP_RECOVERY;
    public int USER_BATTLE_CONSUME;
    public int ARTICLE_AP;
    public string GOLD_NAME;
    public int DUEL_LIMIT_ON_DAY_RIVAL;
    public int DUEL_SPHERE_PROTECT_HOURS;
    public int MESSAGE_LENGTH_LIMIT;
    public bool MESSAGE_ENABLE;
    public int USERNAME_DISPLAY_WIDTH;
    public int CHARACTER_NAME_LENGTH;

    public string ENVIRONMENT_TYPE;
    public float VCOIN_FEE;
    public float VCOIN_MINIMAM;
    public int VCOIN_MINIMAM_PAYMENT;
    public bool VCOIN_RELEASE_FLG;

    public float BTC_AMOUNT_RARE1;
    public float BTC_AMOUNT_RARE2;
    public float BTC_AMOUNT_RARE3;

    public float RAID_AMOUNT_RARE1;
    public float RAID_AMOUNT_RARE2;
    public float RAID_AMOUNT_RARE3;


    public string STARTDUSH_CAMPAIGN_START_DATE;
    public string STARTDUSH_CAMPAIGN_END_DATE;
    public int STARTDUSH_CAMPAIGN_GET_ITEM;
    public int STARTDUSH_CAMPAIGN_GET_AMOUNT;

    public string BTC_CAMPAIGN_NAME;
    public bool BTC_CAMPAIGN_PAYMENT_STOP;
    public string BTC_CAMPAIGN_START_DATE;
    public string BTC_CAMPAIGN_END_DATE;
    public string BTC_APPLY_RESTRICT_DATE;

    public int BATTLE_RANK_WEEK;

    public bool ETH_ADDR_OPEN;

    public string TWITTER_URI;

    public jsonHistory_Log History_Log;
    public jsonTournament_Master Tournament_Master;
    public jsonItem_Master Item_Master;
    public jsonCharacter_Effect Character_Effect;
    public jsonCharacter_Info Character_Info;
    public jsonUser_Info_Tutorial User_Info_Tutorial;
    public jsonDrama_Master_Tutorial Drama_Master_Tutorial;
    public jsonVcoin_Payment_Log Vcoin_Payment_Log;
    public Dictionary<int, jsonRanking_Log_Prize> Ranking_Log_Prize_Week;
    public jsonQuest_Master Quest_Master;
    public jsonRaid_Dungeon Raid_Dungeon;
    public jsonInvitation_Log Invitation_Log;

}

[Serializable]
public class jsonDrama
{
    public List<string> speakers;
    public string[] flow;

    public string BG1;
    public string BG2;
    public string BG3;
}

[Serializable]
public class jsonTutorial
{
    /*
　   * 結果コード
　   * ok 正常 error 不正
    */
    public string result;
    public int tutorial_step;
    public string nextscene;
    public int dramaId;

    public jsonDrama drama;
}

[Serializable]
public class jsonTerminable
{
    /*
　   * 結果コード
　   * ok 正常 error 不正
    */
    public string result;
    public string nextscene;
    public int dramaId;
    public bool skip;

    public jsonDrama drama;
}


[Serializable]
public class jsonQuestDrama
{
    /*
　   * 結果コード
　   * ok 正常 error 不正
    */
    public string result;
    public string nextscene;
    public string errscene;
    public int dramaId;
    public bool under_construct;

    public jsonDrama drama;
}

[Serializable]
public class jsonFieldDrama
{
    /*
　   * 結果コード
　   * ok 正常 error 不正
    */
    public string result;
    public string nextscene;
    public string errscene;
    public int dramaId;

    public jsonDrama drama;
}

[Serializable]
public class jsonShowDrama
{
    /*
　   * 結果コード
　   * ok 正常 error 不正
    */
    public string result;
    public string nextscene;
    public int dramaId;

    public jsonDrama drama;
}

[Serializable]
public class jsonRegist
{
    /*
　   * 結果コード
　   * ok 正常 error 不正
    */
    public int result;

    public string nextscene;

    public int dramaId;

    public int tutorial_step;

}

[Serializable]
public class jsonHistory_Log
{
    public int TYPE_BATTLE_CHALLENGE;
    public int TYPE_BATTLE_DEFENCE;
    public int TYPE_CHANGE_GRADE;
    public int TYPE_LEVEL_UP;
    public int TYPE_EFFECT_TIMEUP;
    public int TYPE_INVITE_SUCCESS;
    public int TYPE_PRESENTED;
    public int TYPE_QUEST_FIN;
    public int TYPE_ITEM_BREAK;
    public int TYPE_ITEM_LVUP;
    public int TYPE_WEEKLY_HIGHER;
    public int TYPE_CAPTURE;
    public int TYPE_ADMIRED;
    public int TYPE_REPLIED;
    public int TYPE_COMMENT;
    public int TYPE_QUEST_FIN2;
    public int TYPE_TEAM_BATTLE;
}
[Serializable]
public class jsonTournament_Master
{
    public int TOUR_MAIN;
    public int TOUR_QUEST;
}
[Serializable]
public class jsonItem_Master
{
    public int ITEM_RECV_HP;
    public int ITEM_RECV_AP;
    public int ITEM_INCR_PARAM;
    public int ITEM_DECR_PARAM;
    public int ITEM_INCR_EXP;
    public int ITEM_REPAIRE;
    public int ITEM_TACT_ATT;
    public int ITEM_ATTRACT;
    public int ITEM_DTECH_UPPER;
    public int ITEM_RECV_MP;
    public int ITEM_CONTINUE_BATTLE;
    public int ITEM_DTECH_UPPER_INVOKE;
    public int ITEM_DTECH_UPPER_POWER;
    public int ITEM_RARE_ENCOUNT_LV1;
    public int ITEM_RARE_ENCOUNT_LV2;
    public int ITEM_RARE_ENCOUNT_LV3;
    public int ITEM_SRARE_ENCOUNT_LV1;
    public int ITEM_SRARE_ENCOUNT_LV2;
    public int ITEM_SRARE_ENCOUNT_LV3;
    public int INFINITE_DURABILITY;
}
[Serializable]
public class jsonCharacter_Effect
{
    public int TYPE_EXP_INCREASE;
    public int TYPE_HP_RECOVER;
    public int TYPE_ATTRACT;
    public int TYPE_DTECH_POWUP;
}
[Serializable]
public class jsonCharacter_Info
{
    public int INITIAL_HP;
    public int INITIAL_ATTACK;
    public int INITIAL_DEFENCE;
    public int INITIAL_SPEED;
    public int INITIAL_FACE;
    public int HP_SCALE;
}

[Serializable]
public class jsonUser_Info_Tutorial
{
    public int TUTORIAL_MORNING;
    public int TUTORIAL_MAINMENU;
    public int TUTORIAL_FIELD;
    public int TUTORIAL_BATTLE;
    public int TUTORIAL_AFTERBATTLE;
    public int TUTORIAL_STATUS;
    public int TUTORIAL_PRESHOP;
    public int TUTORIAL_SHOPPING;
    public int TUTORIAL_GACHA;
    public int TUTORIAL_RIVAL;
    public int TUTORIAL_EQUIP;

    public int TUTORIAL_LAST;
    public int TUTORIAL_END;
    public int TUTORIAL_MOVE;
    public int TUTORIAL_GLOBALMOVE;
    public int TUTORIAL_FINISH;
}
[Serializable]
public class jsonDrama_Master_Tutorial
{
    public int PROLOGUE;
    public int TUTORIAL0;
    public int TUTORIAL30;
    public int TUTORIAL40;
    public int TUTORIAL60;
    public int TUTORIAL90;
}
[Serializable]
public class jsonVcoin_Payment_Log
{
    public int INITIAL;
    public int RECEIVE;
    public int COMPLETE;
    public int CANCEL;
}
[Serializable]
public class jsonQuest_Master
{
    public int EVENT_QUEST;
    public int WILD_PLACE;
    public int MONSTER_DUNGEON;
    public int TEAM_BATTLE;
}



[Serializable]
public class jsonRanking_Log_Prize
{
    public int id;
    public int count;
    public float btc;
    public string item_name;
    public string set_name;
}

[Serializable]
public class jsonRaid_Dungeon
{
    public int NONE;
    public int READY;
    public int START;
    public int SUCCESS;
    public int FAILURE;
    public int REQUIRE_NONE;
    public int REQUIRE_ETHADDR;
}

[Serializable]
public class jsonInvitation_Log
{
    public float INVITE_BTC;
    public float INVITED_BTC;
    public jsonInvitation_Bonus[] INVITE_BONUS;
    public jsonInvitation_Bonus[] ANSWER_BONUS;
}

[Serializable]
public class jsonInvitation_Bonus
{
    public string item_name;
    public int count;
}


//InfoAPI
public class InfoApi
{
    public List<InfoResultSet> resultset;
    public int totalRows;
    public int totalPages;
}

[System.Serializable]
public class InfoResultSet
{
    public int oshirase_id;
    public int importance;
    public string title;
    public string body;
    public string title_en;
    public string body_en;
    public string notify_at;
    public string update_at;
    public bool isNew;
    public string importance_text;
    public string importance_icon;
}

//HomeAPI
public class HomeApi
{
    public jsonChara chara;
    public HomeMember member;
    public HomeExp exp;
    public jsonGrade grade;
    public string player_name;
    public float actionPt;
    public float matchPt;
    public int gold;
    public float vcoin;
    public int MaxActionPt;
    public int MaxMatchPt;
    public bool bitcoin_show;
    public int coin;
    public float ACTION_PT_RECOVERY;//double?
    public float MATCH_PT_RECOVERY;//double?
    public string urlOnTop;
    public string urlOnTutorial;
    public int lastAffected;
    public string firstscene;//string?
    public List<HomeInfo> oshiraseList;
    public HomeBanner bannar;
    public HomeHistory[] history;
    public int tutorial_step;
    public jsonGachaContents fitGacha;
    public int unreadCount;
    public int unanswerCount;
    public int unconfirmCount;
    public bool freeGacha;
    public jsonQuestList sally_quest;
    public string menu0Url;
    public string menu1Url;
    public string menu2Url;
    public string menu3Url;
    public string menu4Url;
    public string menu5Url;
    public string menu6Url;
    public string menu7Url;
    public string guide0;
    public string guide1;
    public string guide2;
    public string guide3;
    public string guide4;
    public string guide5;
    public string guide6;
    public string guide7;
    public string guide8;
    public string start_speak1;
    public string start_speak2;
    public string trans_late_speak;
    public HomeBattleRankInfo battle_rank_info;
    public string menu0State;
    public string menu1State;
    public string menu2State;
    public string menu3State;
    public string menu4State;
    public string menu5State;
    public string menu6State;
    public string menu7State;
    public string menu8tate;
    public string selectmenu;
    public string[] opening;
    public int openingNum;
    public string[] special;
    public int specialNum;
    public string eth_addr;
    public string eth_addr_description;

    public int monster_capture;
    public int monster_count;

    public jsonRaidDungeon raid_dungeon;
    public string[] bitcoin_explain;

}

[System.Serializable]
public class jsonRaidDungeon
{
    public int id;
    public string title;
    public int join_prize_kind;
    public float join_prize;
    public string notice_at;
    public string start_at;
    public string end_at;
    public string close_at;
    public string description;
    public int status;
    public jsonRaidDungeonPrize[] prizelist;
    public int total_count;
    public int defeat_count;
    public int quest_id;
    public int require_kind;

    public int past;

    public string navi_title;
    public string navi_serifu;

}

[System.Serializable]
public class jsonRaidDungeonPrize
{
    public int raid_dungeon_id;
    public int rank_id;
    public int join_prize_kind;
    public float prize;
}

[System.Serializable]
public class jsonChara
{
    public int character_id;
    public int user_id;
    public string entry;
    public string race;
    public int name_id;
    public int graphic_id;
    public int grade_id;
    public int grade_pt;
    public int sally_sphere;
    public int exp;
    public int param_seed;
    public int hp;
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
    public int level;
    public Dictionary<int, jsonEquip> equip;
    public int equip_attack1;
    public int equip_attack2;
    public int equip_attack3;
    public int equip_defence1;
    public int equip_defence2;
    public int equip_defence3;
    public int equip_speed;
    public int total_defenceX;
    public int total_attack1;
    public int total_attack2;
    public int total_attack3;
    public int total_defence1;
    public int total_defence2;
    public int total_defence3;
    public int total_speed;
    public string[] equip_info;

    public jsonUser user;
    public int member;
    public string player_name;
    public string grade_name;
    public jsonGrade grade;
}

[System.Serializable]
public class jsonUser
{
    public int user_id;
    public string platform_uid;
    public string name;
    public int gold;
    public float virtual_coin;
    public float action_pt;
    public float match_pt;
    public int tutorial_step;
    public int place_id;
    public string user_agent;
    public string last_affected;
    public string name_sync_date;
    public string last_access_date;
    public string retire_date;
    public string create_at;
    public string short_name;
}

[System.Serializable]
public class jsonEquip
{
    public int user_item_id;
    public int user_id;
    public int item_id;
    public int num;
    public int item_exp;
    public int durable_count;
    public string create_at;
    public int level;
    public int attack1;
    public int attack2;
    public int attack3;
    public int defence1;
    public int defence2;
    public int defence3;
    public int speed;
    public int defenceX;
    public int evolution;
    public bool is_evol;
    public string item_name;
    public string category;
    public int present_flg;
    public string flavor_text;
    public int durability;
    public int item_type;
    public int item_limitation;
    public int item_value;
    public int item_spread;
    public int item_vfx;
    public int item_flags;
    public int set_id;
    public int rear_level;
    public int max_level;
    public string repaire_useto;
    public string urlOnRepaire;
    public bool holdRecover;
    public int repaire;

    public string set_name;
    public string set_text;
    public int mount_id;
    public string mount_name;
    public int rear_id;

    public bool useable;
    public int useCount;

    public string effect;
}

[System.Serializable]
public class HomeMember
{
    public int current;
    public int request;
    public int receive;
    public int total;
    public int limit;
}

[System.Serializable]
public class HomeExp
{
    public int absolute_next;
    public int relative_exp;
    public int relative_next;
}

[System.Serializable]
public class jsonGrade
{
    public int grade_id;
    public string grade_name;
    public int raise_border;
    public int abase_border;
    public int battle_reward;
    public int dtech_id;
    public jsonDTech dtech;
}

[System.Serializable]
public class jsonDTech
{
    public int dtech_id;
    public string dtech_name;
}

[System.Serializable]
public class HomeInfo
{
    public int oshirase_id;
    public int importance;
    public string title;
    public string body;
    public string notify_at;
    public string update_at;
    public bool isNew;
    public string importance_text;
    public string importance_icon;
}

[System.Serializable]
public class HomeBanner
{
    public jsonQuestList[] quest;
    public string explain;
}

[System.Serializable]
public class HomeHistory
{
    public string player_name;
}

[System.Serializable]
public class HomeBattleRankInfo
{
    public int status;
    public bool in_aggregate;
    public int start_date;
    public int result_date;
    public int end_date;
}

[System.Serializable]
public class HomeSpecial
{

}

[System.Serializable]
public class jsonQuest
{
    public int currRegion;
    public int sally_quest_id;
    public int sally_sphere;
    public jsonQuestList sally_quest;
    public int currPlace;
    public string regionName;
    public bool showGlobal;

    public Dictionary<int, jsonPlaceList> globalplace;
    public Dictionary<int, jsonPlaceList[]> place;
    public Dictionary<int, List<jsonQuestList[]>> quest;

}

[System.Serializable]
public class jsonPlaceList
{
    public float X;
    public float Y;
    public int Id;
    public string Name;
}

[System.Serializable]
public class jsonQuestList
{
    public int quest_id;
    public string quest_name;
    public int place_id;
    public string type;
    public int consume_pt;
    public int penalty_pt;
    public int content_id;
    public int upper_level;
    public int repeatable;

    public string preferred_level;
    public int sort_order;
    public string flavor_text;
    public int gacha_id;
    public int currPlace;
    public string start_date;
    public string end_date;
    public int status;

}

[System.Serializable]
public class jsonFieldReopen
{
    public string result;
    public string Scene;
    public int id;
    public string reopen;
    public int sphereId;

}

[System.Serializable]
public class jsonReady
{
    //ok or error
    public string result;
    public Dictionary<string, List<jsonItems>> item;
    public string[] comment;

    //移動先シーン
    public string Scene;
    //Scene = Sphereの時
    public int id;
    //Scene = Terminable or QuestDrama の時
    public int questId;
    public int sphereId;
    //これがある場合はこのAPIを叩く
    public string Api;

}

[System.Serializable]
public class jsonItems
{
    //ok or error
    public int attack1;
    public int attack2;
    public int attack3;
    public string category;
    public string create_at;
    public int defence1;
    public int defence2;
    public int defence3;
    public int defenceX;
    public int durability;
    public int durable_count;
    public int evolution;
    public int max_level;
    public bool equippable;
    public string flavor_text;
    public int free_count;
    public int item_exp;
    public int item_flags;

    public int item_id;
    public int item_limitation;
    public string item_name;
    public int item_spread;
    public int item_type;

    public int item_value;
    public int item_vfx;
    public int level;
    public int num;
    public bool present_flg;
    public int rear_level;
    public int rear_id;
    public int set_id;
    public string set_name;
    public string set_text;
    public int speed;
    public int user_id;
    public int user_item_id;

    public string effect;
    public bool guaranteed_flg;

    public jsonEquipSet set;

}


[Serializable]
public class jsonEquipSet
{
    public int set_id;
    public string set_name;
    public string set_text;
    public int rear_id;
}


[Serializable]
public class jsonUnit
{
    public int no;
    public string Name;
    public float X;
    public float Y;
    public string code;
    public string act_brain;
    public UnitInfo Info = new UnitInfo();
    public UnitStatus Status = new UnitStatus();
    public List<int> Item = new List<int>();
    public List<int> Eqp = new List<int>();
}

[Serializable]
public class UnitInfo
{
    public int graphNo;
    public int union;
    public int cost;
    public int align;
}
[Serializable]
public class UnitStatus
{
    public int level;
    public int hp;
    public int maxhp;
    public int att1;
    public int att2;
    public int att3;
    public int def1;
    public int def2;
    public int def3;
    public int spd;
    public int defX;
}


[Serializable]
public class jsonSphere
{
    /*
　   * 結果コード
　   * ok 正常 error 不正
    */
    public string result;

    public int readonly_flg;

    public string[] structs;
    public int structWidth;
    public int structHeight;
    public int structWid;
    public int structHei;

    public string[] structbackground;
    public int backgroundWid;
    public int backgroundHei;

    public string[] structcover;
    public int coverWid;
    public int coverHei;

    public string[] structoverlayer1;
    public int overlayer1Wid;
    public int overlayer1Hei;

    public string[] structoverlayer2;
    public int overlayer2Wid;
    public int overlayer2Hei;

    public string[] structhead;
    public int headWid;
    public int headHei;

    public string[] structleft;
    public int leftWid;
    public int leftHei;

    public string[] structright;
    public int rightWid;
    public int rightHei;

    public string[] structfoot;
    public int footWid;
    public int footHei;

    public Dictionary<int, string> tip;
    public Dictionary<int, string> tipId;

    public string[] mat;

    public Dictionary<int, jsonUnit> unit;
    public Dictionary<int, string> unitIcon;
    public int unitNum;

    public Dictionary<int, string> item = new Dictionary<int, string>();

    public Dictionary<int, string> orn = new Dictionary<int, string>();
    public int ornNum;

    public int revision;

    public int actionPt;
    public int consumePt;
    public int EASY_MODE;
    public string ERROR_RELOAD;
    public string ERROR_NO_ACTIONPT;
    public string TRANS_OTHER_SCENE;
    public string SHOWWND_PUSH_BUTTON;
    public string SHOWWND_IN_TRANS;
    public string SHOWWND_FAIL_SEND_CMD;
    public string SHOWWND_FAIL_TRANS;
    public string SHOWWND_RELOAD_FOR_LIMIT;
    public string _STRING_MENU;
    public string _STRING_CANCEL;
    public string _STRING_ENTER;
    public string _STRING_DETAIL;
    public string STR_CMD_MOVE;
    public string STR_CMD_WAIT;
    public string STR_CMD_ATACK;

    public string STR_CMD_ITEM;
    public string STR_CMD_STOP;
    public string STR_CMD_OK;
    public string STR_CMD_CANCEL;
    public string STR_CAPTION_ITEM;
    public string STR_CAPTION_EQUIP;
    public string STR_BEFORE;
    public string STR_ACTION_PT;
    public string STR_CONFIRM_CHANGE_EQP;
    public string[] preLd;

    public string sphere_bg;
    public string environment;
    public int BOTTOM_MARGIN;

    public string validation_code;

    public jsonDrama drama;

    public string reloadUrl;
    public string suspUrl;
    public string transmitUrl;
    public string apShortUrl;

    public string bgm;

    public jsonRaidDungeon raid_dungeon;

    public string jsonfile;
}

[Serializable]
public class jsonSphereCommand
{
    public string result;
    public int leadNum;
    public Dictionary<string, string> lead;
}

[Serializable]
public class jsonSphereItemList
{
    public string result;
    public jsonSphereItems[] itemList;
}

[Serializable]
public class jsonSphereItems
{
    public int item_id;
    public string item_name;
    public string category;
    public bool present_flg;
    public string flavor_text;
    public int durability;
    public int item_type;
    public int item_limitation;
    public int item_value;
    public int item_spread;
    public int item_vfx;
    public int item_flags;
    public int set_id;
    public int rear_level;

    public int slot;
    public int item_no;
    public int evolution;
    public bool useable;

}


[Serializable]
public class jsonBattle
{
    public string result;
    public string urlOnError;

    public string urlOnConfirm;
    public string urlOnContinue;
    public string urlOnBuyItem;
    public string urlOnEnd;
    public string urlOnQuest;
    public string urlOnList;
    public string urlOnMypage;
    public string urlOnParamUp;

    public string validationCode;
    public int battle_id;
    public int repaire_id;

    public int CharaIdP;
    public int CharaIdE;
    public string nameP;
    public int LvP;
    public int hpMaxP;
    public int hpStartP;
    public int att1P;
    public int att2P;
    public int att3P;
    public int def1P;
    public int def2P;
    public int def3P;
    public int continueError;
    public int continueItemCnt;
    public string continueItemName;
    public int continue_count;
    public int CONTINUE_COUNT_LIMIT;

    public string nameE;
    public int LvE;
    public int hpMaxE;
    public int hpStartE;
    public int att1E;
    public int att2E;
    public int att3E;
    public int def1E;
    public int def2E;
    public int def3E;

    public int playerBrainLv;
    public int enemyBrainLv;

    public int spdRate;
    public int timeupTurns;
    public int randomSeed;

    public string navSerif_open;
    public string navSerif_win;
    public string navSerif_lose;
    public string navSerif_draw;
    public string navSerif_timeup;
    public string STR_PUSH_BUTTON_MESSAGE;
    public string STR_BATTLE_START;
    public string STR_CONFIRM_DATA;
    public string STR_WAIT_PLEASE;
    public string STR_ALREADY_START;
    public string STR_ERROR;

    public int statTactP0;
    public int statTactP1;
    public int statTactP2;
    public int statTactP3;
    public int statTactE0;
    public int statTactE1;
    public int statTactE2;
    public int statTactE3;
    public int statNattCntP;
    public int statNattCntE;
    public int statNhitCntP;
    public int statNhitCntE;

    public int statNdamP;
    public int statNdamE;
    public int statRevCntP;
    public int statRevCntE;
    public int statRattCntP;
    public int statRattCntE;
    public int statRhitCntP;
    public int statRhitCntE;
    public int statRdamP;
    public int statRdamE;
    public int statOdamP;
    public int statOdamE;
    public string AUTO_SERIFU_UNISON;
    public string AUTO_SERIFU_UNISONED;
    public string AUTO_SERIFU_STRONG_ATTACK;
    public string AUTO_SERIFU_PRUDENCE;
    public string AUTO_SERIFU_ABSORPTION;
    public string AUTO_SERIFU_MIND_READING;
    public string AUTO_SERIFU_NO_ABSORPTION_1;
    public string AUTO_SERIFU_NO_ABSORPTION_2;
    public string AUTO_SERIFU_STRONG_ATTACK_DESIDE;
    public string AUTO_SERIFU_PRUDENCE_DESIDE;
    public string MANUAL_SERIFU_STRONG_ATTACK;
    public string MANUAL_SERIFU_PRUDENCE;
    public string MANUAL_SERIFU_ABSORPTION;
    public string speaker_charaP;
    public string speaker_charaE;
    public jsonDtech dtech_charaP;
    public jsonDtech dtech_charaE;
    public string[] equip_infoP;
    public string[] equip_infoE;
    public string bgm_sound;
    public string firstscene;

    public string battle_bg;

    public Dictionary<string, Dictionary<string, int[]>> card;

    public int tournament_id;


    //チュートリアル用
    public string[] tutOpen;
    public string[] tutTurn;
    public string[] tutUni;
    public string[] tutStar;
    public string[] tutRevP;

    public string tutClose0;
    public string tutClose1;
    public string tutClose2;

    public string[] tutClose;
    public string navSerif_end;
}


[Serializable]
public class jsonDtech
{
    public int dtech_id;
    public string dtech_name;
    public string dtech_desc;
    public int invoke_rate;
    public int code_id;
    public string value1;
    public string value2;
    public string value3;
    public int graphic_id;

}

[Serializable]
public class jsonBattleConfirm
{
    public string result;
    public string err_code;

}


[Serializable]
public class jsonBattleResult
{
    public string result;

    public string side;

    public jsonBRBattleResult battleresult;

    public jsonBRBattle battle;
    public jsonBRCurrent current;
    public jsonBRCharaInfo ready;

    public bool gradeup;
    public jsonGrade grade;

    public bool levelup;
    public bool capture_flg;
    public bool item_flg;

    public string urlOnHome;
    public string urlOnSphere;
    public string urlOnRivalList;
    public string urlOnHisPage;

    public List<jsonGrade> neighborGrades;
    public jsonChara chara;

    public jsonBRCapture capture;

}

[Serializable]
public class jsonBRBattleResult
{
    public jsonChara character;
    public jsonTotalResult total_result;
    public jsonBRGain gain;
    public jsonResultEquip equip;
    public jsonBRBattleSummary summary;
}

[Serializable]
public class jsonResultEquip
{
    public Dictionary<int, jsonEquip> after;
    public Dictionary<int, jsonEquip> before;
}

[Serializable]
public class jsonTotalResult
{
    public int challenge_win;
    public int challenge_lose;
    public int challenge_timeup;
    public int challenge_draw;
    public int defend_win;
    public int defend_lose;
    public int defend_timeup;
    public int defend_draw;
    public int win;
    public int lose;
    public int timeup;
    public int draw;
    public int fights;
}

[Serializable]
public class jsonBRGain
{
    public int exp;
    public int gold;
    public int grade;
    public int grade_nominal;
    public jsonUserItem[] uitem;
    public int monster;
}

[Serializable]
public class jsonBRCapture
{
    public int character_id;
    public int category;
    public string monster_no;
    public int rare_level;
    public int appearance_area;
    public string habitat;
    public string flavor_text;
    public int user_id;
    public string entry;
    public string race;
    public int name_id;
    public int graphic_id;
    public int grade_id;
    public int grade_pt;
    public int sally_sphere;
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
    public int level;
    public int mobility;
    public int move_pow;
    public int battle_brain;
    public int dtech1_id;
    public int reward_exp;
    public int reward_gold;
    public int normal_drop;
    public int rare_drop;
    public int srare_drop;
    public string monster_name;
    public Dictionary<int, jsonEquip> equip;
    public string image_url;
}

[Serializable]
public class jsonUserItem
{
    public int item_id;
    public string item_name;
    public string category;
    public string flavor_text;
    public string effect;
    public int attack1;
    public int attack2;
    public int attack3;
    public int speed;
    public int defence1;
    public int defence2;
    public int defence3;
    public int defenceX;

}

[Serializable]
public class jsonUserItems
{
    public string result;
    public jsonShopResultSet user_item;

}


[Serializable]
public class jsonMonster
{
    public int character_id;
    public int category;
    public string monster_no;
    public int rare_level;
    public int appearance_area;
    public string habitat;
    public string flavor_text;
}

[Serializable]
public class jsonBRBattle
{
    public int battle_id;
    public int tournament_id;
    public int challenger_id;
    public int defender_id;
    public int player_id;
    public int side_reverse;
    public int relate_id;
    public int status;
    public string validation_code;
    public jsonBRReadyDetail ready_detail;
    public jsonBRResultDetail result_detail;
    public int comment_id;
    public string create_at;
    public string result_at;
    public int true_status;
    public string comment;
    public bool is_challenger;
    public int bias_character_id;
    public int rival_character_id;
    public jsonBRCharaInfo bias_ready;
    public jsonBRCharaInfo rival_ready;
    public jsonBRBattleResult bias_result;
    public jsonBRBattleResult rival_result;
    public string bias_character_name;
    public int bias_user_id;
    public string rival_character_name;
    public int rival_user_id;
    public string bias_user_name;
    public string rival_user_name;
    public string bias_status;

}

[Serializable]
public class jsonBRReadyDetail
{
    public int rand_seed;
    public int continue_count;
    public int in_game_flg;
    public jsonBRCharaInfo challenger;
    public jsonBRCharaInfo defender;

}
[Serializable]
public class jsonBRResultDetail
{
    public int match_length;
    public float get_vcoin;
    public int get_raid_point;
    public jsonBRCapture monster;
    public jsonBRBattle challenger;
    public jsonBRBattle defender;
    public bool get_nft;

}

[Serializable]
public class jsonBRCurrent
{
    public int gold;
    public int grade_pt;
    public Dictionary<string, int> exp;
}

[Serializable]
public class jsonBRCharaInfo
{
    public string code;
    public string name;
    public int union;
    public string icon;
    public int move_pow;
    public string act_brain;
    public int battle_brain;
    public bool player_owner;
    public bool room_takeover;
    public int character_id;
    public int hp;
    public int[] items;
    public int turn;
    public bool transcend_adapt;
    public int[] pos;
    public int align;
    public int no;
    public int user_id;
    public string entry;
    public string race;
    public int name_id;
    public int graphic_id;
    public int grade_id;
    public int grade_pt;
    public int sally_sphere;
    public int exp;
    public int param_seed;
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
    public int level;
    public Dictionary<int, jsonEquip> equip;
    public int equip_attack1;
    public int equip_attack2;
    public int equip_attack3;
    public int equip_defence1;
    public int equip_defence2;
    public int equip_defence3;
    public int equip_speed;
    public int total_defenceX;
    public int equip_defenceX;
    public int total_attack1;
    public int total_attack2;
    public int total_attack3;
    public int total_defence1;
    public int total_defence2;
    public int total_defence3;
    public int total_speed;
    public int[] sequip;
    public int starcnt;
    public jsonBRSummary summary;
    public string grade_name;
}

[Serializable]
public class jsonBRSummary
{
    public int tact0;
    public int tact1;
    public int tact2;
    public int tact3;
    public int nattCnt;
    public int nhitCnt;
    public int ndam;
    public int revCnt;
    public int rattCnt;
    public int rhitCnt;
    public int rdam;
    public int odam;
}

[Serializable]
public class jsonBRBattleSummary
{
    public int hp_on_end;
    public int tact0;
    public int tact1;
    public int tact2;
    public int tact3;
    public int normal_attacks;
    public int normal_hits;
    public int normal_hurt;
    public int revenge_count;
    public int revenge_attacks;
    public int revenge_hits;
    public int revenge_hurt;
    public int total_hurt;
}

[Serializable]
public class jsonBattleContinue
{
    public string result;
    public string err_code;

}

[Serializable]
public class jsonStatus
{
    public string result;
    public jsonChara chara;
    public jsonGrade grade;
    public Dictionary<int, jsonEquip> PLAEQP;
    public Dictionary<int, jsonEffectExpires> effectExpires;
    public jsonParamupItemStatus paramupItemStatus;
}

[Serializable]
public class jsonParamupItemStatus
{
    public int param1;
    public int param2;
    public int param3;
}

[Serializable]
public class jsonEffectExpires
{
    public int type;
    public string effect_name;
    public string expire;
    public int value;
}


[Serializable]
public class jsonParamUp
{
    public string result;
}

[Serializable]
public class jsonShop
{
    public string result;

    public int mount_id;
    public int buy_user_item_id;
    public int num;
    public int coin;
    public int gold;
    public int price;

}


[Serializable]
public class jsonShopList
{
    public string result;
    public int Num;
    public jsonShopResultSet[] resultset;
    public jsonShopResultSetNext next;
}

[Serializable]
public class jsonShopResultSet
{
    public int shop_id;
    public int item_id;
    public int price;
    public int sale;
    public int unlock_level;
    public int sort_order;
    public string item_name;
    public string category;
    public int present_flg;
    public string flavor_text;
    public int durability;
    public int item_type;
    public int item_limitation;
    public int item_value;
    public int item_spread;
    public int item_vfx;
    public int item_flags;
    public int set_id;
    public string set_name;
    public int rear_level;
    public bool show_only;
    public int hold;
    public int attack1;
    public int attack2;
    public int attack3;
    public int defence1;
    public int defence2;
    public int defence3;
    public int defenceX;
    public int speed;
    public string effect;
}

[Serializable]
public class jsonShopResultSetNext
{
    public int shop_id;
    public int item_id;
    public int price;
    public int sale;
    public int unlock_level;
    public int sort_order;
    public jsonItems item;
}

[Serializable]
public class jsonSuggest
{
    public string result;
    public string err_code;
    public string apiOnShop;
    public string apiOnHomeSummary;
    public string suggest_nexturl;
    public jsonEquip uitem;
    public jsonEquip item;
    public int item_id;
    public int price;
    public int coin;

}

[Serializable]
public class jsonBattleBuyItem
{
    public string result;
    public string err_code;
    public jsonEquip item;
    public int item_id;
    public int price;
    public int coin;

}

[Serializable]
public class jsonFieldEnd
{
    public string result;
    public string err_code;
    public int gold;
    public string urlOnQuest;

    public int SPHERE_SUCCESS;
    public int SPHERE_ESCAPE;
    public int SPHERE_FAILURE;
    public int SPHERE_GIVEUP;

    public int sphere_result;

    public jsonQuestList quest;
    public jsonFieldEndSummary summary;

    public jsonItems[] treasures;
    public jsonQuestList next;
    public string urlOnNext;

}

[Serializable]
public class jsonFieldEndSummary
{
    public int quest_id;
    public int result;

    public int turn;
    public int terminate;
    public jsonMission mission;

    public jsonItems[] treasures;

    public string quest_name;

}

[Serializable]
public class jsonMission
{
    public bool achieve;
    public int gold;

}

[Serializable]
public class jsonTutlrialBattle
{
    public string result;
    public string urlOnEnd;
    public int tutorial_step;
}



[Serializable]
public class jsonGacha
{
    public string result;
    public int ticketCount;
    public bool freeGacha;
    public jsonGachaContents[] gacha;
}

[Serializable]
public class jsonGachaContents
{
    public int gacha_id;
    public string gacha_name;
    public int price;
    public int price_bulk;
    public int freeticket_item_id;
    public int freeticket_count;
    public string caption;
    public string flavor_text;
    public int unlock_level;
    public int gacha_kind;
    public bool sp_flg;
    public bool wk_flg;
    public bool close_flg;
    public bool notice_time;
    public int has_freeticket_count;
    public int clear_event_id;
    public int guaranteed_count;
    public bool is_guaranteed;
}

[Serializable]
public class jsonGachaPlay
{
    public string result;
    public string err_code;
    public string nextUrl;
    public jsonGachaContents gacha;

}


[Serializable]
public class jsonGachaResult
{
    public string result;
    public string err_code;
    public string urlOnMain;
    public int gacha_count;
    public int guaranteed_item_id;

    public jsonItems[] atari_item;
    public jsonItems[] getitem;

}

[Serializable]
public class jsonExchange
{
    public string result;
    public string err_code;

    public jsonItems exchange;

}

[Serializable]
public class jsonEquipChange
{
    public string result;
    public string err_code;
    public int price;
}

[Serializable]
public class jsonSuncResult
{
    public string result;
    public string err_code;
    public int aex;
    public int alv;
    public int blv;
    public int bex;
    public int bgld;
    public int agld;
    public int maxlv;
}


[Serializable]
public class jsonEquipList
{
    public string result;
    public Dictionary<int, jsonEquip[]> equip;
    public Dictionary<int, jsonEquip> PLAEQP;
}

[Serializable]
public class jsonGradeList
{
    public string result;
    public jsonGrade[] list;
    public Dictionary<int, int> distribute;
    public jsonChara chara;
}

[Serializable]
public class jsonGradeUser
{
    public string result;
    public jsonGrade grade;
    public jsonGradeUserList list;
}

[Serializable]
public class jsonGradeUserList
{
    public jsonChara[] resultset;
    public int totalRows;
    public int totalPages;
}

[Serializable]
public class jsonMemberSearch
{
    public string result;
    public jsonMemberResultSet[] list;

}


[Serializable]
public class jsonMemberList
{
    public string result;

    public jsonMemberListList list;
}

[Serializable]
public class jsonMemberListList
{
    public jsonMemberResultSet[] resultset;
    public int totalRows;
    public int totalPages;
}

[Serializable]
public class jsonMemberResultSet
{
    public int user_id;
    public string platform_uid;
    public string name;
    public int gold;
    public float virtual_coin;
    public float action_pt;
    public float match_pt;
    public int tutorial_step;
    public int place_id;
    public string user_agent;
    public string last_affected;
    public string name_sync_date;
    public string last_access_date;
    public string retire_date;
    public string create_at;
    public string short_name;
    public string player_name;

    public jsonChara chara;
    public jsonGrade grade;
    public string[] equip_info;
}
[Serializable]
public class jsonApproachResult
{
    public string result;
    public string err_code;
}

[Serializable]
public class jsonApproachAct
{
    public string result;
    public string opCode;
    public int companion_id;
}


[Serializable]
public class jsonApproachList
{
    public string result;
    public int unconfirmed;

    public jsonApproachListResultSet list;
}

[Serializable]
public class jsonApproachListResultSet
{
    public jsonApproach[] resultset;
    public int totalRows;
    public int totalPages;
}


[Serializable]
public class jsonApproach
{
    public int approach_id;
    public int approacher_id;
    public int recipient_id;

    public int status;
    public string answer_date;
    public string create_at;
    public jsonCompanion companion;
    public string[] equip_info;

}


[Serializable]
public class jsonCompanion
{
    public int user_id;
    public string platform_uid;
    public string name;
    public int gold;
    public float virtual_coin;
    public float action_pt;
    public float match_pt;
    public int tutorial_step;
    public int place_id;
    public string user_agent;
    public string last_affected;
    public string name_sync_date;
    public string last_access_date;
    public string retire_date;
    public string create_at;
    public string short_name;
    public string player_name;

    public jsonChara chara;
    public jsonGrade grade;
}



[Serializable]
public class jsonHistoryList
{
    public string result;

    public jsonHistoryListList list;
}

[Serializable]
public class jsonHistoryListList
{
    public jsonHistory[] resultset;
    public int totalRows;
    public int totalPages;
}

[Serializable]
public class jsonHistory
{
    public int history_id;
    public int user_id;
    public int type;
    public int ref1_value;
    public int ref2_value;
    public bool check_flg;
    public string deleted_at;
    public string create_at;
    public int reply_count;
    public int goodness;
    public string[] reply_to;
    public jsonHistoryListSummary summary;
    public string player_name;
    public string[] equip_info;

    public jsonBRBattle battle;
    public jsonGrade grade;
    public jsonItems item;

    public string effect_name;
    public string rare_name;
    public jsonBRCapture monster;
}

[Serializable]
public class jsonHistoryListSummary
{
    public int quest_id;
    public int result;
    public int turn;
    public int terminate;

    public int attain_stair;

    public jsonMission mission;
    public jsonItems[] treasures;

    public string quest_name;

}




[Serializable]
public class jsonBattleHistory
{
    public string result;

    public jsonChara character;
    public string charaName;
    public int win;
    public int lose;
    public int draw;
    public int fights;

    public jsonBattleHistoryList list;
}

[Serializable]
public class jsonBattleHistoryList
{
    public jsonBattleHistoryResult[] resultset;
    public int totalRows;
    public int totalPages;
}

[Serializable]
public class jsonBattleHistoryResult
{
    public int battle_id;
    public int tournament_id;
    public int challenger_id;
    public int defender_id;
    public int player_id;
    public int side_reverse;
    public int relate_id;
    public int status;
    public string validation_code;

    public jsonBRReadyDetail ready_detail;
    public jsonBRResultDetail result_detail;

    public int comment_id;
    public string create_at;
    public string result_at;
    public int true_status;
    public string comment;
    public bool is_challenger;
    public int bias_character_id;
    public int rival_character_id;
    public jsonBRCharaInfo bias_ready;
    public jsonBRCharaInfo rival_ready;
    public jsonBRBattleResult bias_result;
    public jsonBRBattleResult rival_result;
    public string bias_character_name;
    public int bias_user_id;
    public string rival_character_name;
    public int rival_user_id;
    public string bias_user_name;
    public string rival_user_name;
    public string bias_status;

    public string player_name;
    public string[] equip_info;
}


[Serializable]
public class jsonGachaLineup
{
    public string result;

    public jsonGacha gacha;

    public jsonGachaLineupList[] list;
}


[Serializable]
public class jsonGachaLineupList
{
    public string result;

    public int gacha_id;
    public int item_id;
    public int weight;
    public int sort_order;
    public jsonItems item;
    public float rate;
}

[Serializable]
public class ItemUseFire
{
    public string result;
    public string err_code;

    public int use;
    public string effect;

}


[Serializable]
public class jsonHelpList
{
    public string result;
    public jsonChara avatar;

    public Dictionary<string, jsonHelpCaption[]> helpTree;
    public Dictionary<string, string> groups;

}


[Serializable]
public class jsonHelpCaption
{
    public string help_id;
    public string help_title;
    public string help_body;

    public int unlock_level;
    public int sort_order;
    public string group_id;

}

[Serializable]
public class jsonHelpDetail
{
    public string result;
    public jsonHelpContents help;
}

[Serializable]
public class jsonHelpContents
{
    public string help_id;
    public string help_title;
    public string[] help_body;

    public int unlock_level;
    public int sort_order;
    public string group_id;

}

[Serializable]
public class jsonDiscard
{
    public string result;
    public string err_code;
}




[Serializable]
public class jsonRivalList
{
    public string result;
    public jsonTournament tournament;
    public jsonChara[] rivalList;
    public int rivalList_Num;

}



[Serializable]
public class jsonTournament
{
    public int tournament_id;
    public string tournament_name;
    public string open_date;
    public string close_date;
}

[Serializable]
public class jsonBattleRanking
{

    public jsonTerm term;
    public jsonjsonBattleRankingList list;
    public int cycle;
    public jsonPeriod period;
    public jsonRankinfo rankinfo;
}


[Serializable]
public class jsonjsonBattleRankingList
{
    public jsonBattleRankingResultSet[] resultset;
    public int totalRows;
    public int totalPages;
}

[Serializable]
public class jsonBattleRankingResultSet
{
    public int type;
    public int period;
    public int user_id;
    public int point;
    public int rank;
    public string user_name;
    public jsonChara avatar;
    public jsonPeriod highest;
    public int totalPages;
}

[Serializable]
public class jsonTerm
{
    public int begin;
    public int end;
}

[Serializable]
public class jsonPeriod
{
    public int weekly;
    public int daily;
}
[Serializable]
public class jsonRankinfo
{
    public int status;
    public bool in_aggregate;
    public int start_date;
    public int result_date;
    public int end_date;
}

[Serializable]
public class jsonHisPage
{
    public string result;
    public string comment;
    public bool isMember;
    public bool isApproaching;
    public jsonChara chara;
    public jsonCtour ctour;
    public jsonMount[] mounts;
    public jsonBattleRankingResultSet rank;
    public string canBattle;
}

[Serializable]
public class jsonCtour
{
    public int character_id;
    public int tournament_id;
    public int challenge_win;
    public int challenge_lose;
    public int challenge_timeup;
    public int challenge_draw;
    public int defend_win;
    public int defend_lose;
    public int defend_timeup;
    public int defend_draw;
    public int win;
    public int lose;
    public int timeup;
    public int draw;
    public int fights;
}

[Serializable]
public class jsonMount
{
    public string race;
    public int mount_id;
    public string mount_name;
    public int default_id;
    public int slot_no;
    public int sort_order;

}


[Serializable]
public class jsonRivalConfirm
{
    public string result;
    public string err_code;
    public jsonChara chara1;
    public jsonChara chara2;
    public string canBattle;
    public string[] equip_infoP;
    public string[] equip_infoE;
    public int matchPt;
}

[Serializable]
public class jsonRivalBattle
{
    public string result;
    public string url;
}

[Serializable]
public class jsonMonsterList
{
    public string result;
    public string title;
    public Dictionary<int, string> flavor;
    public jsonMonsterListList list;

    public Dictionary<int, string> tab_list;
    public string field;
    public Dictionary<int, string> category_text;
}

[Serializable]
public class jsonMonsterListList
{
    public jsonMonsterListResultSet[] resultset;
    public int totalRows;
    public int totalPages;
}

[Serializable]
public class jsonMonsterListResultSet
{
    public int character_id;
    public int category;
    public string monster_no;
    public int rare_level;
    public int appearance_area;
    public string habitat;
    public string flavor_text;
    public int user_id;
    public string entry;
    public string race;
    public int name_id;
    public int graphic_id;
    public int grade_id;
    public int grade_pt;
    public int sally_sphere;
    public int exp;
    public int param_seed;
    public int hp;
    public int hp_max;
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
    public int level;
    public string mobility;
    public int move_pow;
    public int battle_brain;
    public int dtech1_id;
    public int reward_exp;
    public int reward_gold;
    public int normal_drop;
    public int rare_drop;
    public int srare_drop;
    public string terminate_at;
    public string monster_name;
    public jsonEquip[] equip;
    public string equip_info;
}

[Serializable]
public class jsonPurchase
{
    public string Store;
    public string TransactionID;
    public string Payload;
}

[Serializable]
public class jsonPurchasePayload
{
    public string json;
    public string signature;
    public string skuDetails;
}

[Serializable]
public class jsonPurchaseJson
{
    public string orderId;
    public string packageName;
    public string productId;
    public string purchaseTime;
    public string purchaseState;
    public string purchaseToken;
    public string acknowledged;
}

[Serializable]
public class jsonVcoinSend
{
    public string result;
    public int short_payment;
}

[Serializable]
public class jsonVcoinLog
{
    public string result;
    public List<jsonVcoinLogResult> resultset;

}

public class jsonVcoinLogResult
{
    public string reason;
    public int owner_id;
    public string name;
    public float amount;
    public string update_at;

}

[Serializable]
public class jsonVcoinApplyList

{
    public string result;
    public List<jsonVcoinApplyListResult> resultset;

}

public class jsonVcoinApplyListResult
{
    public int log_id;
    public int user_id;
    public string address;
    public float amount;
    public float fee;
    public string transaction;
    public int status;
    public string status_update_at;
    public string create_at;

}

[Serializable]
public class jsonAppsflyer

{
    public string result;
    public string err_code;
}


[Serializable]
public class jsonRaidMonster
{
    public string result;
    public jsonRaidMonsterList[] monsterlist;
    public jsonRaidDungeon raid_dungeon;
}


[Serializable]
public class jsonRaidMonsterList
{
    public jsonBRCapture monster;
    public jsonDefeatUser defeat_user;

}

[Serializable]
public class jsonDefeatUser
{
    public jsonChara avatar;
    public int point;
    public string create_at;
}

[Serializable]
public class jsonRaidRanking
{
    public string result;
    public jsonRaidRankingList[] rank_list;
    public jsonRaidDungeon raid_dungeon;
}


[Serializable]
public class jsonRaidRankingList
{
    public int raid_dungeon_id;
    public int user_id;
    public int total_point;
    public int rank;

    public jsonChara avatar;

}

[Serializable]
public class jsonMasterData
{
    public string result;
    public Dictionary<string, object> masters;
}
