using CreateWave;
using Scenes.Common.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleEventBehaviour : BaseBehaviour
{
    public TextMeshProUGUI NaviText;
    public GameObject ListNone;
    public GameObject ListMember;
    public GameObject Content;
    public GameObject BaseTab;

    public Toggle Tab_WEEKLY;
    public Toggle Tab_DAYLY;

    public TextMeshProUGUI Text_Tab_WEEKLY;
    public TextMeshProUGUI Text_Tab_DAYLY;

    public TextMeshProUGUI CaptionGrade;
    public TextMeshProUGUI CaptionWeeklyHighestRank;

    public Image BG;

    public GameObject ListLoading;

    public ScrollRect ScrollRect;

    jsonBattleRanking response;

    int count;
    int page;
    int userId;
    string category;

    bool _isUpdate;
    int _totalPage;

    private GameObject objLoading;

    jsonBattleRankingResultSet[] list { get; set; }

    // Start is called before the first frame update
    protected override void Start()
    {
        BG.sprite = Utility.getAssetImage("Image/BG/bg2");

        count = 10;
        page = 0;

        _isUpdate = true;
        category = "12";
        _totalPage = 0;

        CaptionGrade.text = Utility.getText("TEXT_GRADE_POINT_2");
        CaptionWeeklyHighestRank.text = Utility.getText("TEXT_WEEKLY_HIGHEST_RANK");

        // まずは現在のリストを空に。
        ListClear();


        //APIをたたく
        APIConnectManager.Instance.BattleRanking(category, count, page, onStart);

        DispatchEvent(CwEvent.SCENE_READY);
    }

    void onStart(string json)
    {
        response = JsonUtility.FromJson<jsonBattleRanking>(json);


        DateTime begin = Utility.GetDateTime(response.term.begin);
        DateTime end = Utility.GetDateTime(response.term.end);

        DateTime start_date = Utility.GetDateTime(response.rankinfo.start_date);
        DateTime result_date = Utility.GetDateTime(response.rankinfo.result_date - 1);

        Tab_WEEKLY.interactable = true;
        Tab_DAYLY.interactable = true;

        string datef = Utility.getText("TEXT_DATE_FORMAT");

        if (category == "12")
        {
            //曜日限定開催の場合
            if (response.rankinfo.start_date != 0)
            {

                //開催中
                if (response.rankinfo.status == 1)
                {
                    if (response.rankinfo.in_aggregate == true)
                    {
                        Tab_WEEKLY.interactable = false;
                        Tab_DAYLY.interactable = false;

                        Tab_WEEKLY.gameObject.SetActive(false);
                        Tab_DAYLY.gameObject.SetActive(false);

                        response.list.resultset = null;
                        NaviText.text = Utility.getText("BATTLEEVENT_STARTED").Replace("{0}", start_date.ToString(datef)).Replace("{1}", result_date.ToString(datef));
                    }
                    else
                    {
                        NaviText.text = Utility.getText("BATTLEEVENT_RANKINGLIST").Replace("{0}", start_date.ToString(datef)).Replace("{1}", result_date.ToString(datef));
                    }

                    //結果発表
                }
                else if (response.rankinfo.status == 2)
                {
                    if (response.rankinfo.in_aggregate == true)
                    {
                        Tab_WEEKLY.interactable = false;
                        Tab_DAYLY.interactable = false;
                        Tab_WEEKLY.gameObject.SetActive(false);
                        Tab_DAYLY.gameObject.SetActive(false);

                        response.list.resultset = null;
                        NaviText.text = Utility.getText("BATTLEEVENT_TOTALLING").Replace("{0}", start_date.ToString(datef)).Replace("{1}", result_date.ToString(datef));
                    }
                    else
                    {
                        NaviText.text = Utility.getText("BATTLEEVENT_RESULT").Replace("{0}", start_date.ToString(datef)).Replace("{1}", result_date.ToString(datef));
                    }
                    //非開催 or 2日前
                }
                else if (response.rankinfo.status == 3 || response.rankinfo.status == 4)
                {
                    Tab_WEEKLY.interactable = false;
                    Tab_DAYLY.interactable = false;
                    Tab_WEEKLY.gameObject.SetActive(false);
                    Tab_DAYLY.gameObject.SetActive(false);

                    response.list.resultset = null;
                    NaviText.text = Utility.getText("BATTLEEVENT_NOT_START").Replace("{0}", start_date.ToString(datef)).Replace("{1}", result_date.ToString(datef));
                }
                //常時開催の場合
            }
            else
            {
                NaviText.text = Utility.getText("BATTLEEVENT_RANKINGLIST2").Replace("{0}", start_date.ToString(datef)).Replace("{1}", result_date.ToString(datef));
            }
        }
        else
        {
            //曜日限定開催の場合
            if (response.rankinfo.start_date != 0)
            {
                if (response.rankinfo.in_aggregate == true)
                {
                    Tab_WEEKLY.interactable = false;
                    Tab_DAYLY.interactable = false;

                    response.list.resultset = null;
                }
            }

            NaviText.text = Utility.getText("BATTLEEVENT_RANKINGLIST3").Replace("{0}", begin.ToString(datef));
        }

        //タブに文言追加
        int lang = PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        if(lang == 0)
            Text_Tab_WEEKLY.text = start_date.ToString("dd日") + "～" + result_date.ToString("dd日");
        else
            Text_Tab_WEEKLY.text = start_date.ToString("dd") + "-" + result_date.ToString("dd");

        Text_Tab_DAYLY.text = response.period.daily.ToString();

        list = response.list.resultset;

        _totalPage = response.list.totalPages;

        // 一つもなかったら...
        if (list == null)
        {
            // その旨のパネルを表示。
            ListNone.SetActive(true);
            Content.SetActive(false);

            // 処理はここまで。
            return;
        }
        else if (list.Length == 0)
        {
            // その旨のパネルを表示。
            ListNone.SetActive(true);
            Content.SetActive(false);

            // 処理はここまで。
            return;
        }



        int i = 0;
        foreach (jsonBattleRankingResultSet entry in list)
        {
            GameObject board = null;

            // リストを複製
            board = UnityEngine.Object.Instantiate(ListMember, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
            board.name = "ListMember" + i;

            setupEntryBoard(entry, board);

            board.SetActive(true);
            i++;
        }

    }



    //---------------------------------------------------------------------------------------------------------
    /**
     * 引数に指定されたエントリを、指定されたボードに表示するときに呼ばれる。
     */
    void setupEntryBoard(jsonBattleRankingResultSet entry, GameObject board)
    {

        //ランク順位
        board.transform.Find("rank").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_RANK").Replace("{0}", entry.rank.ToString());
        //階級ポイント
        board.transform.Find("point").GetComponent<TextMeshProUGUI>().text = entry.point.ToString();

        //ユーザー名
        board.transform.Find("player_name").GetComponent<TextMeshProUGUI>().text = entry.avatar.player_name;

        //キャラ作成
        Image CharaImage = board.transform.Find("Avatar/Avatar/CharaImage").GetComponent<Image>();
        Main.Instance.makeCharaUI(entry.avatar.equip_info, CharaImage);

        board.transform.Find("level").GetComponent<TextMeshProUGUI>().text = entry.avatar.level.ToString();
        board.transform.Find("grade").GetComponent<TextMeshProUGUI>().text = entry.avatar.grade.grade_name;

        //最高順位
        if (category == "12")
        {
            if (entry.highest.weekly != 0)
                board.transform.Find("highest").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_RANK").Replace("{0}", entry.highest.weekly.ToString());
            else
                board.transform.Find("highest").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_RANK").Replace("{0}", "--");
        }
        else
        {
            if (entry.highest.daily != 0)
                board.transform.Find("highest").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_RANK").Replace("{0}", entry.highest.daily.ToString());
            else
                board.transform.Find("highest").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_RANK").Replace("{0}", "--");
        }

        Button ButtonBattle = board.transform.Find("ButtonBattle").GetComponent<Button>();
        ButtonBattle.onClick.RemoveAllListeners();

        if (Header.Instance.GetSummary().chara.user_id != entry.user_id)
        {

            //対戦ボタンクリック時イベントハンドラ
            ButtonBattle.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySE("se_btn");

                SceneController.Instance.Jump("HisPage", (() =>
                {
                    HisPageBehaviour _scene = FindObjectOfType<HisPageBehaviour>() as HisPageBehaviour;
                    _scene.Param = new HisPageBehaviour.Parameter
                    {
                        userId = entry.user_id,
                    };
                }));
            });
        }
        else
        {
            ButtonBattle.gameObject.SetActive(false);
        }

    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * タブ切り替え時に呼び出される。
     */
    public void onChangeTab(string _category)
    {
        if (this.category == _category)
            return;

        AudioManager.Instance.PlaySE("se_btn");

        this.category = _category;

        _isUpdate = true;
        _totalPage = 0;
        page = 0;

        ListClear();

        //APIをたたく
        APIConnectManager.Instance.BattleRanking(category, count, page, onStart);
    }

    public void onRankingHelp()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.PopUp("HelpDetail", () =>
        {
            HelpDetailBehaviour helpdetail = FindObjectOfType<HelpDetailBehaviour>() as HelpDetailBehaviour;
            helpdetail.Param = new HelpDetailBehaviour.Parameter { id = "other-ranking" };
        });
    }
    /// <summary>
    /// スクロールで追加読み込み
    /// </summary>
    public void onScroll()
    {

        if (ScrollRect.verticalNormalizedPosition < -0.02f)
        {
            //Debug.Log(ScrollRect.verticalNormalizedPosition);
            if (_isUpdate)
            {
                _isUpdate = false;
                if (page < _totalPage)
                {
                    page++;

                    //loadingを出す
                    objLoading = UnityEngine.Object.Instantiate(ListLoading, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
                    objLoading.name = "ListLoading" + page;
                    objLoading.transform.localPosition = new Vector3(0, 0, 0);

                    objLoading.SetActive(true);

                    StartCoroutine(gotoNext());
                }
            }
        }
    }

    IEnumerator gotoNext()
    {
        float delayCount = 1.5f;

        yield return new WaitForSeconds(delayCount);

        APIConnectManager.Instance.BattleRanking(category, count, page, (string json) =>
        {
            response = JsonUtility.FromJson<jsonBattleRanking>(json);

            jsonBattleRankingResultSet[] add_list = response.list.resultset;


            // 追加レコードがある場合
            if (add_list.Length > 0)
            {
                int i = 0;
                foreach (jsonBattleRankingResultSet entry in add_list)
                {
                    GameObject board = null;

                    // リストを複製
                    board = UnityEngine.Object.Instantiate(ListMember, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
                    board.name = "ListUser" + i;

                    setupEntryBoard(entry, board);

                    board.SetActive(true);
                    i++;
                }
            }

            //loadingを消す
            foreach (Transform n in Content.transform)
            {
                if (n.name == objLoading.name)
                    GameObject.Destroy(n.gameObject);
            }

            _isUpdate = true;

        });
    }

    void ListClear()
    {

        //テンプレート非表示
        ListNone.gameObject.SetActive(false);
        ListMember.gameObject.SetActive(false);
        Content.SetActive(true);
        ListLoading.SetActive(false);

        foreach (Transform n in Content.transform)
        {
            //テンプレート以外は全部削除
            if (n.name != ListMember.name && n.name != ListNone.name && n.name != ListLoading.name)
                GameObject.Destroy(n.gameObject);
        }
    }

    public void TapClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.ClosePopUpName("BattleEvent");
    }
}
