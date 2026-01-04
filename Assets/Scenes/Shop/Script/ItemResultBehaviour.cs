using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyScene;
using DG.Tweening;

public class ItemResultBehaviour : MonoBehaviour
{
    public Shop Shop;
    public Image ItemIcon;

    public Image ResultIcon1;
    public Image ResultIcon2;
    public Image ResultIcon3;

    public Image Rotator;

    public TextMeshProUGUI ResultText;

    public Button btnOk;

    string currency { get; set; }
    string category { get; set; }

    int user_item_id { get; set; }

    int after_currency { get; set; }

    private jsonConstants constants;

    // Start is called before the first frame update
    public static ItemResultBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static ItemResultBehaviour instance;

    private void Start()
    {
        instance = this;

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        //アイコン初期化
        ResultIcon1.gameObject.SetActive(true);
        ResultIcon2.gameObject.SetActive(true);
        ResultIcon3.gameObject.SetActive(true);

        ResultIcon1.enabled = false;
        ResultIcon2.enabled = false;
        ResultIcon3.enabled = false;
    }

    public void Init(string _category, string _currency, int _user_item_id, int _after_currency)
    {
        user_item_id = _user_item_id;
        currency = _currency;
        category = _category;
        after_currency = _after_currency;

        if (_category != "ITM")
            category = "EQP";

        AudioManager.Instance.PlaySE("se_congrats");

        //回転アニメ
        Rotator.transform.DOLocalRotate(new Vector3(0, 0, 360f), 30f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);

        Reload();
    }

    void Reload()
    {
        //ShopAPIをたたく
        APIConnectManager.Instance.UserItem(user_item_id, ((string json) =>
        {
            //API結果受け取り
            jsonUserItems response = JsonUtility.FromJson<jsonUserItems>(json);

            if (response.result == "ok")
            {

                jsonShopResultSet user_item = response.user_item;
                //アイテムアイコン
                ItemIcon.sprite = Utility.getAssetImage(Utility.getItemIconURL(user_item.item_id));

                HomeApi summary = Header.Instance.GetSummary();

                //通貨
                if (this.currency == "gold")
                {
                    ResultIcon1.enabled = true;

                    //テキスト表示
                    ResultText.text = Utility.getText("TEXT_NAV_ITEM_GET_1").Replace("{0}", user_item.item_name);

                    //flashに値段を反映(ヘッダは常にrefが定期的に走るので値だけ書き換え)
                    summary.gold = this.after_currency;
                }
                else
                {
                    ResultIcon2.enabled = true;
                    //テキスト表示
                    ResultText.text = Utility.getText("TEXT_NAV_ITEM_GET_2").Replace("{0}", user_item.item_name);

                    summary.coin = this.after_currency;
                }

                //サマリーを反映
                Header.Instance.SetSummary(summary);

                //OKボタンクリック時イベントハンドラ
                btnOk.onClick.AddListener((() =>
                {
                    AudioManager.Instance.PlaySE("se_btn");

                    //チュートリアル中の場合、サマリーを再取得し、次の画面に移動する
                    if (Header.Instance.GetSummary().tutorial_step < constants.User_Info_Tutorial.TUTORIAL_END)
                    {
                        //ホームに戻る
                        SceneController.Instance.Jump("Home");
                    }
                    else
                    {
                        Shop.listClear();
                        //APIをたたく
                        APIConnectManager.Instance.ShopList(this.category, this.currency, Shop.onStart);
                    }

                    btnOk.onClick.RemoveAllListeners();
                    Rotator.DOKill();

                    transform.gameObject.SetActive(false);
                }));
            }
        }));
    }
}
