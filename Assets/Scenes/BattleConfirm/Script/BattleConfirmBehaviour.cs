using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MyScene;

public class BattleConfirmBehaviour : BaseBehaviour
{
    public TextMeshProUGUI gradeP;
    public TextMeshProUGUI gradeE;
    public TextMeshProUGUI nameP;
    public TextMeshProUGUI nameE;

    public Transform gaugeP;
    public TextMeshProUGUI hp_P;
    public TextMeshProUGUI hp_max_P;

    public Transform gaugeE;
    public TextMeshProUGUI hp_E;
    public TextMeshProUGUI hp_max_E;

    public TextMeshProUGUI LevelP;
    public TextMeshProUGUI Att1P;
    public TextMeshProUGUI Att2P;
    public TextMeshProUGUI Att3P;
    public TextMeshProUGUI SpdP;

    public TextMeshProUGUI LevelE;
    public TextMeshProUGUI Def1E;
    public TextMeshProUGUI Def2E;
    public TextMeshProUGUI Def3E;
    public TextMeshProUGUI SpdE;

    public AvatarBehaviour AvatarP;
    public AvatarBehaviour AvatarE;

    public GameObject AvatarPImage;
    public GameObject AvatarEImage;

    public TextMeshProUGUI TextBp;

    public TextMeshProUGUI CaptionEnemy;
    public TextMeshProUGUI CaptionCurrent;

    public Button ButtonBattle;
    public Button ButtonClose;

    public SmorkBehaviour SmorkEffects1;
    public SmorkBehaviour SmorkEffects2;

    public Image BG;

    public class Parameter
    {
        public int rivalId;
        public string BackTo;
    }

    public Parameter Param;

    private jsonRivalConfirm response;
    private jsonConstants constants;

    // Start is called before the first frame update
    protected override void Start()
    {
        //base.Start();

        BG.sprite = Utility.getAssetImage("Image/BG/circle_bg");


        Debug.Log("BattleConfirmBehaviour start.. userId=" + Param.rivalId);
        setSafearea("BattleConfirmCanvas");

        //定数取得
        constants = APIConnectManager.Instance.login.constants;


        CaptionEnemy.text = Utility.getText("TEXT_ENEMY");
        CaptionCurrent.text = Utility.getText("TEXT_CURRENT").Replace("{0}", string.Empty);

        StartCoroutine(AvatarP.PlayAnim("AvatarNone"));
        StartCoroutine(AvatarE.PlayAnim("AvatarNone"));

        //APIをたたく
        APIConnectManager.Instance.RivalConfirm(Param.rivalId, onStart);

        AudioManager.Instance.PlayBGM("bgm_registance", AudioManager.BGM_VOLUME_DEFULT);

        SmorkEffects1.PlayAnim("smork");
        SmorkEffects2.PlayAnim("smork_fast");


        DispatchEvent(CwEvent.SCENE_READY);
    }

