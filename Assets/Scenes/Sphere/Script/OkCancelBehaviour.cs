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
