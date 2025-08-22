using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CreateWave;

public class BitCoinBehaviour : BaseBehaviour
{
    public TextMeshProUGUI btc_amount;
    public TMP_InputField btc_address;

    public TextMeshProUGUI CaptionYourAddress;
    public TextMeshProUGUI TextHelp;
    

    public GameObject List;
    public GameObject ListHelpTitle;
    public GameObject ButtonHelp1;
    public GameObject ButtonHelp2;
    public GameObject ButtonHelp3;
    public GameObject ButtonHelp4;

    public GameObject ListExplain;
    public GameObject Content;

    public BitcoinGetLogBehaviour BitcoinGetLog;
    public BitcoinApplyListBehaviour BitcoinApplyList;

    private jsonConstants constants;
    private HomeApi summary;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        constants = APIConnectManager.Instance.login.constants;

        BitcoinGetLog.gameObject.SetActive(false);
        BitcoinApplyList.gameObject.SetActive(false);

        CaptionYourAddress.text = Utility.getText("TEXT_YOUR_ADDRESS");
        TextHelp.text = Utility.getText("TEXT_BTC_HELP");

        ButtonHelp1.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("help_master_help_title_vcoin-about");
        ButtonHelp2.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("help_master_help_title_vcoin-receive");
        ButtonHelp3.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("help_master_help_title_vcoin-receive-order");
        ButtonHelp4.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("help_master_help_title_vcoin-notice");

        reload();
        makelist();

        DispatchEvent(CwEvent.SCENE_READY);
    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * メイン処理
     */
    void reload()
    {

        //ホームサマリ情報を取得する。
        summary = Header.Instance.GetSummary();

        btc_amount.text = Utility.getText("BITCOIN_TEXT_CURRENT") + "：" + decimal.Parse(summary.vcoin.ToString(), System.Globalization.NumberStyles.AllowExponent | System.Globalization.NumberStyles.AllowDecimalPoint) + "BTC";

        if ((summary.vcoin - constants.VCOIN_MINIMAM) >= 0)
        {
            btc_address.enabled = true;
        }
        else
        {
            btc_address.enabled = false;
        }


    }

    void makelist()
    {

        //リストクリア
        ListClear();

        int i = 0;
        foreach (string str in summary.bitcoin_explain)
        {

            GameObject board = null;

            // リストを複製
            board = UnityEngine.Object.Instantiate(ListExplain, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
            board.name = "ListExplain" + i;
            board.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = str;

            board.GetComponent<RectTransform>().SetAsLastSibling();

            board.SetActive(true);

            i++;
        }

        ListHelpTitle.GetComponent<RectTransform>().SetAsLastSibling();
        ButtonHelp1.GetComponent<RectTransform>().SetAsLastSibling();
        ButtonHelp2.GetComponent<RectTransform>().SetAsLastSibling();
        ButtonHelp3.GetComponent<RectTransform>().SetAsLastSibling();
        ButtonHelp4.GetComponent<RectTransform>().SetAsLastSibling();
    }


    //okボタンクリック時イベントハンドラ
    public void onOkButton()
    {
        AudioManager.Instance.PlaySE("se_btn");

        var address = btc_address.text;

        var txt = Utility.getText("BITCOIN_TEXT_SYUKKIN_CONF");
        var amount = summary.vcoin;

        if ((summary.vcoin - constants.VCOIN_MINIMAM) < 0)
        {
            txt = Utility.getText("BITCOIN_ERROR_SHORT_AMOUNT").Replace("{0}", constants.VCOIN_MINIMAM.ToString());
            Main.Instance.showDialogue(txt);

            return;
        }
        else if (address == "")
        {
            txt = Utility.getText("BITCOIN_ERROR_NO_ADDRESS");
            Main.Instance.showDialogue(txt);

            return;
        }

        //確認ポップアップを立ち上げる
        Main.Instance.showConfirm(txt, () =>
        {
            AudioManager.Instance.PlaySE("se_btn");

            //出金依頼をする
            APIConnectManager.Instance.VcoinSend(amount, address, (string json) =>
            {
                jsonVcoinSend response = JsonUtility.FromJson<jsonVcoinSend>(json);

                if (response.result == "no_address")
                {
                    txt = Utility.getText("BITCOIN_ERROR_NO_ADDRESS");
                }
                else if (response.result == "invalid_address")
                {
                    txt = Utility.getText("BITCOIN_ERROR_INVAILD_ADDRESS");
                }
                else if (response.result == "no_user")
                {
                    txt = Utility.getText("BITCOIN_ERROR_NO_USER");
                }
                else if (response.result == "short_amount")
                {
                    txt = Utility.getText("BITCOIN_ERROR_SHORT_AMOUNT").Replace("{0}", constants.VCOIN_MINIMAM.ToString());
                }
                else if (response.result == "short_payment")
                {
                    txt = Utility.getText("BITCOIN_ERROR_SHORT_PAYMENT").Replace("{0}", constants.VCOIN_MINIMAM_PAYMENT.ToString()).Replace("{1}", response.short_payment.ToString());
                }
                else if (response.result == "invalid_amount")
                {
                    txt = Utility.getText("BITCOIN_ERROR_INVALID_AMOUNT");
                }
                else if (response.result == "payment_stop")
                {
                    txt = Utility.getText("BITCOIN_ERROR_PAYMENT_STOP");
                }
                else if (response.result == "canpain_stop")
                {
                    txt = Utility.getText("BITCOIN_ERROR_CANPAIN_STOP");
                }
                else
                {
                    txt = Utility.getText("BITCOIN_SYUKKIN_OK");

                    summary.vcoin = 0;
                    Footer.Instance.SetSummary(summary);
                }

                Main.Instance.showDialogue(txt, () =>
                {
                    AudioManager.Instance.PlaySE("se_btn");
                    reload();
                });

            });
        });
    }


    //履歴ボタンクリック時イベントハンドラ
    public void onLogShow()
    {
        AudioManager.Instance.PlaySE("se_btn");
        BitcoinGetLog.gameObject.SetActive(true);
        BitcoinGetLog.Show();
    }

    //履歴ボタンクリック時イベントハンドラ
    public void onListShow()
    {
        AudioManager.Instance.PlaySE("se_btn");
        BitcoinApplyList.gameObject.SetActive(true);
        BitcoinApplyList.Show();
    }

    //閉じるボタンクリック時イベントハンドラ
    public void onClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.Jump("Home");
    }

    public void onHelp(string help_id)
    {
        AudioManager.Instance.PlaySE("se_btn");
        SceneController.Instance.PopUp("HelpDetail", () =>
        {
            HelpDetailBehaviour helpdetail = FindObjectOfType<HelpDetailBehaviour>() as HelpDetailBehaviour;
            helpdetail.Param = new HelpDetailBehaviour.Parameter { id = help_id };
        });
    }

    /// <summary>
    /// リストを全部消す
    /// </summary>
    void ListClear()
    {
        //テンプレート非表示
        ListExplain.gameObject.SetActive(false);

        foreach (Transform n in Content.transform)
        {
            //テンプレート以外は全部削除
            if (n.name != ListExplain.name && n.name != List.name && n.name != ListHelpTitle.name && n.name != ButtonHelp1.name && n.name != ButtonHelp2.name && n.name != ButtonHelp3.name && n.name != ButtonHelp4.name)
                GameObject.Destroy(n.gameObject);
        }
    }

}
