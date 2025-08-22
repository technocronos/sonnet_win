using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class ItemSyncResultBehaviour : MonoBehaviour
{
    public Image ItemIcon;

    public TextMeshProUGUI TextResult;
    public Image Rotator;

    jsonEquip sync_base_entry;
    jsonEquip sync_source_entry;
    jsonSuncResult sync_response;

    public void Show(jsonEquip _player_equip, jsonEquip _entry, jsonSuncResult response)
    {
        sync_base_entry = _player_equip;
        sync_source_entry = _entry;
        sync_response = response;

        Rotator.gameObject.SetActive(false);

        reload();
    }
    void reload()
    {
        AudioManager.Instance.PlaySE("se_congrats");

        //アイテムアイコン
        Sprite itemIcon = Utility.getAssetImage(Utility.getItemIconURL(sync_base_entry.item_id));
        // ファイルが存在するものだけ
        if (itemIcon != null)
        {
            //画像を差し替えていく
            ItemIcon.sprite = itemIcon;
        }

        var text = "";

        //レベルアップしてる場合
        if (sync_response.alv > sync_response.blv)
        {
            Rotator.gameObject.SetActive(true);

            //回転アニメ
            Rotator.transform.DOLocalRotate(new Vector3(0, 0, 360f), 30f, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart);

            if (sync_response.alv == sync_base_entry.max_level)
                text = Utility.getText("TEXT_NAVI_EQUIP_SYNCRO_RESULT").Replace("{0}", sync_base_entry.item_name).Replace("{1}", sync_response.alv.ToString()) + "\n\n";
            else
                text = Utility.getText("TEXT_NAVI_EQUIP_SYNCRO_RESULT2").Replace("{0}", sync_base_entry.item_name).Replace("{1}", sync_response.alv.ToString()) + "\n\n";
        }

        text += Utility.getText("TEXT_NAVI_EQUIP_SYNCRO_RESULT3").Replace("{0}", sync_base_entry.item_name).Replace("{1}", sync_source_entry.item_name) + "\n\n";
        text += Utility.getText("TEXT_NAVI_EQUIP_SYNCRO_RESULT_EXP").Replace("{0}", sync_response.bex.ToString()).Replace("{1}", sync_response.aex.ToString()) + "\n";

        if (sync_response.alv > sync_response.blv)
        {
            text += Utility.getText("TEXT_NAVI_EQUIP_SYNCRO_RESULT_LEVEL").Replace("{0}", sync_response.blv.ToString()).Replace("{1}", sync_response.alv.ToString()) + " \n\n";
        }

        text += "\n";
        text += Utility.getText("TEXT_NAVI_EQUIP_SYNCRO_RESULT_END").Replace("{0}", sync_source_entry.item_name) + "\n";

        TextResult.text = text;

        //flashに値段を反映(ヘッダは常にrefが定期的に走るので値だけ書き換え)
        APIConnectManager.Instance.Home((string json) =>
        {
            //API結果受け取り
            HomeApi homeSummary = JsonUtility.FromJson<HomeApi>(json);

            Header.Instance.SetSummary(homeSummary);
            Footer.Instance.SetSummary(homeSummary);
        });

    }


    public void onClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        //APIをたたく
        APIConnectManager.Instance.EquipList(EquipBehaviour.Instance.onStart);

        transform.gameObject.SetActive(false);
    }


}
