using Scenes.Common.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// リベンジアタックを構成するムービー
public class RevengePhaseBehaviour : MonoBehaviour
{
    public GameObject Beam;
    public GameObject Signal;
    public GameObject Square;
    public GameObject Spread;
    public GameObject StarPanel;
    public GameObject StarFlame;

    public LauncherBehaviour Launch0;
    public LauncherBehaviour Launch1;
    public LauncherBehaviour Launch2;
    public LauncherBehaviour Launch3;

    public Dictionary<int, LauncherBehaviour> Launch { set; get; } = new Dictionary<int, LauncherBehaviour>();

    // 発射筒の数。
    public int LAUNCHER_NUM { set; get; } = 4;

    // 発射筒の初回開始の最大遅延秒数
    float DELAY_MAX_SECS { set; get; } = 2.5f;

    // 発射筒の次回開始までの最大遅延秒数
    float REST_MAX_SECS { set; get; } = 1.5f;

    public string side { get; set; }
    public string oppSide { get; set; }

    // Start is called before the first frame update
    public static RevengePhaseBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static RevengePhaseBehaviour instance;

    private void Start()
    {
        instance = this;

        Square.SetActive(false);
    }

    MainPhaseBehaviour MainPhase { get; set; }

    BattleBehaviour Battle { get; set; }

    //
    // リベンジアタックを開始する。
    // 以下の変数をセットした状態で呼び出す。
    //     side    リベンジを発動した側。"P" か "E" で指定する。
    public void RevengeStart()
    {
        Debug.Log("RevengePhaseBehaviour RevengeStart run..");

        Launch[0] = Launch0;
        Launch[1] = Launch1;
        Launch[2] = Launch2;
        Launch[3] = Launch3;

        MainPhase = MainPhaseBehaviour.Instance;
        Battle = BattleBehaviour.Instance;

        // 発動したのと反対の側を取得する。
        oppSide = (side == "P") ? "E" : "P";

        // 非発動側スターストアを非表示に。
        MainPhase.starStore[oppSide].SetActive(false);
        //フレームも非表示
        StarFlame.SetActive(false);

        // 発動側スターストアに増幅をかける。
        //call("../starStore" add side add "/:amp");

        // 発動側のアバタを隠す。
        StartCoroutine(MainPhase.Avatar[side].GetComponent<AvatarBehaviour>().PlayAnim("AvatarHide", 2));
        //Main.Avatar[side].gameObject.SetActive(false);

        // ナビゲータに解説を出させる。まずは迎撃スピードインデックスを取得。
        int spdRate = Battle.battle.spdRate * (side == "P" ? -1 : +1);
        int speed = (int)(spdRate * 100);

        // スピードインデックスに応じてレベルを取得。
        int spdLev = (int)((speed + 105) / 30);
        if (spdLev > 6) spdLev = 6;

        string speed_str = "";
        // ナビに台詞をセットして表示。
        if (speed > 0)
            speed_str = "+" + Mathf.Abs(speed);
        else if (speed < 0)
            speed_str = "-" + Mathf.Abs(speed);
        else
            speed_str = "±" + Mathf.Abs(speed);

        NaviBehaviour.Instance.setText( Utility.getText("TEXT_INTARUPTION_SPEED").Replace("{0}", speed_str)  + "\n" + BattleBehaviour.Instance.NAV_REV[side + spdLev]);
        NaviBehaviour.Instance.setSide("P");
        NaviBehaviour.Instance.Show(1);

        // スプレッドをスターストアに合わせて、効果開始。
        float spread_x = MainPhase.starStore[side].GetComponent<RectTransform>().anchoredPosition.x;
        float spread_y = StarPanel.GetComponent<RectTransform>().anchoredPosition.y - (StarPanel.GetComponent<RectTransform>().rect.height / 2);

        Spread.SetActive(true);
        Spread.GetComponent<RectTransform>().anchoredPosition = new Vector3(spread_x, spread_y);
        Spread.GetComponent<SpreadBehaviour>().PlayAnim("start", RevengeStartStep2);
    }

