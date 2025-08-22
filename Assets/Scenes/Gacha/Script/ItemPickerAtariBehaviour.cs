using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPickerAtariBehaviour : MonoBehaviour
{
    public GameObject ListEquip;
    public GameObject ListNone;
    public GameObject Content;

    jsonConstants constants;
    jsonItems item_picker_entry;

    public static ItemPickerAtariBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static ItemPickerAtariBehaviour instance;

    string from = "";

    // 
    // 終了フェーズの制御を行う。
    private void Start()
    {
        instance = this;
    }

    public void Show(jsonItems[] _entry, string _from)
    {
        //定数取得
        constants = APIConnectManager.Instance.login.constants;
        from = _from;

        foreach (jsonItems item in GachaResultBehaviour.instance.gacha_result.getitem)
        {
            if (GachaResultBehaviour.instance.gacha_result.guaranteed_item_id == item.item_id)
            {
                item_picker_entry = item;
                break;
            }
        }

        reload(_entry);
    }

    public void reload(jsonItems[] list)
    {

        //リストクリア
        listClear();

        // 一つもなかったら...
        if (list.Length == 0)
        {
            // その旨のパネルを表示。
            ListNone.gameObject.SetActive(true);

            // 処理はここまで。
            return;
        }

        int i = 0;
        foreach (jsonItems entry in list)
        {
            if (entry.item_id == GachaResultBehaviour.instance.gacha_result.guaranteed_item_id)
                continue;

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
    void setupEntryBoard(jsonItems entry, GameObject board)
    {

        if (entry.evolution == 1)
            board.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = entry.item_name + "<color=\"red\" >[進化]</color>";
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

                var text = Utility.getText("TEXT_NAVI_EQUIP_EXCHANGE_CONFIRM_2").Replace("{0}", item_picker_entry.item_name).Replace("{1}", entry.item_name);

                Main.Instance.showConfirm(text, () =>
                {
                    AudioManager.Instance.PlaySE("se_btn");

                    //アイテムを使う
                    APIConnectManager.Instance.ItemExchange(item_picker_entry.user_item_id, entry.item_id, (string json) =>
                    {
                        //API結果受け取り
                        jsonExchange response = JsonUtility.FromJson<jsonExchange>(json);

                        var text = "";
                        if (response.result == "ok")
                        {
                            int i = 0;
                            foreach (jsonItems item in GachaResultBehaviour.instance.gacha_result.getitem)
                            {
                                if (item.user_item_id == item_picker_entry.user_item_id)
                                {
                                    //アイテム入れ替え
                                    GachaResultBehaviour.instance.gacha_result.getitem[i] = response.exchange;
                                    //当たりアイテムID入れ替え
                                    GachaResultBehaviour.instance.gacha_result.guaranteed_item_id = GachaResultBehaviour.instance.gacha_result.getitem[i].item_id;

                                    //再表示
                                    if (from != "all")
                                        GachaResultBehaviour.instance.showItem(GachaResultBehaviour.instance.gacha_result.getitem[i]);
                                    else
                                        GachaResultBehaviour.instance.PlayAll();

                                    break;
                                }
                                i++;
                            }

                            transform.gameObject.SetActive(false);
                        }
                        else
                        {
                            text = response.err_code;

                            Main.Instance.showDialogue(text);
                        }
                    });

                });
            });
        }
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
