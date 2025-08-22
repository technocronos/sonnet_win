using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreateWave;

public class StartDushCampainBehaviour : BaseBehaviour
{

    DramaBehaviour Drama { set; get; } = null;

    // Start is called before the first frame update
    protected override void Start()
    {
        //base.Start();

        Debug.Log("StartDushCampainBehaviour Start...");

        SceneController.Instance.Init("Drama", (() =>
        {
            APIConnectManager.Instance.StartDushCampain(Show);
        }), Main.Instance.tapGuard);

        DispatchEvent(CwEvent.SCENE_READY);

    }

    jsonTerminable terminable = null;


    public void Show(string json)
    {
        Debug.Log("TerminableBehaviour Show...");
        terminable = JsonUtility.FromJson<jsonTerminable>(json);

        StartCoroutine("setDrama");
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

        Drama.drama = terminable.drama;
        Drama.dramaId = terminable.dramaId;
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
        Dictionary<string, string> sceneinfo = new Dictionary<string, string>();
        sceneinfo = Utility.ParseUrl(terminable.nextscene);

        SceneController.Instance.Jump(sceneinfo["scene"]);
    }
}
