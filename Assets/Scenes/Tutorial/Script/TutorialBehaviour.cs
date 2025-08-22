using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreateWave;

public class TutorialBehaviour : BaseBehaviour
{
    public class Parameter
    {
        public int TutorialStep;
    }

    public Parameter Param;

    DramaBehaviour Drama = null;

    jsonConstants constants;

    // Start is called before the first frame update
    protected override void Start()
    {
        //base.Start();

        Debug.Log("TutorialBehaviour Start...");

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        SceneController.Instance.Init("Drama", (() =>
        {

            string param = "";

            if (this.Param.TutorialStep != constants.User_Info_Tutorial.TUTORIAL_BATTLE)
            {
                //チュートリアルを再生したことで終了とみなす場合は終了通知を送る
                param = "end=" + this.Param.TutorialStep;
            }
            APIConnectManager.Instance.tutorial(param, Show);

        }), Main.Instance.tapGuard);
    }


    jsonTutorial tutorial = null;

    public void Show(string json)
    {
        Debug.Log("TutorialBehaviour Show...");
        tutorial = JsonUtility.FromJson<jsonTutorial>(json);

        if (tutorial.tutorial_step == constants.User_Info_Tutorial.TUTORIAL_END)
        {
            SceneController.Instance.Jump("Home");
            return;
        }

        DispatchEvent(CwEvent.SCENE_READY);

        StartCoroutine(setDrama());
    }

    //dramaをDramaBehaviourにセットする
    private IEnumerator setDrama()
    {
        Debug.Log("setDrama start..");

        Drama = FindObjectOfType<DramaBehaviour>() as DramaBehaviour;
        while (Drama == null)
        {
            Drama = FindObjectOfType<DramaBehaviour>() as DramaBehaviour;
            yield return new WaitForEndOfFrame();
        }

        Drama.drama = tutorial.drama;
        Drama.dramaId = tutorial.dramaId;

        // 終了時コールバック登録
        Drama.CompleteHandler += endCallback;

        Drama.Show();

        yield return null;
    }

    /// <summary>
    /// 終了時コールバック
    /// </summary>
    /// <param name="result"></param>
    void endCallback(string result)
    {
        SceneController.Instance.Jump(tutorial.nextscene, () =>
        {
            if (tutorial.nextscene == "Battle")
            {
                BattleBehaviour _battle = FindObjectOfType<BattleBehaviour>() as BattleBehaviour;
                _battle.Param = new BattleBehaviour.Parameter
                {
                    tutorial = true,
                    from = null,
                };
            }
        });
    }
}
