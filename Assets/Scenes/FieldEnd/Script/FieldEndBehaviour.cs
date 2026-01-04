using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyScene;
using TMPro;

public class FieldEndBehaviour : BaseBehaviour
{
    public GameObject ListNone;
    public GameObject Content;
    public GameObject List;
    public GameObject NextQuestTitle;
    public GameObject NextButton;
    public GameObject Buttonback;
    public MissionBehaviour Mission;

    public TextMeshProUGUI TxtTurn;
    public TextMeshProUGUI TxtTerminate;
    public TextMeshProUGUI NaviText;
    public TextMeshProUGUI TextNextQuest;

    public TextMeshProUGUI CaptionTurn;
    public TextMeshProUGUI CuptionTerminate;


    public Image BG;

    jsonFieldEnd response;

    private jsonConstants constants;

    public class Parameter
    {
        public int sphereId;
    }

    public Parameter Param;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        BG.sprite = Utility.getAssetImage("Image/BG/circle_bg");

        //セーフエリア対応
        setSafearea("FieldEndCanvas");

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        List.SetActive(false);

        CaptionTurn.text = Utility.getText("TEXT_BATTLE_SUMMARY_TURN_NUM") + "：";
        CuptionTerminate.text = Utility.getText("TEXT_BATTLE_SUMMARY_DEFEATNUM") + "：";

        APIConnectManager.Instance.FieldEnd(Param.sphereId, onLoaded);
    }

    void onLoaded(string json)
    {
        //API結果受け取り
        response = JsonUtility.FromJson<jsonFieldEnd>(json);

        if (response.sphere_result == response.SPHERE_SUCCESS)
        {

            NaviText.text = Utility.getText("TEXT_NAVI_FIELD_CLEAR").Replace("{0}", response.quest.quest_name);

            //チュートリアルが終わってる場合、レビューしてもらう
            if (Header.Instance.GetSummary().tutorial_step >= constants.User_Info_Tutorial.TUTORIAL_END)
            {
                this.RequestReview();
            }
        }
        else if (response.sphere_result == response.SPHERE_ESCAPE)
        {
            NaviText.text = Utility.getText("TEXT_NAVI_FIELD_ESCAPE").Replace("{0}", response.quest.quest_name);
        }
        else if (response.sphere_result == response.SPHERE_FAILURE)
        {
            NaviText.text = Utility.getText("TEXT_NAVI_FIELD_FAILURE").Replace("{0}", response.quest.quest_name);
        }
        else if (response.sphere_result == response.SPHERE_GIVEUP)
        {
            NaviText.text = Utility.getText("TEXT_NAVI_FIELD_GIVEUP").Replace("{0}", response.quest.quest_name);
        }

        TxtTurn.text = response.summary.turn.ToString();
        TxtTerminate.text = response.summary.terminate.ToString();

        //ペナルティがある場合
        if ((response.sphere_result == response.SPHERE_FAILURE || response.sphere_result == response.SPHERE_GIVEUP) && response.quest.penalty_pt > 0)
        {
            //廃止
        }

        //ミッション達成の場合
        if (response.summary.mission != null && response.summary.mission.achieve)
        {
            Mission.gameObject.SetActive(true);
            Mission.Show(response.summary.mission);
        }
        else
        {
            Mission.gameObject.SetActive(false);
        }

        //次のクエストがある場合
        if (response.next != null && response.next.quest_id != 0)
        {
            NextQuestTitle.SetActive(true);
            TextNextQuest.text = response.next.quest_name;

            NextButton.SetActive(true);
        }
        else
        {
            NextQuestTitle.SetActive(false);
            NextButton.SetActive(false);

            //ボタンの位置を変更
            Vector3 pos = Buttonback.transform.localPosition;
            Buttonback.transform.localPosition = new Vector3(0, pos.y, 0);
        }

        // GETアイテムが一つもなかったら...
        if (response.treasures.Length == 0)
        {
            // その旨のパネルを表示。
            ListNone.SetActive(true);
            Content.SetActive(false);
        }
        else
        {
            foreach (jsonItems item in response.treasures)
            {
                // 複製
                GameObject board = UnityEngine.Object.Instantiate(List, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);

                board.SetActive(true);

                setupEntryBoard(item, board);
            }

            //コピー元は非表示
            List.SetActive(false);
        }

        DispatchEvent(CwEvent.SCENE_READY);

    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * 引数に指定されたエントリを、指定されたボードに表示するときに呼ばれる。
     */
    void setupEntryBoard(jsonItems entry, GameObject board)
    {
        board.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = entry.item_name;
        board.transform.Find("TextFlavor").GetComponent<TextMeshProUGUI>().text = entry.flavor_text;
        board.transform.Find("ItemIcon").GetComponent<Image>().sprite = Utility.getAssetImage(Utility.getItemIconURL(entry.item_id));
    }



    /// <summary>
    /// 次へボタンクリック時イベントハンドラ
    /// </summary>
    public void onNextClick()
    {
        AudioManager.Instance.PlaySE("se_btn");
        if (response.urlOnNext != null)
        {
            Dictionary<string, string> transUrl = new Dictionary<string, string>();
            transUrl = Utility.ParseUrl(response.urlOnNext);

            SceneController.Instance.Jump("QuestDrama", (() =>
            {
                QuestDramaBehaviour _drama = FindObjectOfType<QuestDramaBehaviour>() as QuestDramaBehaviour;
                _drama.Param = new QuestDramaBehaviour.Parameter
                {
                    questId = int.Parse(transUrl["questId"]),
                };
            }));
        }
    }

    public void onBackClick()
    {
        AudioManager.Instance.PlaySE("se_btn");

        //SceneController.Instance.Jump("Home");

        SceneController.Instance.Jump("Quest", (() =>
        {
            Quest _q = FindObjectOfType<Quest>() as Quest;
            _q.Param = new Quest.Parameter
            {
                panel = "QuestList"
            };
        }));
    }

}