    void onStart(string json)
    {
        response = JsonUtility.FromJson<jsonRivalConfirm>(json);

        if (response.result == "ok")
        {
            //ユーザー名
            nameP.text = response.chara1.player_name;
            nameE.text = response.chara2.player_name;

            gradeP.text = response.chara1.grade_name;
            gradeE.text = response.chara2.grade_name;

            LevelP.text = response.chara1.level.ToString();
            LevelE.text = response.chara2.level.ToString();

            Att1P.text = response.chara1.total_attack1.ToString();
            Att2P.text = response.chara1.total_attack2.ToString();
            Att3P.text = response.chara1.total_attack3.ToString();
            SpdP.text = response.chara1.total_speed.ToString();

            Def1E.text = response.chara2.total_defence1.ToString();
            Def2E.text = response.chara2.total_defence2.ToString();
            Def3E.text = response.chara2.total_defence3.ToString();
            SpdE.text = response.chara2.total_speed.ToString();

            TextBp.text = response.matchPt.ToString();

            // HPゲージを更新。
            int _hp_p = this.response.chara1.hp;
            float _hp_max_p = this.response.chara1.hp_max;
            float hp_val_p = Mathf.Min(_hp_p, _hp_max_p);

            float gauge_width_p = gaugeP.transform.GetComponent<RectTransform>().rect.width; ;

            int posx_p = (int)(((hp_val_p * 1.0f) / _hp_max_p) * gauge_width_p);
            gaugeP.transform.localPosition = new Vector3(posx_p - gauge_width_p, 0, 0);

            hp_P.text = _hp_p.ToString();
            hp_max_P.text = _hp_max_p.ToString();

            // HPゲージを更新。
            int _hp_e = this.response.chara2.hp;
            float _hp_max_e = this.response.chara2.hp_max;
            float hp_val_e = Mathf.Min(_hp_e, _hp_max_e);

            float gauge_width_e = gaugeE.transform.GetComponent<RectTransform>().rect.width; ;

            int posx_e = (int)(((hp_val_e * 1.0f) / _hp_max_e) * gauge_width_e);
            gaugeE.transform.localPosition = new Vector3(posx_e - gauge_width_e, 0, 0);

            hp_E.text = _hp_e.ToString();
            hp_max_E.text = _hp_max_e.ToString();

            //キャラ作成
            Main.Instance.makeCharaAnim(response.equip_infoP, AvatarPImage);
            Main.Instance.makeCharaAnim(response.equip_infoE, AvatarEImage);

            StartCoroutine(AvatarP.PlayAnim("AvatarAppear"));
            StartCoroutine(AvatarE.PlayAnim("AvatarAppear"));
        }
        else
        {
            switch (response.err_code)
            {
                case "count_rival":
                    Main.Instance.showDialogue(Utility.getText("TEXT_ERROR_BATTLE_LIMIT"), () =>
                    {
                        AudioManager.Instance.PlayBGM("bgm_menu", AudioManager.BGM_VOLUME_DEFULT);
                        SceneController.Instance.ClosePopUpName("BattleConfirm");
                    });
                    break;
                case "consume_pt":
                    Trans("scene=Suggest");
                    break;
            }
        }

    }

    public void onBattle()
    {
        AudioManager.Instance.PlaySE("se_btn");

        APIConnectManager.Instance.RivalBattle(Param.rivalId, (string json) =>
        {
            jsonRivalBattle res = JsonUtility.FromJson<jsonRivalBattle>(json);

            if (res.result == "ok")
            {
                AudioManager.Instance.StopBGM();
                Trans(res.url);
                return;
            }
        });
    }


    public void onClose()
    {
        AudioManager.Instance.PlaySE("se_btn");
        Trans(Param.BackTo);
    }

    //
    //画面遷移をする
    //
    public void Trans(string url)
    {

        AudioManager.Instance.StopBGM();

        Dictionary<string, string> transUrl = new Dictionary<string, string>();
        transUrl = Utility.ParseUrl(url);

        switch (transUrl["scene"])
        {
            case "Battle":
                SceneController.Instance.Jump("Battle", (() =>
                {
                    BattleBehaviour _battle = FindObjectOfType<BattleBehaviour>() as BattleBehaviour;
                    _battle.Param = new BattleBehaviour.Parameter
                    {
                        battleId = int.Parse(transUrl["battleId"]),
                    };
                }));
                break;
            case "Suggest":
                SceneController.Instance.Jump("Suggest", (() =>
                {
                    string suggest_url = "scene=HisPage&his_user_id=" + response.chara2.user_id;

                    SuggestBehaviour _suggest = FindObjectOfType<SuggestBehaviour>() as SuggestBehaviour;
                    _suggest.Param = new SuggestBehaviour.Parameter
                    {
                        type = "mp",
                        targetId = null,
                        backto = suggest_url,
                        useto = suggest_url,
                    };
                }));
                break;
            case "HisPage":
                SceneController.Instance.Jump("HisPage", (() =>
                {
                    HisPageBehaviour _hispage = FindObjectOfType<HisPageBehaviour>() as HisPageBehaviour;
                    _hispage.Param = new HisPageBehaviour.Parameter
                    {
                        userId = int.Parse(transUrl["his_user_id"]),
                    };
                }));
                break;
            default:
                SceneController.Instance.Jump(transUrl["scene"]);
                break;
        }
    }

}
