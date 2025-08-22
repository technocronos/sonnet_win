using Scenes.Common.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//
// ユーザ入力を処理する要素を集めたムービー
public class UserBehaviour : MonoBehaviour
{
    public OkCancelBehaviour OkCancel;
    public ItemListBehaviour ItemList;
    public NaviController naviController;
    public GameObject Arrow;

    public int commUnit { get; set; }

    private int commOrgX { get; set; }
    private int commOrgY { get; set; }
    private int commOrgA { get; set; }

    public int phaseDepth { get; set; }

    private string change { get; set; }

    private int useX { get; set; }
    private int useY { get; set; }

    public int pointX { get; set; }
    public int pointY { get; set; }

    public int itemNo { get; set; }

    public int page { get; set; } = 0;
    public int slot { get; set; } = 0;

    private string phase { get; set; }

    private bool targetOk { get; set; }

    public bool command_flg { get; set; }


    //Sphereインスタンス
    private SphereBehaviour Sphere { get; set; }
    private StageBehaviour Stage { get; set; }
    private InfoWindowBehaviour InfoW { get; set; }

    public bool flick_lock { get; set; }
    public bool tap_flg { get; set; }
    public bool flick_flg { get; set; } = false;

    public int actX { get; set; }
    public int actY { get; set; }

    internal PointR objPointR;
    internal CommBtn objCommBtn;
    internal MoveController objMC;

    public Button BtnItem;

    private string transUrl { get; set; }

    jsonConstants constants;

    public static UserBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static UserBehaviour instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public IEnumerator init()
    {

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        naviController.gameObject.SetActive(false);

        Sphere = SphereBehaviour.Instance;
        Stage = StageBehaviour.Instance;

        InfoW = transform.Find("InfoW").GetComponent<InfoWindowBehaviour>();
        InfoW.init();

        objPointR = new PointR();
        objMC = new MoveController(objPointR);
        objCommBtn = new CommBtn(objMC);

        this.setInfoWindowVisible(false);

        if (Sphere.sphere.EASY_MODE == 1)
        {
            BtnItem.interactable = false;
        }

        objMC.btnBackVisible(false);

        jsonUnit unitinfo = Sphere.sphere.unit[commUnit];

        // コマンドユニットの現在の場所を保持しておく。
        commOrgX = unitinfo.X;
        commOrgY = unitinfo.Y;
        commOrgA = int.Parse(unitinfo.Info.Split(new char[] { ' ' })[3]);

        // フェーズスタックを初期化。
        phaseDepth = -1;

        change = "comm1";

        this.Phase();

        //チュートリアルが終わってる場合
        if (Header.Instance.GetSummary().tutorial_step >= constants.User_Info_Tutorial.TUTORIAL_END)
        {

            int TUTORIAL_SPHERE_NEW = PlayerPrefs.GetInt(Settings.TUTORIAL_SPHERE_NEW, 0);

            //チュートリアル。ナビをしゃべらせる
            if (TUTORIAL_SPHERE_NEW == 0)
            {
                //画面をタップしてカーソルを動かすのを無効
                tap_flg = false;
                //フリックを受け付け無効
                this.flick_lock = true;

                HomeApi summary = Header.Instance.GetSummary();
                summary.opening = Utility.getText("TEXT_NAVI_TUTORIAL_SPHERE_COMMAND").Split("\n");

                summary.openingNum = summary.opening.Length;

                Arrow.SetActive(true);
                Vector3 pos = Arrow.GetComponent<RectTransform>().anchoredPosition;
                Arrow.GetComponent<ArrowBehaviour>().Show("down", pos.x, pos.y);

                naviController.gameObject.SetActive(true);
                naviController.onStart(summary, null, () =>
                {
                    naviController.disappere();
                    Arrow.SetActive(false);

                    //二度と表示しない
                    PlayerPrefs.SetInt(Settings.TUTORIAL_SPHERE_NEW, 1);

                    //画面をタップしてカーソルを動かす
                    tap_flg = true;
                    //フリックを受け付け
                    this.flick_lock = false;

                });
            }
        }
        else
        {
            //新規ユーザーに表示する必要は無いのでチュートリアル中なら二度と表示しない
            PlayerPrefs.SetInt(Settings.TUTORIAL_SPHERE_NEW, 1);

        }


        yield break;
    }

