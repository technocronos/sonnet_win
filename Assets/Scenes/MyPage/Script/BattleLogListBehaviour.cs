using MyScene;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleLogListBehaviour : MonoBehaviour
{
    public GameObject ListNone;
    public GameObject ListMember;
    public GameObject ListLoading;
    public GameObject Content;
    public GameObject BaseTab;
    public ScrollRect ScrollRect;

    jsonConstants constants;

    int page;
    int charaId;
    int tourId;
    string category;

    bool _isUpdate;
    int _totalPage;

    private GameObject objLoading;

    // Start is called before the first frame update
    public void Show(int _charaId)
    {
        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        page = 0;
        _isUpdate = true;
        _totalPage = 0;

        tourId = constants.Tournament_Master.TOUR_MAIN;
        charaId = _charaId;
        category = "challenge";

        ListClear();

        //APIをたたく
        APIConnectManager.Instance.BattleHistory(charaId, tourId, category, page, reload);

    }

    void reload(string json)
    {
        jsonBattleHistory response = JsonUtility.FromJson<jsonBattleHistory>(json);

        // 一つもなかったら...
        if (response.list.resultset.Length == 0)
        {
            // その旨のパネルを表示。
            ListNone.SetActive(true);
            Content.SetActive(false);

            // 処理はここまで。
            return;
        }

        jsonBattleHistoryResult[] list = response.list.resultset;
        _totalPage = response.list.totalPages;

        int i = 0;
        foreach (jsonBattleHistoryResult entry in list)
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
    void setupEntryBoard(jsonBattleHistoryResult entry, GameObject board)
    {
        //ユーザー名
        board.transform.Find("player_name").GetComponent<TextMeshProUGUI>().text = entry.rival_character_name;
        //レベル
        board.transform.Find("level").GetComponent<TextMeshProUGUI>().text = entry.rival_result.character.level.ToString();
        //階級名
        board.transform.Find("grade_name").GetComponent<TextMeshProUGUI>().text = entry.rival_ready.grade_name;

        //キャラ作成
        Image CharaImage = board.transform.Find("Avatar/Avatar/CharaImage").GetComponent<Image>();
        Main.Instance.makeCharaUI(entry.equip_info, CharaImage);

        //日時
        board.transform.Find("create_at").GetComponent<TextMeshProUGUI>().text = entry.create_at;

        //結果アイコン
        board.transform.Find("result_icom/win").GetComponent<Image>().enabled = false;
        board.transform.Find("result_icom/lose").GetComponent<Image>().enabled = false;
        board.transform.Find("result_icom/draw").GetComponent<Image>().enabled = false;
        board.transform.Find("result_icom/timeup").GetComponent<Image>().enabled = false;

        board.transform.Find("result_icom/" + entry.bias_status).GetComponent<Image>().enabled = true;

        //サマリー
        board.transform.Find("match_length").GetComponent<TextMeshProUGUI>().text = entry.result_detail.match_length + Utility.getText("TEXT_BATTLE_SUMMARY_KAISU");
        board.transform.Find("total_hurtP").GetComponent<TextMeshProUGUI>().text = entry.bias_result.summary.total_hurt.ToString();
        board.transform.Find("total_hurtE").GetComponent<TextMeshProUGUI>().text = entry.rival_result.summary.total_hurt.ToString();

        board.transform.Find("normal_hurtP").GetComponent<TextMeshProUGUI>().text = entry.bias_result.summary.normal_hurt.ToString();
        board.transform.Find("normal_hurtE").GetComponent<TextMeshProUGUI>().text = entry.rival_result.summary.normal_hurt.ToString();

        board.transform.Find("normal_hitsP").GetComponent<TextMeshProUGUI>().text = entry.bias_result.summary.normal_hits.ToString();
        board.transform.Find("normal_hitsE").GetComponent<TextMeshProUGUI>().text = entry.rival_result.summary.normal_hits.ToString();

        board.transform.Find("tact0P").GetComponent<TextMeshProUGUI>().text = entry.bias_result.summary.tact0.ToString();
        board.transform.Find("tact0E").GetComponent<TextMeshProUGUI>().text = entry.rival_result.summary.tact0.ToString();

        board.transform.Find("revenge_hurtP").GetComponent<TextMeshProUGUI>().text = entry.bias_result.summary.revenge_hurt.ToString();
        board.transform.Find("revenge_hurtE").GetComponent<TextMeshProUGUI>().text = entry.rival_result.summary.revenge_hurt.ToString();

        board.transform.Find("revenge_countP").GetComponent<TextMeshProUGUI>().text = entry.bias_result.summary.revenge_count.ToString();
        board.transform.Find("revenge_countE").GetComponent<TextMeshProUGUI>().text = entry.rival_result.summary.revenge_count.ToString();

        if (entry.bias_result.summary.revenge_attacks > 0)
        {
            board.transform.Find("revenge_hitsP").GetComponent<TextMeshProUGUI>().text = Mathf.Floor((entry.bias_result.summary.revenge_hits / entry.bias_result.summary.revenge_attacks * 100)) + "%";
        }
        else
        {
            board.transform.Find("revenge_hitsP").GetComponent<TextMeshProUGUI>().text = "---%";
        }
        if (entry.rival_result.summary.revenge_attacks > 0)
        {
            board.transform.Find("revenge_hitsE").GetComponent<TextMeshProUGUI>().text = Mathf.Floor((entry.rival_result.summary.revenge_hits / entry.rival_result.summary.revenge_attacks * 100)) + "%";
        }
        else
        {
            board.transform.Find("revenge_hitsE").GetComponent<TextMeshProUGUI>().text = "---%";
        }

        Button ButtonDetail = board.transform.Find("ButtonDetail").GetComponent<Button>();
        ButtonDetail.gameObject.SetActive(false);

        /*
        ButtonDetail.gameObject.SetActive(true);

        ButtonDetail.onClick.RemoveAllListeners();
        //対戦ボタンクリック時イベントハンドラ
        ButtonDetail.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySE("se_btn");

            SceneController.Instance.Jump("HisPage", (() =>
            {
                HisPageBehaviour _scene = FindObjectOfType<HisPageBehaviour>() as HisPageBehaviour;
                _scene.Param = new HisPageBehaviour.Parameter
                {
                    userId = entry.rival_user_id,
                };
            }));
        });
        */

        board.transform.Find("exp").GetComponent<TextMeshProUGUI>().text = entry.bias_result.gain.exp.ToString();
        board.transform.Find("gold").GetComponent<TextMeshProUGUI>().text = entry.bias_result.gain.gold.ToString();
        board.transform.Find("grade_nominal").GetComponent<TextMeshProUGUI>().text = entry.bias_result.gain.grade_nominal.ToString();
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
        APIConnectManager.Instance.BattleHistory(charaId, tourId, category, page, reload);
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

        APIConnectManager.Instance.BattleHistory(charaId, tourId, category, page, (string json) =>
        {
            jsonBattleHistory response = JsonUtility.FromJson<jsonBattleHistory>(json);

            jsonBattleHistoryResult[] add_list = response.list.resultset;

            // 追加レコードがある場合
            if (add_list.Length > 0)
            {
                int i = 0;
                foreach (jsonBattleHistoryResult entry in add_list)
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
        BaseTab.transform.Find("tab-challenge").GetComponent<Toggle>().isOn = true;

        ListClear();

        transform.gameObject.SetActive(false);
    }
}
