using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyScene;

public class TerminableBehaviour : BaseBehaviour
{
    public class Parameter
    {
        public int questId;
        public int sphereId;
    }

    public Parameter Param;

    DramaBehaviour Drama { set; get; } = null;

    // Start is called before the first frame update
    protected override void Start()
    {
        //base.Start();

        Debug.Log("TerminableBehaviour Start...");

        SceneController.Instance.Init("Drama", (() =>
        {
            APIConnectManager.Instance.terminable(Param.questId, Param.sphereId, Show);
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

        switch (sceneinfo["scene"])
        {
            case "Sphere":
                SceneController.Instance.Jump("Sphere", (() =>
                {
                    SphereBehaviour _sphere = FindObjectOfType<SphereBehaviour>() as SphereBehaviour;
                    _sphere.Param = new SphereBehaviour.Parameter
                    {
                        sphereId = int.Parse(sceneinfo["id"]),
                        reopen = sceneinfo["reopen"],
                    };
                }));
                break;
            case "Ready":
                SceneController.Instance.Jump("FieldEnd", (() =>
                {
                    ReadyBehaviour _fieldend = FindObjectOfType<ReadyBehaviour>() as ReadyBehaviour;
                    _fieldend.Param = new ReadyBehaviour.Parameter
                    {
                        questId = int.Parse(sceneinfo["questId"]),
                    };
                }));
                break;
            default:
                SceneController.Instance.Jump(sceneinfo["scene"]);
                break;
        }
    }

}
