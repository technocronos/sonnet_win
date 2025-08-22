using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class CommBtn
{
    private SphereBehaviour Sphere { get; set; }
    private StageBehaviour Stage { get; set; }
    private UserBehaviour User { get; set; }
    private CommandBehaviour Command { get; set; }

    public int mode { get; set; }

    public string push { get; set; } = "0";

    public MoveController objMC { get; set; }



    public CommBtn(MoveController _objMC)
    {
        User = UserBehaviour.Instance;
        Stage = StageBehaviour.Instance;
        Sphere = SphereBehaviour.Instance;
        Command = CommandBehaviour.Instance;

        objMC = _objMC;

        objMC.btnBackVisible(false);
        objMC.btnExitVisible(false);

    }

    private string comm { get; set; } = "";
    private string code1 { get; set; } = "";
    private string code2 { get; set; } = "";
    private string code3 { get; set; } = "";
    private string code4 { get; set; } = "";
    private string code5 { get; set; } = "";

    // 
    // 何かキーが押されたらcallされる。
    public void onKey()
    {
        comm = "";

        // コマンドパネルのモードにしたがって分岐。
        switch (mode)
        {

            // 「移動」「待機」「攻撃」「アイテム」「中断」
            case 1:
            case 2:
                code1 = "move";
                code2 = "wait";
                code3 = "att";
                code4 = "item";
                code5 = "susp";
                switch (push)
                {
                    case "1":
                        comm = code1;
                        break;
                    case "2":
                        comm = code2;
                        break;
                    case "3":
                        comm = code3;
                        break;
                    case "4":
                        comm = code4;
                        break;
                    case "5":
                        comm = code5;
                        break;
                }

                break;

            // 「ｷｬﾝｾﾙ」「OK」
            case 3:
                //キャンセル＝０　OK＝10としよう
                // キャンセル選択は0キーの選択と同一とする。
                if (push == "10")
                {
                    comm = "ok";
                }
                break;
        }

        // 有効なコマンドが選択されているならば通知。
        if (comm != "")
        {

            User.OkCancel.hide();
            Sphere.Preter.SetActive(false);
            User.command = comm;

            User.selComm();
        }


        // 0キーはキャンセル。
        if (push == "0")
        {
            AudioManager.Instance.PlaySE("se_btn");

            //中断からの戻りの場合はitemNoを初期化
            if (mode == 3 && User.command == "susp")
                User.itemNo = 0;

            User.OkCancel.hide();
            Sphere.Preter.SetActive(false);
            User.Cancel();
        }
    }


    //
    // コマンド受付を初期化する。
    // 変数 mode に以下の値を指定して、コマンドパネルの種類を指定する。
    //     1    「移動」「攻撃」「ｱｲﾃﾑ」「待機」「中断」
    //     2    1と同様だが、「移動」は選択付加
    //     3    「ｷｬﾝｾﾙ」「OK」
    public void reset()
    {

        setMode();

    }

    void setMode()
    {

        switch (mode)
        {
            case 1:
                //コマンドパネルは非表示
                //Command.hide();

                //フリック有効
                User.flick_lock = false;
                //画面をタップしてカーソルを動かすのは無効
                User.tap_flg = false;
                break;
            case 2:

                //コマンドパネル表示
                //Command.show();

                //フリック無効
                User.flick_lock = true;
                //画面をタップしてカーソルを動かすのは無効
                User.tap_flg = false;

                break;
            case 3:
                //コマンドパネルは非表示
                //Command.hide();

                //フリック無効
                User.flick_lock = true;
                //画面をタップしてカーソルを動かすのは無効
                User.tap_flg = false;
                break;

        }
    }


}
