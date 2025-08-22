using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EtheriumCanvasBehaviour : MonoBehaviour
{

    public Button ButtonReg;
    public TMP_InputField Address;
    public TextMeshProUGUI TextAddress;
    public TextMeshProUGUI TextDescription;

    private jsonConstants constants;

    // Start is called before the first frame update
    void Start()
    {
        constants = APIConnectManager.Instance.login.constants;

        HomeApi summary = Header.Instance.GetSummary();

        if (summary.eth_addr != "")
            TextAddress.text = summary.eth_addr;
        else
            TextAddress.text = "ETHアドレスを登録してください";

        //説明
        TextDescription.text = summary.eth_addr_description;

        ButtonReg.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySE("se_btn");

            string addr_text = Address.text;

            if (addr_text == "")
            {
                Main.Instance.showDialogue("アドレスを入力するのだ");
                return;
            }
            else if (summary.eth_addr != "" && summary.eth_addr == addr_text)
            {
                Main.Instance.showDialogue("違うアドレスを入力するのだ");
                return;
            }

            //APIをたたく
            APIConnectManager.Instance.AddrRegist(addr_text, (string json) =>
            {
                CommonError response = JsonUtility.FromJson<CommonError>(json);

                if (response.result == "ok")
                {
                    if (response.err_code != null)
                    {
                        switch (response.err_code)
                        {
                            case "empty_address":
                                Main.Instance.showDialogue("アドレスを入力するのだ");
                                break;
                            case "invaild_address":
                                Main.Instance.showDialogue("それはETHのアドレスじゃないのだ。ETHアドレスを入力するのだ。");
                                break;
                        }
                    }
                    else
                    {
                        Main.Instance.showDialogue("ETHアドレスを登録したのだ");
                        //入力欄はクリア
                        Address.text = "";
                        //サマリー更新
                        summary.eth_addr = addr_text;
                        Header.Instance.SetSummary(summary);
                        //表示
                        TextAddress.text = summary.eth_addr;
                    }
                }
            });
        });
    }
    public void onTwitter()
    {
        AudioManager.Instance.PlaySE("se_btn");

        //Twitter投稿画面の起動
        Application.OpenURL(constants.TWITTER_URI);
    }

}
