using System;
using System.Collections;
using System.Collections.Generic;
using CreateWave;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Help : BaseBehaviour
{
    public GameObject ListTitle;
    public GameObject ListContent;
    public GameObject ListNone;
    public GameObject Content;
    public TextMeshProUGUI Title;

    jsonHelpList HelpList;

    // Start is called before the first frame update
    protected override void Start()
    {
        //base.Start();

        ListTitle.SetActive(false);
        ListContent.SetActive(false);
        ListNone.SetActive(false);

        Title.text = Utility.getText("TEXT_NAVI_HELP");

        //APIをたたく
        APIConnectManager.Instance.HelpList(null, onStart);

        DispatchEvent(CwEvent.SCENE_READY);
    }

    jsonHelpList makeJson(string json)
    {
        jsonHelpList response = JsonUtility.FromJson<jsonHelpList>(json);

        Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        foreach (KeyValuePair<string, object> keyvalue in jsonDict)
        {
            if (keyvalue.Key == "groups")
            {
                try
                {
                    response.groups = new Dictionary<string, string>();
                    Dictionary<string, string> jsonDict2 = JsonConvert.DeserializeObject<Dictionary<string, string>>(keyvalue.Value.ToString());

                    foreach (KeyValuePair<string, string> keyvalue2 in jsonDict2)
                    {
                        if (keyvalue2.Value != null)
                        {
                            response.groups.Add(keyvalue2.Key, keyvalue2.Value);
                        }
                    }

                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    response.groups = null;
                }
            }
            else if (keyvalue.Key == "helpTree")
            {
                try
                {
                    response.helpTree = new Dictionary<string, jsonHelpCaption[]>();
                    Dictionary<string, object> jsonDict2 = JsonConvert.DeserializeObject<Dictionary<string, object>>(keyvalue.Value.ToString());

                    foreach (KeyValuePair<string, object> keyvalue2 in jsonDict2)
                    {
                        if (keyvalue2.Value != null)
                        {

                            jsonHelpCaption[] jsonDict3 = JsonConvert.DeserializeObject<jsonHelpCaption[]>(keyvalue2.Value.ToString());
                            //jsonHelpCaption jsonDict3 = JsonUtility.FromJson<jsonHelpCaption>(keyvalue2.Value.ToString());

                            response.helpTree.Add(keyvalue2.Key, jsonDict3);

                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    response.helpTree = null;
                }
            }
        }
        return response;
    }

    void onStart(string json)
    {

        HelpList = makeJson(json);


        reload();
    }


    public void reload()
    {

        //リストクリア
        ListClear();


        // 一つもなかったら...
        if (HelpList.helpTree.Count == 0)
        {
            // その旨のパネルを表示。
            ListNone.gameObject.SetActive(true);

            // 処理はここまで。
            return;
        }

        Dictionary<int, Dictionary<string, string>> helplist = new Dictionary<int, Dictionary<string, string>>();

        int i = 0;
        foreach (KeyValuePair<string, string> keyvalue in HelpList.groups)
        {
            GameObject board = null;

            // リストを複製
            board = UnityEngine.Object.Instantiate(ListTitle, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
            board.name = "ListTitle" + i;
            board.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = keyvalue.Value;
            board.SetActive(true);

            foreach (jsonHelpCaption entry in HelpList.helpTree[keyvalue.Key])
            {
                //apple申請中の場合は端末引き継ぎ非表示
                if (Main.Instance.in_apply && entry.help_id == "other-inherit")
                    continue;

                //apple申請中の場合は友達招待非表示
                if (Main.Instance.in_apply && entry.help_id == "other-shoutai")
                    continue;

                board = null;
                board = UnityEngine.Object.Instantiate(ListContent, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
                board.name = "ListContent" + i;
                setupEntryBoard(entry, board);

                board.SetActive(true);
            }

            i++;
        }

    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * 引数に指定されたエントリを、指定されたボードに表示するときに呼ばれる。
     */
    void setupEntryBoard(jsonHelpCaption entry, GameObject board)
    {

        board.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = entry.help_title;

        board.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySE("se_btn");

            SceneController.Instance.PopUp("HelpDetail", () =>
            {
                HelpDetailBehaviour helpdetail = FindObjectOfType<HelpDetailBehaviour>() as HelpDetailBehaviour;
                helpdetail.Param = new HelpDetailBehaviour.Parameter { id = entry.help_id };
            });
        });

    }

    /// <summary>
    /// リストを全部消す
    /// </summary>
    void ListClear()
    {
        //テンプレート非表示
        ListTitle.gameObject.SetActive(false);
        ListContent.gameObject.SetActive(false);
        ListNone.gameObject.SetActive(false);

        foreach (Transform n in Content.transform)
        {
            //テンプレート以外は全部削除
            if (n.name != ListTitle.name && n.name != ListContent.name && n.name != ListNone.name)
                GameObject.Destroy(n.gameObject);
        }
    }

    public void TapClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.ClosePopUpName("Help");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
