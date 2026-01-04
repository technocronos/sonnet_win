using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using MyScene;
using Scenes.Common.Scripts;

public class Footer : MonoBehaviour
{
    public GameObject FooterBase;
    public Button Menu0;
    public Button Menu1;
    public Button Menu2;
    public Button Menu3;
    public Button Menu4;
    public Button Menu5;

    private Vector3 _startVector3;
    private static Footer instance;
    private HomeApi summary = null;

    Dictionary<string, Tweener> tweener = new Dictionary<string, Tweener>();

    public static Footer Instance
    {
        get
        {
            return instance;
        }
    }

    private string[] menu_arr { get; set; } = { "Home", "MyPage", "Equip", "Gacha", "Shop", "Book" };

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        _startVector3 = gameObject.transform.localPosition;
        //transform.localPosition = new Vector3(_startVector3.x, _startVector3.y - 200, _startVector3.z);

        tweener[Menu0.name] = null;
        tweener[Menu1.name] = null;
        tweener[Menu2.name] = null;
        tweener[Menu3.name] = null;
        tweener[Menu4.name] = null;
        tweener[Menu5.name] = null;

    }

    /*
     * サマリーから状態を反映する
     * 	"menu0Url": "menu",
     *	"menu1Url": "status",
     *	"menu2Url": "weapon",
     *	"menu3Url": "gacha",
     *	"menu4Url": "shop",
     *	"menu5Url": "zukan",
     *	"menu6Url": "quest",
     *	"menu7Url": "battle",
     */
    public void SetSummary(HomeApi _summary)
    {
        summary = _summary;

        if (summary.menu0State == "disable")
            disableButton(Menu0);
        else
            eableButton(Menu0, summary.menu0State);

        if (summary.menu1State == "disable")
            disableButton(Menu1);
        else
            eableButton(Menu1, summary.menu1State);

        if (summary.menu2State == "disable")
            disableButton(Menu2);
        else
            eableButton(Menu2, summary.menu2State);

        if (summary.menu3State == "disable")
            disableButton(Menu3);
        else
            eableButton(Menu3, summary.menu3State);

        if (summary.menu4State == "disable")
            disableButton(Menu4);
        else
            eableButton(Menu4, summary.menu4State);

        if (summary.menu5State == "disable")
            disableButton(Menu5);
        else
            eableButton(Menu5, summary.menu5State);

    }


    public void setUserId(int UserId)
    {
        PlayerPrefs.SetInt(Settings.PREF_KEY_HIS_USER_ID, UserId);
    }

    public int getUserId()
    {
        return PlayerPrefs.GetInt(Settings.PREF_KEY_HIS_USER_ID, 0);
    }

    /// <summary>
    /// ボタンを差し替えて押せなくする
    /// </summary>
    /// <param name="Menu"></param>
    private void disableButton(Button Menu)
    {
        //選択状態にする
        Image BackgroundSelect = transform.Find("FooterBase/" + Menu.name + "/BackgroundDisabled").GetComponent<Image>();
        Menu.targetGraphic = BackgroundSelect;
        BackgroundSelect.gameObject.SetActive(true);

        Transform objBackground = transform.Find("FooterBase/" + Menu.name + "/Background");
        objBackground.gameObject.SetActive(false);


        Menu.interactable = false;
        Menu.transform.Find("Batch").gameObject.SetActive(false);
    }

    /// <summary>
    /// ボタンを押せるようにする
    /// </summary>
    /// <param name="Menu"></param>
    private void eableButton(Button Menu, string state)
    {
        Menu.interactable = true;

        Image BackgroundSelect = transform.Find("FooterBase/" + Menu.name + "/BackgroundDisabled").GetComponent<Image>();
        Menu.targetGraphic = BackgroundSelect;
        BackgroundSelect.gameObject.SetActive(false);

        if (state == "hot")
        {
            Menu.transform.Find("Batch").gameObject.SetActive(true);
            //点滅
            Image image = Menu.transform.Find("Batch").GetComponent<Image>();

            if (tweener[Menu.name] == null)
                tweener[Menu.name] = image.DOFade(0.0f, 1f).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            Menu.transform.Find("Batch").gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// フッターを出す
    /// </summary>
    public void SetPosition()
    {
        return;

        //Debug.Log("SetPosition");
        transform.DOLocalMove(new Vector3(_startVector3.x, _startVector3.y, _startVector3.z), 1).SetEase(Ease.OutCubic);
    }

    /// <summary>
    /// メニューのスイッチ状態を切り替える
    /// </summary>
    void switchMenu()
    {
        Transform MyPaageMenu = transform.Find("FooterBase/MyPage");
        Transform HisPaageMenu = transform.Find("FooterBase/HisPage");

        //他人のページの場合はMypageと入れ替える
        if (getUserId() != 0)
        {
            menu_arr[1] = "HisPage";

            MyPaageMenu.gameObject.SetActive(false);
            HisPaageMenu.gameObject.SetActive(true);
        }
        else
        {
            menu_arr[1] = "MyPage";

            MyPaageMenu.gameObject.SetActive(true);
            HisPaageMenu.gameObject.SetActive(false);
        }


        //Debug.Log("switchMenu");
        //メニュー外のシーンにいる場合は全部選択解除
        if (!menu_arr.Exists(SceneController.Instance.SceneName))
        {
            //SpriteSwapを切る
            foreach (Transform child in FooterBase.transform)
            {
                child.GetComponent<Button>().transition = Selectable.Transition.None;

                //デフォルト状態にする
                menuInit(child);
            }
        }
        else
        {
            //SpriteSwapをOnにする
            foreach (Transform child in FooterBase.transform)
            {
                child.GetComponent<Button>().transition = Selectable.Transition.SpriteSwap;

                //デフォルト状態にする
                menuInit(child);
            }

            //選択状態にする
            Button Menu = transform.Find("FooterBase/" + SceneController.Instance.SceneName).GetComponent<Button>();

            Image BackgroundSelect = transform.Find("FooterBase/" + SceneController.Instance.SceneName + "/BackgroundSelect").GetComponent<Image>();
            Menu.targetGraphic = BackgroundSelect;
            BackgroundSelect.gameObject.SetActive(true);

            Transform objBackground = transform.Find("FooterBase/" + SceneController.Instance.SceneName + "/Background");
            objBackground.gameObject.SetActive(false);

        }
    }


    /// <summary>
    /// 通常状態に初期化する
    /// </summary>
    /// <param name="child"></param>
    void menuInit(Transform child)
    {
        Image Background = child.transform.Find("Background").GetComponent<Image>();
        child.GetComponent<Button>().targetGraphic = Background;
        Background.gameObject.SetActive(true);

        Transform objBackgroundSelect = child.transform.Find("BackgroundSelect");
        objBackgroundSelect.gameObject.SetActive(false);
    }


    /// <summary>
    /// フッターをひっこめる
    /// </summary>
    public void SetOutPosition()
    {
        return;

        //Debug.Log("SetOutPosition");
        transform.DOLocalMove(new Vector3(_startVector3.x, _startVector3.y - 200, _startVector3.z), 1).SetEase(Ease.OutCubic).OnComplete(switchMenu);
    }
}
