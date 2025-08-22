using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EffectsBehaviour : MonoBehaviour
{
    public Animator Anim;

    private string type { get; set; }

    public delegate void OnCompleteDelegate(string result);
    public OnCompleteDelegate CompleteHandler;

    public void playEffects(string _type, int x, int y)
    {
        type = _type;

        foreach (Transform childTransform in transform)
        {
            //全ての子オブジェクトを非表示
            childTransform.gameObject.SetActive(false);
        }

        switch (type)
        {
            case "bomb":
                transform.Find(type + "1").gameObject.SetActive(true);
                break;
            case "recv":
                transform.Find(type + "1").gameObject.SetActive(true);
                break;
            case "shck":
                transform.Find(type + "1").gameObject.SetActive(true);
                break;
            case "sprk":
                transform.Find(type + "1").gameObject.SetActive(true);
                break;
            case "migt":
                transform.Find(type + "1").gameObject.SetActive(true);
                break;
        }

        transform.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(x, y, 0);

        //前面表示する
        transform.GetComponent<RectTransform>().SetAsLastSibling();

        Anim.SetBool(type, true);
    }

    /// <summary>
    /// サウンドを鳴らす
    /// </summary>
    public void Sound(string _sound)
    {
        AudioManager.Instance.PlaySE(_sound);
    }

    /// <summary>
    /// アニメーション終了コールバック。animatorから呼ばれている
    /// </summary>
    public void onEndEffects()
    {
        Debug.Log("EffectsBehaviour onEndEffects run..");

        //待機アニメに戻す
        Anim.SetBool(type, false);

        // コールバック実行
        CompleteHandler?.Invoke(type);
        CompleteHandler = null;

        //もう使わないので削除
        GameObject.Destroy(transform.gameObject);
    }
}
