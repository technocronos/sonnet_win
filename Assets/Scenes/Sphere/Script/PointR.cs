using Scenes.Common.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//
// ユーザのポイント選択を処理するムービー。
// 以下の変数を指定して、"5"キーを押したときの挙動を指定しておく。
//     fiveText     "5"キーを押せるタイミングのときのキーナビの⑤のテキストを指定する。
//     onlyUnit     この変数で指定されたユニット以外の選択を無視する。
//                  この制約が不要な場合は 0 を指定する。
//                  マップ閲覧モードの場合は -1 を指定する。
//     onFree       trueを指定すると、ユニットが存在しないマスの選択のみを扱う
//     onMark       trueを指定すると、有効なマークがあるマスの選択のみを扱う
class PointR
{
    public int onlyUnit { get; set; }
    public bool onFree { get; set; }
    public bool onMark { get; set; }
    public string fiveText { get; set; }

    public bool onTap { get; set; }


    public string invalid { get; set; }
    public string _invalid { get; set; }

    public float move_x { get; set; }
    public float move_y { get; set; }


    public int focusUnit { get; set; }

    private SphereBehaviour Sphere { get; set; }
    private StageBehaviour Stage { get; set; }
    private UserBehaviour User { get; set; }

    private InfoWindowBehaviour InfoW { get; set; }

    public long tab_enable_time;

    public PointR()
    {
        Sphere = SphereBehaviour.Instance;
        Stage = StageBehaviour.Instance;
        User = UserBehaviour.Instance;
        InfoW = InfoWindowBehaviour.Instance;

        tab_enable_time = Utility.GetUnixTime(System.DateTime.Now);

        // 現在のカーソル位置を参照して初期化する。
        this.refInfo();

        // ステージ上のカーソルを表示。
        Stage._cursor.enabled = true;
    }

    //
    // ステージのカーソル位置を参照して、状態の更新を行う。

    public void refInfo()
    {
        Debug.Log("pointR ref..");

        // カーソル位置に存在するユニットの番号を取得。
        int focusUnit = Sphere.FindUnit(Stage.cursorX, Stage.cursorY);

        // 有効な選択になっているかチェックする。
        this.valid();

        // 情報ウィンドウを更新する。
        InfoW.unitNo = focusUnit;
        InfoW.st();

    }

    // 
    // 現在フォーカスしている場所が有効な選択になっているかどうかを
    // 判定して、無効な理由を表す変数 invalid を更新する。
    void valid()
    {
        Debug.Log("pointR invalid start..");

        // 初期化。
        invalid = "";

        // onFreeの制約が課されていて、現在のカーソル位置にユニットがいる場合は無効。
        if (onFree && focusUnit != 0)
        {
            invalid = "onFree";
        }

        // onMarkの制約が課されている場合...
        if (onMark)
        {
            // カーソル地点のマーク値を取得。マーク値が無効あれば選択は無効。
            int markVal = Stage.objMarker.marks["mark" + Stage.cursorX + "_" + Stage.cursorY];
            if (markVal < 0) invalid = "onMark";
        }

        // ユニット制約が課されていて、フォーカスしているユニットがそれと
        // 一致しないなら無効。
        if (onlyUnit != 0 && onlyUnit != focusUnit)
            invalid = "onlyUnit";

    }

    //内部判定用。validとは移動する前に判定するところとユニットがいればOKとするところが違う
    void _valid()
    {
        Debug.Log("pointR _invalid start..");

        // 初期化。
        _invalid = "";

        // onMarkの制約が課されている場合...
        if (onMark)
        {
            // カーソル地点のマーク値を取得。マーク値が無効あれば選択は無効。
            int markVal = Stage.objMarker.marks["mark" + move_x + "_" + move_y];

            if (markVal < 0) _invalid = "onMark";
        }

        // ユニット制約が課されていて、フォーカスしているユニットがそれと
        // 一致しないなら無効。
        if (onlyUnit != 0 && onlyUnit != focusUnit)
            _invalid = "onlyUnit";

        // 対象ユニットを取得。
        focusUnit = Sphere.FindUnit(move_x, move_y);

        // onFreeの制約が課されていて、対象のカーソル位置にユニットがいる場合はOK。
        if (onFree && focusUnit > 0)
        {
            _invalid = "";
        }

    }

