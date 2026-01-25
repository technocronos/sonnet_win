using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class UnitBehaviour : MonoBehaviour
{

    [SerializeField]
    private Animator Anim;
    [SerializeField]
    private HpBehaviour HP;
    [SerializeField]
    private ExpDispBehaviour EXP;
    [SerializeField]
    private GameObject weapon_slash;
    [SerializeField]
    private Animator SlashAnim;

    SphereBehaviour Sphere { get; set; }
    StageBehaviour Stage { get; set; }
    UserBehaviour User { get; set; }

    /// <summary>
    /// マスが75だがunitは72なのでマージンを入れる 
    /// </summary>
    private float margin { get; set; }
    private const float cooldown = 2f;
    private bool attack_flg = false;


    private Dictionary<string, Sprite> _sprites { get; set; } = new Dictionary<string, Sprite>();
    private int _count { get; set; } = 0;

    //向きの種類　上下左右で4種類
    const int align_num = 4;
    //向きごとのコマ数
    const int align_flame = 2;

    private float movetime = 0.2f;

    public bool commandkeyrecv { get; set; } = true;
    public bool death { get; set; } = false;

    private int graphAlign { get; set; } = 0;

    Transform AvatarImage;

    private float moverate = 0.5f;

    private string UnitName = "";
    private float X = 0f;
    private float Y= 0f;

    private int no = -1;
    private jsonUnit unitinfo = new jsonUnit();

    public const int PLAYER_ID = 1;

    public void Init(jsonUnit _unitinfo)
    {
        Sphere = SphereBehaviour.Instance;
        Stage = StageBehaviour.Instance;
        User = UserBehaviour.Instance;

        unitinfo = _unitinfo;
        no = int.Parse(transform.name.Split('_')[1]);
        unitinfo.no = no;

        margin = (Sphere.TIP_SIZE - Sphere.UNIT_SIZE) / 2;

        AvatarImage = transform.Find("Avatar");

        //画像を読み込んでおく
        for (int i = 0; i < align_num; i++)
        {
            for (int j = 1; j <= align_flame; j++)
            {
                _sprites[i + "_" + j] = Utility.getAssetImage("Image/UnitTip/" + Sphere.sphere.unitIcon[unitinfo.Info.graphNo] + "_" + i + "_" + j);
            }
        }

        weapon_slash.SetActive(false);

        if (no == PLAYER_ID)
        {
            HP.show(no);
            EXP.show(0, Header.Instance.GetSummary().exp.relative_exp, Header.Instance.GetSummary().exp.relative_next, unitinfo.Status.level);
            unitinfo.code = "avatar";
            unitinfo.act_brain = "manual";

            StartCoroutine(SlashAttack());
        }
        else
        {
            unitinfo.code = "enemy" + no;
            unitinfo.act_brain = "generic";
        }

        // Coroutineをとりあえず動かしておく。
        StartCoroutine(this.walk());
    }

    private void Update()
    {
        try
        {
            if (commandkeyrecv && Sphere.gamestate.is_gameover == false)
            {

                if (unitinfo.act_brain == "manual")
                {
                    Vector3 _stage = Stage.GetComponent<RectTransform>().anchoredPosition;
                    int cost = 0;

                    if (Input.GetKey(KeyCode.UpArrow))
                    {
                        Stage.act_start = false;
                        attack_flg = true;
                        cost = Mathf.Max(Stage.cost["cost" + Mathf.Floor(unitinfo.X) + "_" + (Mathf.Ceil(unitinfo.Y) - 1)], Stage.cost["cost" + Mathf.Ceil(unitinfo.X) + "_" + (Mathf.Ceil(unitinfo.Y) - 1)]);
                        this.setAlign(3);

                        if (cost != 9999)
                        {
                            unitinfo.Y -= moverate;

                            if ((unitinfo.Y * Sphere.TIP_SIZE) <= _stage.y + (Sphere.TIP_SIZE))
                            {
                                Vector3 vector = new Vector3(_stage.x, _stage.y - (Sphere.TIP_SIZE * moverate), _stage.z);
                                Stage.GetComponent<RectTransform>().DOAnchorPos(vector, movetime).SetEase(Ease.Linear);
                            }

                            this.setPos(true);
                        }
                    }
                    else if (Input.GetKey(KeyCode.DownArrow))
                    {
                        Stage.act_start = false;
                        attack_flg = true;
                        cost = Mathf.Max(Stage.cost["cost" + Mathf.Floor(unitinfo.X) + "_" + (Mathf.Floor(unitinfo.Y) + 1)], Stage.cost["cost" + Mathf.Ceil(unitinfo.X) + "_" + (Mathf.Floor(unitinfo.Y) + 1)]);
                        this.setAlign(0);

                        if (cost != 9999)
                        {
                            unitinfo.Y += moverate;
                            if ((unitinfo.Y * Sphere.TIP_SIZE) >= _stage.y + Stage.GetComponent<RectTransform>().rect.height - (Sphere.TIP_SIZE * 2))
                            {
                                Vector3 vector = new Vector3(_stage.x, _stage.y + (Sphere.TIP_SIZE * moverate), _stage.z);
                                Stage.GetComponent<RectTransform>().DOAnchorPos(vector, movetime).SetEase(Ease.Linear);
                            }

                            this.setPos(true);
                        }
                    }
                    else if (Input.GetKey(KeyCode.LeftArrow))
                    {
                        Stage.act_start = false;
                        attack_flg = true;
                        cost = Stage.cost["cost" + (Mathf.Ceil(unitinfo.X) - 1) + "_" + Mathf.Ceil(unitinfo.Y)];
                        this.setAlign(1);

                        if (cost != 9999)
                        {
                            unitinfo.X -= moverate;

                            if ((unitinfo.X * Sphere.TIP_SIZE) <= (_stage.x - Sphere.TIP_SIZE * 2) * -1)
                            {
                                Vector3 vector = new Vector3(_stage.x + (Sphere.TIP_SIZE * moverate), _stage.y, _stage.z);
                                Stage.GetComponent<RectTransform>().DOAnchorPos(vector, movetime).SetEase(Ease.Linear);
                            }

                            this.setPos(true);
                        }
                    }
                    else if (Input.GetKey(KeyCode.RightArrow))
                    {
                        Stage.act_start = false;
                        attack_flg = true;
                        cost = Stage.cost["cost" + (Mathf.Floor(unitinfo.X) + 1) + "_" + Mathf.Ceil(unitinfo.Y)];
                        this.setAlign(2);

                        if (cost != 9999)
                        {
                            unitinfo.X += moverate;
                            if ((unitinfo.X * Sphere.TIP_SIZE) >= Stage.GetComponent<RectTransform>().rect.width - _stage.x - (Sphere.TIP_SIZE * 2))
                            {
                                Vector3 vector = new Vector3(_stage.x - (Sphere.TIP_SIZE * moverate), _stage.y, _stage.z);
                                Stage.GetComponent<RectTransform>().DOAnchorPos(vector, movetime).SetEase(Ease.Linear);
                            }

                            this.setPos(true);
                        }
                    }
                }
                else
                {
                    var targetunit = Sphere.getUnitByCode("avatar");

                    var pos = getPos(targetunit.no);
                    var mypos = getPos(unitinfo.no);

                    var _moverate = moverate * 1;

                    if (pos["x"] >= mypos["x"])
                    {
                        var cost = Stage.cost["cost" + (Mathf.Floor(unitinfo.X) + 1) + "_" + Mathf.Ceil(unitinfo.Y)];
                        if(cost != 9999) { 
                            unitinfo.X += _moverate;
                            this.setAlign(2);
                        }
                    }
                    else
                    {
                        var cost = Stage.cost["cost" + (Mathf.Ceil(unitinfo.X) - 1) + "_" + Mathf.Ceil(unitinfo.Y)];
                        if (cost != 9999)
                        {
                            unitinfo.X -= _moverate;
                            this.setAlign(1);
                        }
                    }

                    if (pos["y"] >= mypos["y"])
                    {
                        var cost = Mathf.Max(Stage.cost["cost" + Mathf.Floor(unitinfo.X) + "_" + (Mathf.Floor(unitinfo.Y) + 1)], Stage.cost["cost" + Mathf.Ceil(unitinfo.X) + "_" + (Mathf.Floor(unitinfo.Y) + 1)]);
                        if (cost != 9999)
                        {
                            unitinfo.Y += _moverate;
                            this.setAlign(0);
                        }
                    }
                    else
                    {
                        var cost = Mathf.Max(Stage.cost["cost" + Mathf.Floor(unitinfo.X) + "_" + (Mathf.Ceil(unitinfo.Y) - 1)], Stage.cost["cost" + Mathf.Ceil(unitinfo.X) + "_" + (Mathf.Ceil(unitinfo.Y) - 1)]);
                        if (cost != 9999)
                        {
                            unitinfo.Y -= _moverate;
                            this.setAlign(0);
                        }
                    }
                    this.setPos(true);
                }
            }
        }
        catch (Exception e)
        {
            return;
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollisionOrTrigger(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)//一旦isTriggerになっている敵もOnCollisionと同じ効果にする
    {
        OnCollisionOrTrigger(collision.gameObject);
    }

    private void OnCollisionOrTrigger(GameObject collision)
    {
        try
        {
            if (collision.transform.name.Contains("unit_") && transform.name.Contains("unit_"))
            {
                if (unitinfo.code == "avatar")
                {
                    int colno = int.Parse(collision.transform.name.Split('_')[1]);
                    jsonUnit colunitinfo = Sphere.sphere.unit[colno];

                    StartCoroutine(CalcDamage(colunitinfo));
                }
            }
            else if (collision.transform.name.Contains("weapon_") && transform.name.Contains("unit_"))
            {
                if (unitinfo.code != "avatar")
                {
                    jsonUnit colunitinfo = Sphere.sphere.unit[PLAYER_ID];

                    StartCoroutine(CalcDamage(colunitinfo));
                }
            }
        }
        catch (Exception e)
        {
            return;
        }
    }

    private IEnumerator CalcDamage(jsonUnit emeny)
    {
        //unionが違う場合のみダメージを与える
        if (emeny.Info.union != unitinfo.Info.union)
        {
            if (unitinfo.Status.hp <= 0 || emeny.Status.hp <= 0) yield break;

            var battleResult = omissionBattle(emeny, unitinfo);

            int damage = (int)battleResult["defender"];

            death = false;
            if (unitinfo.Status.hp - damage <= 0)
            {
                damage = unitinfo.Status.hp;
                death = true;
            }

            unitinfo.Status.hp -= damage;

            if (unitinfo.code == "avatar") 
                HP.show(unitinfo.no);

            if (death)
            {
                if (unitinfo.code == "avatar")
                {
                    Sphere.gamestate.is_gameover = true;
                    commandkeyrecv = false;
                    Stage.act_start = true;

                    UnitEvent(unitinfo.no, "dam", damage);
                    StartCoroutine(setEffects("dam"));
                    yield return StartCoroutine(wait_effec());

                    StartCoroutine(setEffects("collap"));
                    yield return StartCoroutine(wait_effec());

                    UnitRemove();

                    Sphere.GameOver(unitinfo.no, damage);
                }
                else
                {
                    //Sphere.DeadEnemy(unitinfo.no, damage);

                    UnitEvent(unitinfo.no, "dam", damage);
                    StartCoroutine(setEffects("dam"));
                    yield return StartCoroutine(wait_effec());

                    StartCoroutine(setEffects("collap"));
                    yield return StartCoroutine(wait_effec());
                    UnitRemove();
                }
            }
            else
            {
                //Sphere.Damage(unitinfo.no, damage);
                UnitEvent(unitinfo.no, "dam",damage);
                StartCoroutine(setEffects("dam"));
            }
        }
    }

    IEnumerator wait_effec()
    {
        while (this.stop)
        {
            Debug.Log("wait_effec run...");
            yield return null;
        }

        Debug.Log("wait_effec end...");
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

        switch (_graphAlign)
        {
            case 0:
                weapon_slash.transform.rotation = Quaternion.Euler(0, 0, 90);
                weapon_slash.transform.localPosition = new Vector3(53f, -146, 0);
                break;
            case 1:
                weapon_slash.transform.rotation = Quaternion.Euler(0, 0, 0);
                weapon_slash.transform.localPosition = new Vector3(-93f, -46f, 0);
                break;
            case 2:
                weapon_slash.transform.rotation = Quaternion.Euler(0, 180, 0);
                weapon_slash.transform.localPosition = new Vector3(172f, -46f, 0);
                break;
            case 3:
                weapon_slash.transform.rotation = Quaternion.Euler(0, 0, -90);
                weapon_slash.transform.localPosition = new Vector3(53f, 35f, 0);
                break;
        }
    }


    public bool stop { get; set; } = false;

    IEnumerator walk()
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

    private Dictionary<string, float> getPos(int unitNo)
    {
        jsonUnit _unitinfo = Sphere.sphere.unit[unitNo];

        Dictionary<string, float> pos = new Dictionary<string, float>();
        pos.Add("x", (int)_unitinfo.X);
        pos.Add("y", (int)_unitinfo.Y);

        return pos;
    }

    public void setPos(bool move = false)
    {
        int no = int.Parse(transform.name.Split('_')[1]);

        jsonUnit unitinfo = Sphere.sphere.unit[no];

        Vector3 vector = new Vector3(unitinfo.X * Sphere.TIP_SIZE + margin, (unitinfo.Y * Sphere.TIP_SIZE + margin) * -1, 0);

        if (move)
        {
            commandkeyrecv = false; 
            var tween = transform.DOLocalMove(vector, movetime).SetEase(Ease.Linear);            
            tween.OnComplete(() => { 
                if(!death)
                    commandkeyrecv = true; 
            });
        }
        else
        {
            transform.localPosition = vector;
        }

    }

    public IEnumerator SlashAttack()
    {
        while (true)
        {
            if (attack_flg)
            {
                weapon_slash.SetActive(true);

                var _attack_name = "Slash";

                int hashAnim = Animator.StringToHash(_attack_name);
                AudioManager.Instance.PlaySE("se_slashblade");
                SlashAnim.Play(hashAnim);

                yield return null;
                yield return new WaitForAnimation(SlashAnim, 0);

                Anim.SetBool(_attack_name, false);
                weapon_slash.SetActive(false);
            }

            yield return new WaitForSeconds(cooldown);
        }
    }

    public void UnitEvent(int targetNo, string effType, int value)
    {
        UeveBehaviour _ueve = UnityEngine.Object.Instantiate(Sphere.ueve, new Vector3(0, 0, 0), Quaternion.identity, Stage.transform);
        _ueve.transform.localPosition = new Vector3(0, 0, 0);

        _ueve.Play(targetNo, effType, value.ToString());
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
        isCancelled = false;

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
        Anim.Play(hashAnim, 0, 0f);

        yield return new WaitUntil(() =>
            Anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
            || isCancelled);

        if (isCancelled)
        {
            Debug.Log("アニメーションがキャンセルされました");
        }
        else
        {
            Debug.Log("アニメーションが通常終了しました");
        }

        stop = false;
        Anim.SetBool(_effectName, false);

    }

    bool isCancelled = false;
    // 他の場所でキャンセルしたい場合
    public void CancelAnimation()
    {
        isCancelled = true;
    }

    private void UnitRemove()
    {
        // 無効なユニットである場合はX座標上での位置でそれを示す
        unitinfo.X = -1;

        GameObject.Destroy(transform.gameObject);
    }

    public double getSpeedBalance(jsonUnit sideP, jsonUnit sideE)
    {
        // バランス1.0となるスピード差を求める。
        // 10 の [両者の平均Lv * 3]% 増し。
        var speedWidth = 10 * (1.0 + (sideP.Status.level + sideE.Status.level / 2 * 0.03));

        // スピードバランスの計算。
        var result = (sideP.Status.spd - sideE.Status.spd) / speedWidth;

        // +1.0 ～ -1.0 に補正する。
        if (result > 1.0)
            result = 1.0;
        else if (result < -1.0) 
            result = -1.0;

        // リターン。
        return result;
    }

    private Dictionary<string, float> omissionBattle(jsonUnit challenger, jsonUnit defender)
    {
        // スピードバランスを取得。
        double speedBalance = getSpeedBalance(challenger, defender);

        // ダメージ初期化。
        Dictionary<string, float> result = new Dictionary<string, float>();
        result.Add("challenger", 0);
        result.Add("defender", 0);

        // リベンジカウントの初期化。
        Dictionary<string, int> star = new Dictionary<string, int>();
        star.Add("challenger", 0);
        star.Add("defender", 0);

        // ノーマルダメージの計算。規定回数繰り返す。
        for (int i = 0; i < 10; i++)
        {
            // 両者の攻撃カードの決定。
            Dictionary<string, int> card = new Dictionary<string, int>();
            card.Add("challenger", UnityEngine.Random.Range(1, 3));
            card.Add("defender", UnityEngine.Random.Range(1, 3));

            // 攻め側⇒受け側の順で処理する。
            for (int side = 0; side < 2; side++)
            {
                var attackerinfo = side == 1 ? defender : challenger;
                var defencerinfo = side == 1 ? challenger : defender;

                string attacker = side == 1 ? "defender" : "challenger";
                string defencer = side == 1 ? "challenger" : "defender";

                // ダメージ計算。
                float damage = calcNormalDamage(
                    attackerinfo, card[attacker], defencerinfo, card[defencer], speedBalance * (side == 1 ? -1 : +1)
                );

                // スターに変換されたならスターカウントを、ダメージになったならダメージをアップ。
                if (damage == -1)
                    star[attacker]++;
                else if (damage == -2)
                    star[defencer]++;
                else
                    result[defencer] += damage;
            }

            // ダメージがHPを上回ったならそこでストップ。
            if (challenger.Status.hp <= result["challenger"] || defender.Status.hp <= result["defender"])
                return result;
        }

        // リベンジの計算。攻め側⇒受け側の順で処理する。
        for (int side = 0; side < 2; side++)
        {
            var attackerinfo = side == 1 ? defender : challenger;
            var defencerinfo = side == 1 ? challenger : defender;

            var attacker = side == 1 ? "defender" : "challenger";
            var defencer = side == 1 ? "challenger" : "defender";

            // ダメージ計算。
            result[defencer] += calcRevengeDamage(
                attackerinfo, defencerinfo, star[attacker], speedBalance * (side == 1 ? -1 : +1)
            );
        }

        // リターン。
        return result;
    }

    public int calcNormalDamage(jsonUnit attacker, int attackCard, jsonUnit defencer, int defenceCard, double speedBalance)
    {

        // カードの相性を判定。有利なら1、不利なら2、同じなら0。
        var affinity = attackCard - defenceCard;
        if (affinity < 0) affinity += 3;

        // 不利なら攻撃側スターに。
        if (affinity == 2)
            return -1;

        // 攻撃側スピードバランスに従って20～60%で吸収判定。
        if (UnityEngine.Random.Range(1, 100) <= 40 - (int)(20 * speedBalance) )
            return -2;

        float attacker_total_attack = 0;
        if(attackCard == 1)
        {
            attacker_total_attack = attacker.Status.att1;
        }
        else if(attackCard == 2)
        {
            attacker_total_attack = attacker.Status.att2;
        }
        else if (attackCard == 3)
        {
            attacker_total_attack = attacker.Status.att3;
        }

        float defencer_total_attack = 0;
        if (attackCard == 1)
        {
            defencer_total_attack = defencer.Status.att1;
        }
        else if (attackCard == 2)
        {
            defencer_total_attack = defencer.Status.att2;
        }
        else if (attackCard == 3)
        {
            defencer_total_attack = defencer.Status.att3;
        }


        // ここまで来たらダメージ計算。
        return calcDamage(attacker_total_attack, defencer_total_attack);
    }

    public int calcRevengeDamage(jsonUnit attacker, jsonUnit defencer, int starCount, double speedBalance)
    {

        // ダメージ初期化。
        int damage = 0;

        // スターの数、判定する。
        for (int i = 0; i < starCount; i++)
        {

            // 攻撃側スピードバランスに従って25～75%で回避判定。
            if (UnityEngine.Random.Range(1, 100) <= 50 - (int)(25 * speedBalance) )
                continue;

            // 攻撃カードの決定。
            var card = UnityEngine.Random.Range(1, 3);

            float attacker_total_attack = 0;
            if (card == 1)
            {
                attacker_total_attack = attacker.Status.att1;
            }
            else if (card == 2)
            {
                attacker_total_attack = attacker.Status.att2;
            }
            else if (card == 3)
            {
                attacker_total_attack = attacker.Status.att3;
            }

            float defencer_total_attack = 0;
            if (card == 1)
            {
                defencer_total_attack = defencer.Status.att1;
            }
            else if (card == 2)
            {
                defencer_total_attack = defencer.Status.att2;
            }
            else if (card == 3)
            {
                defencer_total_attack = defencer.Status.att3;
            }

            // 攻撃力の計算。受け側の該当攻撃力の75% + 受け側の該当攻撃力の25%
            var attackPow = (int)(defencer_total_attack * 0.75 + attacker_total_attack * 0.25);

            // ダメージ計算して戻り値に追加。
            damage += calcDamage(attackPow, defencer_total_attack);
        }

        // リターン。
        return damage;
    }

    private int calcDamage(float attack, float defence)
    {
        int damage = (int)(attack * 0.70 - defence * 0.55);
        if (damage <= 0)
            damage = UnityEngine.Random.Range(0, 1);

        return damage;
    }
}
