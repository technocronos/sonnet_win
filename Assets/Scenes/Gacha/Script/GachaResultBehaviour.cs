using MyScene;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GachaResultBehaviour : BaseBehaviour
{
    public GachaCircleBehaviour Circle;

    public ItemPickerAtariBehaviour ItemPicker;

    public TextMeshProUGUI TextSet;
    public TextMeshProUGUI TextName;

    public Image Item;
    public Image Rare;
    public Image Rare2;
    public Image Atari;
    public Image AtariAll;

    public GameObject ButtonItemPicker;
    public GameObject ButtonItemPickerAll;

    public TextMeshProUGUI TextFlavor;
    public TextMeshProUGUI TextEffect;

    public GameObject ItemPanel;
    public GameObject EqpPanel;
    public GameObject resultPanel;

    public Button ButtonSkip;
    public Button ButtonNext;
    public Button ButtonBack;

    public GameObject StatusPanel;
    public Image EffectPanel;
    public Image WhitePanel;

    public Image Rotator;

    public SmorkBehaviour SmorkEffects1;
    public SmorkBehaviour SmorkEffects2;

    public Image BG;

    public bool atari_flg = false;

    int current_display = 1;



    public static GachaResultBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    public static GachaResultBehaviour instance;

    public class Parameter
    {
        public string dataId;
    }

    public Parameter Param;

    public jsonGachaResult gacha_result { get; set; }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        instance = this;

        BG.sprite = Utility.getAssetImage("Image/BG/circle_bg2");

        Debug.Log("GachaResultBehaviour start..");
        //setSafearea("GachaResultCanvas");

        SmorkEffects1.PlayAnim("smork");
        SmorkEffects2.PlayAnim("smork_fast");

        ItemPicker.gameObject.SetActive(false);

        ButtonItemPicker.SetActive(false);
        ButtonItemPickerAll.SetActive(false);

        Atari.gameObject.SetActive(false);
        AtariAll.gameObject.SetActive(false);

        //APIをたたく
        APIConnectManager.Instance.GachaResult(Param.dataId, onStart);

        DispatchEvent(CwEvent.SCENE_READY);
    }

    void onStart(string json)
    {
        gacha_result = JsonUtility.FromJson<jsonGachaResult>(json);

        //スキップボタン
        if (gacha_result.gacha_count == 1)
        {
            //単発ガチャの場合はスキップは無し
            ButtonSkip.GetComponent< CanvasGroup>().alpha = 0;

            ButtonNext.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_GACHA_GOBACK");

            //次へボタンイベントハンドラ
            ButtonNext.onClick.AddListener(() =>
            {
                //単発ガチャの場合はガチャ画面へ戻る
                onButtonBack();
            });
        }
        else
        {
            //連ガチャの場合はスキップ可能
            ButtonSkip.GetComponent<CanvasGroup>().alpha = 1;

            //スキップボタンイベントハンドラ
            ButtonSkip.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySE("se_btn");

                PlayAll();
            });

            //次へボタンイベントハンドラ
            ButtonNext.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySE("se_btn");

                if (current_display < gacha_result.gacha_count)
                {
                    ButtonNext.interactable = false;

                    current_display++;

                    Hashtable hash = new Hashtable(){
                        {"from", 0f},
                        {"to", 1f},
                        {"time", 0.5f},
                        {"easeType",iTween.EaseType.easeOutQuad},
                        {"loopType",iTween.LoopType.none},
                        {"onupdate", "OnUpdateWhitePanel"},
                        {"onupdatetarget", gameObject},
                        {"oncomplete", "showItem"},
                        { "oncompleteparams", gacha_result.getitem[current_display - 1]}
                    };

                    iTween.ValueTo(gameObject, hash);
                }
                else
                {
                    PlayAll();
                }
            });
        }

        setItem(gacha_result.getitem[current_display - 1]);

        //スタート
        Circle.PlayAnim("start", onStartAnimEnd);
    }

    void OnUpdateWhitePanel(float alpha)
    {
        WhitePanel.color = new Color(EffectPanel.color.r, EffectPanel.color.g, EffectPanel.color.b, alpha);
    }

    public void showItem(jsonItems itemId)
    {
        setItem(itemId);
        Circle.PlayAnim("display", onDisplayAnimEnd);
    }

    void setItem(jsonItems item)
    {
        if (item.category == "ITM" || item.category == "SYS")
        {
            //ItemPanel.SetActive(true);
            //EqpPanel.SetActive(false);

            TextSet.gameObject.SetActive(false);

            TextEffect.gameObject.SetActive(true);
            StatusPanel.SetActive(false);
            TextEffect.text = item.effect;

            Rare.gameObject.SetActive(false);
            Rare2.gameObject.SetActive(false);
        }
        else
        {
            //ItemPanel.SetActive(false);
            // EqpPanel.SetActive(true);

            TextSet.text = item.set_name;

            TextEffect.gameObject.SetActive(false);

            StatusPanel.SetActive(true);

            StatusPanel.transform.Find("att1").GetComponent<TextMeshProUGUI>().text = item.attack1.ToString();
            StatusPanel.transform.Find("att2").GetComponent<TextMeshProUGUI>().text = item.attack2.ToString();
            StatusPanel.transform.Find("att3").GetComponent<TextMeshProUGUI>().text = item.attack3.ToString();
            StatusPanel.transform.Find("spd").GetComponent<TextMeshProUGUI>().text = item.speed.ToString();

            StatusPanel.transform.Find("def1").GetComponent<TextMeshProUGUI>().text = item.defence1.ToString();
            StatusPanel.transform.Find("def2").GetComponent<TextMeshProUGUI>().text = item.defence2.ToString();
            StatusPanel.transform.Find("def3").GetComponent<TextMeshProUGUI>().text = item.defence3.ToString();
            StatusPanel.transform.Find("defX").GetComponent<TextMeshProUGUI>().text = item.defenceX.ToString();

            //レアアイコン
            Rare.sprite = Utility.getAssetImage("Image/RareIcon/rare_icon_" + item.rear_level);
            Rare2.sprite = Utility.getAssetImage("Image/RareIcon/rare_icon_" + item.rear_level);

            atari_flg = false;

            if (gacha_result.guaranteed_item_id == item.item_id)
            {
                atari_flg = true;
            }

        }

        TextName.text = item.item_name;
        TextFlavor.text = item.flavor_text;

        //アイテムアイコン
        Item.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(-1000, 0, 0);
        Item.sprite = Utility.getAssetImage(Utility.getItemIconURL(item.item_id));
    }

    Tweener tweener = null;

    public void PlayAll()
    {

        if (tweener == null)
        {
            //回転アニメ
            tweener = Rotator.transform.DOLocalRotate(new Vector3(0, 0, 360f), 30f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }

        atari_flg = false;

        //画像差し替え
        for (int i = 1; i <= gacha_result.gacha_count; i++)
        {
            if (gacha_result.guaranteed_item_id == gacha_result.getitem[i - 1].item_id)
            {
                atari_flg = true;
            }

            resultPanel.transform.Find("Eqp" + i + "/Item").GetComponent<Image>().sprite = Utility.getAssetImage(Utility.getItemIconURL(gacha_result.getitem[i - 1].item_id));
            resultPanel.transform.Find("Eqp" + i + "/Rare").GetComponent<Image>().sprite = Utility.getAssetImage("Image/RareIcon/rare_icon_" + gacha_result.getitem[i - 1].rear_level);
        }

        Circle.PlayAnim("all", onDisplayAnimEnd);
    }

    public void showItemPicker(string from)
    {
        AudioManager.Instance.PlaySE("se_btn");

        ItemPicker.gameObject.SetActive(true);
        ItemPicker.Show(gacha_result.atari_item, from);
    }

    void onStartAnimEnd()
    {
        //個別表示
        Circle.PlayAnim("display", onDisplayAnimEnd);
    }

    void onDisplayAnimEnd()
    {
        Debug.Log("GachaResultBehaviour onDisplayAnimEnd start..");
        ButtonNext.interactable = true;
    }

    public void onButtonBack()
    {
        AudioManager.Instance.PlaySE("se_btn");

        Dictionary<string, string> transUrl = new Dictionary<string, string>();
        transUrl = Utility.ParseUrl(gacha_result.urlOnMain);

        SceneController.Instance.Jump(transUrl["scene"]);
    }
}
