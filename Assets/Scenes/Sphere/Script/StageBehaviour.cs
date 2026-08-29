using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StateManager;
using Scenes.Common.Scripts;
using DG.Tweening;

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
    public int moveX { get; set; } = 0;
    public int moveY { get; set; } = 0;

    public int cursorX { get; set; } = 0;
    public int cursorY { get; set; } = 0;

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
    private RectTransform _stage_rect { get; set; }
    private RectTransform _viewport_rect { get; set; }
    private Camera _event_camera { get; set; }
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    private Vector2 mouseDownScreenPosition;
    private Vector2 stagePositionAtMouseDown;
    private bool mouseDragging;
    private bool windowsMouseGesture;
#endif

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

        _stage_rect = transform as RectTransform;
        if (_stage_rect != null)
        {
            Canvas stageCanvas = _stage_rect.GetComponentInParent<Canvas>();
            Canvas rootCanvas = stageCanvas == null ? null : stageCanvas.rootCanvas;
            _event_camera = rootCanvas == null || rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
        }

        _cursor = transform.Find("cursor").GetComponent<Image>();
    }

    private bool TryGetStagePosition(Vector2 screenPosition, out Vector2 stagePosition)
    {
        stagePosition = Vector2.zero;

        if (_stage_rect == null)
            _stage_rect = transform as RectTransform;

        if (_stage_rect == null)
            return false;

        if (_viewport_rect == null && Sphere != null)
            _viewport_rect = Sphere.ViewportRect;

        if (_viewport_rect == null)
            return false;

        Vector2 viewportPosition;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewport_rect, screenPosition, _event_camera, out viewportPosition)
            || !_viewport_rect.rect.Contains(viewportPosition))
            return false;

        Vector2 localPosition;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_stage_rect, screenPosition, _event_camera, out localPosition))
            return false;

        Rect rect = _stage_rect.rect;
        stagePosition.x = localPosition.x - rect.xMin;
        stagePosition.y = rect.yMax - localPosition.y;
        return true;
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

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    bool viewportDiagnosticsLogged;
    bool inputDiagnosticsLogged;
    int inputFollowDiagnosticsCount;
    int stageWriteDiagnosticsCount;
    int centerDiagnosticsCount;
    int slideCursorDiagnosticsCount;
