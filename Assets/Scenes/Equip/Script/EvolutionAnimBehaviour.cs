using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionAnimBehaviour : MonoBehaviour
{
    public Animator Anim;

    public Image Rotator;

    public delegate void OnCompleteDelegate();
    public OnCompleteDelegate CompleteHandler;

    Tweener tweener = null;

    string anim { get; set; }

    /// <summary>
    /// アニメを再生する
    /// </summary>
    /// <param name="anim">開始：start 結果個別表示：display 連ガチャ結果表示：all</param>
    /// <param name="_callback">コールバック</param>
    /// <returns></returns>
    public void PlayAnim(string _anim, OnCompleteDelegate _callback = null)
    {
        anim = _anim;

        Debug.Log("GachaCircleBehaviour PlayAnim start.. anim = " + anim);

        CompleteHandler += _callback;

        SetBoolByName(anim, true);
    }

    /// <summary>
    /// サウンドを鳴らす
    /// </summary>
    /// <param name="name">SEサウンド名</param>
    public void playSound(string name)
    {
        AudioManager.Instance.PlaySE(name);
    }

    public void DisplayRotate()
    {
        if (tweener == null)
        {
            //回転アニメ
            tweener = Rotator.transform.DOLocalRotate(new Vector3(0, 0, 360f), 30f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }

    }

    public void onEndAnim()
    {
        Debug.Log("GachaCircleBehaviour onEndAnim start.. anim = " + anim);

        SetBoolByName(anim, false);

        CompleteHandler?.Invoke();
        CompleteHandler = null;
    }

    public void SetBoolByName(string _anim, bool flg = false)
    {
        Anim.SetBool(_anim, flg);
    }
}
