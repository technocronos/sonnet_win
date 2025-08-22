using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUseBehaviour : MonoBehaviour
{

    public Image ItemIcon;

    public TextMeshProUGUI TextItemNameUse;
    public TextMeshProUGUI TextFlavorUse;
    public TextMeshProUGUI TextEffectUse;
    public TextMeshProUGUI TextNaviUse;

    int mount = 1;
    jsonEquip entry;

    jsonConstants constants;

    public void Show(jsonEquip _entry)
    {
        entry = _entry;

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        switch (entry.category)
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

        reload();
    }

    void reload()
    {
        //アイテム名
        TextItemNameUse.text = entry.item_name;

        //アイテムアイコン
        Sprite itemIcon = Utility.getAssetImage(Utility.getItemIconURL(entry.item_id));
        // ファイルが存在するものだけ
        if (itemIcon != null)
        {
            //画像を差し替えていく
            ItemIcon.sprite = itemIcon;
        }

        //フレーバーテキスト
        TextFlavorUse.text = entry.flavor_text;
        TextEffectUse.text = entry.effect;

        //ナビセリフ
        TextNaviUse.text = Utility.getText("TEXT_NAVI_EQUIP_USE_CONFIRM").Replace("{0}", entry.item_name);

    }

    public void onUse()
    {
        AudioManager.Instance.PlaySE("se_btn");

        //アイテムを使う
        APIConnectManager.Instance.ItemUseFire(entry.user_item_id, 0, (string json) =>
        {
            //API結果受け取り
            ItemUseFire response = JsonUtility.FromJson<ItemUseFire>(json);

            var text = "";
            if (response.result == "ok")
            {
                text = Utility.getText("TEXT_NAVI_EQUIP_ITEM_USE_RESULT").Replace("{0}", entry.item_name);

                if (entry.item_type == constants.Item_Master.ITEM_RECV_HP || entry.item_type == constants.Item_Master.ITEM_RECV_AP || entry.item_type == constants.Item_Master.ITEM_RECV_MP)
                {
                    //homeAPIをたたく
                    APIConnectManager.Instance.Home((string json) =>
                    {
                        //API結果受け取り
                        HomeApi homeSummary = JsonUtility.FromJson<HomeApi>(json);

                        Header.Instance.SetSummary(homeSummary);
                        Footer.Instance.SetSummary(homeSummary);
                    });
                }
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
    }

    public void onClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        transform.gameObject.SetActive(false);
    }
}
