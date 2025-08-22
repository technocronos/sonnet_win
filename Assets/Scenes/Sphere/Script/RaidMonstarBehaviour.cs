using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using System;
using UnityEngine.UI;
using CreateWave;
using Scenes.Common.Scripts;
using UnityEngine.Networking;
using UnityEngine.Localization.Settings;

public class RaidMonstarBehaviour : MonoBehaviour
{

    public GameObject ListNone;
    public GameObject ListMonster;
    public GameObject Content;

    public GameObject BannarPanel;
    public Image Bannar;

    public GameObject FilterContent;

    public GameObject Filter;

    public TextMeshProUGUI TitleText;

    private int filter_selected;

    private jsonRaidMonster raidmonster { get; set; }
    jsonRaidMonsterList[] list { get; set; }

    private int raid_dungeon_id;

    HomeApi summary { get; set; }

    jsonConstants constants { get; set; }

    Vector3 _position { set; get; }

    private UserBehaviour User { get; set; }

    private bool flick_lock;
    private bool tap_flg;


    // Start is called before the first frame update
    public void init(int _raid_dungeon_id)
    {
        User = UserBehaviour.Instance;

        raid_dungeon_id = _raid_dungeon_id;

        //flick_lockとtap_flgを取っておく
        this.flick_lock = User.flick_lock;
        this.tap_flg = User.tap_flg;

        //フリック無効
        User.flick_lock = true;
        //画面をタップしてカーソルを動かすのは無効
        User.tap_flg = false;

        Filter.SetActive(false);

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        _position = ListMonster.transform.localPosition;

        BannarPanel.SetActive(false);

        ListClear();

        int date = 0;

        //APIをたたく
        APIConnectManager.Instance.RaidMonsterList(date, onStart);
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
        }
        else
        {
            TitleText.text = raidmonster.raid_dungeon.title + " " + Utility.getText("TEXT_CURRENT").Replace("{0}", raidmonster.raid_dungeon.defeat_count + "/" + raidmonster.raid_dungeon.total_count);
        }

        //バナー初期化
        if (raidmonster.raid_dungeon.require_kind == constants.Raid_Dungeon.REQUIRE_ETHADDR)
        {
            string url = Settings.Domain + "://" + Settings.Host + "/" + "img/parts/sp/bannar/b_r_" + raid_dungeon_id.ToString("D5") + ".png";

            if (Main.Locale != LocalizationSettings.AvailableLocales.Locales[0])
            {
                url = Settings.Domain + "://" + Settings.Host + "/" + "img/parts/sp/bannar/b_r_" + raid_dungeon_id.ToString("D5") + "_en.png";
            }

            StartCoroutine(GetTexture(Bannar, url, "Bannar/b_q_99999_2"));

            Bannar.GetComponent<Button>().onClick.AddListener(() =>
            {
                Main.Instance.EtheriumCanvasShow();
            });

            BannarPanel.SetActive(true);
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
            board.transform.localPosition = new Vector3(_position.x, _position.y, 0);

            setupEntryBoard(entry, board);

            board.SetActive(true);
            i++;
        }

    }

    public IEnumerator GetTexture(Image img, string url, string substitute = null)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);

            img.sprite = Utility.getAssetImage(substitute);
        }
        else
        {
            Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            //Texture2DをSpriteに変換
            Sprite sprite = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height), Vector2.zero);

            img.sprite = sprite;
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

    public void onFilterOpen()
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

    public void onRaidMonstarClose()
    {
        //flick_lockとtap_flgを戻す
        User.flick_lock = this.flick_lock;
        User.tap_flg = this.tap_flg;

        AudioManager.Instance.PlaySE("se_btn");
        transform.gameObject.SetActive(false);
    }

}
