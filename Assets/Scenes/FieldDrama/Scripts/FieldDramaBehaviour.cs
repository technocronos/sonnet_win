using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyScene;

public class FieldDramaBehaviour : BaseBehaviour
{
    public class Parameter
    {
        public int sphereId;
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
            APIConnectManager.Instance.FieldDrama(Param.sphereId, 0, Show);
        }), Main.Instance.tapGuard);

        DispatchEvent(CwEvent.SCENE_READY);

    }

    jsonFieldDrama fielddrama = null;

    public void Show(string json)
    {
        Debug.Log("FieldDramaBehaviour Show...");
        fielddrama = JsonUtility.FromJson<jsonFieldDrama>(json);

        if (fielddrama.errscene != null)
        {
            moveScene(fielddrama.errscene);
        }
        else
        {
            StartCoroutine("setDrama");
        }
    }

    //dramaをDramaBehaviourにセットする
    private IEnumerator setDrama()
    {
        Debug.Log("FieldDramaBehaviour setDrama start..");

        Drama = FindObjectOfType<DramaBehaviour>() as DramaBehaviour;
        while (Drama == null)
        {
            Drama = FindObjectOfType<DramaBehaviour>() as DramaBehaviour;
            yield return new WaitForEndOfFrame();
        }

        Drama.drama = fielddrama.drama;
        Drama.dramaId = fielddrama.dramaId;
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
        APIConnectManager.Instance.FieldDrama(Param.sphereId, fielddrama.dramaId, End);
    }

    public void End(string json)
    {
        fielddrama = JsonUtility.FromJson<jsonFieldDrama>(json);
        moveScene(fielddrama.nextscene);
    }

    void moveScene(string nextscene)
    {
        Dictionary<string, string> sceneinfo = new Dictionary<string, string>();
        sceneinfo = Utility.ParseUrl(nextscene);

        AudioManager.Instance.StopBGM();

        switch (sceneinfo["scene"])
        {
            case "Sphere":
                SceneController.Instance.Jump("Sphere", (() =>
                {
                    SphereBehaviour _sphere = FindObjectOfType<SphereBehaviour>() as SphereBehaviour;
                    _sphere.Param = new SphereBehaviour.Parameter
                    {
                        sphereId = int.Parse(sceneinfo["id"]),
                    };
                }));
                break;
            default:
                SceneController.Instance.Jump(sceneinfo["scene"]);
                break;
        }
    }
}