    //
    // リベンジフェーズのオープンステップの間実行される。
    // スプレッドが開き切るまで待機。
    void RevengeStartStep2(string anim)
    {
        Debug.Log("RevengePhaseBehaviour RevengeStartStep2 run..");

        // RevengePhase自体を座標0にセットする
        transform.gameObject.SetActive(true);
        transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

        // 背景をリベンジ時のものにチェンジ。
        Battle.ShowBg("Rev");

        // 開ききったスプレッドは重いのですぐに停止。
        Spread.SetActive(false);

        // リベンジ陣を所定の位置にセット。
        float square_x = 145 * ((side == "P") ? -1 : 1);
        float square_y = -302.5f;

        Square.GetComponent<RectTransform>().anchoredPosition = new Vector3(square_x, square_y);

        // ビームに起動する側をセットしておく。
        Beam.GetComponent<BeamBehaviour>().SetSide(oppSide);

        // リベンジ陣を表示。
        Square.SetActive(true);

        if (!Battle.Param.tutorial)
        {
            // mainへ。
            StartCoroutine("main" + side);

            // ナビ台詞はここで終わり。
            NaviBehaviour.Instance.Visible(false);
        }
        else
        {
            StartCoroutine(Battle.naviSpeaks(Battle.battle.tutRevP, (() =>
            {
                // mainへ。
                StartCoroutine("main" + side);

            })));
        }

    }

    int bulletNum { get; set; }

    // ストップフラグ
    public bool stopFlg { get; set; } = false;

    //
    // リベンジフェーズのメイン部分。
    IEnumerator mainP()
    {

        Debug.Log("RevengePhaseBehaviour mainP run..");
        // ストップフラグ初期化。
        stopFlg = false;

        // 残弾数を取得。
        bulletNum = MainPhase.starStore[side].GetComponent<StarStoreBehaviour>().value;

        // 各発射筒に対して...
        for (int i = 0; i < LAUNCHER_NUM; i++)
        {

            // 初期化。
            Launch[i].side = side;
            Launch[i].oppSide = oppSide;
            Launch[i].padNo = 0;

            // スタートまでの遅延をセット。
            float randValue = BattleBehaviour.Instance.randomEx.Value();
            Launch[i].restCount = (int)(randValue * DELAY_MAX_SECS * (BattleBehaviour.FRAME_RATE / 2)) + 1;

        }

        // プレイヤーが発動したなら、敵思考ルーチンの動作を開始。
        if (side == "P")
        {
            eBrain.Instance.run();
        }

        // ステップ1へ。
        RunMainPstep2 = true;

        yield return null;
    }

    bool RunMainPstep2 = false;
    bool RunMainEstep2 = false;

    private void FixedUpdate()
    {
        if (RunMainPstep2 && !stopFlg)
            StartCoroutine(mainPstep2());

        if (RunMainEstep2 && !stopFlg)
            StartCoroutine(mainEstep2());

    }

    //
    // メインステップ1。
    // 残弾がなくなるまで、順次発射筒をスタートさせていく。
    IEnumerator mainPstep2()
    {
        Debug.Log("RevengePhaseBehaviour mainPstep2 run..");
        // 一応、終了フラグをチェックする。
        yield return new WaitForSeconds(Main.Instance.getParFrame());

        // すべての発射筒に対して...
        for (int i = 0; i < LAUNCHER_NUM; i++)
        {
            // 停止中ならば。
            if (Launch[i].padNo == 0)
            {

                // 停止カウントをダウン。0になったら...
                Launch[i].restCount--;
                if (Launch[i].restCount == 0)
                {
                    // 発射動作を開始させる。
                    StartCoroutine(setLaunch(i));

                    // 残弾数ダウン。0になったら停止。
                    // あとは発射筒等から起こるイベント等で処理する。
                    if (--bulletNum == 0)
                    {
                        RunMainPstep2 = false;
                        yield break;
                    }
                }
            }
        }
    }

