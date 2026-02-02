using DG.Tweening;
using Scenes.Common.Scripts;
using StateManager;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------
// メッセージウィンドウやユーザ制御を除く、ステージ全体を管理する
//----------------------------------------------------------------------
public class StageBehaviour : BaseBehaviour
{

    public int unitNo { get; set; }

    // スクロールの速さ
    public int SCROLL_SPD { get; set; }
    public int SCROLL_SPD_HI { get; set; }

    public float offsetX { get; set; } = 0;
    public float offsetY { get; set; } = 0;

    //タッチイベント用。actで操作した座標を格納しておく。
    public float _offsetX { get; set; } = 0;
    public float _offsetY { get; set; } = 0;

    // カーソルを0-0へ。
    public float moveX { get; set; } = 0;
    public float moveY { get; set; } = 0;

    public float cursorX { get; set; } = 0;
    public float cursorY { get; set; } = 0;

    public int slideX { get; set; } = 0;
    public int slideY { get; set; } = 0;


    public int lef { get; set; }
    public int top { get; set; }
    public int rig { get; set; }
    public int bot { get; set; }

    public bool scrolling { get; set; } = false;

    //Sphereインスタンス
    SphereBehaviour Sphere { get; set; }
    UserBehaviour User { get; set; }

    //MapTipオブジェクト
    public MapTip objMapTip { get; set; }
    //Units オブジェクト
    public Units objUnits { get; set; }
    //Units オブジェクト
    public RangeMarker objMarker { get; set; }
    public MassEffect objEffects { get; set; }
    public Ornaments objOrnaments { get; set; }

