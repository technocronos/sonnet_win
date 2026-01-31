using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Scenes.Common.Scripts;

class MoveController
{

    public string push { get; set; }

    private SphereBehaviour Sphere { get; set; }
    private StageBehaviour Stage { get; set; }
    private UserBehaviour User { get; set; }

    private Button BtnBack;
    private Button BtnExit;

    public int btn { get; set; }

    public InfoWindowBehaviour InfoW { get; set; }

    public PointR objPointR { get; set; }


    // Start is called before the first frame update
    public MoveController(PointR _objPointR)
    {
        User = UserBehaviour.Instance;
        Stage = StageBehaviour.Instance;
        Sphere = SphereBehaviour.Instance;
        InfoW = InfoWindowBehaviour.Instance;

        objPointR = _objPointR;

        BtnBack = User.transform.Find("InfoW/BtnBack").GetComponent<Button>();
        BtnExit = User.transform.Find("InfoW/BtnExit").GetComponent<Button>();

        BtnBack.onClick.RemoveAllListeners();
        BtnBack.onClick.AddListener((() => BtnBackClick()));

        BtnExit.onClick.RemoveAllListeners();
        BtnExit.onClick.AddListener((() => BtnExitClick()));
    }


    void BtnBackClick()
    {
        if (User.phaseDepth != 1)
        {
            push = "0";
            this.onkey();

            if (User.phaseDepth == 2)
            {
                this.onkey();
            }
        }
        else
        {
            push = "4";
            this.onkey();
        }
    }

    public void BtnExitClick()
    {
        if (User.phaseDepth != 4)
        {
            push = "6";
            this.onkey();
        }
    }

    public void btnBackVisible(bool _visible)
    {
        BtnBack.gameObject.SetActive(_visible);
    }

    public void btnExitVisible(bool _visible)
    {
        BtnExit.interactable = _visible;
    }

    public void btnChange(int _btn)
    {
        TextMeshProUGUI txt = User.transform.Find("InfoW/BtnBack/TextBtnBack").GetComponent<TextMeshProUGUI>();
        switch (_btn)
        {
            case 1:
                txt.text = Utility.getText("TEXT_BACK");
                this.btnBackVisible(true);
                break;
            case 2:

                //ここで待機は廃止
                //txt.text = "ここで待機";
                this.btnBackVisible(false);
                break;
        }
    }

    //
    // 変数 btn で示されたボタンにしたがって、親ムービーに選択の通知を行う。
    // このラベルがcallされても必ずしも別のコントローラに移るわけではないことに注意。
    void end()
    {
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

    //
    // キーが押されたらcallされる。
    public void onkey()
    {
        // 注意ウィンドウを非表示に。
        Sphere.Preter.SetActive(false);

        // 押されたキーを取得。
        btn = int.Parse(push);

        AudioManager.Instance.PlaySE("se_btn");

        // 5キーの場合。
        if (btn == 5)
        {

            // 有効な選択である場合。
            if (objPointR.invalid == "" || (objPointR.focusUnit == User.commUnit && objPointR.invalid == "onFree"))
            {

                // ユニット制約が課されている場合は 0 キーと同じ扱いにする。
                if (objPointR.onlyUnit != 0)
                    btn = 0;

                // 通知。
                this.end();

                // 無効な選択であり、いずれかのユニットにフォーカスしている場合。
            }
            else if (objPointR.focusUnit != 0)
            {
                // 情報ウィンドウのページを次へ。
                InfoW.next();

                // 完全に無効である場合。
            }
            else
            {
                string t = "top";
                // エラーメッセージを決定。
                switch (objPointR.invalid)
                {
                    case "onFree":
                        t = Utility.getText("SPHERE_ERROR_SELECT_OTHER_PLACE");
                        Sphere.showPreter(t);
                        break;
                    case "onMark":
                        t = Utility.getText("SPHERE_ERROR_SELECT_INVALID");
                        Sphere.showPreter(t);
                        break;
                    case "onlyUnit":
                        t = "";
                        Sphere.showPreter(t);
                        btn = 0;
                        break;
                }

                // 表示。
                //if(t != "")
                //    call("/preter/:error");

            }
        }

        //4の場合は待機
        if (btn == 4)
        {
            User.OkCancel.hide();
            Sphere.Preter.SetActive(false);
            User.command = "wait2";
            User.selComm();
        }

        //6の場合は中断
        if (btn == 6)
        {
            User.OkCancel.hide();
            Sphere.Preter.SetActive(false);
            User.command = "susp";
            User.selComm();
        }

        // 0キーの場合はキャンセルとして通知
        if (btn == 0)
            this.end();
    }
}
