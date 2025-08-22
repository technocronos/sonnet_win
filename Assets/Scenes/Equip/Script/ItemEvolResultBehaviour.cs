using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class ItemEvolResultBehaviour : MonoBehaviour
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

        //BGMを止める
        AudioManager.Instance.StopBGM();

        //全画面表示
        Header.Instance.gameObject.SetActive(false);
        Footer.Instance.gameObject.SetActive(false);

        reload();
    }
    void reload()
    {

        //アイテムアイコン
        Sprite itemIcon = Utility.getAssetImage(Utility.getItemIconURL(sync_base_entry.item_id));
        // ファイルが存在するものだけ
        if (itemIcon != null)
        {
            //画像を差し替えていく
            ItemIcon.sprite = itemIcon;
        }

        var text = "";

        Rotator.gameObject.SetActive(true);

        //回転アニメ
        Rotator.transform.DOLocalRotate(new Vector3(0, 0, 360f), 30f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);

        //レベルMAX
        if (sync_response.alv == sync_response.maxlv)
            text = Utility.getText("TEXT_NAVI_EQUIP_EVOLUTION_RESULT").Replace("{0}", sync_base_entry.item_name).Replace("{1}", sync_response.alv.ToString());
        else
            text = Utility.getText("TEXT_NAVI_EQUIP_EVOLUTION_RESULT2").Replace("{0}", sync_base_entry.item_name).Replace("{1}", sync_response.alv.ToString());

        TextResult.text = text;

        //値段を反映(ヘッダは常にrefが定期的に走るので値だけ書き換え)
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

        AudioManager.Instance.PlayBGM("bgm_menu", AudioManager.BGM_VOLUME_DEFULT);
        transform.gameObject.SetActive(false);

        Header.Instance.gameObject.SetActive(true);
        Footer.Instance.gameObject.SetActive(true);

    }


}
