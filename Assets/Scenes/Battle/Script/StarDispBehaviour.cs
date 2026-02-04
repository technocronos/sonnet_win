using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarDispBehaviour : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI Text;
    [SerializeField]
    private Animator Anim;

    public const int RevengeFireCount = 40;
    public const int RevengeConsumeStar = 4;

    private int star = 0;

    SphereBehaviour Sphere { get; set; }


    public static StarDispBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static StarDispBehaviour instance;
    private Vector2 pos;
    private Tweener tween;

    /// <summary>
    /// ムービーにセットされた変数 type にしたがって、カードの表示を
    /// 切り替えるcall用ラベル。
    /// type には 1:火、2:水、3:雷、4:カラ をセットする。
    /// </summary>
    /// <param name="type"></param>
    public void Init()
    {
        instance = this;
        Sphere = SphereBehaviour.Instance;
        pos = transform.localPosition;
        tween = null;

        transform.gameObject.SetActive(true);
        clear();
    }

    public void add()
    {
        if (!Sphere.gamestate.is_gameover)
        {
            AudioManager.Instance.PlaySE("se_coin");

            if (tween == null)
            {
                tween = transform.DOPunchPosition(new Vector3(0, 10, 0), 1f, 10, 1f);
                tween.OnComplete(() => {
                    transform.localPosition = pos;
                    tween = null;
                });
            }

            star++;
            StartCoroutine(StarFlash());

            Text.text = star.ToString();
        }
    }

    public int get()
    {
        return star;
    }


    public void use()
    {
        if(star > 0 && (star - StarDispBehaviour.RevengeConsumeStar) >= 0)
            star -= StarDispBehaviour.RevengeConsumeStar;

        Text.text = star.ToString();
    }

    public void clear()
    {
        star = 0;
        Text.text = star.ToString();
    }

    public void hide()
    {
        transform.gameObject.SetActive(false);
    }

    public IEnumerator StarFlash()
    {
        if (Sphere.gamestate.is_gamestart && !Sphere.gamestate.is_stop)
        {
            var _anim_name = "StarDispFlash";

            int hashAnim = Animator.StringToHash(_anim_name);
            Anim.Play(hashAnim);
            Anim.SetBool(_anim_name, true);

            yield return null;
            yield return new WaitForAnimation(Anim, 0);

            Anim.SetBool(_anim_name, false);

        }
    }

}
