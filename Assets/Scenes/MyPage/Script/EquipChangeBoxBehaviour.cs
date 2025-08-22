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


public class EquipChangeBoxBehaviour : MonoBehaviour, IEnhancedScrollerDelegate
{

    public GameObject ListNone;
    public GameObject Content;

    public GameObject Tab_WPN;
    public GameObject Tab_BOD;
    public GameObject Tab_HED;
    public GameObject Tab_ACS;

    [SerializeField]
    private EnhancedScroller _scroller;

    private EquipListCellView _cellViewEquip;

    List<jsonEquip> itemlist = new List<jsonEquip>();

    int mount = 1;
    jsonEquipList equip_list;
    jsonEquip player_equip;
    string category;

    jsonConstants constants;

    public void Show(string _category)
    {
        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        category = _category;

        Tab_HED.SetActive(false);
        Tab_BOD.SetActive(false);
        Tab_WPN.SetActive(false);
        Tab_ACS.SetActive(false);

        Addressables.LoadAssetAsync<GameObject>("ListEquip.prefab").Completed += handle => {

            if (handle.Result == null)
            {
                Debug.Log("Load Error");
                return;
            }

            GameObject obj = handle.WaitForCompletion();

            _cellViewEquip = obj.GetComponent<EquipListCellView>();

            Addressables.Release(handle);

            //APIをたたく
            APIConnectManager.Instance.EquipList(onStart);
        };

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
    void onStart(string json)
    {
        equip_list = makeJson(json);

        reload();
    }

    public void reload()
    {
        //リストクリア
        this.listClear();

        player_equip = null;

        switch (category)
        {
            case "HED":
                mount = 3;
                Tab_HED.SetActive(true);
                break;
            case "BOD":
                mount = 2;
                Tab_BOD.SetActive(true);
                break;
            case "WPN":
                mount = 1;
                Tab_WPN.SetActive(true);
                break;
            case "ACS":
                mount = 4;
                Tab_ACS.SetActive(true);
                break;
        }

        //装備中の装備をとっておく
        foreach (KeyValuePair<int, jsonEquip> keyValue in equip_list.PLAEQP)
        {
            if (keyValue.Key == mount)
                player_equip = keyValue.Value;
        }

        foreach (KeyValuePair<int, jsonEquip[]> keyValue in equip_list.equip)
        {
            if (keyValue.Key == mount)
            {
                //装備してるものがある場合
                if (player_equip != null)
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

        // Scrollerにデリゲート登録
        _scroller.Delegate = this;

        // セルがインスタンス化されたときの処理
        _scroller.cellViewInstantiated += (scroller, view) =>
        {
            var cellView = (EquipListCellView)view;

            cellView.onClickButtonEquip = x => doEquip(itemlist[x.dataIndex]);

        };

        _scroller.padding.left = 37;
        _scroller.padding.right = 52;
        _scroller.padding.top = 10;
        _scroller.spacing = 8;

        // ReloadDataをするとビューが更新される
        _scroller.ReloadData();

    }

    void listClear()
    {
        //テンプレート非表示
        ListNone.gameObject.SetActive(false);

        itemlist = new List<jsonEquip>();
        player_equip = new jsonEquip();

        _scroller.ClearAll();
    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * 引数に指定されたエントリを、指定されたボードに表示するときに呼ばれる。
     */
    void setupEntryBoard(jsonEquip entry, GameObject board)
    {
        //名前
        if (entry.evolution == 1)
            board.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = entry.item_name + "<color=\"red\" >["+Utility.getText("TEXT_EQUIP_EVOLUTION") +"]</color>";
        else
            board.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = entry.item_name;

        //アイテムアイコン
        Sprite itemIcon = Utility.getAssetImage(Utility.getItemIconURL(entry.item_id));
        // ファイルが存在するものだけ
        if (itemIcon != null)
        {
            //画像を差し替えていく
            board.transform.Find("ItemFlame/ItemIcon").GetComponent<Image>().sprite = itemIcon;
        }

        board.transform.Find("TextFlavor").GetComponent<TextMeshProUGUI>().text = entry.flavor_text;


        board.transform.Find("TextSet").GetComponent<TextMeshProUGUI>().text = entry.set_name;

        //レアアイコン
        Sprite RareIcon = Utility.getAssetImage("Image/RareIcon/rare_icon_" + entry.rear_level);
        if (RareIcon != null)
        {
            board.transform.Find("RareIcon").GetComponent<Image>().sprite = RareIcon;
        }

        //level
        string level = entry.level.ToString();
        if (entry.level == entry.max_level)
            level += "[MAX]";

        board.transform.Find("TextLv").GetComponent<TextMeshProUGUI>().text = level;

        if (entry.durable_count != constants.Item_Master.INFINITE_DURABILITY)
            board.transform.Find("TextDurable").GetComponent<TextMeshProUGUI>().text = entry.durable_count.ToString();
        else
            board.transform.Find("TextDurable").GetComponent<TextMeshProUGUI>().text = "∞";


        //進化している場合
        if (entry.evolution == 1)
        {
            Color color;
            if (ColorUtility.TryParseHtmlString("#56756d", out color))// outキーワードで参照渡しにする
            {
                // Color型への変換成功（colorにColor型の赤色が代入される）
                board.transform.Find("LvImage").GetComponent<Image>().color = color;
            }
        }

        board.transform.Find("StatusPanel/att1").GetComponent<TextMeshProUGUI>().text = entry.attack1.ToString();
        board.transform.Find("StatusPanel/att2").GetComponent<TextMeshProUGUI>().text = entry.attack2.ToString();
        board.transform.Find("StatusPanel/att3").GetComponent<TextMeshProUGUI>().text = entry.attack3.ToString();
        board.transform.Find("StatusPanel/spd").GetComponent<TextMeshProUGUI>().text = entry.speed.ToString();

        board.transform.Find("StatusPanel/def1").GetComponent<TextMeshProUGUI>().text = entry.defence1.ToString();
        board.transform.Find("StatusPanel/def2").GetComponent<TextMeshProUGUI>().text = entry.defence2.ToString();
        board.transform.Find("StatusPanel/def3").GetComponent<TextMeshProUGUI>().text = entry.defence3.ToString();
        board.transform.Find("StatusPanel/defX").GetComponent<TextMeshProUGUI>().text = entry.defenceX.ToString();


        //装備ボタン押下時イベントハンドラ
        Button ButtonEquip = board.transform.Find("ButtonEquip").GetComponent<Button>();
        if (ButtonEquip != null)
        {
            ButtonEquip.onClick.RemoveAllListeners();
            ButtonEquip.onClick.AddListener(() =>
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
                        transform.gameObject.SetActive(false);
                        MyPageBehaviour.Instance.changeReload();
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
            });

            //---------------------------------------------------------------------------------------------------------
            /*
             * ボタン表示/非表示切り替え
             *
            */
            if (player_equip != null)
            {
                //装備中の場合
                if (player_equip.user_item_id == entry.user_item_id)
                {
                    //捨てるボタン非活性＋イベント削除
                    //if (category == "ITM")
                    //    AppUtil.disableButton($("[key='dust_button']", board), "150_66");
                    //else
                    //    AppUtil.disableButton($("[key='dust_button']", board), "174_66");

                    //$("[key='dust_button']", board).off('click');

                    //装備ボタンダーク化＋イベント削除
                    ButtonEquip.interactable = false;
                    ButtonEquip.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_EQUIP_IN_EQUIP");
                    ButtonEquip.onClick.RemoveAllListeners();

                    //装備合成ボタン非活性＋イベント削除
                    //AppUtil.disableButton($("[key='sync_button']", board), "174_66");
                    //$("[key='sync_button']", board).off('click');
                }
            }
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
                    transform.gameObject.SetActive(false);
                    MyPageBehaviour.Instance.changeReload();
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

    public void onClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        listClear();

        transform.gameObject.SetActive(false);
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
            EquipListCellView cellView = scroller.GetCellView(_cellViewEquip) as EquipListCellView;

            // set the name of the game object to the cell's data index.
            // this is optional, but it helps up debug the objects in 
            // the scene hierarchy.
            cellView.name = "Equip " + dataIndex.ToString();

            // in this example, we just pass the data to our cell's view which will update its UI
            cellView.SetData(itemlist[dataIndex], player_equip, true);

            // return the cell to the scroller
            return cellView;

    }
}
