using CreateWave;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MemberListBehaviour : MonoBehaviour
{
    public GameObject ListNone;
    public GameObject ListMember;
    public GameObject Content;
    public GameObject BaseTab;
    public GameObject TabMemberArea;
    public TextMeshProUGUI TextTabMemberArea;

    public GameObject ListLoading;

    public ScrollRect ScrollRect;

    int page;
    int userId;

    string category = "member";

    private bool _isUpdate = true;
    private int _totalPage = 0;

    private GameObject objLoading;

    // Start is called before the first frame update
    public void Show(int _userId, string _player_name)
    {
        page = 0;
        userId = _userId;

        _isUpdate = true;
        _totalPage = 0;

        category = "member";

        ListClear();

        //他人のページの場合は非表示
        if (userId != Header.Instance.GetSummary().chara.user_id)
        {
            BaseTab.SetActive(false);
            TabMemberArea.SetActive(true);
            TextTabMemberArea.text = _player_name;
        }
        else
        {
            BaseTab.SetActive(true);
            TabMemberArea.SetActive(false);
        }

        reload();
    }

    jsonGradeUser jsonParse(string json)
    {
        jsonGradeUser response = JsonUtility.FromJson<jsonGradeUser>(json);

        return response;
    }

    void reload()
    {
        if (category == "member")
        {
            //APIをたたく
            APIConnectManager.Instance.MemberList(userId, page, (string json) =>
            {
                jsonMemberList response = JsonUtility.FromJson<jsonMemberList>(json);

                // 一つもなかったら...
                if (response.list.resultset.Length == 0)
                {
                    // その旨のパネルを表示。
                    ListNone.SetActive(true);
                    Content.SetActive(false);

                    // 処理はここまで。
                    return;
                }
                jsonMemberResultSet[] list = response.list.resultset;
                _totalPage = response.list.totalPages;

                int i = 0;
                foreach (jsonMemberResultSet entry in list)
                {
                    GameObject board = null;

                    // リストを複製
                    board = UnityEngine.Object.Instantiate(ListMember, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
                    board.name = "ListUser" + i;

                    setupEntryBoard(entry, board);

                    board.SetActive(true);
                    i++;
                }
            });

        }
        else if (category == "search")
        {
            //APIをたたく
            APIConnectManager.Instance.MemberSearch((string json) =>
            {
                jsonMemberSearch response = JsonUtility.FromJson<jsonMemberSearch>(json);

                // 一つもなかったら...
                if (response.list.Length == 0)
                {
                    // その旨のパネルを表示。
                    ListNone.SetActive(true);
                    Content.SetActive(false);

                    // 処理はここまで。
                    return;
                }

                jsonMemberResultSet[] list = response.list;

                int i = 0;
                foreach (jsonMemberResultSet entry in list)
                {
                    GameObject board = null;

                    // リストを複製
                    board = UnityEngine.Object.Instantiate(ListMember, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
                    board.name = "ListUser" + i;

                    setupEntryBoard(entry, board);

                    board.SetActive(true);
                    i++;
                }
            });


        }
        else
        {
            //APIをたたく
            APIConnectManager.Instance.ApproachList(category, page, (string json) =>
            {

                jsonApproachList response = JsonUtility.FromJson<jsonApproachList>(json);

                if (response.list.resultset.Length == 0)
                {
                    // その旨のパネルを表示。
                    ListNone.SetActive(true);
                    Content.SetActive(false);

                    // 処理はここまで。
                    return;
                }
                jsonApproach[] list = response.list.resultset;
                _totalPage = response.list.totalPages;

                int i = 0;
                foreach (jsonApproach entry in list)
                {
                    GameObject board = null;

                    // リストを複製
                    board = UnityEngine.Object.Instantiate(ListMember, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
                    board.name = "ListUser" + i;

                    setupEntryBoardApploach(entry, board);

                    board.SetActive(true);
                    i++;
                }
            });

        }
    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * 引数に指定されたエントリを、指定されたボードに表示するときに呼ばれる。
     */
    void setupEntryBoard(jsonMemberResultSet entry, GameObject board)
    {
        //ユーザー名
        board.transform.Find("TextName").GetComponent<TextMeshProUGUI>().text = entry.player_name;
        //キャラ作成
        Image CharaImage = board.transform.Find("Avatar/Avatar/CharaImage").GetComponent<Image>();
        Main.Instance.makeCharaUI(entry.equip_info, CharaImage);

        board.transform.Find("TextLv").GetComponent<TextMeshProUGUI>().text = entry.chara.level.ToString();
        board.transform.Find("TextGrade").GetComponent<TextMeshProUGUI>().text = entry.grade.grade_name;

        Button ButtonDetail = board.transform.Find("ButtonDetail").GetComponent<Button>();

        if (Header.Instance.GetSummary().chara.user_id != entry.user_id)
        {
            ButtonDetail.gameObject.SetActive(true);

            //対戦ボタンクリック時イベントハンドラ
            ButtonDetail.onClick.AddListener(() =>
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
            ButtonDetail.interactable = false;
        }

        board.transform.Find("ButtonCancel").gameObject.SetActive(false);
        board.transform.Find("ButtonAccept").gameObject.SetActive(false);
        board.transform.Find("ButtonReject").gameObject.SetActive(false);

    }


    //---------------------------------------------------------------------------------------------------------
    /**
     * 引数に指定されたエントリを、指定されたボードに表示するときに呼ばれる。
     */
    void setupEntryBoardApploach(jsonApproach entry, GameObject board)
    {

        //キャラ作成
        Image CharaImage = board.transform.Find("Avatar/Avatar/CharaImage").GetComponent<Image>();
        Main.Instance.makeCharaUI(entry.equip_info, CharaImage);

        //ユーザー名
        board.transform.Find("TextName").GetComponent<TextMeshProUGUI>().text = entry.companion.player_name;

        board.transform.Find("TextLv").GetComponent<TextMeshProUGUI>().text = entry.companion.chara.level.ToString();
        board.transform.Find("TextGrade").GetComponent<TextMeshProUGUI>().text = entry.companion.grade.grade_name;

        Button ButtonDetail = board.transform.Find("ButtonDetail").GetComponent<Button>();
        Button ButtonCancel = board.transform.Find("ButtonCancel").GetComponent<Button>();
        Button ButtonAccept = board.transform.Find("ButtonAccept").GetComponent<Button>();
        Button ButtonReject = board.transform.Find("ButtonReject").GetComponent<Button>();

        ButtonDetail.gameObject.SetActive(true);

        if (Header.Instance.GetSummary().chara.user_id != entry.companion.user_id)
        {
            //対戦ボタンクリック時イベントハンドラ
            ButtonDetail.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySE("se_btn");

                SceneController.Instance.Jump("HisPage", (() =>
                {
                    HisPageBehaviour _scene = FindObjectOfType<HisPageBehaviour>() as HisPageBehaviour;
                    _scene.Param = new HisPageBehaviour.Parameter
                    {
                        userId = entry.companion.user_id,
                    };
                }));
            });
        }
        else
        {
            ButtonDetail.interactable = false;
        }

        if (category == "send")
        {

            if (entry.status == 0)
            {
                //キャンセルボタンクリック時イベントハンドラ
                ButtonCancel.gameObject.SetActive(true);
                ButtonCancel.onClick.AddListener(() =>
                {
                    AudioManager.Instance.PlaySE("se_btn");

                    var txt = Utility.getText("TEXT_MYPAGE_APPLY_CANCEL");

                    Main.Instance.showConfirm(txt, () =>
                    {
                        AudioManager.Instance.PlaySE("se_btn");

                        APIConnectManager.Instance.ApproachAct(entry.approach_id, "cancel", (string json) =>
                        {
                            jsonApproachAct approach = JsonUtility.FromJson<jsonApproachAct>(json);

                            if (approach.result == "ok")
                            {
                                APIConnectManager.Instance.Home((string json) =>
                                {
                                    HomeApi homeSummary = JsonUtility.FromJson<HomeApi>(json);

                                    //ヘッダーを更新する
                                    Header.Instance.SetSummary(homeSummary);
                                    Footer.Instance.SetSummary(homeSummary);

                                    ListClear();
                                    reload();
                                });
                            }
                            else if (approach.result == "error")
                            {
                                if (approach.opCode == "")
                                {
                                    Main.Instance.showDialogue(Utility.getText("TEXT_MYPAGE_NO_APPLY_ACTION"), null, 4);
                                }
                            }
                        });
                    });
                });

            }
            else
            {
                ButtonCancel.gameObject.SetActive(false);
                if (entry.status == 1)
                {
                    board.transform.Find("TextStatus").gameObject.SetActive(true);
                    board.transform.Find("TextStatus").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_MYPAGE_APPROVED");
                }
            }
        }
        else
        {
            //okボタンクリック時イベントハンドラ
            ButtonAccept.gameObject.SetActive(true);
            ButtonAccept.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySE("se_btn");

                var txt = Utility.getText("TEXT_MYPAGE_APPROVED_CONFIRM");

                Main.Instance.showConfirm(txt, () =>
                {
                    AudioManager.Instance.PlaySE("se_btn");

                    APIConnectManager.Instance.ApproachAct(entry.approach_id, "accept", (string json) =>
                    {
                        jsonApproachAct approach = JsonUtility.FromJson<jsonApproachAct>(json);

                        if (approach.result == "ok")
                        {
                            APIConnectManager.Instance.Home((string json) =>
                            {
                                HomeApi homeSummary = JsonUtility.FromJson<HomeApi>(json);

                                //ヘッダーを更新する
                                Header.Instance.SetSummary(homeSummary);
                                Footer.Instance.SetSummary(homeSummary);

                                ListClear();
                                reload();
                            });
                        }
                        else if (approach.result == "error")
                        {
                            if (approach.opCode == "")
                            {
                                Main.Instance.showDialogue(Utility.getText("TEXT_MYPAGE_NO_APPLY_ACTION"), null, 4);
                            }
                        }
                    });
                });
            });

            //キャンセルボタンクリック時イベントハンドラ
            ButtonReject.gameObject.SetActive(true);
            ButtonReject.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySE("se_btn");

                var txt = Utility.getText("TEXT_MYPAGE_REJECT_CONFIRM");

                Main.Instance.showConfirm(txt, () =>
                {
                    AudioManager.Instance.PlaySE("se_btn");

                    APIConnectManager.Instance.ApproachAct(entry.approach_id, "reject", (string json) =>
                    {
                        jsonApproachAct approach = JsonUtility.FromJson<jsonApproachAct>(json);

                        if (approach.result == "ok")
                        {
                            APIConnectManager.Instance.Home((string json) =>
                            {
                                HomeApi homeSummary = JsonUtility.FromJson<HomeApi>(json);

                                //ヘッダーを更新する
                                Header.Instance.SetSummary(homeSummary);
                                Footer.Instance.SetSummary(homeSummary);

                                ListClear();
                                reload();
                            });
                        }
                        else if (approach.result == "error")
                        {
                            if (approach.opCode == "")
                            {
                                Main.Instance.showDialogue(Utility.getText("TEXT_MYPAGE_NO_APPLY_ACTION"), null, 4);
                            }
                        }
                    });
                });
            });
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

        ListClear();

        reload();
    }

    /// <summary>
    /// スクロールで追加読み込み
    /// </summary>
    public void onScroll()
    {

        if (category == "member")
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
    }

    IEnumerator gotoNext()
    {
        float delayCount = 1.5f;

        yield return new WaitForSeconds(delayCount);

        APIConnectManager.Instance.MemberList(userId, page, (string json) =>
        {
            jsonMemberList response = JsonUtility.FromJson<jsonMemberList>(json);

            jsonMemberResultSet[] add_list = response.list.resultset;

            // 追加レコードがある場合
            if (add_list.Length > 0)
            {
                int i = 0;
                foreach (jsonMemberResultSet entry in add_list)
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

    public void onClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        BaseTab.transform.Find("tab-member").GetComponent<Toggle>().isOn = true;

        ListClear();

        transform.gameObject.SetActive(false);
    }
}