    //
    // 変数 change で指定されたフェーズに移行する。
    // 値に "cancel" を指定した場合、一つ前のフェーズに戻る。
    // フェーズとして指定できる値は以下の通り
    //     free     フリーモード
    //     comm1    コマンド選択1(移動前)
    //     move     移動先選択
    //     comm2    コマンド選択2(移動後)
    //     item     アイテム選択
    //     target   攻撃・アイテムのターゲット選択
    //              以下の変数も指定する。
    //                  itemNo  使おうとしているアイテムの番号
    //     confirm  確認
    Dictionary<int, string> phaseStack = new Dictionary<int, string>();
    void Phase()
    {

        Debug.Log("infoF phase run.. change=" + change);
        // フィールドマーカーをクリア
        Stage.objMarker.clearMarker();
        jsonUnit commUnitInfo = Sphere.sphere.unit[commUnit];

        // 行動ptが少ない場合、特定のコマンドを選択した段階で不足用ページへ飛ばす。
        if (Sphere.actPt < Sphere.sphere.consumePt && (change == "move" || change == "target"))
        {

            Sphere.showPreter(Sphere.ERROR_NO_ACTIONPT, null, "actpt");
            Sphere.leader.transUrl = Sphere.sphere.apShortUrl;
            Sphere.Trans();

        }
        else
        {

            // キャンセルの場合。
            if (change == "cancel")
            {
                // キャンセル後のフェーズがない場合はコマンド選択1に。
                if (phaseDepth == 0)
                {
                    change = "comm1";
                    phaseDepth++;
                    phaseStack[phaseDepth] = "comm1";
                }
                else
                {
                    // キャンセル後のフェーズがあるならそれへ。
                    phaseDepth--;
                    change = phaseStack[phaseDepth];

                    // キャンセルで移動先選択になる場合は、コマンドユニットの位置を元に戻す。
                    if (change == "move")
                    {
                        Stage.objUnits.move(commUnit, commOrgX, commOrgY, commOrgA);
                    }
                }
            }
            else
            {
                // キャンセル以外の場合。フェーズスタックを更新する。
                // フェーズスタックを更新して、遷移後のフェーズを取得する。
                phaseDepth++;
                phaseStack[phaseDepth] = change;
            }

            // 現在のフェーズとして保持。
            phase = change;

            // 指定されたフェーズに適合するレシーバーを変数 receive に取得。
            switch (phase)
            {

                // フリーモード
                case "free":
                    this.point();
                    objPointR.InitStatus();
                    objPointR.fiveText = Sphere._STRING_MENU;

                    if (Sphere.sphere.readonly_flg == 1)
                        objPointR.onlyUnit = -1;
                    else
                        objPointR.onlyUnit = commUnit;
                    break;
                // コマンド選択1(移動前)
                case "comm1":

                    // ステージカーソルをコマンドユニットのところにセット。
                    Stage.moveX = commUnitInfo.X;
                    Stage.moveY = commUnitInfo.Y;
                    Stage.moveCsr();

                    InfoW.unitNo = commUnit;
                    InfoW.st();

                    // コマンド受付を初期化。
                    objCommBtn.mode = 1;
                    objCommBtn.reset();

                    this.comm();

                    break;

                // 移動先選択
                case "move":

                    // 移動可能なマスにマーカを設定する。
                    Stage.objMarker.stack = 1;
                    Stage.objMarker.x_arr[1] = commUnitInfo.X;
                    Stage.objMarker.y_arr[1] = commUnitInfo.Y;
                    Stage.objMarker.mPow_arr[1] = int.Parse(commUnitInfo.Info.Split(new char[] { ' ' })[2]);
                    Stage.objMarker.mType = 1;
                    Stage.objMarker.union = int.Parse(commUnitInfo.Info.Split(new char[] { ' ' })[1]);
                    Stage.objMarker.color = "move";

                    Stage.objMarker.mark();

                    // ポイント選択受付を初期化。
                    this.point();

                    objPointR.InitStatus();
                    objPointR.fiveText = Utility.getText("TEXT_MOVE");
                    objPointR.onFree = true;
                    objPointR.onMark = true;
                    objPointR.onTap = false;

                    // 情報ウィンドウにコマンドユニットのステータスを表示させる。
                    objPointR.refInfo();

                    break;

                // コマンド選択2(移動後)
                case "comm2":

                    objMC.btnChange(2);
                    objMC.btnBackVisible(false);

                    objCommBtn.mode = 2;
                    objCommBtn.reset();

                    this.comm();

                    // 情報ウィンドウにコマンドユニットのステータスを表示させる。
                    objPointR.refInfo();

                    break;

                // アイテム選択
                case "item":
                    objMC.btnChange(2);
                    objMC.btnBackVisible(false);

                    objCommBtn.mode = 2;
                    objCommBtn.reset();

                    this.item();
                    break;

                // 攻撃・アイテムのターゲット選択
                case "target":

                    // 選択可能なマスにマーカを設定する。
                    Stage.objMarker.stack = 1;
                    Stage.objMarker.x_arr[1] = commUnitInfo.X;
                    Stage.objMarker.y_arr[1] = commUnitInfo.Y;
                    if (itemNo == 0)
                        itemNo = 999;
                    Stage.objMarker.mPow_arr[1] = int.Parse(Sphere.sphere.item[itemNo].Split(new char[] { ' ' })[2]);
                    Stage.objMarker.mType = 0;
                    Stage.objMarker.color = "target";
                    Stage.objMarker.mark();

                    // ポイント選択受付を初期化。
                    this.point();
                    objPointR.InitStatus();
                    objPointR.fiveText = Utility.getText("TEXT_DECIDE");
                    objPointR.onMark = true;

                    //コマンドは非表示
                    CommandBehaviour.Instance.hide();

                    // 情報ウィンドウにコマンドユニットのステータスを表示させる。
                    objPointR.refInfo();

                    break;

                // 確認
                case "confirm":
                    // 何かを行おうとしている場合は...
                    if (itemNo > 0 && useX >= 0)
                    {

                        // 影響範囲をマーカで表示。
                        string data = Sphere.sphere.item[itemNo];

                        Stage.objMarker.stack = 1;
                        Stage.objMarker.x_arr[1] = useX;
                        Stage.objMarker.y_arr[1] = useY;
                        Stage.objMarker.mPow_arr[1] = int.Parse(data.Split(new char[] { ' ' })[3]);
                        Stage.objMarker.mType = 0;
                        Stage.objMarker.color = data.Split(new char[] { ' ' })[1];
                        Stage.objMarker.mark();
                    }

                    // コマンド受付を初期化。
                    objCommBtn.mode = 3;
                    objCommBtn.reset();

                    this.comm();

                    break;
            }
        }

    }