    /// <summary>
    /// 挙動を制御するフラグを初期化する。
    /// </summary>
    public void InitStatus()
    {
        Debug.Log("pointR InitStatus..");

        // フラグ初期化。
        onlyUnit = 0;
        onFree = false;
        onMark = false;

    }

    public int btn { get; set; }

    //
    // 変数 btn で示されたボタンにしたがって、親ムービーに選択の通知を行う。
    // このラベルがcallされても必ずしも別のコントローラに移るわけではないことに注意。
    void end()
    {
        Debug.Log("pointR end.." + btn);

        // キャンセルコマンドの場合。
        if (btn == 0)
        {
            User.Cancel();

            // 決定コマンドの場合。
        }
        else
        {
            User.pointX = Stage.cursorX;
            User.pointY = Stage.cursorY;
            User.selPoint();
        }
    }

    public float touchX { get; set; }
    public float touchY { get; set; }

    public float touchstartX { get; set; }
    public float touchstartY { get; set; }

    public float touchendX { get; set; }
    public float touchendY { get; set; }
    public float screenWidth { get; set; }

    public int slide_x { get; set; }
    public int slide_y { get; set; }


    //javascriptのtouchendから呼び出される
    //touchendX,touchendY,clientWidthも渡される。
    public void onTouchStart()
    {
        Rect _stage = Stage.transform.GetComponent<RectTransform>().rect;

        Debug.Log("onTouchEnd start.. /stage/:scrolling=" + Stage.scrolling);

        touchX = touchstartX * (Sphere.STAGE_WID / _stage.width);
        touchY = touchstartY * (Sphere.STAGE_HEI / _stage.height);

        this.onTouch();
    }


