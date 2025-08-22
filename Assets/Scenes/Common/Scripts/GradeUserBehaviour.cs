using CreateWave;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GradeUserBehaviour : MonoBehaviour
{
    public GameObject ListNone;
    public GameObject ListUser;
    public GameObject Content;

    public GameObject ListLoading;

    public TextMeshProUGUI TextNavi;

    public ScrollRect ScrollRect;

    jsonChara[] list { set; get; }
    jsonGrade grade { set; get; }

    int page { set; get; }
    int gradeId { set; get; }

    private bool _isUpdate { set; get; }
    private int _totalPage { set; get; }

    private GameObject objLoading { set; get; }

    // Start is called before the first frame update
    public void Show(int _gradeId)
    {
        gradeId = _gradeId;
        page = 0;

        _isUpdate = true;
        _totalPage = 0;

        //テンプレート非表示
        ListNone.gameObject.SetActive(false);
        ListUser.gameObject.SetActive(false);
        Content.SetActive(true);
        ListLoading.SetActive(false);

        ListClear();

        //APIをたたく
        APIConnectManager.Instance.GradeUser(gradeId, page, reload);
    }

    jsonGradeUser jsonParse(string json)
    {
        jsonGradeUser response = JsonUtility.FromJson<jsonGradeUser>(json);

        return response;
    }
    void reload(string json)
    {
        jsonGradeUser response = jsonParse(json);

        list = response.list.resultset;
        grade = response.grade;

        _totalPage = response.list.totalPages;

        TextNavi.text = Utility.getText("TEXT_NAVI_GRADE_EXPLAIN").Replace("{0}", grade.grade_name);

        // GETアイテムが一つもなかったら...
        if (list.Length == 0)
        {
            // その旨のパネルを表示。
            ListNone.SetActive(true);
            Content.SetActive(false);

            // 処理はここまで。
            return;
        }

        int i = 0;
        foreach (jsonChara entry in list)
        {
            GameObject board = null;

            // リストを複製
            board = UnityEngine.Object.Instantiate(ListUser, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
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
    void setupEntryBoard(jsonChara entry, GameObject board)
    {

        //ユーザー名
        board.transform.Find("TextName").GetComponent<TextMeshProUGUI>().text = entry.player_name;
        //キャラ作成
        Image CharaImage = board.transform.Find("Avatar/Avatar/CharaImage").GetComponent<Image>();
        Main.Instance.makeCharaUI(entry.equip_info, CharaImage);

        board.transform.Find("TextMember").GetComponent<TextMeshProUGUI>().text = entry.member.ToString();
        board.transform.Find("TextLv").GetComponent<TextMeshProUGUI>().text = entry.level.ToString();
        board.transform.Find("TextGrade").GetComponent<TextMeshProUGUI>().text = grade.grade_name;

        // HPゲージを更新。
        int _hp = entry.hp;
        float _hp_max = entry.hp_max;
        float hp_val = Mathf.Min(_hp, _hp_max);

        Transform gauge = board.transform.Find("HPGauge/hp_gauge_bar/gauge");
        float gauge_width = gauge.GetComponent<RectTransform>().rect.width; ;

        int posx = (int)(((hp_val * 1.0f) / _hp_max) * gauge_width);
        gauge.transform.localPosition = new Vector3(posx - gauge_width, 0, 0);

        board.transform.Find("HPGauge/hp").GetComponent<TextMeshProUGUI>().text = _hp.ToString();
        board.transform.Find("HPGauge/max").GetComponent<TextMeshProUGUI>().text = _hp_max.ToString();

        board.transform.Find("StatusPanel/att1").GetComponent<TextMeshProUGUI>().text = entry.total_attack1.ToString();
        board.transform.Find("StatusPanel/att2").GetComponent<TextMeshProUGUI>().text = entry.total_attack2.ToString();
        board.transform.Find("StatusPanel/att3").GetComponent<TextMeshProUGUI>().text = entry.total_attack3.ToString();
        board.transform.Find("StatusPanel/spd").GetComponent<TextMeshProUGUI>().text = entry.total_speed.ToString();

        board.transform.Find("StatusPanel/def1").GetComponent<TextMeshProUGUI>().text = entry.total_defence1.ToString();
        board.transform.Find("StatusPanel/def2").GetComponent<TextMeshProUGUI>().text = entry.total_defence2.ToString();
        board.transform.Find("StatusPanel/def3").GetComponent<TextMeshProUGUI>().text = entry.total_defence3.ToString();
        board.transform.Find("StatusPanel/defX").GetComponent<TextMeshProUGUI>().text = entry.total_defenceX.ToString();

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

        APIConnectManager.Instance.GradeUser(gradeId, page, (string json) =>
        {
            jsonGradeUser response = jsonParse(json);

            jsonChara[] add_list = response.list.resultset;

            int i = 0;
            foreach (jsonChara entry in add_list)
            {
                GameObject board = null;

                // リストを複製
                board = UnityEngine.Object.Instantiate(ListUser, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
                board.name = "ListUser" + i;

                setupEntryBoard(entry, board);

                board.SetActive(true);
                i++;
            }

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

        TextNavi.text = "";
        foreach (Transform n in Content.transform)
        {
            //テンプレート以外は全部削除
            if (n.name != ListUser.name && n.name != ListNone.name && n.name != ListLoading.name)
                GameObject.Destroy(n.gameObject);
        }
    }

    public void onClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        transform.gameObject.SetActive(false);
    }
}
