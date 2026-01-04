using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using System;
using UnityEngine.UI;
using MyScene;
using Scenes.Common.Scripts;

public class RaidZukanBehaviour : BaseBehaviour
{

    public GameObject ListNone;
    public GameObject ListMonster;
    public GameObject Content;

    public GameObject FilterContent;

    public GameObject Filter;
    public GameObject Detail;

    public TextMeshProUGUI monster_name;
    public TextMeshProUGUI monster_flavor;
    public TextMeshProUGUI HP;
    public TextMeshProUGUI habitat;
    public GameObject StatusPanel;
    public TextMeshProUGUI race;
    public TextMeshProUGUI rare;

    public TextMeshProUGUI TitleText;

    public Image monster_graph;

    public GameObject Card1;
    public GameObject Card2;
    public GameObject Card3;

    public GameObject Tab_0;
    public GameObject Tab_1;
    public GameObject Tab_2;
    public GameObject Tab_3;
    public GameObject Tab_4;
    public GameObject Tab_5;

    public Image BG;

    private int filter_selected;

    private jsonRaidMonster raidmonster { get; set; }
    jsonRaidMonsterList[] list { get; set; }

    private int raid_dungeon_id;

    //何日前か
    private int date;

    HomeApi summary { get; set; }
    jsonConstants constants;
    public class Parameter
    {
        public int raid_dungeon_id;
    }

    public Parameter Param;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //ヘッダー・フッターに情報を渡す
        Header.Instance.SetTitle(Utility.getText("TEXT_RAID_ZUKAN"));

        BG.sprite = Utility.getAssetImage("Image/BG/bg2");

        //safearea対応
        setSafearea("RaidZukanCanvas");

        raid_dungeon_id = Param.raid_dungeon_id;

        Filter.SetActive(false);
        Detail.SetActive(false);

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        date = 0;

        ListClear();

        //APIをたたく
        APIConnectManager.Instance.RaidMonsterList(date, onStart);

