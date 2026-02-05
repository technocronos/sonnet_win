using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
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
    private StarDispBehaviour StarDisp;
    [SerializeField]
    private RevengeBehaviour Revenge;
    [SerializeField]
    private GameObject weapon_slash;
    [SerializeField]
    private Animator SlashAnim;
    [SerializeField]
    private ExpPiece expPref;

    SphereBehaviour Sphere { get; set; }
    StageBehaviour Stage { get; set; }
    UserBehaviour User { get; set; }

    //歩行エフェクトを止める
    public bool walk_stop { get; set; } = false;

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
    public jsonUnit unitinfo = new jsonUnit();

    public Tween currentMoveTween { get; set; } = null;
    private bool wasStopped = false;

    int relative_exp = 0;
    int relative_next = 0;

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
        HP.show(no);

        if (unitinfo.code == "avatar")
        {
            StarDisp.Init();

            relative_exp = Header.Instance.GetSummary().exp.relative_exp;
            relative_next = Header.Instance.GetSummary().exp.relative_next;

            EXP.show(0, relative_exp, relative_next, unitinfo.Status.level);
            movetime = 0.2f;

            attack_flg = true;
            StartCoroutine(SlashAttack());
        }
        else
        {
            movetime = 0.4f * 2;
            unitinfo.Info.cost = 20;//一旦書き換え
        }
        // Coroutineをとりあえず動かしておく。
        StartCoroutine(this.walk());
    }

    private void FixedUpdate()
    {
        try {
            if (Sphere.gamestate.is_gamestart && !Sphere.gamestate.is_gameover)
            {
                // 物理演算で移動した位置を unitinfo に反映
                // DOTween で移動中でない場合のみ補正
                if (currentMoveTween == null || !currentMoveTween.IsActive())
                {
                    // transform.localPosition から unitinfo.X, unitinfo.Y を逆算
                    Vector3 currentPos = transform.localPosition;
                    float expectedX = unitinfo.X * Sphere.TIP_SIZE + margin;
                    float expectedY = (unitinfo.Y * Sphere.TIP_SIZE + margin) * -1;

                    // 物理演算で移動した分を計算（閾値は TIP_SIZE の 10% 以上）
                    float threshold = Sphere.TIP_SIZE * 0.1f;
                    float diffX = currentPos.x - expectedX;
                    float diffY = currentPos.y - expectedY;

                    if (Mathf.Abs(diffX) > threshold || Mathf.Abs(diffY) > threshold)
                    {
                        // 物理演算で移動した分を unitinfo に反映
                        float newX = (currentPos.x - margin) / Sphere.TIP_SIZE;
                        float newY = ((currentPos.y * -1) - margin) / Sphere.TIP_SIZE;

                        // 0.5刻みにスナップ（moverate=0.5 前提）
                        newX = Mathf.Round(newX * 2f) / 2f;
                        newY = Mathf.Round(newY * 2f) / 2f;

                        // コスト9999のマップチップに入っていないかチェック
                        int costX1 = GetCost(Mathf.FloorToInt(newX), Mathf.CeilToInt(newY));
                        int costX2 = GetCost(Mathf.CeilToInt(newX), Mathf.CeilToInt(newY));
                        int costY1 = GetCost(Mathf.CeilToInt(newX), Mathf.FloorToInt(newY));
                        int costY2 = GetCost(Mathf.CeilToInt(newX), Mathf.CeilToInt(newY));
                        int maxCost = Mathf.Max(costX1, costX2, costY1, costY2);

                        // コスト9999のマップチップに入っている場合は、前の位置に戻すか、最も近い通れるマップチップに移動
                        if (maxCost >= 9990)
                        {
                            // 前の位置（expectedX, expectedYから計算）に戻す
                            float safeX = (expectedX - margin) / Sphere.TIP_SIZE;
                            float safeY = ((expectedY * -1) - margin) / Sphere.TIP_SIZE;
                            safeX = Mathf.Round(safeX * 2f) / 2f;
                            safeY = Mathf.Round(safeY * 2f) / 2f;

                            // 前の位置もコスト9999の場合は、最も近い通れるマップチップを探す
                            int safeCostX1 = GetCost(Mathf.FloorToInt(safeX), Mathf.CeilToInt(safeY));
                            int safeCostX2 = GetCost(Mathf.CeilToInt(safeX), Mathf.CeilToInt(safeY));
                            int safeCostY1 = GetCost(Mathf.CeilToInt(safeX), Mathf.FloorToInt(safeY));
                            int safeCostY2 = GetCost(Mathf.CeilToInt(safeX), Mathf.CeilToInt(safeY));
                            int safeMaxCost = Mathf.Max(safeCostX1, safeCostX2, safeCostY1, safeCostY2);

                            if (safeMaxCost >= 9990)
                            {
                                // 最も近い通れるマップチップを探す
                                float[] nearestWalkable = FindNearestWalkableTile(newX, newY);
                                if (nearestWalkable != null)
                                {
                                    newX = nearestWalkable[0];
                                    newY = nearestWalkable[1];
                                }
                                else
                                {
                                    // 見つからない場合は前の位置を維持
                                    return;
                                }
                            }
                            else
                            {
                                // 前の位置が安全な場合は前の位置に戻す
                                newX = safeX;
                                newY = safeY;
                            }

                            // 物理位置も補正
                            Vector3 safePos = new Vector3(newX * Sphere.TIP_SIZE + margin, (newY * Sphere.TIP_SIZE + margin) * -1, 0);
                            transform.localPosition = safePos;
                        }

                        // 安全な位置を unitinfo に反映
                        unitinfo.X = newX;
                        unitinfo.Y = newY;
                    }
                }
            }
        }
        catch (Exception e)
        {
            return;
        }
    }

    private void Update()
    {
        try
        {

            // DOTween の一時停止/再開を制御
            if (currentMoveTween != null && currentMoveTween.IsActive())
            {
                if (Sphere.gamestate.is_stop && !wasStopped)
                {
                    currentMoveTween.Pause();
                    wasStopped = true;
                }
                else if (!Sphere.gamestate.is_stop && wasStopped)
                {
                    currentMoveTween.Play();
                    wasStopped = false;
                }
            }

            if (commandkeyrecv && !Sphere.gamestate.is_gameover && !Sphere.gamestate.is_stop && !Stage.act_start)
            {
                /*
                 manual - プレイヤー操作
                generic - 万能型（回復→アイテム→攻撃→接近）
                rest - その場で待機（アイテム使用→攻撃→何もしない）
                target - 指定ユニットを攻撃（target_unit または target_union を指定）
                destine - 指定座標を目指す（destine_pos を指定）
                keep - 指定座標周辺をキープ（keep_pos を指定、周辺4マス以内）
                guard - 指定ユニットを護衛（guard_unit を指定、周辺3マス以内）
                 */

                if (unitinfo.act_brain == "manual")
                {
                    Vector3 _stage = Stage.GetComponent<RectTransform>().anchoredPosition;
                    int cost = 0;

                    if (Input.GetKey(KeyCode.UpArrow))
                    {
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
                else if (unitinfo.act_brain == "generic")
                {
                    var targetunit = Sphere.getUnitByCode("avatar");
                    if (targetunit != null)
                    {
                        var myPos = new int[] { (int)unitinfo.X, (int)unitinfo.Y };
                        var targetPos = new int[] { (int)targetunit.X, (int)targetunit.Y };
                        int distance = GetManhattanDist(myPos, targetPos);

                        // 最低限のマス以内に到達したら、プレイヤーの位置を直接目標にして体当たり
                        if (distance <= (unitinfo.Info.cost / 10))
                        {
                            // プレイヤーの位置に向かって1マス進む
                            int[] nextPos = GetNextStepToTarget(myPos, targetPos);
                            if (nextPos != null)
                            {
                                // 向きを設定
                                if (nextPos[0] > myPos[0]) this.setAlign(2); // 右
                                else if (nextPos[0] < myPos[0]) this.setAlign(1); // 左
                                else if (nextPos[1] > myPos[1]) this.setAlign(0); // 下
                                else if (nextPos[1] < myPos[1]) this.setAlign(3); // 上

                                // 整数座標で1マス移動（moverateを使わない）
                                unitinfo.X = nextPos[0];
                                unitinfo.Y = nextPos[1];

                                this.setPos(true);
                            }
                        }
                        else
                        {
                            // 攻撃できるマスより遠い場合は、サーバー側の generic ブレインロジック: thinkApproach('nearest')
                            var command = ThinkApproachNearest();
                            if (command != null)
                            {
                                // 移動コマンドを実行
                                if (command.ContainsKey("move"))
                                {
                                    var move = command["move"] as Dictionary<string, object>;
                                    var to = move["to"] as int[];
                                    var path = move["path"] as string;

                                    // 経路の最初の1マスだけ進む（移動可能範囲内で）
                                    if (path != null && path.Length > 0)
                                    {
                                        var currentPos = new int[] { (int)unitinfo.X, (int)unitinfo.Y };
                                        var nextPos = WalkPath(currentPos, path, 1);
                                        if (nextPos != null)
                                        {
                                            // 向きを設定（移動前の位置から移動後の位置へ）
                                            if (nextPos[0] > currentPos[0]) this.setAlign(2); // 右
                                            else if (nextPos[0] < currentPos[0]) this.setAlign(1); // 左
                                            else if (nextPos[1] > currentPos[1]) this.setAlign(0); // 下
                                            else if (nextPos[1] < currentPos[1]) this.setAlign(3); // 上

                                            // 整数座標で1マス移動（moverateを使わない）
                                            unitinfo.X = nextPos[0];
                                            unitinfo.Y = nextPos[1];

                                            this.setPos(true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (unitinfo.act_brain == "rest")
                {
                    var targetunit = Sphere.getUnitByCode("avatar");
                    if (targetunit != null)
                    {
                        var myPos = new int[] { (int)unitinfo.X, (int)unitinfo.Y };
                        var targetPos = new int[] { (int)targetunit.X, (int)targetunit.Y };
                        int distance = GetManhattanDist(myPos, targetPos);

                        if (distance <= 4)
                        {
                            unitinfo.act_brain = "generic";
                        }

                    }
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
                    jsonUnit colunitinfo = Sphere.getUnitByCode("avatar");

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

            if (StarDisp.get() >= StarDispBehaviour.RevengeFireCount)
            {
                StartCoroutine(FireRevenge());
            }

            yield return StartCoroutine(TakeDamage(damage, emeny));
        }
    }

    public IEnumerator TakeDamage(int damage, jsonUnit emeny)
    {
        death = false;
        if (unitinfo.Status.hp - damage <= 0)
        {
            damage = unitinfo.Status.hp;
            death = true;
        }

        unitinfo.Status.hp -= damage;

        HP.show(unitinfo.no);

        if (death)
        {
            if (unitinfo.code == "avatar")
            {
                Sphere.gamestate.is_gameover = true;
                Sphere.gamestate.is_stop = true;
                commandkeyrecv = false;
                //Stage.act_start = true;

                UnitEvent(unitinfo.no, "dam", damage);
                //yield return StartCoroutine(setEffects("dam"));
                yield return StartCoroutine(setEffects("collap"));

                UnitRemove();

                Sphere.GameOver(unitinfo.no, damage);
            }
            else
            {
                if (currentMoveTween != null && currentMoveTween.IsActive())
                {
                    currentMoveTween.Pause();
                }

                UnitEvent(unitinfo.no, "dam", damage);
                //yield return StartCoroutine(setEffects("dam"));
                yield return StartCoroutine(setEffects("collap"));

                // 敵を倒した時の経験値取得（サーバ側のFieldBattleUtil::getFieldReward()に相当）
                int exp = GetFieldReward(unitinfo, emeny);

                // 倒されたユニットの位置から経験値をドロップするため、倒されたユニットのUnitBehaviourから呼び出す
                if (Stage.objUnits.units.ContainsKey("unit_" + unitinfo.no))
                {
                    Stage.objUnits.units["unit_" + unitinfo.no].DropExp(exp);
                }

                UnitRemove();
            }
        }
        else
        {
            UnitEvent(unitinfo.no, "dam", damage);
            StartCoroutine(setEffects("dam"));
        }
    }
    IEnumerator wait_effec()
    {
        while (this.walk_stop)
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

    IEnumerator walk()
    {

        int _frame = align_flame;

        while (true)
        {
            //0.5秒に一回
            yield return new WaitForSeconds(0.5f);

            if (!walk_stop || !Sphere.gamestate.is_stop)
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
            commandkeyrecv = false;
            wasStopped = false;

            // 既存の tween があれば破棄
            if (currentMoveTween != null && currentMoveTween.IsActive())
            {
                currentMoveTween.Kill();
            }

            currentMoveTween = transform.DOLocalMove(vector, movetime).SetEase(Ease.Linear);
            currentMoveTween.OnComplete(() =>
            {
                if (!death)
                {
                    commandkeyrecv = true;
                }

                currentMoveTween = null;
                wasStopped = false;
            });
        }
        else
        {
            transform.localPosition = vector;
        }

        if(unitinfo.code == "avatar")
        {
            Stage.moveX = unitinfo.X;
            Stage.moveY = unitinfo.Y;
            Stage.moveCsr(false);
        }

    }

    public IEnumerator SlashAttack()
    {
        while (true)
        {
            if (attack_flg && !Sphere.gamestate.is_stop && Sphere.gamestate.is_gamestart)
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
        this.walk_stop = true;
        isCancelled = false;

        //サウンド再生。recovはコマンドで鳴らしているので不要。
        switch (_effectName)
        {
            case "dam":
                AudioManager.Instance.PlaySE("se_hit");
                break;
            case "collap":
                HP.hide();
                AudioManager.Instance.PlaySE("se_explosionshort");
                break;
            case "recov":
                AudioManager.Instance.PlaySE("se_repair");
                break;
        }

        Anim.SetBool(_effectName, true);
        //int hashAnim = Animator.StringToHash(_effectName);
        //Anim.Play(hashAnim);

        // アニメーションが実際に開始されるまで1フレーム待つ
        yield return null;
        yield return new WaitForAnimation(Anim, 0);

        walk_stop = false;
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

        // リベンジカウント
        Dictionary<string, int> star = new Dictionary<string, int>();

        star.Add("challenger", 0);
        star.Add("defender", 0);

        // ダメージ初期化。
        Dictionary<string, float> result = new Dictionary<string, float>();
        result.Add("challenger", 0);
        result.Add("defender", 0);

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
                if (damage == -1) { 
                    star[attacker]++;
                    AddStar(challenger.no);
                }
                else if (damage == -2) 
                { 
                    star[defencer]++;
                    AddStar(defender.no);
                }
                else
                {
                    result[defencer] += damage;
                }
            }

            // ダメージがHPを上回ったならそこでストップ。
            if (challenger.Status.hp <= result["challenger"] || defender.Status.hp <= result["defender"])
                break;
        }

        // リターン。
        return result;
    }

    private void AddStar(int unitNo)
    {
        var playerUnit = Sphere.getUnitByCode("avatar");

        if (unitinfo.no == playerUnit.no && playerUnit.Status.hp > 0)
        {

            StarDisp.add();

        }
    }

    private IEnumerator FireRevenge()
    {
        int StarCount = StarDisp.get();

        var playerobj = Stage.objUnits.units["unit_" + unitinfo.no];

        // リベンジオブジェクトの総数（スター数）
        int totalRevengeCount = (int) Mathf.Floor(StarCount / StarDispBehaviour.RevengeConsumeStar);


        Sphere.gamestate.is_stop = true;

        var txt = "リベンジ発動！！";
        Sphere.showPreter(txt, "top");
        StartCoroutine(setEffects("recov"));


        // リベンジオブジェクトの回転半径（RevengeBehaviour.csと同じ値）
        float revengeRadius = Sphere.TIP_SIZE * 0.8f;
        // 退避距離（回転半径より少し外側）
        float retreatDistance = Sphere.TIP_SIZE * 1.5f;

        // プレイヤーの位置を取得
        Vector3 playerPos = playerobj.transform.localPosition;
        float playerCenterX = playerPos.x + Sphere.UNIT_SIZE * 0.5f;
        float playerCenterY = playerPos.y - Sphere.UNIT_SIZE * 0.5f;

        // プレイヤー近くの敵を検出して退避させる
        List<Dictionary<string, object>> retreatingEnemies = new List<Dictionary<string, object>>();
        
        foreach (var kvp in Sphere.sphere.unit)
        {
            var enemyUnit = kvp.Value;
            // プレイヤー自身や既に倒れた敵はスキップ
            if (enemyUnit.code == "avatar" || enemyUnit.X < 0)
                continue;
            
            // 同じ所属（味方）はスキップ
            if (enemyUnit.Info.union == unitinfo.Info.union)
                continue;

            // 敵のUnitBehaviourを取得
            string enemyKey = "unit_" + enemyUnit.no;
            if (!Stage.objUnits.units.ContainsKey(enemyKey))
                continue;
            
            var enemyObj = Stage.objUnits.units[enemyKey];
            Vector3 enemyPos = enemyObj.transform.localPosition;
            float enemyCenterX = enemyPos.x + Sphere.UNIT_SIZE * 0.5f;
            float enemyCenterY = enemyPos.y - Sphere.UNIT_SIZE * 0.5f;

            // プレイヤーからの距離を計算
            float distanceX = enemyCenterX - playerCenterX;
            float distanceY = enemyCenterY - playerCenterY;
            float distance = Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY);

            // リベンジオブジェクトの回転半径より近い場合は退避させる
            if (distance < revengeRadius)
            {
                // 退避先の位置を計算（プレイヤーから離れる方向）
                float angle = Mathf.Atan2(distanceY, distanceX);
                float retreatX = playerCenterX + Mathf.Cos(angle) * retreatDistance;
                float retreatY = playerCenterY + Mathf.Sin(angle) * retreatDistance;

                // 退避先の座標をマップ座標に変換
                float retreatMapX = (retreatX - Sphere.UNIT_SIZE * 0.5f) / Sphere.TIP_SIZE;
                float retreatMapY = ((retreatY + Sphere.UNIT_SIZE * 0.5f) * -1) / Sphere.TIP_SIZE;

                // 0.5刻みにスナップ
                retreatMapX = Mathf.Round(retreatMapX * 2f) / 2f;
                retreatMapY = Mathf.Round(retreatMapY * 2f) / 2f;

                // 敵を退避先に移動
                enemyUnit.X = retreatMapX;
                enemyUnit.Y = retreatMapY;
                enemyObj.setPos(true);

                Vector3 enemyUnitVector = new Vector3(enemyUnit.X * Sphere.TIP_SIZE + margin, (enemyUnit.Y * Sphere.TIP_SIZE + margin) * -1, 0);
                enemyObj.transform.DOLocalMove(enemyUnitVector, movetime / 3).SetEase(Ease.Linear);
            }
        }

        Debug.Log("退避完了");
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < totalRevengeCount; i++)
        {
            // 攻撃カードの決定。
            var card = UnityEngine.Random.Range(1, 3);

            StarDisp.use();

            // リベンジオブジェクトを生成
            GameObject revengeObj = Instantiate(Revenge.gameObject, Stage.transform);
            RevengeBehaviour revengeBehaviour = revengeObj.GetComponent<RevengeBehaviour>();
            if (revengeBehaviour != null)
            {
                // 各リベンジオブジェクトにインデックスと総数を渡して均等に分散させる
                revengeBehaviour.init(playerobj, card, i, totalRevengeCount);
            }
        }
        Debug.Log("revenge ready");
        yield return new WaitForSeconds(0.5f);

        Sphere.Preter.SetActive(false);
        Sphere.gamestate.is_stop = false;

    }

    public void DropExp(int amount)
    {
        var count = amount / expPref.GetExpAmount();
        var playerUnit = Sphere.getUnitByCode("avatar");
        var playerobj = Stage.objUnits.units["unit_" + playerUnit.no];

        var pos = transform.localPosition;

        for (int i = 0; i < count; i++)
        {
            // ランダムオフセットを追加
            float offsetX = UnityEngine.Random.Range(-30f, 30f);
            float offsetY = UnityEngine.Random.Range(-30f, 30f);

            var offset = new Vector3(offsetX, offsetY, 0);

            var expPiece = Instantiate(expPref, Stage.transform);
            expPiece.Setup(playerobj, pos + offset);
        }
    }

    public void DespawnExp(ExpPiece exp)
    {
        exp.gameObject.SetActive(false);
        GameObject.Destroy(exp.gameObject);
    }

    public void AddExp(int value)
    {
        relative_exp += value;
        EXP.show(0, relative_exp, relative_next, unitinfo.Status.level);

        return;
    }

    /// <summary>
    /// フィールド上で敵を倒した時の経験値とお金を計算する（サーバ側のFieldBattleUtil::getFieldReward()に相当）
    /// </summary>
    /// <param name="terminated">倒されたユニットのデータ</param>
    /// <param name="terminator">倒したユニットのデータ</param>
    /// <returns>獲得経験値</returns>
    private int GetFieldReward(jsonUnit terminated, jsonUnit terminator)
    {
        // 経験値は7割（サーバ側のFieldBattleUtil::getFieldReward()と同じ）
        return (int)(GetFullExp(terminator, terminated) * 0.7f);
    }

    /// <summary>
    /// フルターン、フルダメージで勝った時の経験値を計算する（サーバ側のBattleCommon::getFullExp()に相当）
    /// </summary>
    /// <param name="winner">勝ったキャラの情報</param>
    /// <param name="loser">負けたキャラの情報</param>
    /// <returns>フルターン、フルダメージの場合の経験値</returns>
    private float GetFullExp(jsonUnit winner, jsonUnit loser)
    {
        // 基本経験値を取得（固定40）
        float baseExp = GetBaseExp(loser);

        // レベル差による倍率を求める。[(相手の強さ/自分の強さ)の3乗] とする。
        // 「強さ」とはLvに10を足した値とする。
        float winnerStrength = winner.Status.level + 10;
        float loserStrength = loser.Status.level + 10;
        float rate = Mathf.Pow(loserStrength / winnerStrength, 3);

        // 基本経験値に倍率をかけて、完全経験値とする。
        // ただし、倍率は3倍を上限とする。
        return baseExp * Mathf.Min(rate, 3.0f);
    }

    /// <summary>
    /// 基底の経験値計算（サーバ側のBattleCommon::getBaseExp()に相当）
    /// </summary>
    /// <param name="oppositeChara">相手のキャラクター情報</param>
    /// <returns>基本経験値（固定40）</returns>
    private float GetBaseExp(jsonUnit oppositeChara)
    {
        // 基底は固定で40
        return 40;
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

    public int calcRevengeDamage(jsonUnit attacker, jsonUnit defencer, int starCount, double speedBalance, int card_type = 0)
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
            var card = card_type;
            if(card == 0)
                card = UnityEngine.Random.Range(1, 3);

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

    /// <summary>
    /// サーバー側の thinkApproach('nearest') ロジック
    /// 最も近い敵ユニットへの経路を計算して接近する
    /// </summary>
    private Dictionary<string, object> ThinkApproachNearest()
    {
        var myPos = new int[] { (int)unitinfo.X, (int)unitinfo.Y };
        var myUnion = unitinfo.Info.union;
        
        // 移動力
        int movePow = unitinfo.Info.cost; 
        
        // 全敵ユニットへの経路を計算
        Dictionary<int, Dictionary<string, object>> routes = new Dictionary<int, Dictionary<string, object>>();
        
        foreach (var kvp in Sphere.sphere.unit)
        {
            var unit = kvp.Value;
            // 同じ所属のユニットはスキップ
            if (unit.Info.union == myUnion)
                continue;
            
            // そのユニットへの経路と移動コストを取得
            var targetPos = new int[] { (int)unit.X, (int)unit.Y };
            string path = "";
            int cost = GetRoute(myPos, targetPos, ref path);
            
            routes[kvp.Key] = new Dictionary<string, object>
            {
                { "cost", cost },
                { "path", path }
            };
        }
        
        // 最も近い敵（コスト最小）を選択
        Dictionary<string, object> nearestRoute = null;
        int minCost = int.MaxValue;
        int nearestUnitNo = -1;
        
        foreach (var kvp in routes)
        {
            int routeCost = (int)kvp.Value["cost"];
            if (routeCost < minCost)
            {
                minCost = routeCost;
                nearestRoute = kvp.Value;
                nearestUnitNo = kvp.Key;
            }
        }
        
        // 経路がない、あるいは到達不可能な場合はnullを返す
        if (nearestRoute == null || minCost == 0x7FFFFFFF)
            return null;
        
        // 行ける範囲で経路を行く（thinkWalk）
        var moveCommand = ThinkWalk((string)nearestRoute["path"], movePow);
        
        if (moveCommand == null)
            return null;
        
        var command = new Dictionary<string, object>
        {
            { "move", moveCommand }
        };
        
        return command;
    }

    /// <summary>
    /// サーバー側の getRoute() ロジック（A*アルゴリズム）
    /// </summary>
    private int GetRoute(int[] from, int[] to, ref string route)
    {
        route = "";
        
        // 移動元と先が同じという特殊ケースを処理
        if (from[0] == to[0] && from[1] == to[1])
            return 0;
        
        // A*アルゴリズムの実装
        Dictionary<string, AStarNode> map = new Dictionary<string, AStarNode>();
        List<AStarNode> opens = new List<AStarNode>();
        
        // 移動元を無条件にオープン
        var startNode = new AStarNode
        {
            pos = from,
            prev = null,
            cost = 0,
            heur = GetManhattanDist(from, to) * 10
        };
        map[GetPosKey(from)] = startNode;
        opens.Add(startNode);
        
        AStarNode nearest = startNode;
        AStarNode focusSq = null;
        int finalCost = 0x7FFFFFFF;
        
        // ゴールにたどり着くまでループ
        while (true)
        {
            // オープンマスがなくなってしまったら到達はできない
            if (opens.Count == 0)
            {
                focusSq = nearest;
                finalCost = 0x7FFFFFFF;
                break;
            }
            
            // オープンリストから最もコストが低いものを取得
            AStarNode focus = null;
            int focusIndex = -1;
            int minF = int.MaxValue;
            
            for (int i = 0; i < opens.Count; i++)
            {
                int f = opens[i].cost + opens[i].heur;
                if (f < minF)
                {
                    minF = f;
                    focus = opens[i];
                    focusIndex = i;
                }
            }
            
            // 取得したマスをクローズ
            opens.RemoveAt(focusIndex);
            
            // 隣接する4マスを取得
            var neighbors = GetNeighbors(focus.pos);
            
            // 最短経路が複数ある場合になるべく散らばるように、たまに順序を変える
            if (unitinfo.no % 2 == 1)
                neighbors.Reverse();
            
            // 隣接マスを一つずつ処理
            bool goalReached = false;
            foreach (var nei in neighbors)
            {
                if (AStarSquareOpen(map, opens, ref nearest, focus, nei, to))
                {
                    focusSq = map[GetPosKey(to)];
                    finalCost = focusSq.cost;
                    goalReached = true;
                    break;
                }
            }
            
            if (goalReached)
                break;
        }
        
        // 目標マスから親マスをたどって、経路を逆順で作成
        if (focusSq != null)
        {
            var current = focusSq;
            while (current.prev != null)
            {
                int path = 5;
                path += current.pos[0] - current.prev.pos[0];
                path += (current.pos[1] - current.prev.pos[1]) * 3;
                route += path.ToString();
                current = map[GetPosKey(current.prev.pos)];
            }
            
            // 逆順で作成したルートをひっくり返して正順にする
            char[] charArray = route.ToCharArray();
            System.Array.Reverse(charArray);
            route = new string(charArray);
        }
        
        return finalCost;
    }

    /// <summary>
    /// サーバー側の thinkWalk() ロジック
    /// 移動可能範囲内で経路を進む
    /// </summary>
    private Dictionary<string, object> ThinkWalk(string route, int movePow)
    {
        if (route == null || route.Length == 0)
            return null;
        
        var myPos = new int[] { (int)unitinfo.X, (int)unitinfo.Y };
        var movables = GetMovables(myPos, movePow);
        
        // 経路をたどって行けるところまで行く
        string path = route;
        int[] toPoint = WalkPath(myPos, path, movePow, movables);
        
        if (toPoint == null)
            return null;
        
        // そこに別のユニットがいるかどうかチェック
        var unitMap = GetUnitMap();
        if (unitMap.ContainsKey(GetPosKey(toPoint)))
        {
            // 他のユニットがいるなら、そこへは移動できない
            return null;
        }
        
        // 移動先をセットしたコマンドをリターン
        return new Dictionary<string, object>
        {
            { "to", toPoint },
            { "path", path.Substring(0, path.Length - (route.Length - path.Length)) }
        };
    }

    /// <summary>
    /// サーバー側の walk() ロジック
    /// 経路をたどって移動先座標を取得
    /// 経路は文字列で、各文字が方向を表す（"2":上, "4":左, "6":右, "8":下）
    /// </summary>
    private int[] WalkPath(int[] from, string path, int maxSteps = int.MaxValue, Dictionary<string, int> movables = null)
    {
        int[] point = new int[] { from[0], from[1] };
        
        int steps = 0;
        for (int i = 0; i < path.Length && steps < maxSteps; i++)
        {
            int[] prev = new int[] { point[0], point[1] };
            
            // 経路から次の方向を取得（文字列から数値に変換）
            int dir = int.Parse(path[i].ToString());
            
            // 一つ進む（サーバー側の計算式: ($dir-5) % 3 と ($dir-5) / 3）
            // PHPの % 演算子は負の値でもそのまま返すが、C#でも同じ動作
            int dx = (dir - 5) % 3;
            int dy = (dir - 5) / 3;
            
            // PHPの動作に合わせて、dxを-1, 0, 1の範囲に正規化
            if (dx == -1 || dx == 2) dx = -1;
            else if (dx == 1 || dx == -2) dx = 1;
            else dx = 0;
            
            point[0] += dx;
            point[1] += dy;
            
            // 移動可能範囲が指定されている場合、範囲外ならリターン
            if (movables != null && !movables.ContainsKey(GetPosKey(point)))
            {
                return prev;
            }
            
            steps++;
        }
        
        return point;
    }

    /// <summary>
    /// マンハッタン距離を計算
    /// </summary>
    private int GetManhattanDist(int[] from, int[] to)
    {
        return Mathf.Abs(to[0] - from[0]) + Mathf.Abs(to[1] - from[1]);
    }

    /// <summary>
    /// 隣接する4マスを取得
    /// </summary>
    private List<int[]> GetNeighbors(int[] point, int dist = 1)
    {
        List<int[]> result = new List<int[]>();
        
        for (int xDist = -1 * dist; xDist <= dist; xDist++)
        {
            int yDist = dist - Mathf.Abs(xDist);
            result.Add(new int[] { point[0] + xDist, point[1] + yDist });
            if (yDist > 0)
                result.Add(new int[] { point[0] + xDist, point[1] - yDist });
        }
        
        // マップの範囲を超えるマスを削除（簡易実装）
        result.RemoveAll(pos => pos[0] < 0 || pos[1] < 0);
        
        return result;
    }

    /// <summary>
    /// A*アルゴリズムのマスオープン処理
    /// </summary>
    private bool AStarSquareOpen(Dictionary<string, AStarNode> map, List<AStarNode> opens, ref AStarNode nearest, AStarNode prevSq, int[] pos, int[] goal)
    {
        // 踏み込もうとしているマスの移動コストを取得
        int cost = GetCost(pos[0], pos[1]);
        
        // すでにオープンしたことがある場合、それよりも低いコストで踏み込めないなら再オープンしない
        string posKey = GetPosKey(pos);
        if (map.ContainsKey(posKey))
        {
            if (map[posKey].cost <= prevSq.cost + cost)
                return false;
        }
        
        // オープンするかどうか判断する。ただし、到達点である場合はオープンする
        if (pos[0] != goal[0] || pos[1] != goal[1])
        {
            // 踏み込めないマスならオープンしない
            if (cost >= 9990)
                return false;
            
            // 踏み込もうとしている場所にユニットがいないかチェック
            var unitMap = GetUnitMap();
            if (unitMap.ContainsKey(posKey))
            {
                // いる場合に、そのユニットの所属が移動者と違う場合は踏み込めない
                // （簡易実装：ユニットがいる場合は踏み込めない）
                return false;
            }
        }
        
        // ここまで来たらオープンする
        var node = new AStarNode
        {
            pos = pos,
            prev = prevSq,
            cost = prevSq.cost + cost,
            heur = GetManhattanDist(pos, goal) * 10
        };
        map[posKey] = node;
        opens.Add(node);
        
        // 現在の最近傍マスよりもさらに肉薄したなら入れ替える
        if (node.heur < nearest.heur)
            nearest = node;
        
        // 踏み込んだマスはゴールかどうかを返す
        return (pos[0] == goal[0] && pos[1] == goal[1]);
    }

    /// <summary>
    /// 地形コストを取得
    /// </summary>
    private int GetCost(int x, int y)
    {
        string costKey = "cost" + x + "_" + y;
        if (Stage.cost.ContainsKey(costKey))
            return (int)Stage.cost[costKey];
        return 9999; // 到達不能
    }

    /// <summary>
    /// 指定された位置から最も近い通れるマップチップ（コスト9999未満）を見つける
    /// </summary>
    private float[] FindNearestWalkableTile(float x, float y)
    {
        int centerX = Mathf.RoundToInt(x);
        int centerY = Mathf.RoundToInt(y);
        
        // 周囲のマップチップをチェック（最大10マスまで拡大）
        for (int radius = 1; radius <= 10; radius++)
        {
            List<float[]> candidates = new List<float[]>();
            
            // 半径内のすべてのマップチップをチェック
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    // 半径内のマップチップのみ（円形ではなく矩形でチェック）
                    if (Mathf.Abs(dx) == radius || Mathf.Abs(dy) == radius)
                    {
                        int checkX = centerX + dx;
                        int checkY = centerY + dy;
                        
                        // コストをチェック（4隅すべてが9999未満である必要がある）
                        int cost1 = GetCost(checkX, checkY);
                        int cost2 = GetCost(checkX + 1, checkY);
                        int cost3 = GetCost(checkX, checkY + 1);
                        int cost4 = GetCost(checkX + 1, checkY + 1);
                        int maxCost = Mathf.Max(cost1, cost2, cost3, cost4);
                        
                        if (maxCost < 9990)
                        {
                            // ユニットがいるかチェック
                            var unitMap = GetUnitMap();
                            string posKey = GetPosKey(new int[] { checkX, checkY });
                            if (!unitMap.ContainsKey(posKey))
                            {
                                // 距離を計算して候補に追加
                                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                                candidates.Add(new float[] { checkX, checkY, dist });
                            }
                        }
                    }
                }
            }
            
            // 候補がある場合は、最も近いものを返す
            if (candidates.Count > 0)
            {
                float[] nearest = null;
                float minDist = float.MaxValue;
                foreach (var candidate in candidates)
                {
                    if (candidate[2] < minDist)
                    {
                        minDist = candidate[2];
                        nearest = candidate;
                    }
                }
                if (nearest != null)
                {
                    return new float[] { nearest[0], nearest[1] };
                }
            }
        }
        
        // 見つからない場合はnullを返す
        return null;
    }

    /// <summary>
    /// 移動可能範囲を取得（簡易実装）
    /// </summary>
    private Dictionary<string, int> GetMovables(int[] from, int movePow)
    {
        Dictionary<string, int> movables = new Dictionary<string, int>();
        MarkMovables(movables, from, movePow);
        return movables;
    }

    /// <summary>
    /// 移動可能範囲を再帰的にマーク
    /// </summary>
    private void MarkMovables(Dictionary<string, int> movables, int[] point, int pow)
    {
        string key = GetPosKey(point);
        movables[key] = pow;
        
        if (pow <= 0)
            return;
        
        var neighbors = GetNeighbors(point);
        foreach (var nei in neighbors)
        {
            int cost = GetCost(nei[0], nei[1]);
            if (pow >= cost)
            {
                string neiKey = GetPosKey(nei);
                if (!movables.ContainsKey(neiKey) || movables[neiKey] < pow - cost)
                {
                    MarkMovables(movables, nei, pow - cost);
                }
            }
        }
    }

    /// <summary>
    /// ユニットマップを取得
    /// </summary>
    private Dictionary<string, jsonUnit> GetUnitMap()
    {
        Dictionary<string, jsonUnit> unitMap = new Dictionary<string, jsonUnit>();
        foreach (var kvp in Sphere.sphere.unit)
        {
            string key = GetPosKey(new int[] { (int)kvp.Value.X, (int)kvp.Value.Y });
            unitMap[key] = kvp.Value;
        }
        return unitMap;
    }

    /// <summary>
    /// 座標をキー文字列に変換
    /// </summary>
    private string GetPosKey(int[] pos)
    {
        return pos[0] + "_" + pos[1];
    }

    /// <summary>
    /// プレイヤーの位置に向かって1マス進む
    /// 地形コストを考慮し、移動可能な方向を選択
    /// </summary>
    private int[] GetNextStepToTarget(int[] from, int[] to)
    {
        // 目標への方向を計算
        int dx = to[0] - from[0];
        int dy = to[1] - from[1];
        
        // 移動候補（優先順位: 斜め移動を避けて、まずX方向、次にY方向）
        List<int[]> candidates = new List<int[]>();
        var unitMap = GetUnitMap();
        var playerUnit = Sphere.getUnitByCode("avatar");
        int[] playerPos = playerUnit != null ? new int[] { (int)playerUnit.X, (int)playerUnit.Y } : null;
        
        // X方向に移動
        if (dx != 0)
        {
            int[] candidate = new int[] { from[0] + (dx > 0 ? 1 : -1), from[1] };
            int cost = GetCost(candidate[0], candidate[1]);
            if (cost < 9990)
            {
                // プレイヤーの位置の場合は通り抜ける（反対側まで移動）
                if (playerPos != null && candidate[0] == playerPos[0] && candidate[1] == playerPos[1])
                {
                    // プレイヤーの反対側の位置を計算
                    int[] beyondPlayer = new int[] { playerPos[0] + (dx > 0 ? 1 : -1), playerPos[1] };
                    int beyondCost = GetCost(beyondPlayer[0], beyondPlayer[1]);
                    if (beyondCost < 9990)
                    {
                        // 反対側に他のユニットがいないかチェック（プレイヤー以外）
                        string beyondKey = GetPosKey(beyondPlayer);
                        if (!unitMap.ContainsKey(beyondKey) || 
                            (beyondPlayer[0] == playerPos[0] && beyondPlayer[1] == playerPos[1]))
                        {
                            candidates.Add(beyondPlayer);
                        }
                    }
                }
                else
                {
                    // 他のユニットがいるかチェック（プレイヤー以外）
                    string candidateKey = GetPosKey(candidate);
                    if (!unitMap.ContainsKey(candidateKey) || 
                        (playerPos != null && candidate[0] == playerPos[0] && candidate[1] == playerPos[1]))
                    {
                        candidates.Add(candidate);
                    }
                }
            }
        }
        
        // Y方向に移動
        if (dy != 0)
        {
            int[] candidate = new int[] { from[0], from[1] + (dy > 0 ? 1 : -1) };
            int cost = GetCost(candidate[0], candidate[1]);
            if (cost < 9990)
            {
                // プレイヤーの位置の場合は通り抜ける（反対側まで移動）
                if (playerPos != null && candidate[0] == playerPos[0] && candidate[1] == playerPos[1])
                {
                    // プレイヤーの反対側の位置を計算
                    int[] beyondPlayer = new int[] { playerPos[0], playerPos[1] + (dy > 0 ? 1 : -1) };
                    int beyondCost = GetCost(beyondPlayer[0], beyondPlayer[1]);
                    if (beyondCost < 9990)
                    {
                        // 反対側に他のユニットがいないかチェック（プレイヤー以外）
                        string beyondKey = GetPosKey(beyondPlayer);
                        if (!unitMap.ContainsKey(beyondKey) || 
                            (beyondPlayer[0] == playerPos[0] && beyondPlayer[1] == playerPos[1]))
                        {
                            candidates.Add(beyondPlayer);
                        }
                    }
                }
                else
                {
                    // 他のユニットがいるかチェック（プレイヤー以外）
                    string candidateKey = GetPosKey(candidate);
                    if (!unitMap.ContainsKey(candidateKey) || 
                        (playerPos != null && candidate[0] == playerPos[0] && candidate[1] == playerPos[1]))
                    {
                        candidates.Add(candidate);
                    }
                }
            }
        }
        
        // 候補がない場合は、プレイヤーの位置を直接目標にする（接触のため）
        if (candidates.Count == 0 && playerPos != null)
        {
            // プレイヤーの位置に直接移動（接触するため）
            int cost = GetCost(playerPos[0], playerPos[1]);
            if (cost < 9999)
            {
                candidates.Add(playerPos);
            }
        }
        
        // 候補がない場合はnullを返す
        if (candidates.Count == 0)
            return null;
        
        // 最も目標に近い候補を選択
        int[] bestCandidate = null;
        int minDist = int.MaxValue;
        
        foreach (var candidate in candidates)
        {
            int dist = GetManhattanDist(candidate, to);
            if (dist < minDist)
            {
                minDist = dist;
                bestCandidate = candidate;
            }
        }
        
        return bestCandidate;
    }

    /// <summary>
    /// A*アルゴリズム用のノードクラス
    /// </summary>
    private class AStarNode
    {
        public int[] pos;
        public AStarNode prev;
        public int cost;
        public int heur;
    }
}