#endif

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    private void LogStageWrite(string source, Vector3 before, Vector3 after)
    {
        if (stageWriteDiagnosticsCount >= 40)
            return;

        RectTransform viewport = _viewport_rect == null || Sphere == null ? null : _viewport_rect;
        string stageGrid = User == null || User.objPointR == null ? "none" : User.objPointR.move_x + "," + User.objPointR.move_y;
        string mapBounds = Sphere == null || Sphere.sphere == null ? "none" : "0,0 to " + (Sphere.sphere.structWid * Sphere.TIP_SIZE) + "," + (Sphere.sphere.structHei * Sphere.TIP_SIZE);
        Debug.Log("[STAGE_WRITE] source=" + source
            + " before=" + before
            + " after=" + after
            + " cursor=" + cursorX + "," + cursorY
            + " focusUnit=" + unitNo
            + " stageGrid=" + stageGrid
            + " viewportRect=" + (viewport == null ? "none" : viewport.rect.ToString())
            + " mapBounds=" + mapBounds);
        stageWriteDiagnosticsCount++;
    }

    private void LogCenterCalculation(Vector3 stageBefore, Vector3 targetLocal, Vector3 desiredStagePosition, Vector3 clampedStagePosition, Vector2 mapSize, Vector2 viewportSize)
    {
        if (centerDiagnosticsCount >= 16)
            return;

        Debug.Log("[CENTER_CALC] stageBefore=" + stageBefore
            + " targetLocal=" + targetLocal
            + " visibleCenter=" + (viewportSize * 0.5f)
            + " desiredStagePos=" + desiredStagePosition
            + " clampedStagePos=" + clampedStagePosition
            + " mapSize=" + mapSize
            + " viewportSize=" + viewportSize);
        centerDiagnosticsCount++;
    }

    private void LogSlideCursor(Vector3 stageBefore, int clickedX, int clickedY, Vector3 cursorLocal, Vector3 stageAfter)
    {
        if (slideCursorDiagnosticsCount >= 16)
            return;

        Debug.Log("[SLIDE_CSR] stageBefore=" + stageBefore
            + " clickedGrid=" + clickedX + "," + clickedY
            + " cursorLocal=" + cursorLocal
            + " calculatedDestination=" + moveX + "," + moveY
            + " stageAfter=" + stageAfter);
        slideCursorDiagnosticsCount++;
    }

    private void LogInputFollow(Vector2 screenPoint, Vector2 viewportLocalPoint, Vector2 stageLocalPoint, int gridX, int gridY)
    {
        if (inputFollowDiagnosticsCount >= 2)
            return;

        MarkerBehaviour marker = null;
        string markerObjectName = objMarker == null ? string.Empty : objMarker.isExists(gridX, gridY);
        if (!string.IsNullOrEmpty(markerObjectName))
        {
            Transform markerTransform = transform.Find(markerObjectName);
            marker = markerTransform == null ? null : markerTransform.GetComponent<MarkerBehaviour>();
        }
        Vector3 expectedMarkerLocal = new Vector3(gridX * Sphere.TIP_SIZE, gridY * Sphere.TIP_SIZE * -1f, 0f);
        foreach (MarkerBehaviour candidate in GetComponentsInChildren<MarkerBehaviour>(true))
        {
            if (marker != null)
                break;
            Vector3 candidateLocal = candidate.transform.localPosition;
            if (candidate.gameObject.activeInHierarchy
                && Mathf.Approximately(candidateLocal.x, expectedMarkerLocal.x)
                && Mathf.Approximately(candidateLocal.y, expectedMarkerLocal.y))
            {
                marker = candidate;
                break;
            }
        }

        Vector3 markerWorld = marker == null ? Vector3.zero : marker.transform.position;
        Vector2 markerScreen = Vector2.zero;
        if (marker != null)
        {
            RectTransform markerRect = marker.transform as RectTransform;
            if (markerRect != null)
            {
                Vector3[] corners = new Vector3[4];
                markerRect.GetWorldCorners(corners);
                markerWorld = (corners[0] + corners[1] + corners[2] + corners[3]) * 0.25f;
            }
            markerScreen = RectTransformUtility.WorldToScreenPoint(_event_camera, markerWorld);
        }
        Canvas viewportCanvas = _viewport_rect == null ? null : _viewport_rect.GetComponentInParent<Canvas>();
        Camera viewportCamera = viewportCanvas == null || viewportCanvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : viewportCanvas.rootCanvas.worldCamera;
        string markerKey = "mark" + gridX + "_" + gridY;

        Debug.Log("[INPUT_FOLLOW] ScreenPoint=" + screenPoint
            + " StageAnchoredPosition=" + _stage_rect.anchoredPosition
            + " StageWorldPosition=" + _stage_rect.position
            + " StageCamera=" + (_event_camera == null ? "null" : _event_camera.name)
            + " ViewportCamera=" + (viewportCamera == null ? "null" : viewportCamera.name)
            + " ViewportLocalPoint=" + viewportLocalPoint
            + " StageLocalPoint=" + stageLocalPoint
            + " CalculatedGrid=" + gridX + "," + gridY
            + " MarkerKey=" + markerKey
            + " MarkerObjectName=" + markerObjectName
            + " MarkerActive=" + (marker != null && marker.gameObject.activeInHierarchy)
            + " MarkerStageLocal=" + (marker == null ? "missing" : marker.transform.localPosition.ToString())
            + " MarkerWorld=" + (marker == null ? "missing" : markerWorld.ToString())
            + " MarkerScreen=" + (marker == null ? "missing" : markerScreen.ToString())
            + " ScreenDeltaToMarker=" + (marker == null ? "missing" : (screenPoint - markerScreen).ToString()));
        inputFollowDiagnosticsCount++;
    }