        DispatchEvent(CwEvent.SCENE_READY);
    }

    /// <summary>
    /// スタート時呼び出し
    /// </summary>
    /// <param name="json">json</param>
    void onStart(string json)
    {
        raidmonster = JsonUtility.FromJson<jsonRaidMonster>(json);

        //フィルターのタブ初期化
        filter_selected = 0;
        filterSelect(filter_selected);

        summary = Header.Instance.GetSummary();

        if (raidmonster.raid_dungeon.status == constants.Raid_Dungeon.NONE || raidmonster.raid_dungeon.status == constants.Raid_Dungeon.READY)
        {
            TitleText.text = raidmonster.raid_dungeon.title;

            Tab_0.SetActive(true);
            Tab_1.SetActive(false);
            Tab_2.SetActive(false);
            Tab_3.SetActive(false);
            Tab_4.SetActive(false);
            Tab_5.SetActive(false);
        }
        else
        {
            TitleText.text = raidmonster.raid_dungeon.title + " " + Utility.getText("TEXT_CURRENT").Replace("{0}", raidmonster.raid_dungeon.defeat_count + "/" + raidmonster.raid_dungeon.total_count);

            if (raidmonster.raid_dungeon.past == 0)
            {
                Tab_0.SetActive(true);
                Tab_1.SetActive(false);
                Tab_2.SetActive(false);
                Tab_3.SetActive(false);
                Tab_4.SetActive(false);
                Tab_5.SetActive(false);
            }
            else if (raidmonster.raid_dungeon.past == 1)
            {
                Tab_0.SetActive(true);
                Tab_1.SetActive(true);
                Tab_2.SetActive(false);
                Tab_3.SetActive(false);
                Tab_4.SetActive(false);
                Tab_5.SetActive(false);
            }
            else if (raidmonster.raid_dungeon.past == 2)
            {
                Tab_0.SetActive(true);
                Tab_1.SetActive(true);
                Tab_2.SetActive(true);
                Tab_3.SetActive(false);
                Tab_4.SetActive(false);
                Tab_5.SetActive(false);

            }
            else if (raidmonster.raid_dungeon.past == 3)
            {
                Tab_0.SetActive(true);
                Tab_1.SetActive(true);
                Tab_2.SetActive(true);
                Tab_3.SetActive(true);
                Tab_4.SetActive(false);
                Tab_5.SetActive(false);

            }
            else if (raidmonster.raid_dungeon.past == 4)
            {
                Tab_0.SetActive(true);
                Tab_1.SetActive(true);
                Tab_2.SetActive(true);
                Tab_3.SetActive(true);
                Tab_4.SetActive(true);
                Tab_5.SetActive(false);

            }
            else if (raidmonster.raid_dungeon.past == 5)
            {
                Tab_0.SetActive(true);
                Tab_1.SetActive(true);
                Tab_2.SetActive(true);
                Tab_3.SetActive(true);
                Tab_4.SetActive(true);
                Tab_5.SetActive(true);


            }

        }

        reload();
    }

    void filterSelect(int select)
    {

        for (int i = 0; i < 4; i++)
        {
            GameObject ListFilter = FilterContent.transform.Find("ListFilter" + i).gameObject;
            if (i == select)
            {
                ListFilter.transform.Find("Flame/Panel").gameObject.SetActive(false);
                ListFilter.transform.Find("Flame/PanelSelect").gameObject.SetActive(true);
            }
            else
            {
                ListFilter.transform.Find("Flame/Panel").gameObject.SetActive(true);
                ListFilter.transform.Find("Flame/PanelSelect").gameObject.SetActive(false);
            }
        }

        /*
        switch (select)
        {
            case 0:
                TitleText.text = "全部表示";
                break;
            case 1:
                TitleText.text = "倒したモンスター";
                break;
            case 2:
                TitleText.text = "倒されてないモンスター";
                break;
            case 3:
                TitleText.text = "自分が倒したモンスター";
                break;
        }
        */
    }

    void reload()
    {
        ListClear();

        list = raidmonster.monsterlist;

        // GETアイテムが一つもなかったら...
        if (raidmonster.monsterlist.Length == 0)
        {
            // その旨のパネルを表示。
            ListNone.SetActive(true);
            Content.SetActive(false);

            // 処理はここまで。
            return;
        }

        //list作成
        List<jsonRaidMonsterList> monster = new List<jsonRaidMonsterList>();

        foreach (jsonRaidMonsterList entry in list)
        {
            //絞り込む
            if (filter_selected > 0)
            {
                if (filter_selected == 1)
                {
                    if (entry.defeat_user.avatar != null)
                    {
                        monster.Add(entry);
                    }
                }
                else if (filter_selected == 2)
                {
                    if (entry.defeat_user.avatar == null)
                    {
                        monster.Add(entry);
                    }
                }
                else
                {
                    if (entry.defeat_user.avatar != null)
                    {
                        if (entry.defeat_user.avatar.user_id == summary.chara.user_id)
                        {
                            monster.Add(entry);
                        }
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
        foreach (jsonRaidMonsterList entry in monster)
        {
            GameObject board = null;

            // リストを複製
            board = UnityEngine.Object.Instantiate(ListMonster, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
            board.name = "ListMonster" + i;

            setupEntryBoard(entry, board);

            board.SetActive(true);
            i++;
        }

    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * 引数に指定されたエントリを、指定されたボードに表示するときに呼ばれる。
     */
    void setupEntryBoard(jsonRaidMonsterList entry, GameObject board)
    {

        Sprite rarePanelSprite = Utility.getAssetImage("monster_panel" + entry.monster.rare_level);
        board.transform.Find("MonsterFrame").GetComponent<Image>().sprite = rarePanelSprite;

        var icon_url = entry.monster.graphic_id.ToString("D5");
        board.transform.Find("MonsterFrame/Monster").GetComponent<Image>().sprite = Utility.getAssetImage("Image/MOB/" + icon_url);

        Button btn = board.transform.Find("MonsterFrame").GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySE("se_btn");

            monster_name.text = entry.monster.monster_name;
            monster_flavor.text = entry.monster.flavor_text;

            var icon_url = entry.monster.graphic_id.ToString("D5");
            monster_graph.sprite = Utility.getAssetImage("Image/MOB/" + icon_url);

            Card1.SetActive(false);
            Card2.SetActive(false);
            Card3.SetActive(false);

            if (entry.monster.rare_level == 1)
            {
                Card1.SetActive(true);
                monster_name.color = ColorGet.Hex(0xFFFFFF);
            }
            else if (entry.monster.rare_level == 2)
            {
                Card2.SetActive(true);
                monster_name.color = ColorGet.Hex(0xFFFFFF);
            }
            else if (entry.monster.rare_level == 3)
            {
                Card3.SetActive(true);
                monster_name.color = ColorGet.Hex(0x776451);
            }

            rare.text = Utility.getText("Monster_Master_RARE_LEVELS" + entry.monster.rare_level);

            habitat.text = entry.monster.habitat;
            HP.text = entry.monster.hp_max.ToString();


            StatusPanel.transform.Find("att1").GetComponent<TextMeshProUGUI>().text = entry.monster.attack1.ToString();
            StatusPanel.transform.Find("att2").GetComponent<TextMeshProUGUI>().text = entry.monster.attack2.ToString();
            StatusPanel.transform.Find("att3").GetComponent<TextMeshProUGUI>().text = entry.monster.attack3.ToString();
            StatusPanel.transform.Find("spd").GetComponent<TextMeshProUGUI>().text = entry.monster.speed.ToString();

            StatusPanel.transform.Find("def1").GetComponent<TextMeshProUGUI>().text = entry.monster.defence1.ToString();
            StatusPanel.transform.Find("def2").GetComponent<TextMeshProUGUI>().text = entry.monster.defence2.ToString();
            StatusPanel.transform.Find("def3").GetComponent<TextMeshProUGUI>().text = entry.monster.defence3.ToString();
            StatusPanel.transform.Find("defX").GetComponent<TextMeshProUGUI>().text = entry.monster.defenceX.ToString();

            Detail.SetActive(true);

        });

        board.transform.Find("MonsterName").GetComponent<TextMeshProUGUI>().text = entry.monster.monster_name;
        board.transform.Find("Habitat").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_HABITAT").Replace("{0}", entry.monster.habitat);
        board.transform.Find("MonsterLv").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_LEVEL").Replace("{0}", entry.monster.level.ToString());

        string raretext = Utility.getText("Monster_Master_RARE_LEVELS" + entry.monster.rare_level);

        board.transform.Find("Rare").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_RARE").Replace("{0}", raretext.ToString());
        board.transform.Find("FlavorText").GetComponent<TextMeshProUGUI>().text = entry.monster.flavor_text;

        if (entry.defeat_user.avatar == null)
        {
            board.transform.Find("player_name").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_OVERTHROU").Replace("{0}", Utility.getText("TEXT_NONE"));
            board.transform.Find("grade").GetComponent<TextMeshProUGUI>().text = "";
            board.transform.Find("Status").GetComponent<Image>().sprite = Utility.getAssetImage(Utility.getStatusIcon(2));
        }
        else
        {
            board.transform.Find("player_name").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_OVERTHROU").Replace("{0}", entry.defeat_user.avatar.player_name + "(" + "Lv:" + entry.defeat_user.avatar.level + ")");
            board.transform.Find("grade").GetComponent<TextMeshProUGUI>().text = entry.defeat_user.avatar.grade.grade_name;
            board.transform.Find("Status").GetComponent<Image>().sprite = Utility.getAssetImage(Utility.getStatusIcon(3));
        }

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

        //FilterListClear();

        Detail.SetActive(false);
    }
    public void gotoRaidDungeon()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.Jump("RaidInfo");
    }

    public void onFilterClick(int index)
    {
        //クリック時イベントハンドラ
        AudioManager.Instance.PlaySE("se_btn");

        //フィルターのタブ初期化
        filter_selected = index;
        filterSelect(filter_selected);

        reload();

        Filter.SetActive(false);
    }

    /// <summary>
    /// タブクリック時イベントハンドラ
    /// </summary>
    /// <param name="_category">カテゴリ</param>
    public void onChangeDate(int _date)
    {
        if (this.date == _date)
            return;

        AudioManager.Instance.PlaySE("se_btn");

        this.date = _date;

        APIConnectManager.Instance.RaidMonsterList(date, (string json) =>
        {
            raidmonster = JsonUtility.FromJson<jsonRaidMonster>(json);
            if (raidmonster.raid_dungeon.status == constants.Raid_Dungeon.NONE || raidmonster.raid_dungeon.status == constants.Raid_Dungeon.READY)
            {
                TitleText.text = raidmonster.raid_dungeon.title;
            }
            else
            {
                TitleText.text = raidmonster.raid_dungeon.title + " " + Utility.getText("TEXT_CURRENT").Replace("{0}", raidmonster.raid_dungeon.defeat_count + "/" + raidmonster.raid_dungeon.total_count);
            }
            reload();
        });

    }

}
