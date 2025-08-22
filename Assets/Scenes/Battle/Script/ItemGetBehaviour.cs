using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class ItemGetBehaviour : MonoBehaviour
{
    public Image Rotator;
    public GameObject Content;
    public GameObject ListItem;
    public GameObject ListEquip;

    public TextMeshProUGUI Title;

    public delegate void OnCompleteDelegate();
    public OnCompleteDelegate CompleteHandler;

    void Start()
    {
        ListItem.SetActive(false);
        ListEquip.SetActive(false);

        Title.text = Utility.getText("TEXT_TITLE_ITEMGET");

        //回転アニメ
        Rotator.transform.DOLocalRotate(new Vector3(0, 0, 360f), 30f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
    }

    public void Show(jsonBattleResult result, OnCompleteDelegate _callback)
    {
        if (_callback != null)
            CompleteHandler += _callback;

        AudioManager.Instance.PlaySE("se_congrats");

        foreach (jsonUserItem uitem in result.battleresult.gain.uitem)
        {
            GameObject board = null;
            if (uitem.category == "ITM")
            {
                // 複製
                board = UnityEngine.Object.Instantiate(ListItem, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
            }
            else
            {
                board = UnityEngine.Object.Instantiate(ListEquip, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);

            }
            board.SetActive(true);

            setupEntryBoard(uitem, board);
        }

    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * 引数に指定されたエントリを、指定されたボードに表示するときに呼ばれる。
     */
    void setupEntryBoard(jsonUserItem entry, GameObject board)
    {
        //装備名
        board.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = entry.item_name;
        //装備アイコン
        board.transform.Find("ItemIcon").GetComponent<Image>().sprite = Utility.getAssetImage(Utility.getItemIconURL(entry.item_id));
        //フレーバーテキスト
        board.transform.Find("flavor_text").GetComponent<TextMeshProUGUI>().text = entry.flavor_text;

        if (entry.category == "ITM")
        {
            board.transform.Find("Effect").GetComponent<TextMeshProUGUI>().text = entry.effect;
        }
        else
        {
            board.transform.Find("StatusPanel/att1").GetComponent<TextMeshProUGUI>().text = entry.attack1.ToString();
            board.transform.Find("StatusPanel/att2").GetComponent<TextMeshProUGUI>().text = entry.attack2.ToString();
            board.transform.Find("StatusPanel/att3").GetComponent<TextMeshProUGUI>().text = entry.attack3.ToString();
            board.transform.Find("StatusPanel/spd").GetComponent<TextMeshProUGUI>().text = entry.speed.ToString();

            board.transform.Find("StatusPanel/def1").GetComponent<TextMeshProUGUI>().text = entry.defence1.ToString();
            board.transform.Find("StatusPanel/def2").GetComponent<TextMeshProUGUI>().text = entry.defence2.ToString();
            board.transform.Find("StatusPanel/def3").GetComponent<TextMeshProUGUI>().text = entry.defence3.ToString();
            board.transform.Find("StatusPanel/defX").GetComponent<TextMeshProUGUI>().text = entry.defenceX.ToString();
        }
    }

    public void onClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        if (CompleteHandler != null)
        {
            CompleteHandler?.Invoke();
            CompleteHandler = null;
        }

        transform.gameObject.SetActive(false);
    }
}