    //
    // リベンジフェーズのメイン部分。
    IEnumerator mainE()
    {

        Debug.Log("RevengePhaseBehaviour mainE run..");
        // ストップフラグ初期化。
        stopFlg = false;

        // 残弾数を取得。
        bulletNum = MainPhase.starStore[side].GetComponent<StarStoreBehaviour>().value;

        // 各発射筒に対して...
        for (int i = 0; i < LAUNCHER_NUM; i++)
        {
            // 初期化。
            Launch[i].side = side;
            Launch[i].oppSide = oppSide;
            Launch[i].padNo = 0;

            // スタートまでの遅延をセット。
            float randValue = BattleBehaviour.Instance.randomEx.Value();
            Launch[i].restCount = (int)(randValue * DELAY_MAX_SECS * (BattleBehaviour.FRAME_RATE / 2)) + 1;

        }

        if (Battle.auto_flg)
        {
            //オートの場合はプレイヤー思考ルーチン動作
            pBrain.Instance.run();
        }

        // ステップ1へ。
        RunMainEstep2 = true;

        yield return null;
    }

    //
    // メインステップ1。
    // 残弾がなくなるまで、順次発射筒をスタートさせていく。
    IEnumerator mainEstep2()
    {
        Debug.Log("RevengePhaseBehaviour mainEstep2 run..");

        // 一応、終了フラグをチェックする。
        yield return new WaitForSeconds(Main.Instance.getParFrame());
        // すべての発射筒に対して...
        for (int i = 0; i < LAUNCHER_NUM; i++)
        {
            // 停止中ならば。
            if (Launch[i].padNo == 0)
            {
                // 停止カウントをダウン。0になったら...
                Launch[i].restCount--;
                if (Launch[i].restCount == 0)
                {
                    // 発射動作を開始させる。
                    StartCoroutine(setLaunch(i));

                    // 残弾数ダウン。0になったら停止。
                    // あとは発射筒等から起こるイベント等で処理する。
                    if (--bulletNum == 0)
                    {
                        RunMainEstep2 = false;
                        yield break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 変数launcherで示された発射筒の発射動作を開始する。
    /// </summary>
    /// <returns></returns>
    IEnumerator setLaunch(int lnch)
    {
        Debug.Log("RevengePhaseBehaviour setLaunch run..");
        // 問題なくセットできるまで試行し続ける。
        while (true)
        {
            yield return new WaitForSeconds(Main.Instance.getParFrame());

            // 乱数で1～9を取得。発射台番号として使用する。
            float randValue = BattleBehaviour.Instance.randomEx.Value();
            int no = (int)(randValue * 9) + 1;

            // いずれかの発射筒が同じ番号を使用中でないかチェック。
            bool used = false;
            for (int s = 0; s < LAUNCHER_NUM; s++)
            {
                if (Launch[s].padNo == no)
                {
                    used = true;
                    break;
                }
            }

            // いずれも使用していないなら...
            if (!used)
            {
                Debug.Log("RevengePhaseBehaviour setLaunch ok.. no = " + no);
                // セット。
                Launch[lnch].padNo = no;
                Launch[lnch].LaunchStart();

                // 敵思考ルーチンにセットされたことを伝える。
                eBrain.Instance.launcher = lnch;
                eBrain.Instance.notify();

                if (Battle.auto_flg)
                {
                    // プレイヤー思考ルーチンにセットされたことを伝える。
                    pBrain.Instance.launcher = lnch;
                    pBrain.Instance.notify();
                }
                // ループを抜ける。
                yield break;
            }
        }
    }

    //
    // 発射筒の処理が完了したらcallされる。
    // 変数 firer にはこのラベルをcallした発射筒の_nameが代入されている。
    public IEnumerator launchFin(int firer)
    {
        Debug.Log("RevengePhaseBehaviour launchFin run..");
        // 発射台を開放するため、padNo をクリア。
        Launch[firer].padNo = 0;

        if (HpGaugeBehaviour.Instance.HpInfo[oppSide].value <= 0)
        {
            // リベンジを受ける側のHPが0になったら終了。
            StartCoroutine(Kill());
        }
        else if (bulletNum <= 0)
        {
            // すでに残弾がない場合。

            // すべての発射筒が処理を完了しているかどうか調べる。
            bool allFin = true;
            for (int i = 0; i < LAUNCHER_NUM; i++)
            {
                if (Launch[i].padNo != 0)
                {
                    allFin = false;
                    break;
                }
            }

            // すべて処理を完了しているなら、終了処理に入る。
            if (allFin)
                StartCoroutine(Kill());

        }
        else
        {

            // まだ残弾がある場合。
            // 発射筒の一時停止カウントをランダムでセット。
            float randValue = BattleBehaviour.Instance.randomEx.Value();
            Launch[firer].restCount = (int)(randValue * REST_MAX_SECS * (BattleBehaviour.FRAME_RATE / 2)) + 1;
        }

        yield break;
    }

    float waitCount { get; set; } = 0;

    //
    // すべての活動が終了したとき、あるいはリベンジ中に相手のHPが
    // 0になった場合にcallされる。
    IEnumerator Kill()
    {

        Debug.Log("RevengePhaseBehaviour Kill run..");
        // まだ終了処理を行っていないなら処理する。
        if (!stopFlg)
        {
            // 終了済みのフラグを立てる。
            stopFlg = true;

            // 待機カウントをセットしてwaitフレームへ。
            waitCount = 0.75f;

            yield return new WaitForSeconds(waitCount);

            // 指定されたフレーム数待機。待機が完了したら...
            StartCoroutine(Close());
        }

    }

    //
    // リベンジフェーズのクローズステップ1。
    IEnumerator Close()
    {
        Debug.Log("RevengePhaseBehaviour Close run..");
        // 非発動側スターストアを表示に戻す。
        MainPhase.starStore[oppSide].SetActive(true);

        //フレームも表示
        StarFlame.SetActive(true);

        // ナビゲータを消去。
        NaviBehaviour.Instance.Visible(false);

        // スプレッドを再表示
        Spread.SetActive(true);
        // 背景を元に戻す。
        Battle.ShowBg("Norm");
        // リベンジ陣を非表示に。
        Square.SetActive(false);
        //魔法陣を非表示に。
        Signal.SetActive(false);

        //スプレッドを縮小。
        Spread.GetComponent<SpreadBehaviour>().PlayAnim("reverse", reverseEnd);

        yield return null;
    }

    void reverseEnd(string anim)
    {
        // spredを非表示に。
        Spread.SetActive(false);

        // 発動側のアバタを戻す。
        //Main.Avatar[side].gameObject.SetActive(true);
        StartCoroutine(MainPhase.Avatar[side].GetComponent<AvatarBehaviour>().PlayAnim("AvatarAppear", 2));

        //チュートリアルの場合・・
        if (Battle.Param.tutorial)
        {
            // プレイヤー側リベンジが終わった場合...
            if (side == "P")
            {
                string endstr;
                // リベンジヒット数に応じてセリフを差し替える。
                if (HpGaugeBehaviour.Instance.HpInfo[oppSide].value <= 0)
                {
                    endstr = Battle.battle.tutClose2;
                }
                else if (Battle.stat["R"].hitCnt["P"] >= 6)
                {
                    endstr = Battle.battle.tutClose1;
                }
                else
                {
                    endstr = Battle.battle.tutClose0;
                }
                Battle.battle.tutClose[0] = endstr;

                //リベンジ説明が終わったらBGMを止めてバトル終了ジングルを流す
                //BGMを止めてバトル終了ジングルを流す
                AudioManager.Instance.StopBGM();
                AudioManager.Instance.PlaySE("se_battle_end");

                // ナビに喋らせる
                StartCoroutine(Battle.naviSpeaks(Battle.battle.tutClose, (() =>
                {
                    transform.gameObject.SetActive(false);
                    ClosePhaseBehaviour.Instance.tutorialEnd();
                })));
            }
            else
            {
                // 親のprogEndフレームをcallする。
                MainPhase.ProgEnd();
            }
        }
        else
        {
            transform.gameObject.SetActive(false);

            // 親のprogEndフレームをcallする。
            MainPhase.ProgEnd();

        }
    }

    //
    // ユーザが発射or迎撃のボタンを押したら呼ばれる。
    // 変数 btnNo にはどの数字キーを押したかが格納されている。
    public void onButton(int btnNo)
    {
        Debug.Log("RevengePhaseBehaviour onButton run.. btnNo=" + btnNo);
        if (Battle.auto_flg == false)
        {
            // ユーザがリベンジを発動している場合は発射。
            if (side == "P")
            {
                //魔法陣を表示。
                Signal.SetActive(true);

                // リップルがエフェクト中でないならば行う。
                if (!Signal.GetComponent<SignalBehaviour>().motion)
                {
                    // リップル発生座標を取得。
                    Vector3 _dest = this.getCirclePos(btnNo);
                    Signal.GetComponent<RectTransform>().anchoredPosition = new Vector3(_dest.x, _dest.y, 0);

                    // 何番に対して発射命令を出したのかを保持させる。
                    Signal.GetComponent<SignalBehaviour>().target = btnNo;

                    // 発射
                    StartCoroutine(Signal.GetComponent<SignalBehaviour>().PlayAnim(fire));
                }
            }
            else
            {
                // ユーザがリベンジを発動されている場合は迎撃。
                StartCoroutine(intercept(btnNo));
            }
        }
    }

    //
    // 変数 targetNo で指定された目標に向かって、迎撃ビームを発射する。
    public IEnumerator intercept(int targetNo)
    {
        Debug.Log("RevengePhaseBehaviour intercept run..");
        // ビーム発射
        Beam.GetComponent<BeamBehaviour>().targetNo = targetNo;
        Beam.GetComponent<BeamBehaviour>().fire(blast);

        yield return null;
    }
    //
    // ビーム到達後にcallされる。
    void blast()
    {
        Debug.Log("RevengePhaseBehaviour blast run..");
        int i = 0;
        // ビームのターゲットになっている発射台で動作している発射筒番号を
        // 変数 i に取得する。
        for (i = 0; i < LAUNCHER_NUM; i++)
        {
            if (Launch[i].padNo == Beam.GetComponent<BeamBehaviour>().target)
            {
                break;
            }
        }

        // 見つかったなら爆発フラグをセット。
        if (i < LAUNCHER_NUM)
        {
            AudioManager.Instance.PlaySE("se_damage");
            Launch[i].explodeFlg = true;
        }
    }

    //
    // 発射シグナル送信後にcallされる。
    void fire()
    {
        Debug.Log("RevengePhaseBehaviour fire run..");
        int i = 0;
        // 発射シグナルのターゲットになっている発射台で動作している発射筒番号を
        // 変数 i に取得する。
        for (i = 0; i < LAUNCHER_NUM; i++)
        {
            if (Launch[i].padNo == Signal.GetComponent<SignalBehaviour>().target)
            {
                break;
            }
        }

        // 見つかったなら爆発フラグをセット。
        if (i < LAUNCHER_NUM)
        {
            Launch[i].fireFlg = true;
        }
    }

    public Vector3 getCirclePos(int no)
    {
        Debug.Log("RevengePhaseBehaviour getCirclePos run.. no=" + no);

        Vector3 _square_pos = Square.GetComponent<RectTransform>().anchoredPosition;
        Rect _square_rect = Square.GetComponent<RectTransform>().rect;

        Rect _circle_rect = Square.transform.Find("circle1").GetComponent<RectTransform>().rect;

        float x_margin = (_square_rect.width / 2 * -1) + _square_pos.x + (_circle_rect.width / 2);
        float y_margin = _square_pos.y - (_circle_rect.height / 2);

        float _x = x_margin + (((no - 1) % 3) * ((_square_rect.width - _circle_rect.width) / 2));
        float _y = y_margin - ((int)((no - 1) / 3) * ((_square_rect.height - _circle_rect.height) / 2));

        Debug.Log("RevengePhaseBehaviour getCirclePos _x=" + _x + " _y=" + _y);

        return new Vector3(_x, _y, 0);
    }

}
