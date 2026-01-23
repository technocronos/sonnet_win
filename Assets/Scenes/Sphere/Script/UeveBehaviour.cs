using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class UeveBehaviour : MonoBehaviour
{

    public GameObject RecovPanel;
    public GameObject DamPanel;

    public TextMeshProUGUI RecovText;
    public TextMeshProUGUI DamText;

    SphereBehaviour Sphere { get; set; }
    StageBehaviour Stage { get; set; }

    //
    // イベント表示を開始するためのcallラベル。
    // 次の変数をセットして呼び出す。
    //     no       ユニット番号
    //     type     イベントアイコンの種類
    //     num      数字表示が伴う(ダメージや回復など)場合はその数値
    public void Play(int no, string type, string num)
    {
        Sphere = SphereBehaviour.Instance;
        Stage = StageBehaviour.Instance;

        // 指定されたユニットの基本情報を取得。
        jsonUnit unitinfo = Sphere.sphere.unit[no];

        // 自身の位置をセット。対象ユニットに合わせる。
        float x = (unitinfo.X * Sphere.TIP_SIZE) + (Sphere.UNIT_SIZE / 2);
        float y = ((unitinfo.Y * Sphere.TIP_SIZE)) * -1;

        // アイコンをセット。
        switch (type)
        {
            case "recov":
                RecovPanel.SetActive(true);
                DamPanel.SetActive(false);
                RecovText.text = num;
                break;
            case "dam":
                DamPanel.SetActive(true);
                RecovPanel.SetActive(false);
                DamText.text = num;
                break;
        }

        //ポジションを設定する
        transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y, 0);

        //前面表示する
        transform.GetComponent<RectTransform>().SetAsLastSibling();

        float _x = x;
        float _y = y + 40;

        // ブレーキをかけながらに飛び出すようにする
        transform.GetComponent<RectTransform>().DOAnchorPos(new Vector3(_x, _y, 0), 1.0f).SetEase(Ease.OutCubic).OnComplete(onEnd);
    }

    void onEnd()
    {
        //transform.gameObject.SetActive(false);
        // ムービを破棄。
        GameObject.Destroy(transform.gameObject);
    }

}
