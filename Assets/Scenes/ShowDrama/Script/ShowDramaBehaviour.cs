using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyScene;
using System.Web;
using UnityEngine.Networking;

public class ShowDramaBehaviour : BaseBehaviour
{
    public class Parameter
    {
        public int DramaId;
        public string endTo;
    }

    public Parameter Param;

    DramaBehaviour Drama = null;

    // Start is called before the first frame update
    protected override void Start()
    {
        //base.Start();

        Debug.Log("ShowDramaBehaviour Start...");

        //this.Param = new ShowDramaBehaviour.Parameter {DramaId = 9900002, endTo = "scene=Home"};

        SceneController.Instance.Init("Drama", (() =>
        {
            APIConnectManager.Instance.showdrama(this.Param.DramaId, UnityWebRequest.UnEscapeURL(this.Param.endTo), Show);
        }), Main.Instance.tapGuard);

        DispatchEvent(CwEvent.SCENE_READY);

    }

    jsonShowDrama drm = null;

    public void Show(string json)
    {
        Debug.Log("ShowDramaBehaviour Show...");
        drm = JsonUtility.FromJson<jsonShowDrama>(json);

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

        Drama.drama = drm.drama;
        Drama.dramaId = drm.dramaId;

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
        sceneinfo = Utility.ParseUrl(drm.nextscene);

        SceneController.Instance.Jump(sceneinfo["scene"]);
    }
}