    //当該キーに対応するチップIDを格納している。key:nox_y
    public Dictionary<string, string> no { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, int> cost { get; set; } = new Dictionary<string, int>();


    public static StageBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static StageBehaviour instance;
    public Image _cursor { get; set; } = null;
    TouchManager _touch_manager { get; set; } = null;

    // Start is called before the first frame update
    protected override void Start()
    {
        //base.Start();

        Debug.Log("StageBehaviour Start running..");
        instance = this;
        objMapTip = new MapTip();
        objUnits = new Units();
        objMarker = new RangeMarker();
        objEffects = new MassEffect();
        objOrnaments = new Ornaments();

        // タッチ管理マネージャ生成
        this._touch_manager = new TouchManager();

        _cursor = transform.Find("cursor").GetComponent<Image>();
    }

    private float touchstartX { get; set; }
    private float touchstartY { get; set; }
    private float _x { get; set; }
    private float _y { get; set; }

    private float _touchX { get; set; } = 0;
    private float _touchY { get; set; } = 0;

    public bool left_over { get; set; }
    public bool right_over { get; set; }
    public bool top_over { get; set; }
    public bool bottom_over { get; set; }

    public bool act_start { get; set; } = false;

    public void init()
    {
        Debug.Log("StageBehaviour init running..");

        Sphere = SphereBehaviour.Instance;
        User = UserBehaviour.Instance;

        Sphere.sphere_bg.SetActive(false);

        // コマンドユニットにカーソルを合わせる。

        // スクロールの速さ
        SCROLL_SPD_HI = 36;
        SCROLL_SPD = 22;

        // ルーム内各マスの情報を取得しておく。
        // 以下の変数がセットされる(X, Yは可変。マスの座標)。
        //     noX_Y    そのマスの地形番号
        //     costX_Y  そのマスの移動コスト
        string structure = "stage";
        for (int x = 0; x < Sphere.sphere.structWid; x++)
        {
            for (int y = 0; y < Sphere.sphere.structHei; y++)
            {
                this.readbg(structure, x, y);
            }
        }

        structure = "background";
        for (int x = 0; x < Sphere.sphere.backgroundWid; x++)
        {
            for (int y = 0; y < Sphere.sphere.backgroundHei; y++)
            {
                this.readbg(structure, x, y);
            }
        }

        structure = "overlayer1";
        for (int x = 0; x < Sphere.sphere.overlayer1Wid; x++)
        {
            for (int y = 0; y < Sphere.sphere.overlayer1Hei; y++)
            {
                this.readbg(structure, x, y);
            }
        }

        structure = "overlayer2";
        for (int x = 0; x < Sphere.sphere.overlayer2Wid; x++)
        {
            for (int y = 0; y < Sphere.sphere.overlayer2Hei; y++)
            {
                this.readbg(structure, x, y);
            }
        }

        structure = "cover";
        for (int x = 0; x < Sphere.sphere.coverWid; x++)
        {
            for (int y = 0; y < Sphere.sphere.coverHei; y++)
            {
                this.readbg(structure, x, y);
            }
        }

        structure = "head";
        for (int x = 0; x < Sphere.sphere.headWid; x++)
        {
            for (int y = 0; y < Sphere.sphere.headHei; y++)
            {
                this.readbg(structure, x, y);
            }
        }

        structure = "left";
        for (int x = 0; x < Sphere.sphere.leftWid; x++)
        {
            for (int y = 0; y < Sphere.sphere.leftHei; y++)
            {
                this.readbg(structure, x, y);
            }
        }
        structure = "right";
        for (int x = 0; x < Sphere.sphere.rightWid; x++)
        {
            for (int y = 0; y < Sphere.sphere.rightHei; y++)
            {
                this.readbg(structure, x, y);
            }
        }
        structure = "foot";
        for (int x = 0; x < Sphere.sphere.footWid; x++)
        {
            for (int y = 0; y < Sphere.sphere.footHei; y++)
            {
                this.readbg(structure, x, y);
            }
        }

        //マップチップ作成
        StartCoroutine(objMapTip.Init(transform.Find("tip").GetComponent<Image>()));
        //敷物作成
        objOrnaments.Init(transform.Find("orns").GetComponent<OrnamentBehaviour>());
        //ユニット作成
        StartCoroutine(objUnits.Init(transform.Find("units").GetComponent<UnitBehaviour>()));
        //レンジマーカー作成
        StartCoroutine(objMarker.Init(transform.Find("marker").GetComponent<Image>()));
        //マスエフェクト初期化
        objEffects.Init(transform.Find("effects").GetComponent<EffectsBehaviour>());

        //マップチップ作成
        StartCoroutine(objMapTip.refinfo());

        objMapTip.setCost();

        // あるべき画面位置をチップ単位で表している変数を初期化。
        offsetX = 0;
        offsetY = 0;

        //タッチイベント用座標を初期化
        _offsetX = 0;
        _offsetY = 0;

        // カーソルを0-0へ。
        moveX = 0;
        moveY = 0;
        this.moveCsr();

        // カーソルを非表示に。
        _cursor.enabled = false;

        //battle_movie._visible = false;

        // 主人公ユニットにカーソルを合わせる。
        //TODO::主人公ユニットが必ずしも1とは限らない・・
        unitNo = 1;
        this.focus();

        // カーソルを追随して画面を動かすようにする。
        act_start = true;
    }

    int actX;
    int actY;
    float csrActX;
    float csrActY;
    int speed;
    public int vib { get; set; } = 0;

    //
    // 変数offsetX, offsetY で示された場所に、ステージを
    // 一定スピードで移動させる。
    void FixedUpdate()
    {
        if (!act_start) return;

        if (User.flick_flg == false)
        {
            //Debug.Log("act run..");

            Vector3 _stage = transform.GetComponent<RectTransform>().anchoredPosition;

            // offsetX, offsetY はチップ単位の座標になっているので、ピクセル単位に直す。
            actX = (int)offsetX * Sphere.TIP_SIZE;
            actY = (int)offsetY * Sphere.TIP_SIZE * -1;

            // X座標の修正。
            if (_stage.x != actX)
            {

                // カーソルが画面外から出ているかどうかで、スクロールスピードを切り替える。
                csrActX = cursorX * Sphere.TIP_SIZE;
                if (csrActX + _stage.x < 0 || csrActX + _stage.x + Sphere.TIP_SIZE > Sphere.STAGE_WID)
                    speed = SCROLL_SPD_HI;
                else
                    speed = SCROLL_SPD;

                transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(Sphere.propMove(actX, (int)_stage.x, speed), _stage.y, _stage.z);
                _stage = transform.GetComponent<RectTransform>().anchoredPosition;

                _offsetX = actX;
            }

            // Y座標の修正。
            if (_stage.y != actY)
            {

                csrActY = cursorY * Sphere.TIP_SIZE * -1;
                if (csrActY + _stage.y < 0 || csrActY + _stage.y + Sphere.TIP_SIZE > Sphere.STAGE_HEI)
                    speed = SCROLL_SPD_HI;
                else
                    speed = SCROLL_SPD;

                transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(_stage.x, Sphere.propMove(actY, (int)_stage.y, speed), _stage.z);
                _stage = transform.GetComponent<RectTransform>().anchoredPosition;

                _offsetY = actY * -1;
            }

            // スクロール中かどうかを表すフラグを設定
            if ((int)_stage.x == actX && (int)_stage.y == actY)
            {
                scrolling = false;
                User.actX = actX;
                User.actY = actY;

                //Sphere.gamestate.is_stop = false;
                act_start = false;

            }

            // 振動が設定されている場合は処理する。
            if (vib != 0)
            {
                _stage.x += vib;
                transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(_stage.x, _stage.y, _stage.z);
                vib *= -1;
            }
        }
    }

    private void Update()
    {

        // Escキーでポーズ/ポーズ解除（ゲームオーバー中は無効）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Sphere.gamestate.is_gameover)
            {
                // ゲームオーバー中は何もしない
                return;
            }
            if (Sphere.gamestate.is_pause == false)
            {
                Sphere.gamestate.is_pause = true;
                // ポーズ中はEscキーでポーズ解除
                Sphere.showPreter("Esc:pause解除", "center");
                Time.timeScale = 0.0f;
                AudioManager.Instance.PauseBGM();
                //User.objMC.BtnExitClick();
            }
            else
            {
                Sphere.gamestate.is_pause = false;
                // ポーズ中でない場合はポーズを開く
                Sphere.Preter.SetActive(false);
                Time.timeScale = 1f;
                AudioManager.Instance.UnPauseBGM();
                //User.OkCancel.onCancel();
            }
        }

        return;

        if (User == null) return;
        if (User.objPointR == null) return;

        // タッチ状態更新
        this._touch_manager.update();

        // タッチ取得
        TouchManager touch_state = this._touch_manager.getTouch();

        // タッチされていたら処理
        if (touch_state._touch_flag)
        {
            //Debug.Log("touch ok.. x=" + touch_state._touch_position.x + " y=" + touch_state._touch_position.y);

            float touch_x = (touch_state._touch_position.x) / Screen.width * transform.GetComponent<RectTransform>().rect.width;
            float touch_y = ((touch_state._touch_position.y - Screen.height) / Screen.height * transform.GetComponent<RectTransform>().rect.height) * -1;

            //Debug.Log("touch_info.. x=" + touch_x + " y=" + touch_y);


            // タッチした瞬間の処理
            if (touch_state._touch_phase == TouchPhase.Began)
            {
                //タッチ開始座標をとっておく
                touchstartX = touch_x;
                touchstartY = touch_y;

                //開始時点の_offset値をとっておく
                _x = _offsetX;
                _y = _offsetY;


                //座標を渡しておく
                User.objPointR.touchstartX = touch_x;
                User.objPointR.touchstartY = touch_y;

                //User.objPointR.onTouchStart();

            }
            else if (touch_state._touch_phase == TouchPhase.Moved)
            {
                //flick_lockがかかってる場合はリターン
                if (User.flick_lock) return;

                var touchX = _x + ((touch_x - touchstartX) * (Sphere.STAGE_WID / transform.GetComponent<RectTransform>().rect.width));
                var touchY = _y + ((touch_y - touchstartY) * (Sphere.STAGE_WID / transform.GetComponent<RectTransform>().rect.width));

                //右に進んでいる場合
                if (this._touchX > touchX)
                {
                    if (this.right_over == true)
                        touchX = this._touchX;
                    //右に進んでいるのに左ロックがかかっている場合、左ロックは解除
                    if (this.left_over == true)
                        this.left_over = false;
                    //左に進んでいる場合
                }
                else
                {
                    //これ以上進めない場合
                    if (this.left_over == true)
                        touchX = this._touchX;
                    //左に進んでいるのに右ロックがかかっている場合、右ロックは解除
                    if (this.right_over == true)
                        this.right_over = false;
                }

                //下に進んでいる場合
                if (this._touchY > touchY)
                {
                    //これ以上進めない場合
                    if (this.bottom_over == true)
                        touchY = this._touchY;
                    //下に進んでいるのに上ロックがかかっている場合、上ロックは解除
                    if (this.top_over == true)
                        this.top_over = false;
                    //上に進んでいる場合
                }
                else
                {
                    //これ以上進めない場合
                    if (this.top_over == true)
                        touchY = this._touchY;
                    //上に進んでいるのに下ロックがかかっている場合、下ロックは解除
                    if (this.bottom_over == true)
                        this.bottom_over = false;
                }

                this._touchX = touchX;
                this._touchY = touchY;

                User.objPointR.gainX = (int)touchX;
                User.objPointR.gainY = (int)touchY;

                //フリックスタートフラグ
                User.flick_flg = true;
                //フリックする
                User.objPointR.onFlick();

                //_offset値を変更する
                _offsetX = touchX;
                _offsetY = touchY;
            }
            else if (touch_state._touch_phase == TouchPhase.Ended)
            {
                User.objPointR.touchendX = touch_x;
                User.objPointR.touchendY = touch_y;

                User.objPointR.onTouchEnd();

            }
        }
    }