    void onTouch()
    {

        if (touchY <= Sphere.STAGE_HEI)
        {
            //ちょっと汚いが・・キャンセルボタン等を押した時に反応しないように二度押し対策。
            //ボタンを押した際に
            //User.objPointR.tab_enable_time = Utility.GetUnixTime(System.DateTime.Now);
            //のように現在時刻を入れたらその時間から1秒間はタップ処理が抑制される
            long now = Utility.GetUnixTime(System.DateTime.Now);
            if (now - tab_enable_time < 600)
            {
                return;
            }

            if (User.tap_flg == true)
            {
                Vector3 _stage = Stage.transform.GetComponent<RectTransform>().anchoredPosition;

                // 注意ウィンドウを非表示に。
                Sphere.Preter.SetActive(false);

                //フリックで中途半端になってもマスをタップ領域に合わせるため
                //余りを引くことで完全にチップが隠れきるまでは座標が生きているものとする
                int amari_x = (int)(_stage.x % Sphere.TIP_SIZE);
                int amari_y = (int)(_stage.y % Sphere.TIP_SIZE) * -1;

                int xpos = (int)(touchX - amari_x) / Sphere.TIP_SIZE;
                int ypos = (int)(touchY - amari_y) / Sphere.TIP_SIZE;

                int offsetX = (int)_stage.x / Sphere.TIP_SIZE;
                int offsetY = (int)_stage.y / Sphere.TIP_SIZE * -1;

                slide_x = (int)(xpos - (Stage.cursorX + offsetX));
                slide_y = (int)(ypos - (Stage.cursorY + offsetY));

                move_x = Stage.cursorX + slide_x;
                move_y = Stage.cursorY + slide_y;

                //どっちかでも範囲外なら反応しない
                if (move_x >= 0 && move_y >= 0 && move_x < Sphere.sphere.structWid && move_y < Sphere.sphere.structHei)
                {

                    // 範囲外へ出ないように補正する。
                    if (move_x < 0)
                        move_x = 0;
                    else if (move_x >= Sphere.sphere.structWid)
                        move_x = Sphere.sphere.structWid - 1;

                    if (move_y < 0)
                        move_y = 0;
                    else if (move_y >= Sphere.sphere.structHei)
                        move_y = Sphere.sphere.structHei - 1;

                    //一回フォーカス判定する
                    this._valid();

                    // 有効な選択である場合。
                    if (_invalid == "")
                    {

                        // カーソルを移動。範囲限界の制御などは /stage/:slideCsr で行う。
                        Stage.slideX = slide_x;
                        Stage.slideY = slide_y;

                        AudioManager.Instance.PlaySE("se_btn");

                        //同じ個所をクリックした場合
                        if (slide_x == 0 && slide_y == 0 && onTap)
                        {
                            //マーカークリア
                            for (int i = 1; i <= 4; i++)
                                Stage.objMarker.clearOneMarker("no_target_" + i);

                            User.objMC.push = "5";
                            User.objMC.onkey();
                        }
                        else
                        {
                            onTap = true;

                            int focusUnit = Sphere.FindUnit(move_x, move_y);

                            //ユニット選択の場合同じユニオンか判定
                            bool same_union = false;
                            if (focusUnit > 0)
                            {
                                jsonUnit u = Sphere.sphere.unit[focusUnit];
                                int unionTar = u.Info.align;

                                jsonUnit cu = Sphere.sphere.unit[User.commUnit];
                                int unionMe = cu.Info.align;

                                if (unionMe == unionTar)
                                    same_union = true;
                            }

                            //targetマーカーがある攻撃対象を選択した
                            if (User.phaseDepth == 1 && !same_union && focusUnit > 0 && Stage.objMarker.isExists(move_x, move_y).Contains("no_target_"))
                            {
                                User.command_flg = true;

                                User.objMC.push = "5";
                                User.objMC.onkey();

                                User.command_flg = false;

                                Stage.slideCsr();

                                // カーソル位置の変更を反映。
                                refInfo();

                                //マーカークリア
                                for (int i = 1; i <= 4; i++)
                                    Stage.objMarker.clearOneMarker("no_target_" + i);

                                //続けて攻撃ボタンを押したことにする
                                User.objCommBtn.push = "3";
                                User.objCommBtn.onKey();

                                //さらに攻撃対象を選択したことにする
                                User.objMC.push = "5";
                                User.objMC.onkey();
                            }
                            else
                            {
                                Stage.slideCsr();

                                // カーソル位置の変更を反映。
                                refInfo();

                                //青いマス選択の時のみ
                                if (User.phaseDepth == 1 && Stage.objMarker.isExists(move_x, move_y) != string.Empty)
                                {
                                    // 攻撃対象ユニットにマーカーをつける。

                                    //右
                                    string markerName = "no_target_1";
                                    this.setMarker(markerName, move_x + 1, move_y, "target");
                                    //左
                                    markerName = "no_target_2";
                                    this.setMarker(markerName, move_x - 1, move_y, "target");
                                    //下
                                    markerName = "no_target_3";
                                    this.setMarker(markerName, move_x, move_y + 1, "target");
                                    //上
                                    markerName = "no_target_4";
                                    this.setMarker(markerName, move_x, move_y - 1, "target");
                                }
                                else
                                {
                                    //マーカークリア
                                    //for (int i = 1; i <= 4; i++)
                                    //    Stage.objMarker.clearOneMarker("no_target_" + i);
                                }
                            }

                        }
                    }
                }
            }
        }
    }

    void setMarker(string markerName, float x, float y, string color)
    {
        // 攻撃対象ユニットを取得。
        //マーカークリア
        Stage.objMarker.clearOneMarker(markerName);

        int focusUnit = Sphere.FindUnit(x, y);
        if (focusUnit > 0)
        {
            jsonUnit UnitInfo = Sphere.sphere.unit[focusUnit];

            Stage.objMarker.mType = 1;
            Stage.objMarker.union = UnitInfo.Info.union;
            Stage.objMarker.color = "target";

            //敵の場合
            if (Stage.objMarker.union != 1)
            {
                Stage.objMarker.setOneMarker(markerName, x, y, color);


                int TUTORIAL_SPHERE_ATT = PlayerPrefs.GetInt(Settings.TUTORIAL_SPHERE_ATT, 0);

                //チュートリアル。ナビをしゃべらせる
                if (TUTORIAL_SPHERE_ATT == 0)
                {

                    //画面をタップしてカーソルを動かすのを無効
                    User.tap_flg = false;
                    //フリックを受け付け無効
                    User.flick_lock = true;

                    HomeApi summary = Header.Instance.GetSummary();
                    summary.opening = Utility.getText("TEXT_NAVI_TUTORIAL_SPHERE_ATTACK").Split("\n");

                    summary.openingNum = summary.opening.Length;

                    //二度と表示しない
                    PlayerPrefs.SetInt(Settings.TUTORIAL_SPHERE_ATT, 1);

                    User.naviController.gameObject.SetActive(true);
                    User.naviController.onStart(summary, null, () =>
                    {
                        User.naviController.disappere();

                        //画面をタップしてカーソルを動かす
                        User.tap_flg = true;
                        //フリックを受け付け
                        User.flick_lock = false;
                    });
                }

            }
        }
    }


