using MyScene;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Book : MonoBehaviour
{
    public Quest Quest;

    public TextMeshProUGUI capturetext;

    public GameObject BookCanvas;
    public GameObject BookDetailCanvas;

    public GameObject ListNone;
    public GameObject ListMonster;
    public GameObject Content;

    public GameObject ListFilter;
    public GameObject FilterContent;

    public GameObject Filter;
    public GameObject Detail;

    public TextMeshProUGUI monster_text;

    public TextMeshProUGUI navigator;
    public TextMeshProUGUI filternavigator;

    public TextMeshProUGUI monster_name;
    public TextMeshProUGUI monster_flavor;
    public TextMeshProUGUI HP;
    public TextMeshProUGUI habitat;
    public GameObject StatusPanel;
    public TextMeshProUGUI race;
    public TextMeshProUGUI rare;

    public Image monster_graph;

    public GameObject Card1;
    public GameObject Card2;
    public GameObject Card3;

    jsonMonsterList response { get; set; }
    jsonMonsterListResultSet[] list { get; set; }
    int category { get; set; }

    // Start is called before the first frame update
    public void show()
    {
        Header.Instance.SetTitle(Utility.getText("TEXT_BOOK"));

        HomeApi summary = Header.Instance.GetSummary();

        capturetext.text = Utility.getText("BOOK_CAPTURE_PER") + " " + summary.monster_capture + "/" + summary.monster_count;

        BookCanvas.SetActive(true);
        BookDetailCanvas.SetActive(false);

    }

    public void onButton(int category)
    {
        AudioManager.Instance.PlaySE("se_btn");

        show_detail(category);
    }

    public void show_detail(int _category)
    {
        category = _category;

        Filter.SetActive(false);
        Detail.SetActive(false);

        BookCanvas.SetActive(false);
        BookDetailCanvas.SetActive(true);

        ListClear();

        //APIをたたく
        APIConnectManager.Instance.MonsterList(category, onStart);
    }
    jsonMonsterList jsonParse(string json)
    {
        jsonMonsterList response = JsonUtility.FromJson<jsonMonsterList>(json);

        Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        foreach (KeyValuePair<string, object> keyvalue in jsonDict)
        {
            if (keyvalue.Key == "flavor")
            {
                try
                {
                    response.flavor = new Dictionary<int, string>();
                    Dictionary<int, string> jsonDict2 = JsonConvert.DeserializeObject<Dictionary<int, string>>(keyvalue.Value.ToString());

                    foreach (KeyValuePair<int, string> keyvalue2 in jsonDict2)
                    {
                        if (keyvalue2.Value != null)
                        {
                            response.flavor.Add(keyvalue2.Key, keyvalue2.Value);
                        }
                    }

                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    response.flavor = new Dictionary<int, string>();
                    string[] jsonDict2 = JsonConvert.DeserializeObject<string[]>(keyvalue.Value.ToString());

                    int i = 0;
                    foreach (string keyvalue2 in jsonDict2)
                    {
                        response.flavor.Add(i, keyvalue2);
                        i++;
                    }
                }
            }
            else if (keyvalue.Key == "tab_list")
            {
                try
                {
                    response.tab_list = new Dictionary<int, string>();
                    Dictionary<int, string> jsonDict2 = JsonConvert.DeserializeObject<Dictionary<int, string>>(keyvalue.Value.ToString());

                    foreach (KeyValuePair<int, string> keyvalue2 in jsonDict2)
                    {
                        if (keyvalue2.Value != null)
                        {
                            response.tab_list.Add(keyvalue2.Key, keyvalue2.Value);
                        }
                    }

                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    response.tab_list = null;
                }
            }
            else if (keyvalue.Key == "category_text")
            {
                try
                {
                    response.category_text = new Dictionary<int, string>();
                    Dictionary<int, string> jsonDict2 = JsonConvert.DeserializeObject<Dictionary<int, string>>(keyvalue.Value.ToString());

                    foreach (KeyValuePair<int, string> keyvalue2 in jsonDict2)
                    {
                        if (keyvalue2.Value != null)
                        {
                            response.category_text.Add(keyvalue2.Key, keyvalue2.Value);
                        }
                    }

                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    response.category_text = null;
                }
            }
        }

        return response;
    }

    void onStart(string json)
    {
        response = jsonParse(json);

        Header.Instance.SetTitle(response.title);

        FilterListClear();

        int i = 1;
        //フィルターのタブ作成
        foreach (KeyValuePair<int, string> tab_list in response.tab_list)
        {
            GameObject board = null;

            // リストを複製
            board = UnityEngine.Object.Instantiate(ListFilter, new Vector3(0, 0, 0), Quaternion.identity, FilterContent.transform);
            board.name = "ListFilter" + tab_list.Key;

            //選択状態を決定する
            if (i == 1)
            {
                //デフォルト値
                filternavigator.text = response.title + " ＞ " + tab_list.Value;

                category = tab_list.Key;
                board.transform.Find("Flame/Panel").gameObject.SetActive(false);
                board.transform.Find("Flame/PanelSelect").gameObject.SetActive(true);
            }
            else
            {
                board.transform.Find("Flame/Panel").gameObject.SetActive(true);
                board.transform.Find("Flame/PanelSelect").gameObject.SetActive(false);
            }

            //キャプション
            board.transform.Find("Flame/caption").GetComponent<TextMeshProUGUI>().text = tab_list.Value;

            //クリック時イベントハンドラ
            Button btn = board.transform.Find("Flame").GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySE("se_btn");

                onChangeTab(tab_list.Key);

                Filter.SetActive(false);
            });

            board.SetActive(true);

            i++;
        }
        ;

        reload();
    }

    void reload()
    {
        ListClear();

        list = response.list.resultset;

        //タイトル設定
        //MainContentsDisplay.HeaderCanvas.caption(self.response["title"]);
        //MainContentsDisplay.HeaderCanvas.init();
        //MainContentsDisplay.HeaderCanvas.in();

        navigator.text = response.title + " ＞ " + response.tab_list[category];

        // GETアイテムが一つもなかったら...
        if (response.list.resultset.Length == 0)
        {
            // その旨のパネルを表示。
            ListNone.SetActive(true);
            Content.SetActive(false);

            // 処理はここまで。
            return;
        }

        //list作成
        List<jsonMonsterListResultSet> monster = new List<jsonMonsterListResultSet>();

        foreach (jsonMonsterListResultSet entry in list)
        {
            //カテゴリで絞り込む
            if (response.field != "terminate")
            {
                if (response.field == "appearance")
                {
                    if (entry.appearance_area == category)
                    {
                        monster.Add(entry);
                    }
                }
                else if (response.field == "rare_level")
                {
                    if (entry.rare_level == category)
                    {
                        monster.Add(entry);
                    }
                }
                else
                {
                    if (entry.category == category)
                    {
                        monster.Add(entry);
                    }
                }
            }
            else
            {
                monster.Add(entry);
            }
        }

        // 絞り込んだ地点でアイテムが一つもなかったら...
        if (monster.Count == 0)
        {
            // その旨のパネルを表示。
            ListNone.SetActive(true);
            Content.SetActive(false);

            // 処理はここまで。
            return;
        }

        int i = 0;
        foreach (jsonMonsterListResultSet entry in monster)
        {
            GameObject board = null;

            // リストを複製
            board = UnityEngine.Object.Instantiate(ListMonster, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
            board.name = "ListMonster" + i;

            setupEntryBoard(entry, board);

            board.SetActive(true);
            i++;
        }

        if (response.field == "terminate" || category == 4)
            monster_text.text = response.flavor[0];
        else
            monster_text.text = response.flavor[category];

    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * 引数に指定されたエントリを、指定されたボードに表示するときに呼ばれる。
     */
    void setupEntryBoard(jsonMonsterListResultSet entry, GameObject board)
    {
        if (entry.terminate_at != "")
        {
            board.transform.Find("Flame/monster_panel0").gameObject.SetActive(false);
            board.transform.Find("Flame/monster_panel1").gameObject.SetActive(false);
            board.transform.Find("Flame/monster_panel2").gameObject.SetActive(false);
            board.transform.Find("Flame/monster_panel3").gameObject.SetActive(false);

            board.transform.Find("Flame/monster_panel" + entry.rare_level).gameObject.SetActive(true);

            board.transform.Find("Flame/monster").gameObject.SetActive(true);

            var icon_url = entry.graphic_id.ToString("D5");
            board.transform.Find("Flame/monster").GetComponent<Image>().sprite = Utility.getAssetImage("Image/MOB/" + icon_url);

            Button btn = board.transform.Find("Flame").GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySE("se_btn");

                monster_name.text = entry.monster_name;
                monster_flavor.text = entry.flavor_text;

                var icon_url = entry.graphic_id.ToString("D5");
                monster_graph.sprite = Utility.getAssetImage("Image/MOB/" + icon_url);

                Card1.SetActive(false);
                Card2.SetActive(false);
                Card3.SetActive(false);

                if (entry.rare_level == 1)
                {
                    rare.text = Utility.getText("Monster_Master_RARE_LEVELS" + entry.rare_level);
                    Card1.SetActive(true);
                    monster_name.color = ColorGet.Hex(0xFFFFFF);
                }
                else if (entry.rare_level == 2)
                {
                    rare.text = Utility.getText("Monster_Master_RARE_LEVELS" + entry.rare_level);
                    Card2.SetActive(true);
                    monster_name.color = ColorGet.Hex(0xFFFFFF);
                }
                else if (entry.rare_level == 3)
                {
                    rare.text = Utility.getText("Monster_Master_RARE_LEVELS" + entry.rare_level);
                    Card3.SetActive(true);
                    monster_name.color = ColorGet.Hex(0x776451);
                }

                race.text = response.category_text[entry.category];
                habitat.text = entry.habitat;
                HP.text = entry.hp_max.ToString();


                StatusPanel.transform.Find("att1").GetComponent<TextMeshProUGUI>().text = entry.attack1.ToString();
                StatusPanel.transform.Find("att2").GetComponent<TextMeshProUGUI>().text = entry.attack2.ToString();
                StatusPanel.transform.Find("att3").GetComponent<TextMeshProUGUI>().text = entry.attack3.ToString();
                StatusPanel.transform.Find("spd").GetComponent<TextMeshProUGUI>().text = entry.speed.ToString();

                StatusPanel.transform.Find("def1").GetComponent<TextMeshProUGUI>().text = entry.defence1.ToString();
                StatusPanel.transform.Find("def2").GetComponent<TextMeshProUGUI>().text = entry.defence2.ToString();
                StatusPanel.transform.Find("def3").GetComponent<TextMeshProUGUI>().text = entry.defence3.ToString();
                StatusPanel.transform.Find("defX").GetComponent<TextMeshProUGUI>().text = entry.defenceX.ToString();

                Detail.SetActive(true);

            });
        }
        else
        {
            board.transform.Find("Flame/monster_panel0").gameObject.SetActive(true);
            board.transform.Find("Flame/monster_panel1").gameObject.SetActive(false);
            board.transform.Find("Flame/monster_panel2").gameObject.SetActive(false);
            board.transform.Find("Flame/monster_panel3").gameObject.SetActive(false);
            board.transform.Find("Flame/monster").gameObject.SetActive(false);

        }

        board.transform.Find("Flame/monster_no").GetComponent<TextMeshProUGUI>().text = entry.monster_no;

    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * タブ切り替え時に呼び出される。
     */
    void onChangeTab(int _category)
    {
        if (category == _category)
            return;

        AudioManager.Instance.PlaySE("se_btn");

        category = _category;

        //前のタブの選択を削除,新しいタブの選択
        //タブ作成
        foreach (KeyValuePair<int, string> tab_list in response.tab_list)
        {
            if (tab_list.Key == category)
            {
                FilterContent.transform.Find("ListFilter" + tab_list.Key + "/Flame/Panel").gameObject.SetActive(false);
                FilterContent.transform.Find("ListFilter" + tab_list.Key + "/Flame/PanelSelect").gameObject.SetActive(true);

                filternavigator.text = response.title + " ＞ " + tab_list.Value;
            }
            else
            {
                FilterContent.transform.Find("ListFilter" + tab_list.Key + "/Flame/Panel").gameObject.SetActive(true);
                FilterContent.transform.Find("ListFilter" + tab_list.Key + "/Flame/PanelSelect").gameObject.SetActive(false);
            }
        }
        ;

        reload();
    }

    public void onFilterClick()
    {
        AudioManager.Instance.PlaySE("se_btn");

        Filter.SetActive(true);
    }
    public void onFilterClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        //FilterListClear();

        Filter.SetActive(false);
    }


    public void onDetailClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        Detail.SetActive(false);
    }

    public void onBackClick()
    {
        AudioManager.Instance.PlaySE("se_btn");

        BookCanvas.SetActive(true);
        BookDetailCanvas.SetActive(false);

    }

    void ListClear()
    {
        //テンプレート非表示
        ListNone.gameObject.SetActive(false);
        ListMonster.gameObject.SetActive(false);
        Content.SetActive(true);

        foreach (Transform n in Content.transform)
        {
            //テンプレート以外は全部削除
            if (n.name != ListMonster.name && n.name != ListNone.name)
                GameObject.Destroy(n.gameObject);
        }
    }

    void FilterListClear()
    {
        //テンプレート非表示
        ListFilter.gameObject.SetActive(false);
        FilterContent.SetActive(true);

        foreach (Transform n in FilterContent.transform)
        {
            //テンプレート以外は全部削除
            if (n.name != ListFilter.name)
                GameObject.Destroy(n.gameObject);
        }
    }

    public void onButtonClose()
    {
        AudioManager.Instance.PlaySE("se_btn");
        Header.Instance.SetTitle(Quest.quest_title);
        this.gameObject.SetActive(false);
    }
}