    void point()
    {

        Debug.Log("infoF point run.. phaseDepth=" + phaseDepth);

        objMC.btnBackVisible(true);
        objMC.btnExitVisible(true);

        //画面をタップしてカーソルを動かすのを有効
        this.tap_flg = true;

        //フリックを受け付け
        this.flick_lock = false;

        if (phaseDepth == 1)
        {
            objMC.btnChange(2);
        }
        else
        {
            objMC.btnChange(1);
        }

        if (phaseDepth == 4)
        {
            objMC.btnExitVisible(false);
            //icon_susp/:_visible = false;
        }

        this.setInfoWindowVisible(true);
    }


    //
    // コマンドが選択されたらcallされる。
    // 変数 command には選択されたコマンドのコードが代入される。
    //     move     移動
    //     wait     待機
    //     att      攻撃
    //     item     アイテム
    //     susp     中断
    //     cancel   キャンセル
    public string command { get; set; }
    public void selComm()
    {
        string okcanceltext = "";

        // 選択されたコマンドによって分岐。
        switch (command)
        {

            // 移動
            case "move":
                change = "move";
                this.Phase();

                break;

            // 待機
            case "wait":
                itemNo = 0;
                change = "confirm";

                // 確認のメッセージを出す。
                this.setInfoWindowVisible(false);

                okcanceltext = Utility.getText("SPHERE_MOVE_CONFIRM");
                OkCancel.show(okcanceltext, "relative");

                this.Phase();

                break;
            // その場で待機
            case "wait2":
                itemNo = 0;
                change = "confirm";

                // 確認のメッセージを出す。
                this.setInfoWindowVisible(false);

                okcanceltext = Utility.getText("SPHERE_WAIT_CONFIRM");
                OkCancel.show(okcanceltext, "bottom");

                command = "wait";

                this.Phase();

                break;
            // 攻撃
            case "att":
                itemNo = 999;
                change = "target";
                this.Phase();
                break;

            // アイテム
            case "item":
                this.setInfoWindowVisible(false);

                change = "item";
                this.Phase();
                break;

            // 中断
            case "susp":
                itemNo = -1;
                change = "confirm";

                this.setInfoWindowVisible(false);
                // 確認のメッセージを出す。
                okcanceltext = Utility.getText("SPHERE_SASP_CONFIRM");

                OkCancel.show(okcanceltext, "bottom");

                this.Phase();
                break;

            // 確認後のOK
            case "ok":

                // フィールドマーカーをクリア
                Stage.objMarker.clearMarker();

                // 中断の場合は、中断用URLに遷移するようにする。
                if (itemNo == -1)
                {
                    Sphere.leader.transUrl = "scene=Home";
                    Sphere.Trans();
                }
                else
                {
                    this.send();
                }

                break;
        }

    }

