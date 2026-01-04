using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using System;
using UnityEngine.UI;
using MyScene;
using Scenes.Common.Scripts;

public class RaidRankingBehaviour : BaseBehaviour
{
    public TextMeshProUGUI NaviText;
    public GameObject ListNone;
    public GameObject ListMember;
    public GameObject Content;

    public Image BG;

    jsonRaidRanking response { get; set; }

    private int raid_dungeon_id { get; set; }

    jsonRaidDungeon raid_dungeon { get; set; }

    jsonConstants constants { get; set; }

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
        Header.Instance.SetTitle(Utility.getText("TEXT_RAIDRANKING"));

        //safearea対応
        setSafearea("RaidRankingCanvas");

        BG.sprite = Utility.getAssetImage("Image/BG/bg2");

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        raid_dungeon_id = Param.raid_dungeon_id;

        // まずは現在のリストを空に。
        ListClear();


        //APIをたたく
        APIConnectManager.Instance.RaidRanking(raid_dungeon_id, onStart);

        DispatchEvent(CwEvent.SCENE_READY);
    }

    /// <summary>
    /// スタート時呼び出し
    /// </summary>
    /// <param name="json">json</param>
    void onStart(string json)
    {
        response = JsonUtility.FromJson<jsonRaidRanking>(json);

        raid_dungeon = response.raid_dungeon;

        if (raid_dungeon.status == constants.Raid_Dungeon.NONE || raid_dungeon.status == constants.Raid_Dungeon.READY)
        {
            NaviText.text = Utility.getText("RAIDDUNGEON_TEXT_READY");

            // その旨のパネルを表示。
            ListNone.SetActive(true);
            Content.SetActive(false);

            // 処理はここまで。
            return;
        }

        jsonRaidRankingList[] list = response.rank_list;

        if (raid_dungeon.status == constants.Raid_Dungeon.NONE || raid_dungeon.status == constants.Raid_Dungeon.READY)
        {
            NaviText.text = Utility.getText("RAIDDUNGEON_TEXT_READY_2").Replace("{0}", response.raid_dungeon.title);
        }
        else if (raid_dungeon.status == constants.Raid_Dungeon.START)
        {
            NaviText.text = Utility.getText("RAIDDUNGEON_TEXT_START").Replace("{0}", response.raid_dungeon.title);

        }
        else if (raid_dungeon.status == constants.Raid_Dungeon.SUCCESS)
        {
            NaviText.text = Utility.getText("RAIDDUNGEON_TEXT_SUCCESS").Replace("{0}", response.raid_dungeon.title);

        }
        else if (raid_dungeon.status == constants.Raid_Dungeon.FAILURE)
        {
            NaviText.text = Utility.getText("RAIDDUNGEON_TEXT_FAILURE").Replace("{0}", response.raid_dungeon.title);

        }

        // GETアイテムが一つもなかったら...
        if (response.rank_list.Length == 0)
        {

            // その旨のパネルを表示。
            ListNone.SetActive(true);
            Content.SetActive(false);

            // 処理はここまで。
            return;
        }

        int i = 0;
        int cur_rank = 0;
        int cur_point = 0;
        foreach (jsonRaidRankingList entry in list)
        {
            GameObject board = null;

            // リストを複製
            board = UnityEngine.Object.Instantiate(ListMember, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
            board.name = "ListMember" + i;

            setupEntryBoard(entry, board);

            board.SetActive(true);
            i++;

            if (entry.user_id == Header.Instance.GetSummary().chara.user_id)
            {
                cur_rank = entry.rank;
                cur_point = entry.total_point;
            }
        }

        if (cur_rank > 0)
        {
            NaviText.text = NaviText.text + "\n" + Utility.getText("RAIDDUNGEON_NAV_TEXT_RANKIN").Replace("{0}", cur_point.ToString()).Replace("{1}", cur_rank.ToString());
        }
        else if (raid_dungeon.status == constants.Raid_Dungeon.START)
        {
            NaviText.text = NaviText.text + Utility.getText("RAIDDUNGEON_NAV_TEXT_NORANK");
        }


    }


    //---------------------------------------------------------------------------------------------------------
    /**
     * 引数に指定されたエントリを、指定されたボードに表示するときに呼ばれる。
     */
    void setupEntryBoard(jsonRaidRankingList entry, GameObject board)
    {

        //ランク順位
        board.transform.Find("rank").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_RANK").Replace("{0}", entry.rank.ToString());
        board.transform.Find("ImageKing").gameObject.SetActive(false);

        if (entry.rank == 1)
        {
            board.transform.Find("ImageKing").gameObject.SetActive(true);
        }

        //ポイント
        board.transform.Find("point").GetComponent<TextMeshProUGUI>().text = entry.total_point.ToString();

        //ユーザー名
        board.transform.Find("player_name").GetComponent<TextMeshProUGUI>().text = entry.avatar.player_name;

        //キャラ作成
        Image CharaImage = board.transform.Find("Avatar/Avatar/CharaImage").GetComponent<Image>();
        Main.Instance.makeCharaUI(entry.avatar.equip_info, CharaImage);

        board.transform.Find("level").GetComponent<TextMeshProUGUI>().text = entry.avatar.level.ToString();
        board.transform.Find("grade").GetComponent<TextMeshProUGUI>().text = entry.avatar.grade.grade_name;


        Button ButtonBattle = board.transform.Find("ButtonBattle").GetComponent<Button>();
        ButtonBattle.onClick.RemoveAllListeners();

        if (Header.Instance.GetSummary().chara.user_id != entry.user_id)
        {

            //対戦ボタンクリック時イベントハンドラ
            ButtonBattle.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySE("se_btn");

                SceneController.Instance.Jump("HisPage", (() =>
                {
                    HisPageBehaviour _scene = FindObjectOfType<HisPageBehaviour>() as HisPageBehaviour;
                    _scene.Param = new HisPageBehaviour.Parameter
                    {
                        userId = entry.user_id,
                    };
                }));
            });
        }
        else
        {
            ButtonBattle.gameObject.SetActive(false);
        }

    }


    void ListClear()
    {

        //テンプレート非表示
        ListNone.gameObject.SetActive(false);
        ListMember.gameObject.SetActive(false);
        Content.SetActive(true);

        foreach (Transform n in Content.transform)
        {
            //テンプレート以外は全部削除
            if (n.name != ListMember.name && n.name != ListNone.name)
                GameObject.Destroy(n.gameObject);
        }
    }

    public void gotoRaidDungeon()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.Jump("RaidInfo");
    }
}
