using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemEvolBehaviour : MonoBehaviour
{
    public Image ItemIcon;

    public TextMeshProUGUI TextItemName;
    public TextMeshProUGUI TextNavi;
    public TextMeshProUGUI TextMagna;
    public TextMeshProUGUI TextNeedMagna;

    int mount = 1;
    jsonEquip entry;
    jsonEquip base_equip;


    public ItemEvolResultBehaviour ItemEvolResult;

    jsonConstants constants;

    public void Show(jsonEquip _player_equip, jsonEquip _entry)
    {
        entry = _entry;
        base_equip = _player_equip;

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

        //APIをたたく
        APIConnectManager.Instance.SyncGetPrice(base_equip.user_item_id, entry.user_item_id, true, (string json) =>
        {
            jsonEquipChange res = JsonUtility.FromJson<jsonEquipChange>(json);

            //アイテム名
            TextItemName.text = entry.item_name;

            //アイテムアイコン
            Sprite itemIcon = Utility.getAssetImage(Utility.getItemIconURL(entry.item_id));
            // ファイルが存在するものだけ
            if (itemIcon != null)
            {
                //画像を差し替えていく
                ItemIcon.sprite = itemIcon;
            }

            //必要マグナ
            TextNeedMagna.text = res.price.ToString();
            //所持マグナ
            TextMagna.text = Header.Instance.GetSummary().gold.ToString();
            //ナビセリフ
            TextNavi.text = Utility.getText("TEXT_NAVI_EQUIP_EVOLUTION_CONFIRM").Replace("{0}", entry.item_name);


        });

    }

    public void onEvol()
    {
        AudioManager.Instance.PlaySE("se_btn");

        int charaId = Header.Instance.GetSummary().chara.character_id;

        //合成を行う
        APIConnectManager.Instance.EquipEvol(charaId, entry.user_item_id, mount, (string json) =>
        {
            //API結果受け取り
            jsonSuncResult response = JsonUtility.FromJson<jsonSuncResult>(json);

            if (response.result == "ok")
            {

                transform.gameObject.SetActive(false);

                ItemEvolResult.gameObject.SetActive(true);
                ItemEvolResult.Show(base_equip, entry, response);

            }
            else
            {
                switch (response.err_code)
                {
                    case "noitem":
                    case "notsame":
                    case "not_me":
                        Main.Instance.showDialogue(Utility.getText("API_ERROR_EquipEvol_" + response.err_code), null, 4);
                        break;
                    case "equipping":
                    case "notmaxlevel":
                    case "maxlevel":
                    case "nomoney":
                    case "in_quest":
                    case "noevol":
                        Main.Instance.showDialogue(Utility.getText("API_ERROR_EquipEvol_" + response.err_code));
                        break;
                }


                transform.gameObject.SetActive(false);
            }
        });

    }

    public void onClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        transform.gameObject.SetActive(false);
    }
}
