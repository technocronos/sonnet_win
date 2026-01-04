using MyScene;
using DG.Tweening;
using Newtonsoft.Json;
using Scenes.Common.Scripts;
using StateManager;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
public class Quest : BaseBehaviour
{
    public GameObject Map;

    public Image BG;

    public Sprite Point_B;
    public Sprite Point_R;

    public GameObject ButtonPanel;
    public GameObject MypagePanel;
    public GameObject EquipPanel;
    public GameObject ShopPanel;
    public GameObject BookPanel;

    public Button Button_Mypage;
    public Button Button_Equip;
    public Button Button_Shop;
    public Button Button_Book;

    //Imageをアタッチしておく
    public Image CaptionImage;
    public TextMeshProUGUI CaptionText;
    public Sprite Caption_S;
    public Sprite Caption_M;
    public Sprite Caption_L;

    //アニメーションしたいCharaImageをアタッチしておく
    public Image CharaImage;
    public Sprite cell0;//アニメーションセル1
    public Sprite cell1;//アニメーションセル2

    //アニメーションしたいCursorImageをアタッチしておく
    public Image CursorImage;

    //クエストリストのコピー元
    public GameObject ListSource;
    //クエストリストパネル
    public GameObject ListPanel;
    //確認ウィンドウ
    public GameObject ConfirmWin;

    public Text NormalTitleCanvas;
    public Text SpecialTitleCanvas;

    public GameObject SpecialTitlePanel;
    public GameObject SpBannar;
    public GameObject MonBannar;

    public GameObject Content;
    public GameObject ListGroup;

    public GameObject Arrow;
    public GameObject Arrow2;
    public GameObject Arrow3;

    private jsonQuest QuestInfo;
    private Sequence seq;
    private Sequence seqC;

    private int currRegion = 0;
    private int currPlace = 0;

    const float area_x_add = 0;
    const float area_y_add = 0;

    const float scale = 1f;

    const float cursor_x_add = 30;
    const float cursor_y_add = 33;

    const float chara_x_add = 40;
    const float chara_y_add = -20;

    const float caption_x_add = -1;
    const float caption_y_add = -70;

    const int questlisthide = 750;

    private List<jsonQuestList> questlist = new List<jsonQuestList>();

    public NaviController naviController;
    public NaviController naviController2;

    public GameObject QuestList;

    public Button questGiveup;

    jsonConstants constants;

    public class Parameter
    {
        public string panel = "QuestList";

    }

    public Parameter Param;


    public Button BtnGlobal;

    TouchManager _touch_manager { get; set; } = null;

    private float touchstartX { get; set; }
    private float touchstartY { get; set; }

    private float _x { get; set; }
    private float _y { get; set; }
    private float _touchX { get; set; } = 0;
    private float _touchY { get; set; } = 0;
    //タッチイベント用。actで操作した座標を格納しておく。
    public float _offsetX { get; set; } = 0;
    public float _offsetY { get; set; } = 0;
    public float offsetX { get; set; } = 0;
    public float offsetY { get; set; } = 0;

    public bool flick_lock { get; set; } = true;
    public string quest_title { get; set; } = "";

    public HomeApi homeSummary = null;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        BG.sprite = Utility.getAssetImage("Image/BG/bg2");

        //safearea対応
        setSafearea("QuestCanvas");

        constants = APIConnectManager.Instance.login.constants;

        QuestList.SetActive(false);

        //クエストリストはまだ非表示
        ListPanel.SetActive(false);
        //確認はまだ非表示
        ConfirmWin.SetActive(false);

        //spクエは非表示
        SpecialTitlePanel.SetActive(false);
        SpBannar.SetActive(false);
        MonBannar.SetActive(false);

        //ナビのタッチは無効にしておく
        naviController.TouchPanel.gameObject.SetActive(false);
        naviController.gameObject.SetActive(false);

        naviController2.TouchPanel.gameObject.SetActive(false);
        naviController2.gameObject.SetActive(false);

        Arrow.SetActive(false);

        ButtonPanel.SetActive(true);
        MypagePanel.SetActive(false);