    // 
    // 変数 x, y で示された座標のルーム構造を読み込む。
    void readbg(string kind, int x, int y)
    {
        //Debug.Log("readbg running..");
        if (kind == "stage")
        {
            int _no = int.Parse(Sphere.sphere.structs[y].Substring(x * 4, 4));
            if (Sphere.sphere.tip[_no] != null && int.Parse(Sphere.sphere.tip[_no]) > 0)
            {
                no["no" + x + "_" + y] = Sphere.sphere.tipId[_no];
                cost["cost" + x + "_" + y] = Sphere.sphere.tip[_no] != null ? int.Parse(Sphere.sphere.tip[_no]) : 0;
            }
        }
        else if (kind == "background")
        {
            int _no = int.Parse(Sphere.sphere.structbackground[y].Substring(x * 4, 4));
            no["no" + kind + x + "_" + y] = Sphere.sphere.tipId[_no];
            //コストは設定しない。常に通れない。
            //cost["cost" + kind + x + "_" + y] = 9999;
        }
        else if (kind == "overlayer1")
        {
            int _no = int.Parse(Sphere.sphere.structoverlayer1[y].Substring(x * 4, 4));
            if (Sphere.sphere.tip[_no] != null && int.Parse(Sphere.sphere.tip[_no]) > 0)
            {
                no["no" + kind + x + "_" + y] = Sphere.sphere.tipId[_no];
                //コストだけ上書き
                cost["cost" + x + "_" + y] = int.Parse(Sphere.sphere.tip[_no]);
            }
        }
        else if (kind == "overlayer2")
        {
            int _no = int.Parse(Sphere.sphere.structoverlayer2[y].Substring(x * 4, 4));
            if (Sphere.sphere.tip[_no] != null && int.Parse(Sphere.sphere.tip[_no]) > 0)
            {
                no["no" + kind + x + "_" + y] = Sphere.sphere.tipId[_no];
                //コストだけ上書き
                cost["cost" + x + "_" + y] = int.Parse(Sphere.sphere.tip[_no]);
            }
        }
        else if (kind == "cover")
        {
            int _no = int.Parse(Sphere.sphere.structcover[y].Substring(x * 4, 4));

            if (Sphere.sphere.tip[_no] != null && int.Parse(Sphere.sphere.tip[_no]) > 0)
            {
                no["no" + kind + x + "_" + y] = Sphere.sphere.tipId[_no];
                //コストは設定しない。常に通れる。
                //cost["cost" + kind + x + "_" + y] = 10;
            }
        }
        else
        {
            int _no = 0;
            if (kind == "head")
                _no = int.Parse(Sphere.sphere.structhead[y].Substring(x * 4, 4));
            else if (kind == "left")
                _no = int.Parse(Sphere.sphere.structleft[y].Substring(x * 4, 4));
            else if (kind == "right")
                _no = int.Parse(Sphere.sphere.structright[y].Substring(x * 4, 4));
            else if (kind == "foot")
                _no = int.Parse(Sphere.sphere.structfoot[y].Substring(x * 4, 4));


            no["no" + kind + x + "_" + y] = Sphere.sphere.tipId[_no];
            cost["cost" + kind + x + "_" + y] = Sphere.sphere.tip[_no] != null ? int.Parse(Sphere.sphere.tip[_no]) : 0;
        }
    }

