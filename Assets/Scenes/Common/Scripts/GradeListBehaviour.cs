using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GradeListBehaviour : MonoBehaviour
{
    public TextMeshProUGUI TextNavi;
    public TextMeshProUGUI CaptionGrade;
    public TextMeshProUGUI CaptionUserNum;
    public TextMeshProUGUI TextTani;
    

    public GameObject ListGrade;
    public GameObject Content;
    public GameObject objGradeUserPanel;

    jsonGradeList grade_list;
    jsonGrade[] list;

    public void Show()
    {
        TextNavi.text = Utility.getText("TEXT_NAV_GRADE_LIST_EXPLAIN");
        CaptionGrade.text = Utility.getText("TEXT_GRADE_CAPTION1");
        CaptionUserNum.text = Utility.getText("TEXT_USER_NUM");
        TextTani.text = Utility.getText("TEXT_TANI_USER_NUM");

        //APIをたたく
        APIConnectManager.Instance.GradeList(onStart);

    }


    jsonGradeList makeJson(string json)
    {
        jsonGradeList response = JsonUtility.FromJson<jsonGradeList>(json);

        Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        foreach (KeyValuePair<string, object> keyvalue in jsonDict)
        {
            if (keyvalue.Key == "distribute")
            {
                try
                {
                    response.distribute = new Dictionary<int, int>();
                    Dictionary<int, int> jsonDict2 = JsonConvert.DeserializeObject<Dictionary<int, int>>(keyvalue.Value.ToString());

                    foreach (KeyValuePair<int, int> keyvalue2 in jsonDict2)
                    {
                        response.distribute.Add(keyvalue2.Key, keyvalue2.Value);
                    }

                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
        }
        return response;
    }

    void onStart(string json)
    {
        grade_list = makeJson(json);
        list = grade_list.list;

        reload();
    }

    void reload()
    {
        //テンプレート非表示
        ListGrade.gameObject.SetActive(false);

        listClear();

        // 一つもなかったら...
        if (list.Length == 0)
        {
            // 処理はここまで。
            return;
        }

        int i = 0;
        foreach (jsonGrade entry in list)
        {
            GameObject board = null;

            // リストを複製
            board = UnityEngine.Object.Instantiate(ListGrade, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
            board.name = "ListGrade" + i;

            setupEntryBoard(entry, board);

            board.SetActive(true);
            i++;
        }


    }

    /**
     * 引数に指定されたエントリを、指定されたボードに表示するときに呼ばれる。
     */
    void setupEntryBoard(jsonGrade entry, GameObject board)
    {
        board.transform.Find("TextGradeName").GetComponent<TextMeshProUGUI>().text = entry.grade_name;
        board.transform.Find("ButtonGrade/Text").GetComponent<TextMeshProUGUI>().text = grade_list.distribute[entry.grade_id].ToString();

        //階級ユーザー表示ボタン
        Button ButtonGrade = board.transform.Find("ButtonGrade").GetComponent<Button>();
        ButtonGrade.onClick.RemoveAllListeners();
        ButtonGrade.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySE("se_btn");

            objGradeUserPanel.SetActive(true);
            objGradeUserPanel.transform.GetComponent<GradeUserBehaviour>().Show(entry.grade_id);
        });
    }

    void listClear()
    {
        foreach (Transform n in Content.transform)
        {
            //テンプレート以外は全部削除
            if (n.name != ListGrade.name)
                GameObject.Destroy(n.gameObject);
        }
    }

    public void onClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        transform.gameObject.SetActive(false);
    }

}
