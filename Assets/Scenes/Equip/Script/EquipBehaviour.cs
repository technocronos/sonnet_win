using MyScene;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EnhancedUI.EnhancedScroller;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EquipBehaviour : BaseBehaviour, IEnhancedScrollerDelegate
{

    public GameObject ListNone;

    public ItemUseBehaviour ItemUse;
    public ItemPickerBehaviour ItemPicker;
    public ItemSyncBehaviour ItemSync;
    public ItemEvolBehaviour ItemEvol;

    public ItemSyncResultBehaviour ItemSyncResult;
    public ItemEvolResultBehaviour ItemEvolResult;

    public Image BG;

    public TextMeshProUGUI TextNavi;

    int mount = 1;
    jsonEquipList equip_list;
    jsonEquip player_equip;
    string category;

    jsonConstants constants;

    [SerializeField]
    private EnhancedScroller _scroller;

    private EquipListCellView _cellViewItem;
    private EquipListCellView _cellViewEquip;

    List<jsonEquip> itemlist = new List<jsonEquip>();

    public static EquipBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static EquipBehaviour instance;

    protected override void Start()
    {
        base.Start();
        //BG.sprite = Utility.getAssetImage("Image/BG/bg2");
        instance = this;

        Debug.Log("EquipBehaviour start..");
        setSafearea("EquipCanvas");

        Header.Instance.SetTitle(Utility.getText("TITLE_EQUIP"));

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        ItemUse.gameObject.SetActive(false);
        ItemPicker.gameObject.SetActive(false);
        ItemSync.gameObject.SetActive(false);
        ItemSyncResult.gameObject.SetActive(false);
        ItemEvol.gameObject.SetActive(false);
        ItemEvolResult.gameObject.SetActive(false);

        category = "WPN";

        Addressables.LoadAssetAsync<GameObject>("ListEquip.prefab").Completed += handle => {

            if (handle.Result == null)
            {
                Debug.Log("Load Error");
                return;
            }

            GameObject obj = handle.WaitForCompletion();

            _cellViewEquip = obj.GetComponent<EquipListCellView>();

            Addressables.Release(handle);

            Addressables.LoadAssetAsync<GameObject>("ListItem.prefab").Completed += handle => {

                if (handle.Result == null)
                {
                    Debug.Log("Load Error");
                    return;
                }

                GameObject obj = handle.WaitForCompletion();

                _cellViewItem = obj.GetComponent<EquipListCellView>();

                Addressables.Release(handle);

                //APIをたたく
                APIConnectManager.Instance.EquipList(onStart);
            };
        };

        DispatchEvent(CwEvent.SCENE_READY);
    }

    jsonEquipList makeJson(string json)
    {
        jsonEquipList response = JsonUtility.FromJson<jsonEquipList>(json);

        Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        foreach (KeyValuePair<string, object> keyvalue in jsonDict)
        {
            if (keyvalue.Key == "PLAEQP")
            {
                try
                {
                    response.PLAEQP = new Dictionary<int, jsonEquip>();
                    Dictionary<int, jsonEquip> jsonDict2 = JsonConvert.DeserializeObject<Dictionary<int, jsonEquip>>(keyvalue.Value.ToString());

                    foreach (KeyValuePair<int, jsonEquip> keyvalue2 in jsonDict2)
                    {
                        if (keyvalue2.Value != null)
                        {
                            response.PLAEQP.Add(keyvalue2.Key, keyvalue2.Value);
                        }
                    }

                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    for (int i = 1; i <= 4; i++)
                        response.PLAEQP.Add(i, null);
                }
            }
            else if (keyvalue.Key == "equip")
            {
                try
                {
                    response.equip = new Dictionary<int, jsonEquip[]>();
                    Dictionary<int, object> jsonDict2 = JsonConvert.DeserializeObject<Dictionary<int, object>>(keyvalue.Value.ToString());

                    foreach (KeyValuePair<int, object> keyvalue2 in jsonDict2)
                    {
                        if (keyvalue2.Value != null)
                        {

                            jsonEquip[] jsonDict3 = JsonConvert.DeserializeObject<jsonEquip[]>(keyvalue2.Value.ToString());

                            response.equip.Add(keyvalue2.Key, jsonDict3);

                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    response.equip = null;
                }
            }
        }
        return response;
    }

    public void onStart(string json)
    {
        equip_list = makeJson(json);

        reload();
    }

    public void reload()
    {

        //リストクリア
        listClear();

        string mount_str = "";
        switch (category)
        {
            case "HED":
                mount = 3;
                break;
            case "BOD":
                mount = 2;
                break;
            case "WPN":
                mount = 1;
                break;
            case "ACS":
                mount = 4;
                break;
            case "ITM":
                mount = 5;
                break;
        }

        mount_str = Utility.getText("mount_master_mount_name_PLA_" + mount);

        if (category != "ITM")
        {
            //装備中の装備をとっておく
            foreach (KeyValuePair<int, jsonEquip> keyValue in equip_list.PLAEQP)
            {
                if (keyValue.Key == mount)
                    player_equip = keyValue.Value;
            }
        }

        
        foreach (KeyValuePair<int, jsonEquip[]> keyValue in equip_list.equip)
        {
            if (keyValue.Key == mount)
            {
                //装備してるものがある場合
                if (player_equip != null && player_equip.user_item_id != 0)
                {
                    itemlist.Add(player_equip);
                    foreach (jsonEquip keyValue2 in keyValue.Value)
                    {
                        if (player_equip.user_item_id != keyValue2.user_item_id)
                            itemlist.Add(keyValue2);
                    }
                }
                else
                {
                    foreach (jsonEquip keyValue2 in keyValue.Value)
                    {
                        itemlist.Add(keyValue2);
                    }
                }
            }
        }

        // 一つもなかったら...
        if (itemlist.Count == 0)
        {
            // その旨のパネルを表示。
            ListNone.gameObject.SetActive(true);

            // 処理はここまで。
            return;
        }

        if (player_equip == null || (player_equip.user_item_id == 0 && category != "ITM"))
        {
            TextNavi.text = Utility.getText("TEXT_NAVI_EQUIP_1").Replace("{0}", mount_str);
        }
        else
        {
            TextNavi.text = Utility.getText("TEXT_NAVI_EQUIP_0");
        }

        // Scrollerにデリゲート登録
        _scroller.Delegate = this;

        // セルがインスタンス化されたときの処理
        _scroller.cellViewInstantiated += (scroller, view) =>
        {
            var cellView = (EquipListCellView)view;

            cellView.onClickButtonUse = x => doUse(itemlist[x.dataIndex]);
            cellView.onClickButtonEquip = x => doEquip(itemlist[x.dataIndex]);
            cellView.onClickButtonSync = x => doSync(itemlist[x.dataIndex]);
            cellView.onClickButtonEvol = x => doEvol(itemlist[x.dataIndex]);
            cellView.onClickButtonDust = x => doDust(itemlist[x.dataIndex]);

        };

        _scroller.padding.left = 17;
        _scroller.padding.top = 10;
        _scroller.spacing = 8;

        // ReloadDataをするとビューが更新される
        _scroller.ReloadData();
    }

    /// <summary>
    /// アイテムを使用する
    /// </summary>
    /// <param name="entry"></param>
    void doUse(jsonEquip entry)
    {
        AudioManager.Instance.PlaySE("se_btn");

        if (entry.item_type == constants.Item_Master.ITEM_REPAIRE)
        {
            //修理アイテムは対象装備を選択する。
            ItemPicker.gameObject.SetActive(true);
            ItemPicker.Show(entry);
        }
        else
        {
            ItemUse.gameObject.SetActive(true);
            ItemUse.Show(entry);
        }
    }

    /// <summary>
    /// 装備をする
    /// </summary>
    /// <param name="entry"></param>
    void doEquip(jsonEquip entry)
    {
        AudioManager.Instance.PlaySE("se_btn");

        var text = Utility.getText("TEXT_NAVI_EQUIP_CONFIRM").Replace("{0}", entry.item_name);

        Main.Instance.showConfirm(text, () =>
        {
            AudioManager.Instance.PlaySE("se_btn");

            int charaId = Header.Instance.GetSummary().chara.character_id;
            string func = null;

            //APIをたたく
            APIConnectManager.Instance.EquipChange(charaId, func, entry.user_item_id, 0, mount, 0, 0, (string json) =>
            {
                jsonEquipChange res = JsonUtility.FromJson<jsonEquipChange>(json);

                if (res.result == "ok")
                {
                    //APIをたたく
                    APIConnectManager.Instance.EquipList(onStart);
                }
                else
                {
                    switch (res.err_code)
                    {
                        case "noitem":
                        case "not_me":
                            Main.Instance.showDialogue(Utility.getText("API_ERROR_EquipChange_" + res.err_code), null, 4);
                            break;
                        case "equipping":
                        case "maxlevel":
                        case "nomoney":
                        case "in_quest":
                            Main.Instance.showDialogue(Utility.getText("API_ERROR_EquipChange_" + res.err_code));
                            break;
                    }
                }
            });
        });
    }

    /// <summary>
    /// 装備の合成をする
    /// </summary>
    /// <param name="entry"></param>
    void doSync(jsonEquip entry)
    {
        AudioManager.Instance.PlaySE("se_btn");

        ItemSync.gameObject.SetActive(true);
        ItemSync.Show(player_equip, entry);
    }

    void doEvol(jsonEquip entry)
    {
        AudioManager.Instance.PlaySE("se_btn");

        ItemEvol.gameObject.SetActive(true);
        ItemEvol.Show(player_equip, entry);
    }


    /// <summary>
    /// アイテムを捨てる
    /// </summary>
    /// <param name="entry"></param>
    void doDust(jsonEquip entry)
    {
        AudioManager.Instance.PlaySE("se_btn");

        var text = Utility.getText("TEXT_NAVI_EQUIP_DISCARD_CONFIRM").Replace("{0}", entry.item_name);

        if (entry.num > 1)
        {
            text += "\n" + Utility.getText("TEXT_NAVI_EQUIP_DISCARD_CONFIRM_NOTICE").Replace("{0}", entry.num.ToString());
        }

        Main.Instance.showConfirm(text, () =>
        {
            AudioManager.Instance.PlaySE("se_btn");

            //APIをたたく
            APIConnectManager.Instance.Discard(entry.user_item_id, (string json) =>
            {
                jsonDiscard res = JsonUtility.FromJson<jsonDiscard>(json);

                if (res.result == "ok")
                {
                    text = Utility.getText("TEXT_NAVI_EQUIP_DISCARD_RESULT");

                    Main.Instance.showDialogue(text, () =>
                    {
                        AudioManager.Instance.PlaySE("se_btn");
                        //APIをたたく
                        APIConnectManager.Instance.EquipList(onStart);
                    });

                }
                else
                {
                    switch (res.err_code)
                    {
                        case "noitem":
                        case "not_me":
                            Main.Instance.showDialogue(Utility.getText("API_ERROR_Discard_" + res.err_code), null, 4);
                            break;
                        case "equipping":
                        case "few_num":
                        case "forbidden":
                            Main.Instance.showDialogue(Utility.getText("API_ERROR_Discard_" + res.err_code));
                            break;
                    }
                }
            });
        });
    }

    /// <summary>
    /// 装備・アイテム切り替えイベントハンドラ
    /// </summary>
    public void onChangeCategory(string category)
    {

        if (this.category == category)
            return;

        AudioManager.Instance.PlaySE("se_btn");

        this.category = category;

        reload();
    }

    public void onSyncHelp()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.PopUp("HelpDetail", () =>
        {
            HelpDetailBehaviour helpdetail = FindObjectOfType<HelpDetailBehaviour>() as HelpDetailBehaviour;
            helpdetail.Param = new HelpDetailBehaviour.Parameter { id = "item-sync" };
        });
    }

    /// <summary>
    /// リストを全部消す
    /// </summary>
    void listClear()
    {
        //テンプレート非表示
        ListNone.gameObject.SetActive(false);

        itemlist = new List<jsonEquip>();
        player_equip = new jsonEquip();

        _scroller.ClearAll();
    }

    int IEnhancedScrollerDelegate.GetNumberOfCells(EnhancedScroller scroller)
    {
        return itemlist.Count;
    }

    float IEnhancedScrollerDelegate.GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return 295;
    }

    // セルのViewを返す
    EnhancedScrollerCellView IEnhancedScrollerDelegate.GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        // Scroller.GetCellView()を呼ぶと新規生成orリサイクルを自動的に行ったViewを返してくれる
        if (category == "ITM") {
            EquipListCellView cellView = scroller.GetCellView(_cellViewItem) as EquipListCellView;

            // set the name of the game object to the cell's data index.
            // this is optional, but it helps up debug the objects in 
            // the scene hierarchy.
            cellView.name = "Item " + dataIndex.ToString();

            // in this example, we just pass the data to our cell's view which will update its UI
            cellView.SetData(itemlist[dataIndex], player_equip);

            // return the cell to the scroller
            return cellView;
        }
        else {
            EquipListCellView cellView = scroller.GetCellView(_cellViewEquip) as EquipListCellView;

            // set the name of the game object to the cell's data index.
            // this is optional, but it helps up debug the objects in 
            // the scene hierarchy.
            cellView.name = "Equip " + dataIndex.ToString();

            // in this example, we just pass the data to our cell's view which will update its UI
            cellView.SetData(itemlist[dataIndex], player_equip);

            // return the cell to the scroller
            return cellView;
        }
    }
}