    //
    // カーソルを変数 moveX, moveY で示された場所へ移動する。
    public void moveCsr(bool is_center = true)
    {
        Debug.Log("moveCsr running...");

        // 移動。
        cursorX = moveX;
        cursorY = moveY;

        // 反映。
        _cursor.transform.localPosition = new Vector3(cursorX * Sphere.TIP_SIZE, cursorY * Sphere.TIP_SIZE * -1);

        if (is_center)
        {
            // カーソルが画面内に収まるようにオフセットを設定し直す。
            lef = Sphere.STAGE_MARGIN;
            top = Sphere.TOP_MARGIN;
            rig = Sphere.STAGE_MARGIN;
            bot = Sphere.BOTTOM_MARGIN;
            this.center();
        }

    }

    //
    // カーソルを変数 slideX, slideY で示された量だけ移動する。
    public void slideCsr()
    {

        Debug.Log("slideCsr running...");

        moveX = cursorX + slideX;
        moveY = cursorY + slideY;

        // 範囲外へ出ないように補正する。
        if (moveX < 0) moveX = 0;
        else if (moveX >= Sphere.sphere.structWid) moveX = Sphere.sphere.structWid - 1;
        if (moveY < 0) moveY = 0;
        else if (moveY >= Sphere.sphere.structHei) moveY = Sphere.sphere.structHei - 1;

        // ちゃんと動いているなら動かす。
        if (cursorX != moveX || cursorY != moveY)
        {
            // 移動。
            cursorX = moveX;
            cursorY = moveY;

            // 反映。
            _cursor.transform.localPosition = new Vector3(cursorX * Sphere.TIP_SIZE, cursorY * Sphere.TIP_SIZE * -1);
        }
    }

