using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandBehaviour : MonoBehaviour
{
    private UserBehaviour User { get; set; }

    public Button BtnMove;
    public Button BtnItem;
    public Button BtnAttack;
    public Button BtnClose;

    public static CommandBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static CommandBehaviour instance;

    public Rect rect { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        transform.gameObject.SetActive(false);
        rect = transform.GetComponent<RectTransform>().rect;
    }

    private void setPos()
    {
        SphereBehaviour Sphere = SphereBehaviour.Instance;
        StageBehaviour Stage = StageBehaviour.Instance;

        //X座標を決定する
        double x;
        double y;
        double x_margin;
        double y_margin;

        Vector3 _stage = Stage.transform.GetComponent<RectTransform>().anchoredPosition;
        Rect rect = transform.GetComponent<RectTransform>().rect;

        //カーソルが画面より右にある
        if ((Stage.cursorX * Sphere.TIP_SIZE) + _stage.x >= (Sphere.STAGE_WID / 2))
        {
            //ポップアップは左側に表示する
            x_margin = Sphere.TIP_SIZE * 0.5;
            x = (Stage.cursorX * Sphere.TIP_SIZE) - rect.width - x_margin;

            //コマンドポップアップが左にはみ出してしまう場合は調整する
            if (x + _stage.x < 0)
                x = x_margin - _stage.x;

        }
        else
        {
            //ポップアップは右側に表示する
            x_margin = Sphere.TIP_SIZE * 1.5;
            x = (Stage.cursorX * Sphere.TIP_SIZE) + x_margin;

            //コマンドポップアップが右にはみ出してしまう場合は調整する
            if (x + rect.width + _stage.x > Sphere.STAGE_WID)
                x = Sphere.STAGE_WID - _stage.x - (rect.width + x_margin);
        }


        float _x = (float)x;

        InfoWindowBehaviour InfoW = InfoWindowBehaviour.Instance;
        float h = InfoW.GetComponent<RectTransform>().rect.height;

        //Y座標を決定する
        y_margin = Sphere.TIP_SIZE * 0.5;
        y = (Stage.cursorY * Sphere.TIP_SIZE) + y_margin;

        //コマンドポップアップが座標0よりは下にいかないように調整する
        if (y + rect.height + y_margin > (Sphere.STAGE_HEI - h + _stage.y))
            y = (Sphere.STAGE_HEI - h + _stage.y) - (rect.height + y_margin);

        float _y = (float)y * -1;

        transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(_x, _y, 0);
    }

    public void show()
    {
        User = UserBehaviour.Instance;

        this.setPos();
        transform.gameObject.SetActive(true);

        //前面表示
        transform.GetComponent<RectTransform>().SetAsLastSibling();
    }

    public void hide()
    {
        transform.gameObject.SetActive(false);
    }

    public void onCloseClick()
    {

        if (User.objCommBtn.mode == 1)
        {
            User.objCommBtn.push = "1";
        }
        else if (User.objCommBtn.mode == 2)
        {
            User.objCommBtn.push = "0";
        }

        if (User.objCommBtn.mode != 3)
        {
            AudioManager.Instance.PlaySE("se_btn");
            this.hide();
            User.objCommBtn.onKey();
        }
    }
    public void onMoveClick()
    {
        AudioManager.Instance.PlaySE("se_btn");
        User.objCommBtn.push = "2";
        User.objCommBtn.onKey();
    }
    public void onItemClick()
    {

        AudioManager.Instance.PlaySE("se_btn");
        User.objCommBtn.push = "4";
        User.objCommBtn.onKey();

    }
    public void onAttackClick()
    {
        AudioManager.Instance.PlaySE("se_btn");
        User.objCommBtn.push = "3";
        User.objCommBtn.onKey();
    }


}
