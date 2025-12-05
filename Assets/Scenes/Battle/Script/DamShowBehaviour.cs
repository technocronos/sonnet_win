using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

//
// ダメージ表示を行うムービー
public class DamShowBehaviour : MonoBehaviour
{
    public int value { get; set; }
    public string side { get; set; }
    public int type { get; set; }
    public int dir { get; set; }
    public string way { get; set; }
    GameObject Shower { get; set; } = null;

    float startX { get; set; }
    float startY { get; set; }

    float destX { get; set; }
    float destY { get; set; }

    //
    // ダメージ表示を開始するためのcallラベル。
    // 次の変数をセットして呼び出す。
    //     value    ダメージの値。
    //     type     どの属性でのダメージか。1, 2, 3 のいずれか。
    //              必殺技などでいずれでもないならば 4 を指定する。
    //     side     表示を開始する位置。"P" か "E" で指定する。
    //     dir      ダメージが飛び出る方向。0, 1, 2 で指定する。
    public void DamShowStart()
    {
        //初期化
        transform.Find("damShow1").gameObject.SetActive(false);
        transform.Find("damShow2").gameObject.SetActive(false);
        transform.Find("damShow3").gameObject.SetActive(false);
        transform.Find("damShow4").gameObject.SetActive(false);
        transform.Find("recov").gameObject.SetActive(false);

        switch (type)
        {
            case 1:
            case 2:
            case 3:
            case 4:
                Shower = transform.Find("damShow" + type).gameObject;
                break;
            case 5:
                Shower = transform.Find("recov").gameObject;
                break;
        }

        Shower.SetActive(true);

        // ダイナミックテキストにダメージの値をセット。
        Shower.transform.Find("value").GetComponent<TextMeshProUGUI>().text = value.ToString();

        int marginX = 450;
        int marginY = 70;

        // 飛び出す開始位置と終了位置を決める。
        if (side == "P")
        {
            startX = BattleBehaviour.HIT_XP;
            startY = BattleBehaviour.HIT_YP;
            destX = ((BattleBehaviour.STAGE_WIDTH / 2) - marginX) * -1;
            destY = startY + (marginY * (dir - 1));
            Debug.Log("destY=" + destY + " destX = " + destX);
        }
        else
        {
            startX = BattleBehaviour.HIT_XE;
            startY = BattleBehaviour.HIT_YE;
            destX = (BattleBehaviour.STAGE_WIDTH / 2) - marginX;
            destY = startY + (marginY * (dir - 1));
        }

        //開始位置にセット
        transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(startX, startY, 0);

        // ブレーキをかけながら飛び出すようにする
        transform.GetComponent<RectTransform>().DOAnchorPos(new Vector3(destX, destY, 0), 1.0f).SetEase(Ease.OutCubic).OnComplete(onEnd);

    }

    void onEnd()
    {
        transform.gameObject.SetActive(false);
    }


}
