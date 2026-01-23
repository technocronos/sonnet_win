using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// ユーザーのステータス情報を表示する情報ウィンドウ
/// </summary>
class InfoWindowBehaviour : MonoBehaviour
{

    public static InfoWindowBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static InfoWindowBehaviour instance;

    public int unitNo { get; set; }

    //Sphereインスタンス
    private SphereBehaviour Sphere { get; set; }
    private StageBehaviour Stage { get; set; }
    private UserBehaviour User { get; set; }

    public int page { get; set; } = 0;

    public void init()
    {
        instance = this;

        Sphere = SphereBehaviour.Instance;
        Stage = StageBehaviour.Instance;
        User = UserBehaviour.Instance;
    }

    public void setVisible(bool _visible)
    {
        transform.gameObject.SetActive(_visible);
    }

    public void st()
    {
        page = -1;
        this.refInfo();
    }

    //
    // 変数 unitNo で示されたユニット情報を表示する。
    // unitNo に 0 を指定するとウィンドウを非表示にする。
    // また、どのページを表示するかを変数 page に以下の値で指定する。
    //     -1       ステータス
    //     0        装備
    //     1以上    アイテム
    // page>0 の場合、変数 noUse にtrueを指定すると、使用できないアイテムを
    // そのように表示するようになる。
    public void refInfo()
    {
        Debug.Log("infoW ref run..");

        if (unitNo == 0)
        {
            // 有効なユニットが指定されていないならウィンドウを非表示に。
            User.setStatusWindowVisible(false);

        }
        else
        {
            jsonUnit commUnitInfo = Sphere.sphere.unit[unitNo];

            //インフォウィンドウ表示
            User.setInfoWindowVisible(true);

            // 有効なユニットが指定されているならステータス表示。
            //User.setStatusWindowVisible(true);

            Stage.lef = 2; Stage.top = 2; Stage.rig = 2; Stage.bot = 4;
            Stage.center();

            if (page == -1)
            // ステータスページの場合。
            {
                // 前景をステータス用のものに。
                // 指定されたユニット番号をセットしてリフレッシュ。
                showStatus(unitNo);
            }
            else
            // アイテムページの場合。
            {
                // 前景をアイテム用のものに。
                // 指定されたユニット番号をセットしてリフレッシュ。
                showItem(unitNo, page);

            }
        }
    }

    // 変数 unitNo で示された番号のユニットのステータス情報を表示する。
    public void next()
    {
        //
        // 情報ウィンドウを次のページにする。
        // ステータス ⇒ アイテム1 ⇒ アイテム2 ... ⇒ 装備
        // の順になるようにする。
        /*
        // ページを一つ進める。
        page++;

        // けど、数値の通りには進まないので...
        switch (page)
        {

            // 「ステータス」の次は「装備」じゃなくて「アイテム1」
            case 0:
                page = 1;
                break;

            // 「装備」の次は「アイテム1」じゃなくて「ステータス」
            case 1:
                page = -1;
                break;

            // 「アイテムN」は順当に進めるけど...
            default:

                // アイテムページが終わったなら「装備」へ。
                if (itemPages < page)
                    page = 0;
        }

        // 表示更新。
        call("ref");

        */
    }


    // 変数 unitNo で示された番号のユニットのステータス情報を表示する。
    void showItem(int unitNo, int page)
    {
        User.setInfoWindowVisible(false);
    }

    // 
    // 変数 unitNo で示された番号のユニットのステータス情報を表示する。
    void showStatus(int unitNo)
    {
        jsonUnit unitinfo = Sphere.sphere.unit[unitNo];
        TextMeshProUGUI name = User.transform.Find("InfoW/Name").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI level = User.transform.Find("InfoW/LvIcon/Level").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI hp = User.transform.Find("InfoW/HPGauge/hp").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI max = User.transform.Find("InfoW/HPGauge/max").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI att1 = User.transform.Find("InfoW/StatusPanel/att1").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI att2 = User.transform.Find("InfoW/StatusPanel/att2").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI att3 = User.transform.Find("InfoW/StatusPanel/att3").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI def1 = User.transform.Find("InfoW/StatusPanel/def1").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI def2 = User.transform.Find("InfoW/StatusPanel/def2").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI def3 = User.transform.Find("InfoW/StatusPanel/def3").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI spd = User.transform.Find("InfoW/StatusPanel/spd").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI defX = User.transform.Find("InfoW/StatusPanel/defX").GetComponent<TextMeshProUGUI>();

        Transform gauge = User.transform.Find("InfoW/HPGauge/hp_gauge_bar/gauge");

        // 各種情報を表示。
        name.text = unitinfo.Name;
        level.text = "Lv" + unitinfo.Status.level;

        hp.text = unitinfo.Status.hp.ToString();
        max.text = unitinfo.Status.maxhp.ToString();
        att1.text = unitinfo.Status.att1.ToString();
        att2.text = unitinfo.Status.att2.ToString();
        att3.text = unitinfo.Status.att3.ToString();
        def1.text = unitinfo.Status.def1.ToString();
        def2.text = unitinfo.Status.def2.ToString();
        def3.text = unitinfo.Status.def3.ToString();
        spd.text = unitinfo.Status.spd.ToString();

        // 特殊防御の表示非表示を制御
        defX.text = unitinfo.Status.defX.ToString();
        defX.enabled = (defX.text != "");

        // HPゲージを更新。
        int _hp = unitinfo.Status.hp;
        int _hp_max = unitinfo.Status.maxhp;
        float hp_val = Mathf.Min(_hp, _hp_max);

        int posx = (int)(((hp_val * 1.0f) / _hp_max) * 292);
        gauge.transform.localPosition = new Vector3(posx - 292, 0, 0);

    }

}
