using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemListBehaviour : MonoBehaviour
{

    public GameObject Content;
    public GameObject ListItem;
    public GameObject ListNone;

    private SphereBehaviour Sphere { get; set; }
    private StageBehaviour Stage { get; set; }
    private UserBehaviour User { get; set; }

    jsonSphereItemList SphereItemList { get; set; }

    Vector3 _position { set; get; }

    // Start is called before the first frame update
    void Start()
    {
        transform.gameObject.SetActive(false);
    }

    public void showItemList()
    {
        Sphere = SphereBehaviour.Instance;
        Stage = StageBehaviour.Instance;
        User = UserBehaviour.Instance;

        //X座標を決定する
        double x;
        double y;
        double x_margin;
        double y_margin;

        Vector3 _stage = Stage.transform.GetComponent<RectTransform>().anchoredPosition;
        Rect rect = transform.GetComponent<RectTransform>().rect;

        //カーソルが画面より右にある
        if ((Stage.cursorX * Sphere.TIP_SIZE) + _stage.x >= (Sphere.STAGE_WID / 2))
        {
            //ポップアップは左側に表示する
            x_margin = Sphere.TIP_SIZE * 0.5;
            x = (Stage.cursorX * Sphere.TIP_SIZE) - rect.width - x_margin;

            //コマンドポップアップが左にはみ出してしまう場合は調整する
            if (x + _stage.x < 0)
                x = x_margin - _stage.x;

        }
        else
        {
            //ポップアップは右側に表示する
            x_margin = Sphere.TIP_SIZE * 1.5;
            x = (Stage.cursorX * Sphere.TIP_SIZE) + x_margin;

            //コマンドポップアップが右にはみ出してしまう場合は調整する
            if (x + rect.width + _stage.x > Sphere.STAGE_WID)
                x = Sphere.STAGE_WID - _stage.x - (rect.width + x_margin);
        }


        float _x = (float)x;

        InfoWindowBehaviour InfoW = InfoWindowBehaviour.Instance;
        float h = InfoW.GetComponent<RectTransform>().rect.height;

        //Y座標を決定する
        y_margin = Sphere.TIP_SIZE * 0.5;
        y = (Stage.cursorY * Sphere.TIP_SIZE) + y_margin;

        //コマンドポップアップが座標0よりは下にいかないように調整する
        if (y + rect.height + y_margin > (Sphere.STAGE_HEI - h + _stage.y))
            y = (Sphere.STAGE_HEI - h + _stage.y) - (rect.height + y_margin);

        float _y = (float)y * -1;

        transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(_x, _y, 0);

        _position = ListItem.transform.localPosition;

        ListItem.SetActive(false);

        //APIをたたく
        APIConnectManager.Instance.SphereItemList(Sphere.Param.sphereId, Sphere.sphere.validation_code, onStart);

    }

    void onStart(string json)
    {
        //API結果受け取り
        SphereItemList = JsonUtility.FromJson<jsonSphereItemList>(json);

        Reload();
        transform.gameObject.SetActive(true);
    }

    void Reload()
    {
        //リストを初期化
        this.listClear();

        ListNone.gameObject.SetActive(false);

        //表示すべきものが無い場合
        if (SphereItemList.itemList.Length == 0)
        {
            ListNone.gameObject.SetActive(true);
            Content.gameObject.SetActive(false);

            return;
        }

        int i = 0;
        foreach (jsonSphereItems entry in SphereItemList.itemList)
        {
            GameObject _list = null;
            // リストを複製
            _list = UnityEngine.Object.Instantiate(ListItem, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
            _list.name = "ListItem" + i;
            _list.transform.localPosition = new Vector3(_position.x, _position.y, 0);

            if (entry.evolution == 1)
                _list.transform.Find("Flame/ItemName").GetComponent<TextMeshProUGUI>().text = entry.item_name + "["+Utility.getText("TEXT_EQUIP_EVOLUTION") +"]";
            else
                _list.transform.Find("Flame/ItemName").GetComponent<TextMeshProUGUI>().text = entry.item_name;

            // ファイルが存在するものだけ
            Sprite itemIcon = Utility.getAssetImage(Utility.getItemIconURL(entry.item_id));
            if (itemIcon != null)
            {
                //画像を差し替えていく
                _list.transform.Find("Flame/ItemIcon").GetComponent<Image>().sprite = itemIcon;
            }

            // セルクリックイベントハンドラ
            if (entry.useable == true)
            {
                _list.transform.Find("Flame/ButtonUse").GetComponent<Button>().onClick.AddListener((() => onItemPick(entry)));
                if (entry.category != "ITM")
                {
                    _list.transform.Find("Flame/ButtonUse/txtUse").GetComponent<TextMeshProUGUI>().text = Utility.getText("SPHERE_STR_CAPTION_EQUIP");
                }
            }
            else
            {
                _list.transform.Find("Flame/ButtonUse").GetComponent<Button>().interactable = false;

                if (entry.category != "ITM")
                    _list.transform.Find("Flame/ButtonUse/txtUse").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_EQUIP_IN_EQUIP");
            }

            _list.gameObject.SetActive(true);

            i++;
        }

        transform.GetComponent<RectTransform>().SetAsLastSibling();
    }

    void onItemPick(jsonSphereItems entry)
    {
        //タップ抑制
        User.objPointR.tab_enable_time = Utility.GetUnixTime(System.DateTime.Now);


        AudioManager.Instance.PlaySE("se_btn");

        transform.gameObject.SetActive(false);
        User.itemNo = entry.item_no;
        User.page = 1;
        User.slot = entry.slot;

        User.SelItem();
    }

    public void onClose()
    {
        //タップ抑制
        User.objPointR.tab_enable_time = Utility.GetUnixTime(System.DateTime.Now);

        AudioManager.Instance.PlaySE("se_btn");

        transform.gameObject.SetActive(false);

        //仕様変更によりアイテムキャンセルの場合は2回フェーズを戻す必要がある
        User.Cancel();
        User.Cancel();
        User.BtnItem.interactable = true;
    }

    /// <summary>
    /// リストを全部消す
    /// </summary>
    void listClear()
    {
        foreach (Transform n in Content.transform)
        {
            //テンプレート以外は全部削除
            if (n.name != ListItem.name)
                GameObject.Destroy(n.gameObject);
        }
    }

}
