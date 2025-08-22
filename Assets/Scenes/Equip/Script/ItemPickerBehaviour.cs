using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPickerBehaviour : MonoBehaviour
{
    public GameObject ListEquip;
    public GameObject ListNone;
    public GameObject Content;

    int mount;
    jsonEquipList equip_list;
    jsonEquip player_equip;
    string category;

    jsonConstants constants;
    jsonEquip item_picker_entry;

    public static ItemPickerBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static ItemPickerBehaviour instance;

    public void Show(jsonEquip _entry)
    {
        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        item_picker_entry = _entry;

        category = "WPN";

        //APIをたたく
        APIConnectManager.Instance.EquipList(onStart);
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
        }

        //リストクリア
        listClear();

        player_equip = null;

        //装備中の装備をとっておく
        foreach (KeyValuePair<int, jsonEquip> keyValue in equip_list.PLAEQP)
        {
            if (keyValue.Key == mount)
                player_equip = keyValue.Value;
        }

        List<jsonEquip> list = new List<jsonEquip>();
        foreach (KeyValuePair<int, jsonEquip[]> keyValue in equip_list.equip)
        {
            if (keyValue.Key == mount)
            {
                //装備してるものがある場合
                if (player_equip != null)
                {
                    list.Add(player_equip);
                    foreach (jsonEquip keyValue2 in keyValue.Value)
                    {
                        if (player_equip.user_item_id != keyValue2.user_item_id)
                            list.Add(keyValue2);
                    }
                }
                else
                {
                    foreach (jsonEquip keyValue2 in keyValue.Value)
                    {
                        list.Add(keyValue2);
                    }
                }
            }
        }

        // 一つもなかったら...
        if (list.Count == 0)
        {
            // その旨のパネルを表示。
            ListNone.gameObject.SetActive(true);

            // 処理はここまで。
            return;
        }

        int i = 0;
        foreach (jsonEquip entry in list)
        {
            GameObject board = null;

            // リストを複製
            board = UnityEngine.Object.Instantiate(ListEquip, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);

            board.name = "ListEquip" + i;

            setupEntryBoard(entry, board);

            board.SetActive(true);
            i++;
        }

    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * 引数に指定されたエントリを、指定されたボードに表示するときに呼ばれる。
     */
    void setupEntryBoard(jsonEquip entry, GameObject board)
    {

        if (entry.evolution == 1)
            board.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = entry.item_name + "<color=\"red\" >["+Utility.getText("TEXT_EVOL") +"]</color>";
        else
            //名前
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
        board.transform.Find("TextLv").GetComponent<TextMeshProUGUI>().text = entry.level.ToString();

        if (entry.durable_count != constants.Item_Master.INFINITE_DURABILITY)
            board.transform.Find("TextDurable").GetComponent<TextMeshProUGUI>().text = entry.durable_count.ToString();
        else
            board.transform.Find("TextDurable").GetComponent<TextMeshProUGUI>().text = "∞";


        board.transform.Find("StatusPanel/att1").GetComponent<TextMeshProUGUI>().text = entry.attack1.ToString();
        board.transform.Find("StatusPanel/att2").GetComponent<TextMeshProUGUI>().text = entry.attack2.ToString();
        board.transform.Find("StatusPanel/att3").GetComponent<TextMeshProUGUI>().text = entry.attack3.ToString();
        board.transform.Find("StatusPanel/spd").GetComponent<TextMeshProUGUI>().text = entry.speed.ToString();

        board.transform.Find("StatusPanel/def1").GetComponent<TextMeshProUGUI>().text = entry.defence1.ToString();
        board.transform.Find("StatusPanel/def2").GetComponent<TextMeshProUGUI>().text = entry.defence2.ToString();
        board.transform.Find("StatusPanel/def3").GetComponent<TextMeshProUGUI>().text = entry.defence3.ToString();
        board.transform.Find("StatusPanel/defX").GetComponent<TextMeshProUGUI>().text = entry.defenceX.ToString();


        //useボタン押下時イベントハンドラ
        Button ButtonUse = board.transform.Find("ButtonUse").GetComponent<Button>();
        if (ButtonUse != null)
        {
            ButtonUse.onClick.RemoveAllListeners();
            ButtonUse.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySE("se_btn");

                var text = Utility.getText("TEXT_NAVI_EQUIP_USE_CONFIRM_2").Replace("{0}", item_picker_entry.item_name).Replace("{1}", entry.item_name);

                Main.Instance.showConfirm(text, () =>
                {
                    AudioManager.Instance.PlaySE("se_btn");

                    //アイテムを使う
                    APIConnectManager.Instance.ItemUseFire(item_picker_entry.user_item_id, entry.user_item_id, (string json) =>
                    {
                        //API結果受け取り
                        ItemUseFire response = JsonUtility.FromJson<ItemUseFire>(json);

                        var text = "";
                        if (response.result == "ok")
                        {
                            text = Utility.getText("TEXT_NAVI_EQUIP_ITEM_USE_RESULT").Replace("{0}", item_picker_entry.item_name);
                        }
                        else
                        {
                            text = response.err_code;
                        }

                        Main.Instance.showDialogue(text, () =>
                        {
                            AudioManager.Instance.PlaySE("se_btn");
                            transform.gameObject.SetActive(false);

                            //APIをたたく
                            APIConnectManager.Instance.EquipList(EquipBehaviour.Instance.onStart);
                        });

                    });

                });
            });
        }
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

    /// <summary>
    /// リストを全部消す
    /// </summary>
    void listClear()
    {
        //テンプレート非表示
        ListEquip.gameObject.SetActive(false);
        ListNone.gameObject.SetActive(false);

        foreach (Transform n in Content.transform)
        {
            //テンプレート以外は全部削除
            if (n.name != ListEquip.name && n.name != ListNone.name)
                GameObject.Destroy(n.gameObject);
        }
    }

    public void onClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        transform.gameObject.SetActive(false);
    }
}