    // 
    // フェーズでキャンセルが選択されたらcallされる。

    // 閲覧モードの場合は無視する。
    // 本当は戻るとかリフレッシュとかのメニューを表示するとこだが…
    public void Cancel()
    {
        if (Sphere.sphere.readonly_flg == 0)
        {
            change = "cancel";
            this.Phase();
        }
    }


    //
    // ポイントが選択されたらcallされる。
    // 変数 pointX, pointY には、選択された座標がセットされる。

    public void selPoint()
    {
        Debug.Log("selpoint phase=" + phase);

        switch (phase)
        {

            // 移動先選択の場合。
            case "move":

                // 指定されたポイントへユニットを移動。
                //移動先選択時はとりあえず正面向いておく
                Stage.objUnits.move(commUnit, pointX, pointY, 0);

                //コマンド選択の場合（アイテムか攻撃）
                if (command_flg)
                {
                    // 移動後のコマンド選択へ。        
                    change = "comm2";
                    this.Phase();
                }
                else
                {

                    //イージーモードの場合は常に0
                    itemNo = 0;

                    //イージーモードの場合はすぐ移動してしまう
                    objCommBtn.mode = 3;
                    objCommBtn.push = "10";
                    objCommBtn.onKey();

                    //フリック無効
                    this.flick_lock = true;
                }
                break;

            // ターゲット選択の場合。
            case "target":

                // 指定されたポイントを使用座標として保持
                useX = pointX;
                useY = pointY;

                // エラーチェックフラグを初期化。
                targetOk = true;

                // 「攻撃」をしようとしていている場合は...
                if (itemNo == 999)
                {

                    // 対象ユニットを取得。
                    int focusUnit = Sphere.FindUnit(useX, useY);

                    // 対象ユニットがいないならエラー。
                    if (focusUnit == 0)
                    {
                        targetOk = false;

                        // 対象ユニットがいるが、コマンドユニットと同じ所属の場合はエラー。
                    }
                    else
                    {
                        //コマンドユニットunion取得
                        jsonUnit commUnitInfo = Sphere.sphere.unit[commUnit];
                        string unionComm = commUnitInfo.Info.Split(new char[] { ' ' })[1];

                        //対象ユニットunion取得
                        jsonUnit focusUnitInfo = Sphere.sphere.unit[focusUnit];
                        string unionTrgt = focusUnitInfo.Info.Split(new char[] { ' ' })[1];

                        if (unionComm == unionTrgt)
                            targetOk = false;
                    }

                    // 対象不正な場合はエラー表示。
                    if (!targetOk)
                    {
                        Sphere.showPreter(Utility.getText("TEXT_SPHERE_NO_ATTACK"));
                    }
                }

                // 対象が選択可能なら確認へ。        
                if (targetOk)
                {

                    this.setInfoWindowVisible(false);
                    string okcanceltext = "";
                    // 確認のメッセージを出す。
                    if (command == "att")
                    {
                        okcanceltext = Utility.getText("SPHERE_ATTACK_CONFIRM");
                    }
                    else if (command == "item")
                    {
                        okcanceltext = Utility.getText("SPHERE_ITEMUSE_CONFIRM");
                    }

                    OkCancel.show(okcanceltext, "relative");

                    change = "confirm";
                    this.Phase();
                }

                break;
        }

    }

