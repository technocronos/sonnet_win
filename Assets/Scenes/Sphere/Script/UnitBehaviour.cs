using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitBehaviour : MonoBehaviour
{

    public Animator Anim;

    SphereBehaviour Sphere { get; set; }
    StageBehaviour Stage { get; set; }

    /// <summary>
    /// マスが75だがunitは72なのでマージンを入れる 
    /// </summary>
    private float margin { get; set; }

    public int graphNo { get; set; }

    private Dictionary<string, Sprite> _sprites { get; set; } = new Dictionary<string, Sprite>();
    private int _count { get; set; } = 0;

    //向きの種類　上下左右で4種類
    const int align_num = 4;
    //向きごとのコマ数
    const int align_flame = 2;

    private int graphAlign { get; set; } = 0;

    Transform AvatarImage;

    public void Init()
    {
        Sphere = SphereBehaviour.Instance;
        Stage = StageBehaviour.Instance;

        margin = (Sphere.TIP_SIZE - Sphere.UNIT_SIZE) / 2;

        AvatarImage = transform.Find("Avatar");

        //画像を読み込んでおく
        for (int i = 0; i < align_num; i++)
        {
            for (int j = 1; j <= align_flame; j++)
            {
                _sprites[i + "_" + j] = Utility.getAssetImage("Image/UnitTip/" + Sphere.sphere.unitIcon[graphNo] + "_" + i + "_" + j);
            }
        }

        // Coroutineをとりあえず動かしておく。
        StartCoroutine(this.anim());
    }


    //
    // 変数 no で示されたユニットの位置情報を参照して、そのユニットのムービーの
    // 表示座標を変更する。
    public void setAlign(int _graphAlign)
    {

        //向きを更新する
        this.graphAlign = _graphAlign;
        //コルーチンとタイミング合わない時があるのでひとまず変えておく
        ParticleSystemRenderer avatar_renderer = AvatarImage.GetComponent<ParticleSystemRenderer>();
        avatar_renderer.material.SetTexture("_MainTex", _sprites[this.graphAlign + "_1"].texture);
    }


    public bool stop { get; set; } = false;

    IEnumerator anim()
    {

        int _frame = align_flame;

        while (true)
        {
            //0.5秒に一回
            yield return new WaitForSeconds(0.5f);

            if (!stop)
            {
                if (_count >= _frame)
                    _count = 0;

                string _flame = (_count + 1).ToString();
                ParticleSystemRenderer avatar_renderer = AvatarImage.GetComponent<ParticleSystemRenderer>();
                avatar_renderer.material.SetTexture("_MainTex", _sprites[this.graphAlign + "_" + _flame].texture);
                _count++;
            }
        }
    }

    public void setPos(bool move = false)
    {
        int no = int.Parse(transform.name.Split('_')[1]);

        jsonUnit unitinfo = Sphere.sphere.unit[no];

        Vector3 vector = new Vector3(unitinfo.X * Sphere.TIP_SIZE + margin, (unitinfo.Y * Sphere.TIP_SIZE + margin) * -1, 0);

        if (move)
        {
            transform.DOLocalMove(vector, 0.2f).SetEase(Ease.InOutSine);
        }
        else
        {
            transform.localPosition = vector;
        }

    }

    /// <summary>
    /// recov
    /// damag
    /// collap
    /// </summary>
    /// <param name="_effectName"></param>
    public IEnumerator setEffects(string _effectName)
    {
        Debug.Log("UnitBehaviour setEffects run.. _effectName=" + _effectName);

        //エフェクト中は歩かない
        this.stop = true;

        //サウンド再生。recovはコマンドで鳴らしているので不要。
        switch (_effectName)
        {
            case "dam":
                AudioManager.Instance.PlaySE("se_hit");
                break;
            case "collap":
                AudioManager.Instance.PlaySE("se_explosionshort");
                break;
        }

        int hashAnim = Animator.StringToHash(_effectName);
        Anim.Play(hashAnim);

        yield return null;
        yield return new WaitForAnimation(Anim, 0);

        stop = false;
        Anim.SetBool(_effectName, false);

    }

}
