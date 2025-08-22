using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardBehaviour : MonoBehaviour
{

    public Sprite fire;
    public Sprite thunder;
    public Sprite water;

    public Animator Anim;

    public int type { get; set; }
    public bool reverse { get; set; } = false;

    public bool motion { get; set; } = false;

    public delegate void OnCompleteDelegate(string cardname);
    public OnCompleteDelegate CompleteHandler;

    public static CardBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static CardBehaviour instance;

    private void Start()
    {
        instance = this;
    }

    /// <summary>
    /// ムービーにセットされた変数 type にしたがって、カードの表示を
    /// 切り替えるcall用ラベル。
    /// type には 1:火、2:水、3:雷、4:カラ をセットする。
    /// </summary>
    /// <param name="type"></param>
    public void CardInit()
    {
        ParticleSystemRenderer ObjSpriteRenderer = transform.Find("Card").GetComponent<ParticleSystemRenderer>();

        switch (type)
        {
            case 1:
                ObjSpriteRenderer.material.SetTexture("_MainTex", fire.texture);
                break;
            case 2:
                ObjSpriteRenderer.material.SetTexture("_MainTex", water.texture);
                break;
            case 3:
                ObjSpriteRenderer.material.SetTexture("_MainTex", thunder.texture);
                break;
            default:
                break;
        }
    }

    public IEnumerator PlayAnim(string anim, OnCompleteDelegate _callback = null)
    {
        Debug.Log("CardBehaviour PlayAnim start.. anim = " + anim);

        if (_callback != null)
            CompleteHandler += _callback;

        int hashAnim = Animator.StringToHash(anim);
        Anim.Play(hashAnim);

        motion = true;

        yield return null;
        yield return new WaitForAnimation(Anim, 0);

        Debug.Log("CardBehaviour PlayAnim end..");

        motion = false;
        if (CompleteHandler != null)
        {
            CompleteHandler?.Invoke(transform.name);
            CompleteHandler = null;
        }

        //CardMinimize以外はノーマル表示。CardMinimizeはそのまま。
        if (anim == "CardUnizon" || anim == "CardAppear" || anim == "CardAppear2")
        {
            int hashCardNorm = Animator.StringToHash("CardNorm");
            Anim.Play(hashCardNorm);
        }
    }

}
