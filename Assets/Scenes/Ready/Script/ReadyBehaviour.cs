using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using System;
using UnityEngine.UI;
using CreateWave;
using Scenes.Common.Scripts;

public class ReadyBehaviour : BaseBehaviour
{

    public GameObject ListItem;
    public GameObject ListEquip;
    public GameObject ListNone;
    public GameObject Content;
    public NaviController naviController;

    public Image BG;

    private string category = "RCV";
    private bool navi_speak = false;

    const int RCV_LIMIT = 6;
    const int ATT_LIMIT = 6;
    const int EQP_LIMIT = 2;

    public TextMeshProUGUI Title;
    public TextMeshProUGUI CaptionRCV_TAKE;
    public TextMeshProUGUI CaptionATT_TAKE;
    public TextMeshProUGUI CaptionEQP_TAKE;

    Dictionary<string, int> LIMIT = new Dictionary<string, int>();
    Dictionary<string, int> category_num = new Dictionary<string, int>();
    Dictionary<string, int> slot_count = new Dictionary<string, int>();
    Dictionary<int, List<jsonItems>> slot = new Dictionary<int, List<jsonItems>>();

    private GameObject Arrow;
    jsonConstants constants;
    public class Parameter
    {
        public int questId;
        public int placeId;
        public int consume_pt;
        public string FromScene = "Quest";
    }

    public Parameter Param;

    jsonReady ready { get; set; }
    jsonReady readyEnd { get; set; }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        BG.sprite = Utility.getAssetImage("Image/BG/circle_bg");

        //safearea対応
        setSafearea("ReadyCanvas");

        Title.text = Utility.getText("TEXT_QUEST_READY");
        CaptionRCV_TAKE.text = Utility.getText("TEXT_READY_CAPTION_RCV_TAKE");
        CaptionATT_TAKE.text = Utility.getText("TEXT_READY_CAPTION_ATT_TAKE");
        CaptionEQP_TAKE.text = Utility.getText("TEXT_READY_CAPTION_EQP_TAKE");

        //初期化
        category_num["RCV"] = 1;
        category_num["ATT"] = 2;
        category_num["EQP"] = 3;

        slot[category_num["RCV"]] = new List<jsonItems>();
        slot[category_num["ATT"]] = new List<jsonItems>();
        slot[category_num["EQP"]] = new List<jsonItems>();

        slot_count["RCV"] = 0;
        slot_count["ATT"] = 0;
        slot_count["EQP"] = 0;

        LIMIT["RCV"] = RCV_LIMIT;
        LIMIT["ATT"] = ATT_LIMIT;
        LIMIT["EQP"] = EQP_LIMIT;