    //
    // 現在のステージオフセットとカーソル位置を比較して、
    // カーソルが画面端に来すぎないようにオフセットを調節する。
    // 変数 lef, top, rig, bot で画面端のマージンをチップ単位で指定する。
    public void center()
    {
        Debug.Log("center running...");

        float stage_wid = Sphere.STAGE_WID / Sphere.TIP_SIZE;
        float stage_hei = Sphere.STAGE_HEI / Sphere.TIP_SIZE;


        if (cursorX + offsetX < lef)
        {
            offsetX = lef - cursorX;
        }
        else if (offsetX > lef)
        {
            //コマンド入力切り替え時はマージンが4→2になる。その分を戻す。
            offsetX = lef;
        }

        if (cursorY + offsetY < top)
        {
            offsetY = top - cursorY;
        }
        else if (offsetY > top)
        {
            //コマンド入力切り替え時はマージンが4→2になる。その分を戻す。
            offsetY = top;
        }

        if (cursorX + offsetX >= stage_wid - rig)
        {
            offsetX = stage_wid - rig - cursorX - 1;
        }
        else if (Sphere.sphere.structWid + offsetX < stage_wid - rig)
        {
            //コマンド入力切り替え時はマージンが4→2になる。その分を戻す。
            offsetX = stage_wid - rig - cursorX - 1;
        }

        if (cursorY + offsetY >= stage_hei - bot)
        {
            offsetY = stage_hei - bot - cursorY - 1;
        }

        // スクロール中かどうかを表すフラグを初期化。
        scrolling = true;

        //Sphere.gamestate.is_stop = true;
        act_start = true;

    }

