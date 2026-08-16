using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitBehaviour : MonoBehaviour
{
    private const float ENEMY_BREATHING_AMPLITUDE = 3.0f;
    private const float ENEMY_BREATHING_PERIOD = 2.6f;

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
    Vector3 avatarBaseLocalPosition;
    float enemyBreathingPhase;
    int unitNo;
    bool enemyClassificationResolved;
    bool isEnemy;

    public void Init()
    {
        Sphere = SphereBehaviour.Instance;
        Stage = StageBehaviour.Instance;

        margin = (Sphere.TIP_SIZE - Sphere.UNIT_SIZE) / 2;

        AvatarImage = transform.Find("Avatar");
        avatarBaseLocalPosition = AvatarImage.localPosition;

        string[] nameParts = transform.name.Split('_');
        if (nameParts.Length > 1)
        {
            int.TryParse(nameParts[1], out unitNo);
        }
        enemyBreathingPhase = Mathf.Repeat(unitNo * 1.6180339f, Mathf.PI * 2.0f);

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

    void LateUpdate()
    {
        if (AvatarImage == null || Sphere == null) return;
        if (!enemyClassificationResolved && !resolveEnemyClassification()) return;

        if (!isEnemy || stop)
        {
            AvatarImage.localPosition = avatarBaseLocalPosition;
            return;
        }

        Vector3 visualPosition = avatarBaseLocalPosition;
        float angle = Time.unscaledTime / ENEMY_BREATHING_PERIOD * Mathf.PI * 2.0f + enemyBreathingPhase;
        visualPosition.y += Mathf.Sin(angle) * ENEMY_BREATHING_AMPLITUDE;
        AvatarImage.localPosition = visualPosition;
    }

    bool resolveEnemyClassification()
    {
        int playerUnitNo = Sphere.leader.commUnit;
        if (playerUnitNo <= 0 || unitNo <= 0) return false;
        if (!Sphere.sphere.unit.ContainsKey(playerUnitNo) || !Sphere.sphere.unit.ContainsKey(unitNo)) return false;

        int playerUnion;
        int ownUnion;
        if (!tryGetUnion(Sphere.sphere.unit[playerUnitNo], out playerUnion)) return false;
        if (!tryGetUnion(Sphere.sphere.unit[unitNo], out ownUnion)) return false;

        isEnemy = ownUnion != playerUnion;
        enemyClassificationResolved = true;
        return true;
    }

    bool tryGetUnion(jsonUnit unit, out int union)
    {
        union = 0;
        if (unit == null || string.IsNullOrEmpty(unit.Info)) return false;

        string[] unitInfo = unit.Info.Split(' ');
        return unitInfo.Length > 1 && int.TryParse(unitInfo[1], out union);
    }

    void OnDisable()
    {
        if (AvatarImage != null)
        {
            AvatarImage.localPosition = avatarBaseLocalPosition;
        }
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