#endif

    public void init(SphereBehaviour sphere)
    {
        Debug.Log("StageBehaviour init running..");

        Sphere = sphere;
        _viewport_rect = Sphere.ViewportRect;
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
    int csrActX;
    int csrActY;
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

                Vector3 beforeWrite = _stage;
                transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(Sphere.propMove(actX, (int)_stage.x, speed), _stage.y, _stage.z);
                _stage = transform.GetComponent<RectTransform>().anchoredPosition;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                LogStageWrite("FixedUpdate.X", beforeWrite, _stage);
#endif

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

                Vector3 beforeWrite = _stage;
                transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(_stage.x, Sphere.propMove(actY, (int)_stage.y, speed), _stage.z);
                _stage = transform.GetComponent<RectTransform>().anchoredPosition;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                LogStageWrite("FixedUpdate.Y", beforeWrite, _stage);
#endif

                _offsetY = actY * -1;
            }

            // スクロール中かどうかを表すフラグを設定
            if ((int)_stage.x == actX && (int)_stage.y == actY)
            {
                scrolling = false;
                User.actX = actX;
                User.actY = actY;
            }

            // 振動が設定されている場合は処理する。
            if (vib != 0)
            {
                _stage.x += vib;
                Vector3 beforeWrite = transform.GetComponent<RectTransform>().anchoredPosition;
                transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(_stage.x, _stage.y, _stage.z);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                LogStageWrite("FixedUpdate.vibration", beforeWrite, transform.GetComponent<RectTransform>().anchoredPosition);
#endif
                vib *= -1;
            }
        }
    }

    private void Update()
    {
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

            Vector2 stagePosition;
            if (!TryGetStagePosition(touch_state._touch_position, out stagePosition)) return;

            float touch_x = stagePosition.x;
            float touch_y = stagePosition.y;

            //Debug.Log("touch_info.. x=" + touch_x + " y=" + touch_y);


            // タッチした瞬間の処理
            if (touch_state._touch_phase == TouchPhase.Began)
            {
                User.objPointR.ClearStageGrid();
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                // The Windows bridge reports held mouse input as Moved.  Keep the
                // original mobile Touch path intact and classify its gesture here.
                windowsMouseGesture = Input.touchCount == 0;
                if (windowsMouseGesture)
                {
                    mouseDownScreenPosition = touch_state._touch_position;
                    stagePositionAtMouseDown = (transform as RectTransform).anchoredPosition;
                    mouseDragging = false;
                }
#endif
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
                User.objPointR.ClearStageGrid();
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if (windowsMouseGesture && !mouseDragging)
                {
                    float dragThreshold = UnityEngine.EventSystems.EventSystem.current == null
                        ? 0f
                        : UnityEngine.EventSystems.EventSystem.current.pixelDragThreshold;
                    if ((touch_state._touch_position - mouseDownScreenPosition).sqrMagnitude < dragThreshold * dragThreshold)
                        return;

                    mouseDragging = true;
                }
#endif
                //flick_lockがかかってる場合はリターン
                if (User.flick_lock) return;

                var touchX = _x + (touch_x - touchstartX);
                var touchY = _y + (touch_y - touchstartY);

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
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if (windowsMouseGesture && mouseDragging)
                {
                    User.objPointR.ClearStageGrid();
                    windowsMouseGesture = false;
                    mouseDragging = false;
                    return;
                }
                windowsMouseGesture = false;
                mouseDragging = false;
#endif
                User.objPointR.touchendX = touch_x;
                User.objPointR.touchendY = touch_y;
                User.objPointR.SetStageGrid((int)(touch_x / Sphere.TIP_SIZE), (int)(touch_y / Sphere.TIP_SIZE));

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                Vector2 viewportPosition = Vector2.zero;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewport_rect, touch_state._touch_position, _event_camera, out viewportPosition);
                LogInputFollow(touch_state._touch_position, viewportPosition, stagePosition, (int)(touch_x / Sphere.TIP_SIZE), (int)(touch_y / Sphere.TIP_SIZE));
                if (!inputDiagnosticsLogged)
                {
                    Debug.Log("[SPHERE_INPUT] ScreenPoint=" + touch_state._touch_position
                        + " StageLocalPoint=" + stagePosition
                        + " GridXY=" + ((int)(touch_x / Sphere.TIP_SIZE)) + "," + ((int)(touch_y / Sphere.TIP_SIZE))
                        + " MarkerFound=" + (objMarker != null));
                    inputDiagnosticsLogged = true;
                }
#endif

                User.objPointR.onTouchEnd();

            }
            else if (touch_state._touch_phase == TouchPhase.Canceled)
            {
                User.objPointR.ClearStageGrid();
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                windowsMouseGesture = false;
                mouseDragging = false;
#endif
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
    public void moveCsr()
    {
        Debug.Log("moveCsr running...");

        // 移動。
        cursorX = moveX;
        cursorY = moveY;

        // 反映。
        _cursor.transform.localPosition = new Vector3(cursorX * Sphere.TIP_SIZE, cursorY * Sphere.TIP_SIZE * -1);

        // カーソルが画面内に収まるようにオフセットを設定し直す。
        lef = Sphere.STAGE_MARGIN;
        top = Sphere.TOP_MARGIN;
        rig = Sphere.STAGE_MARGIN;
        bot = Sphere.BOTTOM_MARGIN;
        this.center();

    }

    //
    // カーソルを変数 slideX, slideY で示された量だけ移動する。
    public void slideCsr()
    {

        Debug.Log("slideCsr running...");

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Vector3 stageBefore = transform.GetComponent<RectTransform>().anchoredPosition;
        int clickedX = cursorX + slideX;
        int clickedY = cursorY + slideY;
#endif

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

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        LogSlideCursor(stageBefore, clickedX, clickedY, _cursor.transform.localPosition, transform.GetComponent<RectTransform>().anchoredPosition);
#endif
    }

    //
    // 現在のステージオフセットとカーソル位置を比較して、
    // カーソルが画面端に来すぎないようにオフセットを調節する。
    // 変数 lef, top, rig, bot で画面端のマージンをチップ単位で指定する。
    public void center()
    {
        Debug.Log("center running...");

        RectTransform viewport = _viewport_rect == null ? Sphere.ViewportRect : _viewport_rect;
        if (viewport == null)
            return;

        float stage_wid = viewport.rect.width / Sphere.TIP_SIZE;
        float stage_hei = viewport.rect.height / Sphere.TIP_SIZE;


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

        // A small map is content, not a viewport: keep it centred without
        // scaling its legacy 110px grid.
        float mapWidth = Sphere.sphere.structWid * Sphere.TIP_SIZE;
        float mapHeight = Sphere.sphere.structHei * Sphere.TIP_SIZE;
        if (mapWidth <= viewport.rect.width)
            offsetX = (viewport.rect.width - mapWidth) / (2f * Sphere.TIP_SIZE);
        if (mapHeight <= viewport.rect.height)
            offsetY = -((viewport.rect.height - mapHeight) / (2f * Sphere.TIP_SIZE));

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Vector3 stageBefore = transform.GetComponent<RectTransform>().anchoredPosition;
        Vector3 targetLocal = new Vector3(cursorX * Sphere.TIP_SIZE, cursorY * Sphere.TIP_SIZE * -1f, 0f);
        Vector3 desiredStagePosition = new Vector3(offsetX * Sphere.TIP_SIZE, offsetY * Sphere.TIP_SIZE * -1f, stageBefore.z);
        LogCenterCalculation(stageBefore, targetLocal, desiredStagePosition, desiredStagePosition,
            new Vector2(mapWidth, mapHeight), viewport.rect.size);
#endif

        // スクロール中かどうかを表すフラグを初期化。
        scrolling = true;
    }

    public void ApplyFlickPosition(float gainX, float gainY)
    {
        RectTransform viewport = _viewport_rect == null ? Sphere.ViewportRect : _viewport_rect;
        if (viewport == null)
            return;

        float mapWidth = Sphere.sphere.structWid * Sphere.TIP_SIZE;
        float mapHeight = Sphere.sphere.structHei * Sphere.TIP_SIZE;
        float x = mapWidth <= viewport.rect.width ? (viewport.rect.width - mapWidth) / 2f : Mathf.Clamp(gainX, viewport.rect.width - mapWidth, 0f);
        float y = mapHeight <= viewport.rect.height ? -((viewport.rect.height - mapHeight) / 2f) : Mathf.Clamp(-gainY, -(mapHeight - viewport.rect.height), 0f);
        Vector3 beforeWrite = transform.GetComponent<RectTransform>().anchoredPosition;
        transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y, 0);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        LogStageWrite("ApplyFlickPosition", beforeWrite, transform.GetComponent<RectTransform>().anchoredPosition);
#endif
    }

    //
    // 変数 unitNo で示されたユニットにカーソルを合わせる
    public void focus()
    {
        if (!Sphere.sphere.unit.ContainsKey(unitNo)) return;

        jsonUnit unitinfo = Sphere.sphere.unit[unitNo];

        moveX = unitinfo.X;
        moveY = unitinfo.Y;

        if (moveX >= 0)
        {
            this.moveCsr();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (!viewportDiagnosticsLogged)
            {
                RectTransform viewport = _viewport_rect == null ? Sphere.ViewportRect : _viewport_rect;
                RectTransform stage = transform as RectTransform;
                Vector3 unitLocal = Vector3.zero;
                string unitKey = "unit_" + unitNo;
                if (objUnits != null && objUnits.units != null && objUnits.units.ContainsKey(unitKey))
                    unitLocal = objUnits.units[unitKey].transform.localPosition;

                Debug.Log("[SPHERE_VIEWPORT] Screen=" + Screen.width + "x" + Screen.height
                    + " ViewportRect=" + (viewport == null ? "none" : viewport.rect.ToString())
                    + " StageRect=" + (stage == null ? "none" : stage.rect.ToString())
                    + " StageAnchoredPosition=" + (stage == null ? "none" : stage.anchoredPosition.ToString())
                    + " MapBounds=0,0 to " + (Sphere.sphere.structWid * Sphere.TIP_SIZE) + "," + (Sphere.sphere.structHei * Sphere.TIP_SIZE)
                    + " FocusUnitLocal=" + unitLocal
                    + " FocusUnitViewport=" + (stage == null ? "none" : (new Vector3(stage.anchoredPosition.x, stage.anchoredPosition.y, 0f) + unitLocal).ToString())
                    + " VisibleTipBounds=viewport intersection");
                viewportDiagnosticsLogged = true;
            }
#endif
        }
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
