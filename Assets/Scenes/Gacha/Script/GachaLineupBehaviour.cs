using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaLineupBehaviour : MonoBehaviour
{
    public GameObject ListEquip;
    public GameObject ListItem;
    public GameObject ListNone;
    public GameObject ListLoading;

    public GameObject Content;

    int gachaId;

    // Start is called before the first frame update
    public void Show(int _gachaId)
    {
        gachaId = _gachaId;

        ListClear();

        //APIをたたく
        APIConnectManager.Instance.GachaLineup(gachaId, reload);

    }


    void reload(string json)
    {
        jsonGachaLineup response = JsonUtility.FromJson<jsonGachaLineup>(json);

        // 一つもなかったら...
        if (response.list.Length == 0)
        {
            // その旨のパネルを表示。
            ListNone.SetActive(true);
            Content.SetActive(false);

            // 処理はここまで。
            return;
        }

        jsonGachaLineupList[] list = response.list;

        int i = 0;
        foreach (jsonGachaLineupList entry in list)
        {
            GameObject board = null;

            // リストを複製
            if (entry.item.category == "ITM")
            {
                board = UnityEngine.Object.Instantiate(ListItem, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
            }
            else
            {
                board = UnityEngine.Object.Instantiate(ListEquip, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
            }

            board.name = "ListLineUp" + i;

            setupEntryBoard(entry, board);

            board.SetActive(true);
            i++;
        }
    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * 引数に指定されたエントリを、指定されたボードに表示するときに呼ばれる。
     */
    void setupEntryBoard(jsonGachaLineupList entry, GameObject board)
    {
        board.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = entry.item.item_name;

        board.transform.Find("FlavorText").GetComponent<TextMeshProUGUI>().text = entry.item.flavor_text;

        board.transform.Find("rate").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_OFFER_RATE").Replace("{0}", entry.rate.ToString());

        //アイテムアイコン
        Sprite itemIcon = Utility.getAssetImage(Utility.getItemIconURL(entry.item.item_id));
        // ファイルが存在するものだけ
        if (itemIcon != null)
        {
            //画像を差し替えていく
            board.transform.Find("ItemIcon").GetComponent<Image>().sprite = itemIcon;
        }

        if (entry.item.category != "ITM")
        {
            string mount = "";

            switch (entry.item.category)
            {
                case "HED":
                    mount = Utility.getText("mount_master_mount_name_PLA_3");
                    break;
                case "BOD":
                    mount = Utility.getText("mount_master_mount_name_PLA_2");
                    break;
                case "WPN":
                    mount = Utility.getText("mount_master_mount_name_PLA_1");
                    break;
                case "ACS":
                    mount = Utility.getText("mount_master_mount_name_PLA_4");
                    break;
            }

            board.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = mount + "：" + entry.item.item_name;
            board.transform.Find("SetName").GetComponent<TextMeshProUGUI>().text = entry.item.set_name;
            board.transform.Find("Durability").GetComponent<TextMeshProUGUI>().text = entry.item.durability.ToString();

            board.transform.Find("StatusPanel/att1").GetComponent<TextMeshProUGUI>().text = entry.item.attack1.ToString();
            board.transform.Find("StatusPanel/att2").GetComponent<TextMeshProUGUI>().text = entry.item.attack2.ToString();
            board.transform.Find("StatusPanel/att3").GetComponent<TextMeshProUGUI>().text = entry.item.attack3.ToString();
            board.transform.Find("StatusPanel/spd").GetComponent<TextMeshProUGUI>().text = entry.item.speed.ToString();

            board.transform.Find("StatusPanel/def1").GetComponent<TextMeshProUGUI>().text = entry.item.defence1.ToString();
            board.transform.Find("StatusPanel/def2").GetComponent<TextMeshProUGUI>().text = entry.item.defence2.ToString();
            board.transform.Find("StatusPanel/def3").GetComponent<TextMeshProUGUI>().text = entry.item.defence3.ToString();
            board.transform.Find("StatusPanel/defX").GetComponent<TextMeshProUGUI>().text = entry.item.defenceX.ToString();

            //レアアイコン
            Sprite RareIcon = Utility.getAssetImage("Image/RareIcon/rare_icon_" + entry.item.rear_level);
            if (RareIcon != null)
            {
                board.transform.Find("RareIcon").GetComponent<Image>().sprite = RareIcon;
            }
        }


    }
    void ListClear()
    {

        //テンプレート非表示
        ListNone.gameObject.SetActive(false);
        ListEquip.gameObject.SetActive(false);
        ListItem.gameObject.SetActive(false);

        Content.SetActive(true);
        ListLoading.SetActive(false);

        foreach (Transform n in Content.transform)
        {
            //テンプレート以外は全部削除
            if (n.name != ListItem.name && n.name != ListEquip.name && n.name != ListNone.name && n.name != ListLoading.name)
                GameObject.Destroy(n.gameObject);
        }
    }

    public void onClose()
    {
        AudioManager.Instance.PlaySE("se_btn");


        ListClear();

        transform.gameObject.SetActive(false);
    }
}