    //
    // アイテムが選択されたらcallされる。
    // 以下の変数がセットされている。
    //     itemNo   選択されたアイテムの番号
    //     page     アイテムページ番号。0は装備欄であることを表す
    //     slot     選択位置。0～7まで
    public void SelItem()
    {
        // アイテム欄から装備を選択している場合...
        if (Sphere.sphere.item[itemNo].Substring(0, 3) == "eqp")
        {
            // 使用座標を無効な値に。
            useX = -1;
            useY = -1;

            // 確認のメッセージを出す。
            string okcanceltext = Sphere.STR_CONFIRM_CHANGE_EQP;
            OkCancel.show(okcanceltext, "relative");

            // 次は確認フェーズ。
            change = "confirm";

        }
        else
        {
            // それ以外は、次はターゲットフェーズ
            change = "target";
        }

        // 次のフェーズへ。
        this.Phase();
    }

    string act { get; set; } = "";

    //
    // コマンドがすべて決定されたらcallされる。
    void send()
    {
        Sphere.ApDispPanel.gameObject.SetActive(false);

        // アイテム番号から行動種別を取得。
        switch (itemNo)
        {
            case 0:
                act = "wait";
                break;
            case 999:
                act = "att";
                break;
            default:
                act = "item";
                break;
        }

        Dictionary<string, string> varVal = new Dictionary<string, string>();

        // 送信する値をセット。
        jsonUnit unitinfo = Sphere.sphere.unit[commUnit];
        varVal["moveX"] = unitinfo.X.ToString();
        varVal["moveY"] = unitinfo.Y.ToString();
        varVal["act"] = act;
        varVal["slot"] = (page * 8 + slot).ToString();
        varVal["useX"] = useX.ToString();
        varVal["useY"] = useY.ToString();

        // コマンドユニットの場所を戻す。
        Stage.objUnits.move(commUnit, commOrgX, commOrgY, commOrgA);

        // 親ムービーをコマンドレスポンス待機フェーズへ。
        APIConnectManager.Instance.SphereCommand(Sphere.Param.sphereId, Sphere.sphere.validation_code, Sphere.sphere.revision, varVal, Sphere.Mitter);

    }

    void comm()
    {
        Debug.Log("infoF comm run..");
        if (objCommBtn.mode == 1)
        {
            objCommBtn.push = "1";
            objCommBtn.onKey();
        }
    }

    void item()
    {

        //アイテム選択ポップアップ立ち上げ
        ItemList.showItemList();

        //コマンドは非表示
        CommandBehaviour.Instance.hide();

        //情報パネル非表示
        this.setInfoWindowVisible(false);

        // 情報ウィンドウにコマンドユニットのステータスを表示させる。
        InfoW.unitNo = commUnit;
        InfoW.refInfo();

    }


    /// <summary>
    /// アイテムポップアップを出す
    /// phaseがcomm1->comm2->itemだがcomm2を飛ばしてしまう
    /// </summary>
    public void onItemClick()
    {
        BtnItem.interactable = false;

        command_flg = true;

        objPointR.onTap = true;

        objMC.push = "5";
        objMC.onkey();

        command_flg = false;

        //その後アイテムを立ち上げる
        objCommBtn.push = "4";
        objCommBtn.onKey();

        int TUTORIAL_SPHERE_ITM = PlayerPrefs.GetInt(Settings.TUTORIAL_SPHERE_ITM, 0);

        //チュートリアル。ナビをしゃべらせる
        if (TUTORIAL_SPHERE_ITM == 0)
        {
            //画面をタップしてカーソルを動かすのを無効
            tap_flg = false;

            HomeApi summary = Header.Instance.GetSummary();
            summary.opening = Utility.getText("TEXT_NAVI_TUTORIAL_SPHERE_ITEM").Split("\n");

            summary.openingNum = summary.opening.Length;

            naviController.gameObject.SetActive(true);
            naviController.onStart(summary, null, () =>
            {
                naviController.disappere();

                //二度と表示しない
                PlayerPrefs.SetInt(Settings.TUTORIAL_SPHERE_ITM, 1);

                //画面をタップしてカーソルを動かすのを有効
                this.tap_flg = true;

            });
        }

    }

    public void setInfoWindowVisible(bool _visible)
    {
        InfoWindowBehaviour.Instance.setVisible(_visible);
    }

    public void setStatusWindowVisible(bool _visible)
    {
        InfoW.transform.Find("BG").gameObject.SetActive(_visible);
        InfoW.transform.Find("Name").gameObject.SetActive(_visible);
        InfoW.transform.Find("LvIcon").gameObject.SetActive(_visible);
        InfoW.transform.Find("HPGauge").gameObject.SetActive(_visible);
        InfoW.transform.Find("StatusPanel").gameObject.SetActive(_visible);
    }

}
