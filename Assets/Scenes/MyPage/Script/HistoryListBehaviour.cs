using MyScene;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HistoryListBehaviour : MonoBehaviour
{
    public GameObject ListNone;
    public GameObject ListMember;
    public GameObject Content;
    public GameObject BaseTab;
    public GameObject TabMemberArea;
    public TextMeshProUGUI TextName;

    public GameObject ListLoading;

    public ScrollRect ScrollRect;

    int page;
    int userId;
    string category;

    readonly string type = "history";

    bool _isUpdate;
    int _totalPage;

    private GameObject objLoading;

    // Start is called before the first frame update
    public void Show(int _userId, string _player_name)
    {
        page = 0;
        userId = _userId;
        _isUpdate = true;
        category = "me";
        _totalPage = 0;

        ListClear();

        TextName.text = _player_name;

        //他人のページの場合は非表示
        if (userId != Header.Instance.GetSummary().chara.user_id)
        {
            BaseTab.transform.GetComponent<Image>().enabled = false;
            TabMemberArea.SetActive(false);

        }

        //APIをたたく
        APIConnectManager.Instance.HistoryList(userId, this.category, type, page, reload);

    }

    void reload(string json)
    {
        jsonHistoryList response = JsonUtility.FromJson<jsonHistoryList>(json);

        // 一つもなかったら...
        if (response.list.resultset.Length == 0)
        {
            // その旨のパネルを表示。
            ListNone.SetActive(true);
            Content.SetActive(false);

            // 処理はここまで。
            return;
        }
        jsonHistory[] list = response.list.resultset;
        _totalPage = response.list.totalPages;

        int i = 0;
        foreach (jsonHistory entry in list)
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

    //---------------------------------------------------------------------------------------------------------
    /**
     * 引数に指定されたエントリを、指定されたボードに表示するときに呼ばれる。
     */
    void setupEntryBoard(jsonHistory entry, GameObject board)
    {
        //ユーザー名
        board.transform.Find("player_name").GetComponent<TextMeshProUGUI>().text = entry.player_name;

        //キャラ作成
        Image CharaImage = board.transform.Find("Avatar/Avatar/CharaImage").GetComponent<Image>();
        Main.Instance.makeCharaUI(entry.equip_info, CharaImage);

        board.transform.Find("create_at").GetComponent<TextMeshProUGUI>().text = entry.create_at;
        board.transform.Find("history_id").GetComponent<TextMeshProUGUI>().text = "NO" + entry.history_id;

        board.transform.Find("history_log").GetComponent<TextMeshProUGUI>().text = Utility.getHistoryText(entry);

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

        page = 0;
        ListClear();

        //APIをたたく
        APIConnectManager.Instance.HistoryList(userId, category, type, page, reload);
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

        APIConnectManager.Instance.HistoryList(userId, category, type, page, (string json) =>
        {
            jsonHistoryList response = JsonUtility.FromJson<jsonHistoryList>(json);

            jsonHistory[] add_list = response.list.resultset;


            // 追加レコードがある場合
            if (add_list.Length > 0)
            {
                int i = 0;
                foreach (jsonHistory entry in add_list)
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

        //tab選択状態初期化
        BaseTab.transform.Find("tab-me").GetComponent<Toggle>().isOn = true;

        ListClear();

        transform.gameObject.SetActive(false);
    }
}