        ListItem.gameObject.SetActive(false);
        ListEquip.gameObject.SetActive(false);
        ListNone.gameObject.SetActive(false);
        naviController.gameObject.SetActive(false);

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        //APIをたたく
        APIConnectManager.Instance.Ready(Param.questId, Param.placeId, Param.consume_pt, onStart);
    }

    /// <summary>
    /// スタート時呼び出し
    /// </summary>
    /// <param name="json">json</param>
    void onStart(string json)
    {
        Debug.Log("ReadyBehaviour onStart run.." + json);
        //API結果受け取り
        ready = JsonUtility.FromJson<jsonReady>(json);

        Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        foreach (KeyValuePair<string, object> keyvalue in jsonDict)
        {
            if (keyvalue.Key == "item")
            {
                Dictionary<string, List<jsonItems>> items = new Dictionary<string, List<jsonItems>>();

                Dictionary<string, object> dict1 = JsonConvert.DeserializeObject<Dictionary<string, object>>(keyvalue.Value.ToString());
                foreach (KeyValuePair<string, object> keyvalue2 in dict1)
                {
                    Debug.Log(keyvalue2.Value);

                    List<jsonItems> item = new List<jsonItems>();

                    List<object> dict2 = JsonConvert.DeserializeObject<List<object>>(keyvalue2.Value.ToString());

                    foreach (object arr in dict2)
                    {
                        Debug.Log(arr.ToString());

                        jsonItems jsonItemlist = new jsonItems();
                        jsonItemlist = JsonUtility.FromJson<jsonItems>(arr.ToString());

                        item.Add(jsonItemlist);
                    }


                    items.Add(keyvalue2.Key, item);
                }

                ready.item = items;
            }
            else if (keyvalue.Key == "comment")
            {
                //ready.comment = JsonConvert.DeserializeObject<List<string>>(keyvalue.Value.ToString());
            }
        }

        if (ready.Api == "FieldReopen")
        {
            //APIをたたく
            APIConnectManager.Instance.FieldReopen(null, onFieldReopen);
        }
        else if (ready.Scene != "" && ready.Scene != null)
        {

            switch (ready.Scene)
            {
                case "Sphere":
                    SceneController.Instance.Jump("Sphere", (() =>
                    {
                        SphereBehaviour _sphere = FindObjectOfType<SphereBehaviour>() as SphereBehaviour;
                        _sphere.Param = new SphereBehaviour.Parameter
                        {
                            sphereId = ready.id,
                        };
                    }));
                    break;
                case "Terminable":
                    SceneController.Instance.Jump("Terminable", (() =>
                    {
                        TerminableBehaviour _terminable = FindObjectOfType<TerminableBehaviour>() as TerminableBehaviour;
                        _terminable.Param = new TerminableBehaviour.Parameter
                        {
                            questId = ready.questId,
                            sphereId = ready.sphereId,
                        };
                    }));
                    break;
                case "QuestDrama":
                    SceneController.Instance.Jump("Terminable", (() =>
                    {
                        QuestDramaBehaviour _questdrama = FindObjectOfType<QuestDramaBehaviour>() as QuestDramaBehaviour;
                        _questdrama.Param = new QuestDramaBehaviour.Parameter
                        {
                            questId = ready.questId,
                        };
                    }));
                    break;
                default:
                    SceneController.Instance.Jump(ready.Scene);
                    break;
            }
        }
        else
        {

            AudioManager.Instance.PlayBGM("bgm_registance", AudioManager.BGM_VOLUME_DEFULT);

            transform.Find("ReadyCanvas/Take/RCV_TAKE").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_KOSUU").Replace("{0}", RCV_LIMIT.ToString());
            transform.Find("ReadyCanvas/Take/ATT_TAKE").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_KOSUU").Replace("{0}", ATT_LIMIT.ToString());
            transform.Find("ReadyCanvas/Take/EQP_TAKE").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_KOSUU").Replace("{0}", EQP_LIMIT.ToString());

            this.Reload();

            DispatchEvent(CwEvent.SCENE_READY);

        }

    }

    void onFieldReopen(string json)
    {
        //API結果受け取り
        jsonFieldReopen results = JsonUtility.FromJson<jsonFieldReopen>(json);

        switch (results.Scene)
        {
            case "Sphere":
                SceneController.Instance.Jump("Sphere", (() =>
                {
                    SphereBehaviour _sphere = FindObjectOfType<SphereBehaviour>() as SphereBehaviour;
                    _sphere.Param = new SphereBehaviour.Parameter
                    {
                        sphereId = results.id,
                        reopen = results.reopen,
                    };
                }));
                break;
            case "FieldEnd":
                SceneController.Instance.Jump("Sphere", (() =>
                {
                    FieldEndBehaviour _fieldend = FindObjectOfType<FieldEndBehaviour>() as FieldEndBehaviour;
                    _fieldend.Param = new FieldEndBehaviour.Parameter
                    {
                        sphereId = results.sphereId,
                    };
                }));
                break;
            default:
                SceneController.Instance.Jump(results.Scene);
                break;
        }
    }

    List<GameObject> curList = new List<GameObject>();

    void Reload()
    {
        List<jsonItems> itemList = ready.item[category];
        ListNone.gameObject.SetActive(false);

        //リスト一旦クリア
        this.listClear();

        if (itemList.Count == 0)
        {
            ListNone.gameObject.SetActive(true);

            return;
        }

        string _category = "";

        //カテゴリをRCV,ATT,EQPに翻訳する
        if (this.category == "RCV" || this.category == "ATT")
            _category = this.category;
        else
            _category = "EQP";

        int i = 0;

        foreach (jsonItems item in itemList)
        {
            GameObject _list = null;
            if (_category == "RCV" || _category == "ATT")
            {
                // リストを複製
                _list = UnityEngine.Object.Instantiate(ListItem, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
                _list.name = "ListItem" + i;
                if (_category == "RCV")
                {
                    _list.transform.Find("Caption1").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_RECOVER");
                }
                else
                {
                    _list.transform.Find("Caption1").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_ATTACK");
                }

                _list.transform.Find("TextNum").GetComponent<TextMeshProUGUI>().text = item.num.ToString();

                _list.transform.Find("TextValue").GetComponent<TextMeshProUGUI>().text = item.item_value.ToString();
                _list.transform.Find("TextIimitation").GetComponent<TextMeshProUGUI>().text = item.item_limitation.ToString();
                _list.transform.Find("TextSpread").GetComponent<TextMeshProUGUI>().text = item.item_spread.ToString();

            }
            else
            {
                // リストを複製
                _list = UnityEngine.Object.Instantiate(ListEquip, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
                _list.name = "ListEquip" + i;

                string level = item.level.ToString();
                if (item.level == item.max_level)
                    _list.transform.Find("TextLv").GetComponent<TextMeshProUGUI>().text = level + "[MAX]";
                else
                    _list.transform.Find("TextLv").GetComponent<TextMeshProUGUI>().text = level;

                if (item.durable_count != constants.Item_Master.INFINITE_DURABILITY)
                    _list.transform.Find("TextDurable").GetComponent<TextMeshProUGUI>().text = item.durability.ToString();
                else
                    _list.transform.Find("TextDurable").GetComponent<TextMeshProUGUI>().text = "∞";

                //進化している場合
                if (item.evolution == 1)
                {
                    Color color;
                    if (ColorUtility.TryParseHtmlString("#FD6200", out color))// outキーワードで参照渡しにする
                    {
                        // Color型への変換成功（colorにColor型の赤色が代入される）
                        // _list.transform.Find("LvImage").GetComponent<Image>().color = color;
                    }
                }

                _list.transform.Find("StatusPanel/att1").GetComponent<TextMeshProUGUI>().text = item.attack1.ToString();
                _list.transform.Find("StatusPanel/att2").GetComponent<TextMeshProUGUI>().text = item.attack2.ToString();
                _list.transform.Find("StatusPanel/att3").GetComponent<TextMeshProUGUI>().text = item.attack3.ToString();
                _list.transform.Find("StatusPanel/spd").GetComponent<TextMeshProUGUI>().text = item.speed.ToString();

                _list.transform.Find("StatusPanel/def1").GetComponent<TextMeshProUGUI>().text = item.defence1.ToString();
                _list.transform.Find("StatusPanel/def2").GetComponent<TextMeshProUGUI>().text = item.defence2.ToString();
                _list.transform.Find("StatusPanel/def3").GetComponent<TextMeshProUGUI>().text = item.defence3.ToString();
                _list.transform.Find("StatusPanel/defX").GetComponent<TextMeshProUGUI>().text = item.defenceX.ToString();
            }

            //非活性パネルは非表示
            _list.transform.Find("GrayCovor").GetComponent<Image>().enabled = false;

            //パネル初期化
            _list.transform.Find("Flame/Selected").GetComponent<Image>().enabled = false;
            _list.transform.Find("Flame/Normal").GetComponent<Image>().enabled = true;

            // ファイルが存在するものだけ
            Sprite itemIcon = Utility.getAssetImage(Utility.getItemIconURL(item.item_id));
            if (itemIcon != null)
            {
                //画像を差し替えていく
                _list.transform.Find("ItemIcon").GetComponent<Image>().sprite = itemIcon;
            }

            if (item.evolution == 1)
                _list.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = item.item_name + "<color=\"red\" >[" + Utility.getText("TEXT_EQUIP_EVOLUTION") + "]</color>";
            else
                //アイテム名
                _list.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = item.item_name;


            //持ち出し選択個数を調べる
            var item_count = 0;
            foreach (jsonItems value in this.slot[this.category_num[_category]])
            {
                if (value.item_id == item.item_id)
                {
                    item_count++;
                }
            }

            Transform Cancel = _list.transform.Find("Cancel");
            Transform Badage = _list.transform.Find("Badage");

            //スロットにあるものがある場合、状態を反映する
            if (item_count > 0)
            {
                //キャンセルボタン表示                
                Cancel.gameObject.SetActive(true);

                //個別持ち出し個数パネル表示
                Badage.gameObject.SetActive(true);

                //持ち出し個数を反映
                if (_category == "RCV" || _category == "ATT")
                    _list.transform.Find("TextNum").GetComponent<TextMeshProUGUI>().text = (item.num - item_count).ToString();

                //持っている個数を反映
                Badage.Find("TextBadage").GetComponent<TextMeshProUGUI>().text = (item_count).ToString();

                //個別リミットが来た場合は非活性
                if (item_count >= item.num)
                {
                    //非活性にする
                    _list.transform.Find("GrayCovor").GetComponent<Image>().enabled = true;
                }

                //何か選択されてる場合はONの画像にする
                _list.transform.Find("Flame/Normal").GetComponent<Image>().enabled = false;
                _list.transform.Find("Flame/Selected").GetComponent<Image>().enabled = true;
            }
            else
            {
                //キャンセルボタン非表示
                Cancel.gameObject.SetActive(false);
            }

            //位置設定
            _list.transform.localPosition = new Vector3(0, 0, 0);

            // セルクリックイベントハンドラ
            Badage.GetComponent<Button>().onClick.RemoveAllListeners();
            Badage.GetComponent<Button>().onClick.AddListener((() =>
            {
                onItemPick(_category, item, _list);
            }));

            //キャンセルボタンクリックイベントハンドラ
            Cancel.GetComponent<Button>().onClick.RemoveAllListeners();
            Cancel.GetComponent<Button>().onClick.AddListener((() => onItemUnPick(_category, item)));

            //表示
            _list.SetActive(true);

            i++;
        }

        int TUTORIAL_READY = PlayerPrefs.GetInt(Settings.TUTORIAL_READY, 0);

        //チュートリアル。ナビをしゃべらせる
        if (TUTORIAL_READY == 0)
        {

            HomeApi summary = Header.Instance.GetSummary();
            summary.opening = Utility.getText("TEXT_NAVI_TUTORIAL_READY").Split("\n");

            summary.openingNum = summary.opening.Length;

            Transform liistitem = Content.transform.Find("ListItem0");
            //アイテム持ってない場合はしょうがない・・
            if (liistitem != null)
            {
                Arrow = liistitem.Find("Arrow").gameObject;

                Arrow.SetActive(true);
                Vector3 pos = Arrow.GetComponent<RectTransform>().anchoredPosition;
                Arrow.GetComponent<ArrowBehaviour>().Show("up", pos.x, pos.y);
            }

            naviController.gameObject.SetActive(true);
            naviController.onStart(summary, null, () =>
            {
                naviController.disappere();

                if (liistitem != null)
                    Arrow.SetActive(false);

                //二度と表示しない
                PlayerPrefs.SetInt(Settings.TUTORIAL_READY, 1);
            });
        }
    }


    void onItemUnPick(string _category, jsonItems _entry)
    {

        AudioManager.Instance.PlaySE("se_btn");

        var clone = Utility.DeepCopy(this.slot);

        this.slot[this.category_num[_category]] = new List<jsonItems>();

        foreach (jsonItems value in clone[this.category_num[_category]])
        {
            if (value.item_id != _entry.item_id)
            {
                this.slot[this.category_num[_category]].Add(value);
            }
            else
            {
                //スロットの数を戻す
                this.slot_count[_category]--;
            }
        }

        //持ち出し個数反映
        transform.Find("ReadyCanvas/Take/" + _category + "_TAKE").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_KOSUU").Replace("{0}", (this.LIMIT[_category] - this.slot_count[_category]).ToString()); 
        

        this.Reload();
    }

    /// <summary>
    /// リストを全部消す
    /// </summary>
    void listClear()
    {
        foreach (Transform n in Content.transform)
        {
            //テンプレート以外は全部削除
            if (n.name != ListItem.name && n.name != ListEquip.name && n.name != ListNone.name)
                GameObject.Destroy(n.gameObject);
        }
    }

    /// <summary>
    /// タブクリック時イベントハンドラ
    /// </summary>
    /// <param name="_category">カテゴリ</param>
    public void onChangeCategory(string _category)
    {
        if (this.category == _category)
            return;

        AudioManager.Instance.PlaySE("se_btn");

        this.category = _category;

        this.Reload();

    }

    /// <summary>
    /// 出発ボタンクリック時イベントハンドラ
    /// </summary>
    public void onOkClick()
    {
        AudioManager.Instance.PlaySE("se_btn");

        AudioManager.Instance.StopBGM();

        Dictionary<string, int> slot = new Dictionary<string, int>();
        int counter = 0;

        foreach (KeyValuePair<int, List<jsonItems>> value in this.slot)
        {
            foreach (jsonItems value2 in value.Value)
            {
                slot["slot" + counter] = value2.user_item_id;
                counter++;
            }
        }

        if (counter == 0)
            slot["slot0"] = 0;

        //クエストへ行く。
        APIConnectManager.Instance.ReadyEnd(Param.questId, Param.placeId, Param.consume_pt, slot, onStart);

    }

    /// <summary>
    /// Cancelボタンクリック時イベントハンドラ
    /// </summary>
    public void onCancelClick()
    {
        AudioManager.Instance.PlaySE("se_btn");
        AudioManager.Instance.StopBGM();
        SceneController.Instance.Jump(Param.FromScene);
    }

    public void onItemPick(string _category, jsonItems _entry, GameObject _list)
    {
        AudioManager.Instance.PlaySE("se_btn");

        //全体リミットでないのであれば追加
        if (this.LIMIT[_category] > this.slot_count[_category])
        {
            //そのアイテムを持ち出せるかどうか
            var item_count = 0;

            foreach (jsonItems value in this.slot[this.category_num[_category]])
            {
                if (value.item_id == _entry.item_id)
                    item_count++;
            };

            //持ち出し個数にまだ余裕がある
            if (item_count < _entry.num)
            {
                //レコード追加
                this.slot[this.category_num[_category]].Add(_entry);

                //スロットの数を増やす
                this.slot_count[_category]++;
                item_count++;

                //持ち出し個数反映
                transform.Find("ReadyCanvas/Take/" + _category + "_TAKE").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_KOSUU").Replace("{0}", (this.LIMIT[_category] - this.slot_count[_category]).ToString());
                
                //キャンセルボタン表示
                _list.transform.Find("Cancel").gameObject.SetActive(true);
                //個別持ち出し個数パネル表示
                _list.transform.Find("Badage").gameObject.SetActive(true);
                //持っている個数を反映
                if (_category == "RCV" || _category == "ATT")
                    _list.transform.Find("TextNum").GetComponent<TextMeshProUGUI>().text = (_entry.num - item_count).ToString();

                //持ち出し個数を反映
                _list.transform.Find("Badage/TextBadage").GetComponent<TextMeshProUGUI>().text = (item_count).ToString();

                //個別リミットが来た場合は非活性
                if (item_count >= _entry.num)
                {
                    //非活性にする
                    _list.transform.Find("GrayCovor").GetComponent<Image>().enabled = true;
                }

                //何か選択されてる場合はONの画像にする
                _list.transform.Find("Flame/Normal").GetComponent<Image>().enabled = false;
                _list.transform.Find("Flame/Selected").GetComponent<Image>().enabled = true;
            }

            if (item_count == 0)
            {
                _list.transform.Find("Cancel").gameObject.SetActive(false);
            }
            else
            {
                _list.transform.Find("Cancel").gameObject.SetActive(true);
            }

        }
    }

}
