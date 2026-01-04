using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyScene;

public class QuestDramaBehaviour : BaseBehaviour
{

    public class Parameter
    {
        public int questId;
        public int placeId;
    }

    public Parameter Param;

    DramaBehaviour Drama { set; get; } = null;

    // Start is called before the first frame update
    protected override void Start()
    {
        //base.Start();

        Debug.Log("QuestDramaBehaviour Start...");

        SceneController.Instance.Init("Drama", (() =>
        {
            APIConnectManager.Instance.QuestDrama(Param.questId, Param.placeId, false, null, Show);
        }), Main.Instance.tapGuard);

        DispatchEvent(CwEvent.SCENE_READY);

    }

    jsonQuestDrama questdrama = null;

    public void Show(string json)
    {
        Debug.Log("QuestDramaBehaviour Show...");
        questdrama = JsonUtility.FromJson<jsonQuestDrama>(json);

        if (questdrama.errscene != null)
        {
            moveScene(questdrama.errscene);
        }
        else
        {
            StartCoroutine("setDrama");
        }
    }

    //dramaをDramaBehaviourにセットする
    private IEnumerator setDrama()
    {
        Debug.Log("QuestDramaBehaviour setDrama start..");

        Drama = FindObjectOfType<DramaBehaviour>() as DramaBehaviour;
        while (Drama == null)
        {
            Drama = FindObjectOfType<DramaBehaviour>() as DramaBehaviour;
            yield return new WaitForEndOfFrame();
        }

        Drama.drama = questdrama.drama;
        Drama.dramaId = questdrama.dramaId;
        // 終了時コールバック登録
        Drama.CompleteHandler += endCallback;
        Drama.Show();

        yield return null;
    }

    /// <summary>
    /// 終了時コールバック
    /// </summary>
    /// <param name="result"></param>
    void endCallback(string trailer)
    {
        //終了通知を出す
        APIConnectManager.Instance.QuestDrama(Param.questId, 0, true, trailer, End);
    }

    public void End(string json)
    {
        questdrama = JsonUtility.FromJson<jsonQuestDrama>(json);
        moveScene(questdrama.nextscene);
    }

    void moveScene(string nextscene)
    {
        Dictionary<string, string> sceneinfo = new Dictionary<string, string>();
        sceneinfo = Utility.ParseUrl(nextscene);

        AudioManager.Instance.StopBGM();

        switch (sceneinfo["scene"])
        {
            case "Quest":
                SceneController.Instance.Jump(sceneinfo["scene"]);
                break;
            case "Suggest":
                //SceneController.Instance.Jump(sceneinfo["scene"]);
                break;
            default:
                SceneController.Instance.Jump(sceneinfo["scene"]);
                break;
        }
    }

}