        Button_Mypage.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySE("se_btn");
            MypagePanel.SetActive(true);
        });

        if(Param == null)
        {
            Param = new Quest.Parameter
            {
                panel = "QuestList"
            };            
        }

        APIConnectManager.Instance.Home(onLoaded);

        // タッチ管理マネージャ生成
        this._touch_manager = new TouchManager();

        // あるべき画面位置をチップ単位で表している変数を初期化。
        offsetX = 0;
        offsetY = 0;

        //タッチイベント用座標を初期化
        _offsetX = 0;
        _offsetY = 0;

        flick_lock = true;

    }

    private void onLoaded(string json)
    {
        //API結果受け取り
        homeSummary = JsonUtility.FromJson<HomeApi>(json);

        Header.Instance.SetSummary(homeSummary);

        APIConnectManager.Instance.QuestList(onStart);

    }

    private void onStart(string json)
    {
        //API結果受け取り
        QuestInfo = jsonToClass(json);

        this.currRegion = QuestInfo.currRegion;
        this.currPlace = QuestInfo.currPlace;

        Reload();

        switch (Param.panel) { 
            case "QuestList":
                QuestList.SetActive(true);
                showQuestList(QuestInfo.currRegion, QuestInfo.currPlace);
                break;
            case "MyPage":
                MypagePanel.SetActive(true);
                break;
        }

        //実行中クエストがある場合
        if (homeSummary.sally_quest.quest_id != 0)
        {
            //$("#bannar_panel").hide();
            //クエストやめボタンイベントハンドラ
            questGiveup.onClick.AddListener((() =>
            {
                AudioManager.Instance.PlaySE("se_btn");
                this.showGiveup(homeSummary.sally_quest);
            }));

            questGiveup.gameObject.SetActive(true);

        }
        else
        {
            questGiveup.gameObject.SetActive(false);
        }

        AudioManager.Instance.PlayBGM("bgm_menu", AudioManager.BGM_VOLUME_DEFULT);

        DispatchEvent(CwEvent.SCENE_READY);
    }

    jsonQuest jsonToClass(string json)
    {
        jsonQuest _QuestInfo = JsonUtility.FromJson<jsonQuest>(json);

        Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        foreach (KeyValuePair<string, object> keyvalue in jsonDict)
        {
            if (keyvalue.Key == "place")
            {
                Debug.Log(keyvalue.Value.ToString());
                Dictionary<int, jsonPlaceList[]> dicttmp = new Dictionary<int, jsonPlaceList[]>();

                Dictionary<int, object> dict1 = JsonConvert.DeserializeObject<Dictionary<int, object>>(keyvalue.Value.ToString());

                foreach (KeyValuePair<int, object> keyvalue2 in dict1)
                {
                    Debug.Log(keyvalue2.Value);

                    jsonPlaceList[] jsonplacelist;
                    jsonplacelist = JsonHelper.FromJson<jsonPlaceList>(keyvalue2.Value.ToString());

                    dicttmp.Add(keyvalue2.Key, jsonplacelist);
                }

                _QuestInfo.place = dicttmp;
            }
            else if (keyvalue.Key == "globalplace")
            {
                Debug.Log(keyvalue.Value.ToString());
                Dictionary<int, jsonPlaceList> dicttmp = new Dictionary<int, jsonPlaceList>();

                Dictionary<int, object> dict1 = JsonConvert.DeserializeObject<Dictionary<int, object>>(keyvalue.Value.ToString());

                foreach (KeyValuePair<int, object> keyvalue2 in dict1)
                {
                    Debug.Log(keyvalue2.Value);

                    jsonPlaceList jsonplacelist;
                    jsonplacelist = JsonUtility.FromJson<jsonPlaceList>(keyvalue2.Value.ToString());

                    dicttmp.Add(keyvalue2.Key, jsonplacelist);
                }

                _QuestInfo.globalplace = dicttmp;

            }
            else if (keyvalue.Key == "quest")
            {
                Debug.Log(keyvalue.Value.ToString());

                Dictionary<int, List<jsonQuestList[]>> dicttmp = new Dictionary<int, List<jsonQuestList[]>>();

                Dictionary<int, List<object>> dict1 = JsonConvert.DeserializeObject<Dictionary<int, List<object>>>(keyvalue.Value.ToString());

                foreach (KeyValuePair<int, List<object>> keyvalue2 in dict1)
                {
                    Debug.Log(keyvalue2.Value);

                    jsonQuestList[] jq;
                    List<jsonQuestList[]> lst = new List<jsonQuestList[]>();

                    foreach (object o in keyvalue2.Value)
                    {
                        Debug.Log(o.ToString());
                        jq = JsonHelper.FromJson<jsonQuestList>(o.ToString());
                        lst.Add(jq);
                    }
                    dicttmp.Add(keyvalue2.Key, lst);
                }

                _QuestInfo.quest = dicttmp;

            }
        }

        return _QuestInfo;
    }

    private void Update()
    {

        if (MypagePanel.active || EquipPanel.active || ShopPanel.active || BookPanel.active)
        {
            return;
        }

        // タッチ状態更新
        this._touch_manager.update();

        // タッチ取得
        TouchManager touch_state = this._touch_manager.getTouch();

        // タッチされていたら処理
        if (touch_state._touch_flag)
        {
            float touch_x = (touch_state._touch_position.x) / Screen.width * transform.GetComponent<RectTransform>().rect.width;
            float touch_y = (touch_state._touch_position.y) / Screen.height * transform.GetComponent<RectTransform>().rect.height;

            //flick_lockがかかってる場合はリターン
            if (flick_lock) return;


            // タッチした瞬間の処理
            if (touch_state._touch_phase == TouchPhase.Began)
            {
                //タッチ開始座標をとっておく
                touchstartX = touch_x;
                touchstartY = touch_y;

                //開始時点の_offset値をとっておく
                _x = _offsetX;
                _y = _offsetY;

            }
            else if (touch_state._touch_phase == TouchPhase.Moved)
            {

                this._touchX = _x + (touch_x - touchstartX) ;
                this._touchY = _y + (touch_y - touchstartY) ;

                if (this._touchX > 0)
                    this._touchX = 0;
                else if (this._touchX < (transform.GetComponent<RectTransform>().rect.width * -1))
                    this._touchX = transform.GetComponent<RectTransform>().rect.width * -1;

                if (this._touchY < 0)
                    this._touchY = 0;
                else if (this._touchY > transform.GetComponent<RectTransform>().rect.height)
                    this._touchY = transform.GetComponent<RectTransform>().rect.height;

                //フリックする
                onFlick();

                _offsetX = this._touchX;
                _offsetY = this._touchY;
            }
        }
    }

    public void onFlick()
    {
        if (touchstartX > 0 && touchstartY > 0)
        {
            Vector3 _stage = Map.transform.GetComponent<RectTransform>().anchoredPosition;

            //X座標可動範囲
            if ((_stage.x <= 0 && _stage.x >= transform.GetComponent<RectTransform>().rect.width * -1) && (_stage.y >= 0 && _stage.y <= transform.GetComponent<RectTransform>().rect.height))
            {
                Map.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(this._touchX, this._touchY, 0); 
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * メイン処理
     */
    private void Reload()
    {
        //ヘッダー・フッターに情報を渡す
        quest_title = this.currRegion != 0 ? QuestInfo.globalplace[currRegion].Name : Utility.getText("TEXT_GLOBALMAP");
        Header.Instance.SetTitle(quest_title);

        //マップを差し替える
        /*
        Image Map = transform.Find("QuestCanvas/MapPanel/Map").GetComponent<Image>();
        switch (this.currRegion)
        {
            case 0:
                Map.sprite = Utility.getAssetImage("Image/MoveMap/00");
                break;
            case 1:
                Map.sprite = Utility.getAssetImage("Image/MoveMap/01");
                break;
            case 2:
                Map.sprite = Utility.getAssetImage("Image/MoveMap/02");
                break;
            case 3:
                Map.sprite = Utility.getAssetImage("Image/MoveMap/03");
                break;
            case 4:
                Map.sprite = Utility.getAssetImage("Image/MoveMap/04");
                break;
            case 5:
                Map.sprite = Utility.getAssetImage("Image/MoveMap/05");
                break;
            case 6:
                Map.sprite = Utility.getAssetImage("Image/MoveMap/01");
                break;
        }
        */

        //コピー元表示
        Button Point = transform.Find("QuestCanvas/MapPanel/Map/PointPanel/Point").GetComponent<Button>();
        Point.GetComponent<Image>().enabled = true;

        //地点作成
        if (this.currRegion != 0)
        {
            int Key = 0;
            foreach (jsonPlaceList _placelist in QuestInfo.place[this.currRegion])
            {
                makeList(_placelist, Key);
                Key++;
            }
        }
        else
        {
            foreach (KeyValuePair<int, jsonPlaceList> keyvalue in QuestInfo.globalplace)
                makeList(keyvalue.Value, keyvalue.Key);
        }

        //現在いる位置をデフォルト表示する
        int x = 0;
        int y = 0;

        switch (this.currRegion)
        {
            case 1:
                x = 0;
                y = 1334;
                break;
            case 2:
                x = -619;
                y = 0;
                break;
            case 3:
                x = -1796;
                y = 716;
                break;
            case 4:
                x = -2372;
                y = 0;
                break;
        }

        _offsetX = x;
        _offsetY = y;
        Map.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y, 0);

        //コピー元非表示
        Point.GetComponent<Image>().enabled = false;

        //前面表示
        CharaImage.GetComponent<RectTransform>().SetAsLastSibling();
        CursorImage.GetComponent<RectTransform>().SetAsLastSibling();
        CaptionImage.GetComponent<RectTransform>().SetAsLastSibling();

    }

    private void makeList(jsonPlaceList jsl, int Key)
    {
        //地点を作成する
        Button Point = transform.Find("QuestCanvas/MapPanel/Map/PointPanel/Point").GetComponent<Button>();

        //地点を作成する親パネル
        Transform Parent = transform.Find("QuestCanvas/MapPanel/Map/PointPanel");

        //生成オブジェクトの位置と回転+親オブジェクトを指定
        float x = jsl.X * scale;
        float y = (jsl.Y * scale) * -1;
        Button point = Instantiate(Point, new Vector3(0, 0, 0), Quaternion.identity, Parent);
        point.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y, 0);
        point.name = "Point" + Key;

        // ボタンがクリックされたときのハンドラを登録
        int k = Key;
        point.onClick.RemoveAllListeners();
        point.onClick.AddListener((() => PointOnClick(k)));

        //回転アニメ
        point.transform.DOLocalRotate(new Vector3(0, 0, 360f), 6f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);

        //ローカルマップにいる場合
        if (this.currRegion != constants.Quest_Master.WILD_PLACE && Key == constants.Quest_Master.WILD_PLACE)
        {
            //最初の地点は青
            point.GetComponent<Image>().sprite = Point_B;
        }


        //現在いる地点
        if (Key == this.currPlace)
        {
            //Cursorの位置を設定する
            float x_cur = x + cursor_x_add;
            float y_cur = y + cursor_y_add;
            CursorImage.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(x_cur, y_cur, 0);

            //CursorのSequenceを宣言する
            seqC = DOTween.Sequence();
            seqC.Append(DOVirtual.DelayedCall(0.8f, () => CursorImage.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(x_cur, y_cur - 5, 0)));
            seqC.Append(DOVirtual.DelayedCall(0.8f, () => CursorImage.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(x_cur, y_cur + 5, 0)));
            seqC.SetLoops(-1, LoopType.Restart);//無限ループする

            //キャラの位置を設定する
            float x_c = x + chara_x_add;
            float y_c = y + chara_y_add;
            CharaImage.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(x_c, y_c, 0);

            //Sequenceを宣言する
            seq = DOTween.Sequence();
            seq.Append(DOVirtual.DelayedCall(0.8f, () => CharaImage.sprite = cell0));
            seq.Append(DOVirtual.DelayedCall(0.8f, () => CharaImage.sprite = cell1));
            seq.SetLoops(-1, LoopType.Restart);//無限ループする

            //キャプションを設定する
            setCaption(x, y, jsl.Name);

            //
            point.GetComponent<Image>().sprite = Point_R;
        }
    }

    private void PointOnClick(int key)
    {
        AudioManager.Instance.PlaySE("se_btn");

        if (this.currRegion != 0)
        {
            if (this.currPlace != key)
            {
                //ローカルマップの場合は地域名を切り替える
                onChangePlace(this.currRegion, key);
            }
            else
            {
                //クエストリストを出す。
                showQuestList(this.currRegion, this.currPlace);
            }
        }
        else
        {
            if (this.currPlace != key)
            {
                //グローバルマップの場合は選択された地域名を再度クリックしたらローカルマップに切り替え
                onChangePlace(this.currRegion, key);
            }
            else
            {
                //移動しようとしているマップがもともといる地域の場合はその地点を選択。その他は最初の地点を選択。
                if (this.currPlace == QuestInfo.currRegion)
                    onChangeRegion(QuestInfo.currRegion, QuestInfo.currPlace);
                else
                    onChangeRegion(this.currPlace, 0);
            }
        }

    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * キャプションを設定する
     */
    private void setCaption(float x, float y, string name)
    {
        //キャプションの位置を設定する
        float x_cap = x + caption_x_add;
        float y_cap = y + caption_y_add;
        CaptionImage.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(x_cap, y_cap, 0);

        //キャプションの大きさ
        if (name.Length <= 4)
        {
            CaptionImage.sprite = Caption_S;
        }
        else if (name.Length <= 8)
        {
            CaptionImage.sprite = Caption_M;
        }
        else
        {
            CaptionImage.sprite = Caption_L;
        }

        CaptionText.text = name;
    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * 地域をタップしたときのイベントハンドラ
     */
    private void onChangePlace(int currRegion, int currPlace)
    {
        Debug.Log("currRegion = " + currRegion);
        Debug.Log("currPlace = " + currPlace);

        //Debug.Log(QuestInfo.place[currRegion][currPlace]);

        //地域名を書き換え
        this.currRegion = currRegion;
        this.currPlace = currPlace;

        jsonPlaceList entry = null;
        if (currRegion != 0)
        {
            entry = QuestInfo.place[currRegion][currPlace];
        }
        else
        {
            entry = QuestInfo.globalplace[currPlace];
        }

        seqC.Kill();

        float x = entry.X * scale;
        float y = entry.Y * scale * -1;

        //Cursorの位置を設定する
        float x_cur = x + cursor_x_add;
        float y_cur = y + cursor_y_add;
        CursorImage.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(x_cur, y_cur, 0);

        //CursorのSequenceを宣言する
        seqC = DOTween.Sequence();
        seqC.Append(DOVirtual.DelayedCall(0.8f, () => CursorImage.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(x_cur, y_cur - 5, 0)));
        seqC.Append(DOVirtual.DelayedCall(0.8f, () => CursorImage.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(x_cur, y_cur + 5, 0)));
        seqC.SetLoops(-1, LoopType.Restart);//無限ループする

        //名前位置を変更
        setCaption(x, y, entry.Name);
    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * クエストリストを表示する
     */
    private void showQuestList(int region, int place)
    {
        Debug.Log("showQuestList region=" + currRegion + " place =" + place);

        List<GameObject> SpBannarList = new List<GameObject>();

        //コピー元をアクティブ
        ListSource.SetActive(true);

        //クエストリストを作成する
        Transform Parent = Content.transform;

        //ListGroupを画面外にしておく
        ListGroup.transform.localPosition = new Vector3(questlisthide, 0, 0);

        foreach (jsonQuestList quest in QuestInfo.quest[region][place])
        {
            if (quest.place_id == constants.Quest_Master.WILD_PLACE)
            {

                GameObject ObjMonBannar = Instantiate(MonBannar, new Vector3(0, 0, 0), Quaternion.identity, Parent);
                ObjMonBannar.transform.localPosition = new Vector3(0, 0, 0);

                //モンスターの洞窟
                SpecialTitlePanel.SetActive(true);

                string url;

                if (Header.Instance.GetSummary().raid_dungeon.status == constants.Raid_Dungeon.START) { 
                    url = Settings.Domain + "://" + Settings.Host + "/" + "img/parts/sp/bannar/b_q_" + quest.quest_id + "_2.png";
                    if (Main.Locale != LocalizationSettings.AvailableLocales.Locales[0])
                    {
                        url = Settings.Domain + "://" + Settings.Host + "/" + "img/parts/sp/bannar/b_q_" + quest.quest_id + "_2_en.png";
                    }
                }
                else { 
                    url = Settings.Domain + "://" + Settings.Host + "/" + "img/parts/sp/bannar/b_q_" + quest.quest_id + ".png";
                    if (Main.Locale != LocalizationSettings.AvailableLocales.Locales[0])
                    {
                        url = Settings.Domain + "://" + Settings.Host + "/" + "img/parts/sp/bannar/b_q_" + quest.quest_id + "_en.png";
                    }
                }

                StartCoroutine(GetTexture(ObjMonBannar.transform.Find("Bannar").GetComponent<Image>(), url));

                //クエスト実行ボタンイベントハンドラ
                ObjMonBannar.GetComponent<Button>().onClick.RemoveAllListeners();
                ObjMonBannar.GetComponent<Button>().onClick.AddListener((() =>
                {
                    AudioManager.Instance.PlaySE("se_btn");
                    showConfirm(quest, QuestInfo.sally_quest_id);
                }));

                ObjMonBannar.SetActive(true);
                SpBannarList.Add(ObjMonBannar);
            }
            else if (quest.place_id == constants.Quest_Master.EVENT_QUEST)
            {
                //生成オブジェクトの位置と回転+親オブジェクトを指定
                GameObject ObjSpBannar = Instantiate(SpBannar, new Vector3(0, 0, 0), Quaternion.identity, Parent);
                ObjSpBannar.transform.localPosition = new Vector3(0, 0, 0);

                //イベントキャプション
                SpecialTitlePanel.SetActive(true);

                string url = Settings.Domain + "://" + Settings.Host + "/" + "img/parts/sp/bannar/" + quest.quest_id + ".png";
                StartCoroutine(GetTexture(ObjSpBannar.transform.Find("Bannar").GetComponent<Image>(), url));

                //クエスト実行ボタンイベントハンドラ
                ObjSpBannar.GetComponent<Button>().onClick.RemoveAllListeners();
                ObjSpBannar.GetComponent<Button>().onClick.AddListener((() =>
                {
                    AudioManager.Instance.PlaySE("se_btn");
                    showConfirm(quest, QuestInfo.sally_quest_id);
                }));

                //表示
                ObjSpBannar.SetActive(true);

                SpBannarList.Add(ObjSpBannar);
            }
            else
            {

                //生成オブジェクトの位置と回転+親オブジェクトを指定
                GameObject list = Instantiate(ListSource, new Vector3(0, 0, 0), Quaternion.identity, Parent);
                list.transform.localPosition = new Vector3(0, 0, 0);

                //クエスト名
                list.transform.Find("QuestTitle").GetComponent<TextMeshProUGUI>().text = quest.quest_name;

                //ステータス作成
                var status = quest.status;

                //実行中クエの場合
                if (QuestInfo.sally_quest_id != 0 && quest.quest_id == QuestInfo.sally_quest_id)
                {
                    status = 4;
                }

                Sprite status_icon1 = Utility.getAssetImage(Utility.getStatusIcon(status));
                Image statusIcon = list.transform.Find("Status").GetComponent<Image>();
                statusIcon.sprite = status_icon1;

                if (status == 1 || status == 4)
                {
                    Sprite status_icon2 = Utility.getAssetImage(Utility.getStatusIcon(status, "_2"));

                    //Sequenceを宣言する
                    seq = DOTween.Sequence();
                    seq.Append(DOVirtual.DelayedCall(0.8f, () => statusIcon.sprite = status_icon1));
                    seq.Append(DOVirtual.DelayedCall(0.8f, () => statusIcon.sprite = status_icon2));
                    seq.SetLoops(-1, LoopType.Restart);//無限ループする
                }

                // ボタンがクリックされたときのハンドラを登録
                Button btn = list.transform.Find("BtnGo").GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener((() =>
                {
                    AudioManager.Instance.PlaySE("se_btn");
                    showConfirm(quest, QuestInfo.sally_quest_id);
                }));

            }
        }

        //タイトル
        NormalTitleCanvas.text = QuestInfo.place[region][place].Name;
        SpecialTitleCanvas.text = Utility.getText("TEXT_SPECIAL_QUEST");

        //コピー元は非表示
        ListSource.SetActive(false);

        //リストを表示
        ListPanel.SetActive(true);

        //spクエは前へ
        SpecialTitlePanel.GetComponent<RectTransform>().SetAsLastSibling();

        foreach (GameObject ObjSp in SpBannarList)
            ObjSp.GetComponent<RectTransform>().SetAsLastSibling();

        MonBannar.GetComponent<RectTransform>().SetAsLastSibling();

        //登場
        ListGroup.transform.DOLocalMove(new Vector3(0, 0, 0), 1.5f).SetEase(Ease.OutCubic);
        AudioManager.Instance.PlaySE("se_hover");

        //グローバルボタン表示フラグ反映
        if (!QuestInfo.showGlobal)
        {
            //BtnGlobal.interactable = false;
        }

        BtnGlobal.gameObject.SetActive(false);
        flick_lock = true;

        //チュートリアル中の場合
        if (Header.Instance.GetSummary().tutorial_step == constants.User_Info_Tutorial.TUTORIAL_MAINMENU)
        {
            Arrow3.SetActive(false);

            HomeApi summary = Header.Instance.GetSummary();

            summary.opening = Utility.getText("TEXT_NAVI_TUTORIAL_QUEST_1").Split("\n");

            summary.openingNum = summary.opening.Length;

            naviController.gameObject.SetActive(true);
            naviController.onStart(summary, null, TutorialNaviSpeakEnd);
        }

    }

    /// <summary>
    /// パネルを閉じる（flick_lockを解除）
    /// </summary>
    public void closePanel()
    {
        flick_lock = false;
    }

    /// <summary>
    /// ナビがしゃべり終わった
    /// </summary>
    void TutorialNaviSpeakEnd()
    {
        naviController.disappere();

        //ナビカーソルを表示する
        Arrow.SetActive(true);
        Arrow.GetComponent<ArrowBehaviour>().Show("down", 248, -310);

        Arrow2.SetActive(true);
        Arrow2.GetComponent<ArrowBehaviour>().Show("down", -104, 106);
    }

    private void showConfirm(jsonQuestList entry, int sally_quest_id)
    {

        ConfirmWin.SetActive(true);
        //クエスト名
        ConfirmWin.transform.Find("QuestTitle").GetComponent<TextMeshProUGUI>().text = entry.quest_name;

        //フレーバーテキスト
        ConfirmWin.transform.Find("flavor_text").GetComponent<TextMeshProUGUI>().text = entry.flavor_text;

        if (entry.quest_id == Header.Instance.GetSummary().raid_dungeon.quest_id && Header.Instance.GetSummary().raid_dungeon.status == constants.Raid_Dungeon.START)
        {
            ConfirmWin.transform.Find("flavor_text").GetComponent<TextMeshProUGUI>().text = entry.flavor_text + "\n\n" + Utility.getText("TEXT_HOME_RAID_STARTED2");
        }

        //クエストタイプ
        switch (entry.type)
        {
            case "FLD":
                ConfirmWin.transform.Find("quest_type").GetComponent<TextMeshProUGUI>().text = Utility.getText("CAPTION_FIELD");
                break;
            default:
                ConfirmWin.transform.Find("quest_type").GetComponent<TextMeshProUGUI>().text = Utility.getText("CAPTION_EVENT");
                break;
        }

        //推奨レベル
        if (entry.preferred_level != "" && entry.preferred_level != null)
            ConfirmWin.transform.Find("preferred_Image/preferred_level").GetComponent<TextMeshProUGUI>().text = entry.preferred_level;
        else
            ConfirmWin.transform.Find("preferred_Image/preferred_level").GetComponent<TextMeshProUGUI>().text = "---";

        //消費AP
        ConfirmWin.transform.Find("consume_pt").GetComponent<TextMeshProUGUI>().text = entry.consume_pt + "pt";

        //確認文言
        ConfirmWin.transform.Find("navispeak/navitext").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_HOME_QUEST_CONFIRM_DO");

        //ステータス作成
        var status = entry.status;

        //実行中クエの場合
        if (sally_quest_id != 0 && entry.quest_id == sally_quest_id)
        {
            status = 4;
        }

        Sprite status_icon1 = Utility.getAssetImage(Utility.getStatusIcon(status));
        Image statusIcon = ConfirmWin.transform.Find("Status").GetComponent<Image>();
        statusIcon.sprite = status_icon1;

        if (status == 1 || status == 4)
        {
            Sprite status_icon2 = Utility.getAssetImage(Utility.getStatusIcon(status, "_2"));

            //Sequenceを宣言する
            seq = DOTween.Sequence();
            seq.Append(DOVirtual.DelayedCall(0.8f, () => statusIcon.sprite = status_icon1));
            seq.Append(DOVirtual.DelayedCall(0.8f, () => statusIcon.sprite = status_icon2));
            seq.SetLoops(-1, LoopType.Restart);//無限ループする
        }

        Button btn_ok = ConfirmWin.transform.Find("BtnOk").GetComponent<Button>();
        Button btn_cancel = ConfirmWin.transform.Find("BtnCancel").GetComponent<Button>();

        //実行中クエがあり、それでない場合
        if (sally_quest_id != 0 && entry.quest_id != sally_quest_id)
        {
            ConfirmWin.transform.Find("navispeak/navitext").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_HOME_QUEST_OTHER_DO");
            btn_ok.interactable = false;
        }
        else
        {
            btn_ok.interactable = true;

            // OKボタンがクリックされたときのハンドラを登録
            btn_ok.onClick.RemoveAllListeners();
            btn_ok.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySE("se_btn");

                //実行中クエがあり、それの場合
                if (sally_quest_id != 0 && entry.quest_id == sally_quest_id)
                {
                    SceneController.Instance.Jump("Sphere", (() =>
                    {
                        SphereBehaviour _sphere = FindObjectOfType<SphereBehaviour>() as SphereBehaviour;
                        _sphere.Param = new SphereBehaviour.Parameter
                        {
                            sphereId = QuestInfo.sally_sphere,
                            reopen = "resume",
                        };

                        AudioManager.Instance.StopBGM();
                    }));
                }
                else
                {
                    if (entry.type == "FLD")
                    {
                        //準備画面へ
                        SceneController.Instance.Jump("Ready", (() =>
                        {
                            ReadyBehaviour ready = FindObjectOfType<ReadyBehaviour>() as ReadyBehaviour;
                            ready.Param = new ReadyBehaviour.Parameter
                            {
                                questId = entry.quest_id,
                                placeId = entry.place_id,
                                consume_pt = entry.consume_pt,
                                FromScene = "Quest",
                            };
                            AudioManager.Instance.StopBGM();
                        }));

                    }
                    else
                    {
                        //ドラマの場合は直接遷移
                        SceneController.Instance.Jump("QuestDrama", (() =>
                        {
                            QuestDramaBehaviour terminable = FindObjectOfType<QuestDramaBehaviour>() as QuestDramaBehaviour;
                            terminable.Param = new QuestDramaBehaviour.Parameter
                            {
                                questId = entry.quest_id,
                                placeId = entry.place_id,
                            };
                            AudioManager.Instance.StopBGM();
                        }));
                    }
                }

                ConfirmWin.SetActive(false);
            });
        }

        // cancelボタンがクリックされたときのハンドラを登録
        btn_cancel.onClick.RemoveAllListeners();
        btn_cancel.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySE("se_btn");
            ConfirmWin.SetActive(false);
        });
    }
    //---------------------------------------------------------------------------------------------------------
    /**
     * クエストリストを閉じる
     */
    public void closeQuestList()
    {
        Debug.Log("closeQuestList run..");
        AudioManager.Instance.PlaySE("se_btn");

        AudioManager.Instance.PlaySE("se_hover");

        //退場
        ListGroup.transform.DOLocalMove(new Vector3(questlisthide, 0, 0), 0.7f).SetEase(Ease.OutCubic).OnComplete((() =>
        {
            Transform Parent = Content.transform;
            foreach (Transform n in Parent.transform)
            {
                //コピー元だけ残して全部削除
                if (n.name != "List" && n.name != "SpecialTitlePanel" && n.name != "SpBannar" && n.name != "MonBannar" && n.name != "TitlePanel")
                    GameObject.Destroy(n.gameObject);
            }
            //リストを非表示
            ListPanel.SetActive(false);

            flick_lock = false;

            //spクエは非表示
            SpecialTitlePanel.SetActive(false);
            SpBannar.SetActive(false);
            MonBannar.SetActive(false);

            //ナビのタッチは無効にしておく
            naviController.TouchPanel.gameObject.SetActive(false);
            naviController.gameObject.SetActive(false);
            naviController2.TouchPanel.gameObject.SetActive(false);
            naviController2.gameObject.SetActive(false);

            //チュートリアル中の場合
            if (Header.Instance.GetSummary().tutorial_step == constants.User_Info_Tutorial.TUTORIAL_MAINMENU)
            {
                Arrow.SetActive(false);
                Arrow2.SetActive(false);


                HomeApi summary = Header.Instance.GetSummary();

                summary.opening = Utility.getText("TEXT_NAVI_TUTORIAL_QUEST_2").Split("\n");

                summary.openingNum = summary.opening.Length;

                naviController2.gameObject.SetActive(true);
                naviController2.onStart(summary, null, TutorialNaviSpeakEnd2);

            }
        }));
    }

    /// <summary>
    /// ナビがしゃべり終わった
    /// </summary>
    void TutorialNaviSpeakEnd2()
    {
        naviController2.disappere();

        //ナビカーソルを表示する
        Arrow3.SetActive(true);

        //地点を作成する親パネル
        Transform Parent = transform.Find("QuestCanvas/MapPanel/Map/PointPanel");
        Transform point = Parent.Find("Point0");

        Vector3 pos = point.GetComponent<RectTransform>().anchoredPosition;

        Arrow3.GetComponent<ArrowBehaviour>().Show("down", pos.x, pos.y + 130);
    }

    public void onClickGlobal()
    {
        AudioManager.Instance.PlaySE("se_btn");

        int TUTORIAL_GLOBALMAP = PlayerPrefs.GetInt(Settings.TUTORIAL_GLOBALMAP, 0);

        if (TUTORIAL_GLOBALMAP == 0)
        {

            HomeApi summary = Header.Instance.GetSummary();

            summary.opening = Utility.getText("TEXT_NAVI_TUTORIAL_QUEST_3").Split("\n");

            summary.openingNum = summary.opening.Length;

            naviController2.gameObject.SetActive(true);
            naviController2.onStart(summary, null, () =>
            {
                naviController2.disappere();
                //二度と表示しない
                PlayerPrefs.SetInt(Settings.TUTORIAL_GLOBALMAP, 1);
            });
        }

        this.onChangeRegion(0, this.currRegion);
    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * マップを切り替える
     */
    private void onChangeRegion(int changeRegion, int currPlace)
    {

        Debug.Log("changeRegion = " + changeRegion);
        Debug.Log("currPlace = " + currPlace);

        //地域名を書き換え
        this.currRegion = changeRegion;
        this.currPlace = currPlace;

        //グローバルボタン表示切替
        if (this.currRegion == constants.Quest_Master.WILD_PLACE)
        {
            //BtnGlobal.gameObject.SetActive(false);
        }
        else
        {
            //BtnGlobal.gameObject.SetActive(true);
        }

        Transform Parent = transform.Find("QuestCanvas/MapPanel/Map/PointPanel");
        foreach (Transform n in Parent.transform)
        {
            //コピー元だけ残して全部削除
            if (n.name != "Point" && n.name != "Cursor" && n.name != "Chara" && n.name != "Caption" && n.name != "Arrow")
                GameObject.Destroy(n.gameObject);
        }

        seq.Kill();
        seqC.Kill();

        //questlistAPIを再度たたく
        APIConnectManager.Instance.QuestList(((string json) =>
        {
            //API結果受け取り
            QuestInfo = jsonToClass(json);

            Reload();
        }));
    }
    //---------------------------------------------------------------------------------------------------------
    /**
     * クエスト実行確認ポップアップを立ち上げる
     */
    void showGiveup(jsonQuestList entry)
    {
        string text = Utility.getText("TEXT_HOME_QUEST_CONFIRM_GIVEUP").Replace("{0}", entry.quest_name);

        Main.Instance.showConfirm(text, (() =>
        {
            //二度押しは効かない
            if (SceneController.Instance.SceneName == "FieldEnd")
                return;

            AudioManager.Instance.PlaySE("se_btn");

            //ギブアップをする
            APIConnectManager.Instance.FieldReopen("1", ((string json) =>
            {
                //API結果受け取り
                jsonFieldReopen results = JsonUtility.FromJson<jsonFieldReopen>(json);
                if (results.result == "ok")
                {
                    SceneController.Instance.Jump("FieldEnd", (() =>
                    {
                        FieldEndBehaviour _fieldend = FindObjectOfType<FieldEndBehaviour>() as FieldEndBehaviour;
                        _fieldend.Param = new FieldEndBehaviour.Parameter
                        {
                            sphereId = homeSummary.chara.sally_sphere,
                        };
                    }));
                }
            }));
        }));
    }

    protected override void OnDestroy()
    {
        seq.Kill();
        seqC.Kill();
        base.OnDestroy();
    }
}
