using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BitcoinGetLogBehaviour : MonoBehaviour
{
    public GameObject ListContent;
    public GameObject ListNone;
    public GameObject Content;

    jsonVcoinLog loglist;

    // Start is called before the first frame update
    public void Show()
    {
        //APIをたたく
        APIConnectManager.Instance.VcoinLog(onStart);
    }

    jsonVcoinLog makeJson(string json)
    {
        jsonVcoinLog response = JsonUtility.FromJson<jsonVcoinLog>(json);

        Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        foreach (KeyValuePair<string, object> keyvalue in jsonDict)
        {
            if (keyvalue.Key == "resultset")
            {
                try
                {
                    response.resultset = new List<jsonVcoinLogResult>();
                    List<object> jsonDict2 = JsonConvert.DeserializeObject<List<object>>(keyvalue.Value.ToString());

                    foreach (object keyvalue2 in jsonDict2)
                    {
                        if (keyvalue2 != null)
                        {
                            response.resultset.Add(JsonUtility.FromJson<jsonVcoinLogResult>(keyvalue2.ToString()));
                        }
                    }

                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    response.resultset = new List<jsonVcoinLogResult>();
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
        foreach (jsonVcoinLogResult table in loglist.resultset)
        {
            GameObject board = null;

            // リストを複製
            board = UnityEngine.Object.Instantiate(ListContent, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
            board.name = "ListContent" + i;
            board.transform.Find("TextReason").GetComponent<TextMeshProUGUI>().text = table.reason;
            board.transform.Find("TextName").GetComponent<TextMeshProUGUI>().text = table.name;
            board.transform.Find("TextAmount").GetComponent<TextMeshProUGUI>().text = decimal.Parse(table.amount.ToString(), System.Globalization.NumberStyles.AllowExponent | System.Globalization.NumberStyles.AllowDecimalPoint).ToString();
            board.transform.Find("TextUpdateAt").GetComponent<TextMeshProUGUI>().text = table.update_at;

            board.SetActive(true);

            i++;
        }

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