    //javascriptのtouchendから呼び出される
    //touchendX,touchendY,clientWidthも渡される。
    public void onTouchEnd()
    {
        Rect _stage = Stage.transform.GetComponent<RectTransform>().rect;

        Debug.Log("onTouchEnd start.. /stage/:scrolling=" + Stage.scrolling);

        touchX = touchendX * (Sphere.STAGE_WID / _stage.width);
        touchY = touchendY * (Sphere.STAGE_HEI / _stage.height);

        this.onTouch();
    }

    public int gainX { get; set; }
    public int gainY { get; set; }

    //
    // キーが押されたらcallされる。

    //任意の箇所でフリックフラグをオンにする。
    //stageのactが停止されフリックでステージを移動できるようにする。
    //用がすんだらどこかで必ずフラグをOFFにしなければいけない。
    public void onFlick()
    {
        if (touchstartX > 0 && touchstartY > 0)
        {
            Vector3 _stage = Stage.transform.GetComponent<RectTransform>().anchoredPosition;

            //X座標可動範囲
            if ((Sphere.sphere.structWid * Sphere.TIP_SIZE) + (Sphere.TIP_SIZE * (Sphere.STAGE_MARGIN * 2)) > Sphere.STAGE_WID)
            {
                //マップX全体がそもそもステージX+チップ左右に2個分マージンより小さい場合は動かない
                if (gainX > Sphere.TIP_SIZE * Sphere.STAGE_MARGIN)
                {
                    //チップ2個分以上左に飛び出ない
                    float __x = (Sphere.TIP_SIZE * Sphere.STAGE_MARGIN);
                    Stage.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(__x, _stage.y, 0);
                    //Stageに通知する
                    Stage.left_over = true;
                }
                else if (gainX < (((Sphere.sphere.structWid * Sphere.TIP_SIZE) + (Sphere.TIP_SIZE * Sphere.STAGE_MARGIN)) - Sphere.STAGE_WID) * -1)
                {
                    //チップ2個分以上右に飛び出ない
                    float __x = (((Sphere.sphere.structWid * Sphere.TIP_SIZE) + (Sphere.TIP_SIZE * Sphere.STAGE_MARGIN)) - Sphere.STAGE_WID) * -1;
                    Stage.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(__x, _stage.y, 0);

                    //Stageに通知する
                    Stage.right_over = true;
                }
                else
                {
                    Stage.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(gainX, _stage.y, 0);
                }
            }

            float stage_height = Sphere.STAGE_HEI;

            if ((Sphere.sphere.structHei * Sphere.TIP_SIZE) + (Sphere.TIP_SIZE * (Sphere.STAGE_MARGIN * 2)) > stage_height)
            {
                //再取得しておく
                _stage = Stage.transform.GetComponent<RectTransform>().anchoredPosition;

                //マップY全体がそもそもステージY+チップ左右に2個分マージンより小さい場合は動かない
                if (gainY > Sphere.TIP_SIZE * Sphere.TOP_MARGIN)
                {
                    //チップ2個分以上、上に飛び出ない
                    float __y = Sphere.TIP_SIZE * Sphere.TOP_MARGIN;
                    Stage.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(_stage.x, __y * -1, 0);

                    //Stageに通知する
                    Stage.top_over = true;
                }
                else if (gainY < (((Sphere.sphere.structHei * Sphere.TIP_SIZE) + (Sphere.TIP_SIZE * (Sphere.STAGE_MARGIN * Sphere.BOTTOM_MARGIN))) - stage_height) * -1)
                {
                    //チップ2個分以上、下に飛び出ない
                    float __y = (((Sphere.sphere.structHei * Sphere.TIP_SIZE) + (Sphere.TIP_SIZE * (Sphere.STAGE_MARGIN * Sphere.BOTTOM_MARGIN))) - stage_height) * -1;
                    Stage.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(_stage.x, __y * -1, 0);

                    //Stageに通知する
                    Stage.bottom_over = true;
                }
                else
                {
                    Stage.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(_stage.x, gainY * -1, 0);
                }
            }
        }
    }

}
