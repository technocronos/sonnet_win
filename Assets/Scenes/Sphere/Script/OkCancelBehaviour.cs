using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OkCancelBehaviour : MonoBehaviour
{

    public TextMeshProUGUI Text;


    // Start is called before the first frame update
    void Start()
    {
    }

    public void show(string _text, string mode)
    {
        Text.text = _text;

        SphereBehaviour Sphere = SphereBehaviour.Instance;
        StageBehaviour Stage = StageBehaviour.Instance;
        UserBehaviour User = UserBehaviour.Instance;

        //X座標を決定する
        double y;
        double y_margin;

        //ステージの座標を得る
        Vector3 _stage = Stage.transform.GetComponent<RectTransform>().anchoredPosition;
        Rect _stage_rect = Stage.transform.GetComponent<RectTransform>().rect;

        //ステージにまず親を合わせる
        Rect rect = transform.GetComponent<RectTransform>().rect;
        transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(_stage.x * -1, 0, 0);

        //ダイアログを合わせる
        Rect rect_dialogue = transform.Find("OkCancelPanel").GetComponent<RectTransform>().rect;

        //Y座標を決定する
        if (_stage_rect.height / 2 > Mathf.Abs(Stage.cursorY * Sphere.TIP_SIZE))
        {
            y = (Mathf.Abs(Stage.cursorY) * Sphere.TIP_SIZE) + (rect_dialogue.height / 2);
        }
        else
        {
            y = (Mathf.Abs(Stage.cursorY) * Sphere.TIP_SIZE) - rect_dialogue.height;
        }

        transform.Find("OkCancelPanel").GetComponent<RectTransform>().anchoredPosition = new Vector3(0, (float)y * -1, 0);

        transform.GetComponent<RectTransform>().SetAsLastSibling();

        transform.gameObject.SetActive(true);
    }

    public void hide()
    {
        Text.text = "";
        if (transform.gameObject.activeSelf)
            transform.gameObject.SetActive(false);
    }

    public void onOK()
    {
        UserBehaviour User = UserBehaviour.Instance;

        //mode3のOKキャンセルのみ反応
        if (User.objCommBtn.mode == 3)
        {
            //タップ抑制
            User.objPointR.tab_enable_time = Utility.GetUnixTime(System.DateTime.Now);

            AudioManager.Instance.PlaySE("se_btn");
            User.objCommBtn.push = "10";
            User.objCommBtn.onKey();

            User.BtnItem.interactable = true;
        }
    }

    public void onCancel()
    {
        UserBehaviour User = UserBehaviour.Instance;

        //mode3のOKキャンセルのみ反応
        if (User.objCommBtn.mode == 3)
        {
            //タップ抑制
            User.objPointR.tab_enable_time = Utility.GetUnixTime(System.DateTime.Now);

            AudioManager.Instance.PlaySE("se_btn");
            User.objCommBtn.push = "0";
            User.objCommBtn.onKey();

            //攻撃の場合はもう二回フェーズを戻す
            if (User.itemNo == 999)
            {
                User.objMC.push = "0";
                User.objMC.onkey();
                User.objMC.onkey();
            }
        }
    }


}
