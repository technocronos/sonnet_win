using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BitcoinApplyListBehaviour : MonoBehaviour
{
    public GameObject ListContent;
    public GameObject ListNone;
    public GameObject Content;

    jsonVcoinApplyList loglist;
    private jsonConstants constants;

    // Start is called before the first frame update
    public void Show()
    {
        constants = APIConnectManager.Instance.login.constants;

        //APIをたたく
        APIConnectManager.Instance.VcoinList(onStart);
    }

    jsonVcoinApplyList makeJson(string json)
    {
        jsonVcoinApplyList response = JsonUtility.FromJson<jsonVcoinApplyList>(json);

        Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        foreach (KeyValuePair<string, object> keyvalue in jsonDict)
        {
            if (keyvalue.Key == "resultset")
            {
                try
                {
                    response.resultset = new List<jsonVcoinApplyListResult>();
                    List<object> jsonDict2 = JsonConvert.DeserializeObject<List<object>>(keyvalue.Value.ToString());

                    foreach (object keyvalue2 in jsonDict2)
                    {
                        if (keyvalue2 != null)
                        {
                            response.resultset.Add(JsonUtility.FromJson<jsonVcoinApplyListResult>(keyvalue2.ToString()));
                        }
                    }

                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    response.resultset = new List<jsonVcoinApplyListResult>();
                }
            }
        }
        return response;
    }

    void onStart(string json)
    {
        loglist = makeJson(json);

        reload();
    }

    void reload()
    {

        //リストクリア
        ListClear();

        // 一つもなかったら...
        if (loglist.resultset.Count == 0)
        {
            // その旨のパネルを表示。
            ListNone.gameObject.SetActive(true);

            // 処理はここまで。
            return;
        }

        Dictionary<int, Dictionary<string, string>> helplist = new Dictionary<int, Dictionary<string, string>>();

        int i = 0;
        foreach (jsonVcoinApplyListResult table in loglist.resultset)
        {
            GameObject board = null;

            // リストを複製
            board = UnityEngine.Object.Instantiate(ListContent, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
            board.name = "ListContent" + i;
            board.transform.Find("TextCreateAt").GetComponent<TextMeshProUGUI>().text = table.create_at;
            board.transform.Find("TextAmount").GetComponent<TextMeshProUGUI>().text = Utility.getVCoinAmount(table.amount) + "BTC";
            board.transform.Find("TextFee").GetComponent<TextMeshProUGUI>().text = decimal.Parse(table.fee.ToString(), System.Globalization.NumberStyles.AllowExponent | System.Globalization.NumberStyles.AllowDecimalPoint).ToString() + "BTC";
            board.transform.Find("TextAddress").GetComponent<TextMeshProUGUI>().text = table.address;
            board.transform.Find("TextStatus").GetComponent<TextMeshProUGUI>().text = getStatus(table);
            board.transform.Find("TextTransaction").GetComponent<TextMeshProUGUI>().text = table.transaction;

            Button ButtonLink = board.transform.Find("TextTransaction").GetComponent<Button>();
            ButtonLink.onClick.RemoveAllListeners();
            ButtonLink.onClick.AddListener(() =>
            {
                Application.OpenURL("https://live.blockcypher.com/btc/tx/" + table.transaction);
            });

            //クリップボードへ文字を設定(コピー)
            Button ButtonCopy = board.transform.Find("ButtonCopy").GetComponent<Button>();
            ButtonCopy.onClick.RemoveAllListeners();
            ButtonCopy.onClick.AddListener(() =>
            {
                GUIUtility.systemCopyBuffer = table.address;
                Main.Instance.showDialogue(Utility.getText("BITCOIN_COPY_ADDRESS"));
            });

            board.SetActive(true);

            i++;
        }

    }

    private string getStatus(jsonVcoinApplyListResult entry)
    {
        string text = "";
        if (entry.status == constants.Vcoin_Payment_Log.INITIAL)
        {
            text = Utility.getText("BITCOIN_STATUS_INITIAL");
        }
        else if (entry.status == constants.Vcoin_Payment_Log.RECEIVE)
        {
            text = Utility.getText("BITCOIN_STATUS_RECEIVE").Replace("{0}", entry.status_update_at);
        }
        else if (entry.status == constants.Vcoin_Payment_Log.CANCEL)
        {
            text = Utility.getText("BITCOIN_STATUS_CANCEL").Replace("{0}", entry.status_update_at);
        }
        else if (entry.status == constants.Vcoin_Payment_Log.COMPLETE)
        {
            text = Utility.getText("COMPLETE").Replace("{0}", entry.status_update_at);
        }

        return text;
    }

    /// <summary>
    /// リストを全部消す
    /// </summary>
    void ListClear()
    {
        //テンプレート非表示
        ListContent.gameObject.SetActive(false);
        ListNone.gameObject.SetActive(false);

        foreach (Transform n in Content.transform)
        {
            //テンプレート以外は全部削除
            if (n.name != ListContent.name && n.name != ListNone.name)
                GameObject.Destroy(n.gameObject);
        }
    }

    //閉じるボタンクリック時イベントハンドラ
    public void onClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        transform.gameObject.SetActive(false);
    }

}