    //
    // 変数 unitNo で示されたユニットにカーソルを合わせる
    public void focus()
    {
        if (!Sphere.sphere.unit.ContainsKey(unitNo)) return;

        jsonUnit unitinfo = Sphere.sphere.unit[unitNo];

        moveX = unitinfo.X;
        moveY = unitinfo.Y;

        //Sphere.gamestate.is_stop = true;
        act_start = true;

        if (moveX >= 0)
            this.moveCsr();
    }
    // 
    // 変数 x, y で示された座標のルーム構造を読み込みなおす。
    public void RepBg(int x, int y, string tip)
    {
        //本体のチップを変更するが上のレイヤがある場合はそちらを変更する
        string structure = "stage";

        if (no.ContainsKey("no" + "overlayer1" + x + "_" + y))
        {
            structure = "overlayer1";
            // データの更新
            string before = Sphere.sphere.structoverlayer1[y];
            Sphere.sphere.structoverlayer1[y] = before.Substring(0, 4 * x) + tip + before.Substring(4 * x + 4);

        }

        if (no.ContainsKey("no" + "overlayer2" + x + "_" + y))
        {
            structure = "overlayer2";
            // データの更新
            string before = Sphere.sphere.structoverlayer2[y];
            Sphere.sphere.structoverlayer2[y] = before.Substring(0, 4 * x) + tip + before.Substring(4 * x + 4);
        }

        if (structure == "stage")
        {
            // データの更新
            string before = Sphere.sphere.structs[y];
            Sphere.sphere.structs[y] = before.Substring(0, 4 * x) + tip + before.Substring(4 * x + 4);
        }

        // もう一度読む。
        this.readbg(structure, x, y);

        // 反映させる。
        objMapTip.change(structure, x, y);
    }

    void gobattle()
    {

        /*
         //fscommand2("JavaScript", "showmsg", "gobattle running...");

        marginX = 36;
        marginY = (96 / 2);

        if(bteffA == 0){
	        //下向き
	        //marginY = marginY - (Sphere.TIP_SIZE / 2);
        }else if(bteffA == 1){
	        marginX = marginX + (Sphere.TIP_SIZE / 2);
        }else if(bteffA == 2){
	        marginX = marginX - (Sphere.TIP_SIZE / 2);
        }else if(bteffA == 3){
        //上向き
	        //marginY = marginY + (Sphere.TIP_SIZE / 2);
        }

        fscommand2("JavaScript", "showmsg", "gobattle bteffA:" add bteffA add " marginY:" add marginY add " marginX:" add marginX);

        // 反映。
        battle_movie._x = (bteffX * Sphere.TIP_SIZE) - marginX;
        battle_movie._y = (bteffY * Sphere.TIP_SIZE) - marginY;

        battle_movie._visible = true;

        tellTarget("battle_movie")
	        gotoAndPlay(1);
         */
    }



}
