using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine.UI;

public class EquipListCellView : EnhancedScrollerCellView
{
    public GameObject board;

    jsonConstants constants;

    public Action<EquipListCellView> onClick;
    
    public Action<EquipListCellView> onClickButtonDust;
    public Action<EquipListCellView> onClickButtonEquip;
    public Action<EquipListCellView> onClickButtonSync;
    public Action<EquipListCellView> onClickButtonEvol;
    public Action<EquipListCellView> onClickButtonUse;

    public void SetData(jsonEquip entry, jsonEquip player_equip, bool eqponly = false)
    {
        if (entry.evolution == 1)
            board.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = entry.item_name + "<color=\"red\" >[" + Utility.getText("TEXT_EQUIP_EVOLUTION") + "]</color>";
        else
            //名前
            board.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = entry.item_name;

        //アイテムアイコン
        Sprite itemIcon = Utility.getAssetImage(Utility.getItemIconURL(entry.item_id));
        // ファイルが存在するものだけ
        if (itemIcon != null)
        {
            //画像を差し替えていく
            board.transform.Find("ItemFlame/ItemIcon").GetComponent<Image>().sprite = itemIcon;
        }

        board.transform.Find("TextFlavor").GetComponent<TextMeshProUGUI>().text = entry.flavor_text;

        Button ButtonDust;
        Button ButtonEquip;
        Button ButtonUse;
        Button ButtonSync;
        Button ButtonEvol;

        Transform objButtonEquip = board.transform.Find("ButtonEquip");
        Transform objButtonSync = board.transform.Find("ButtonSync");
        Transform objButtonEvol = board.transform.Find("ButtonEvol");        
        Transform objButtonUse = board.transform.Find("ButtonUse");
        Transform objButtonDust = board.transform.Find("ButtonDust");

        if (objButtonDust != null)
        {
            ButtonDust = objButtonDust.GetComponent<Button>();
        }

        if (entry.category == "ITM")
        {
            board.transform.Find("TextEffects").GetComponent<TextMeshProUGUI>().text = entry.effect;

            board.transform.Find("HasCount/TextHasCount").GetComponent<TextMeshProUGUI>().text = entry.num.ToString();


            //使用ボタン押下時イベントハンドラ
            if (objButtonUse != null)
            {
                ButtonUse = objButtonUse.GetComponent<Button>();
                ButtonUse.onClick.RemoveAllListeners();
                ButtonUse.interactable = true;

                //使用できないアイテムの場合
                if (!entry.useable)
                {
                    ButtonUse.interactable = false;
                }
                else
                {
                    ButtonUse.onClick.AddListener(() =>
                    {
                        onClickButtonUse?.Invoke(this);
                        //doUse(entry);
                    });
                }
            }

            //捨てるボタン押下時イベントハンドラ
            if (objButtonDust != null)
            {
                ButtonDust = objButtonDust.GetComponent<Button>();
                ButtonDust.interactable = true;

                if (entry.present_flg == 0)
                {
                    //ButtonDust.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("ERROR_MSG_CANT_DISCARD");
                    ButtonDust.interactable = false;
                }
                else
                {
                    ButtonDust.onClick.RemoveAllListeners();
                    ButtonDust.onClick.AddListener(() =>
                    {
                        onClickButtonDust?.Invoke(this);
                        //doDust(entry);
                    });
                }
            }
        }
        else
        {
            board.transform.Find("TextSet").GetComponent<TextMeshProUGUI>().text = entry.set_name;

            //レアアイコン
            Sprite RareIcon = Utility.getAssetImage("Image/RareIcon/rare_icon_" + entry.rear_level);
            if (RareIcon != null)
            {
                board.transform.Find("RareIcon").GetComponent<Image>().sprite = RareIcon;
            }

            string level = entry.level.ToString();
            if (entry.level == entry.max_level)
                level += "[MAX]";

            //進化している場合
            if (entry.evolution == 1)
            {
                Color color;
                if (ColorUtility.TryParseHtmlString("#56756d", out color))// outキーワードで参照渡しにする
                {
                    // Color型への変換成功（colorにColor型の赤色が代入される）
                    board.transform.Find("LvImage").GetComponent<Image>().color = color;
                }
            }

            board.transform.Find("TextLv").GetComponent<TextMeshProUGUI>().text = level;

            if (entry.durable_count != constants.Item_Master.INFINITE_DURABILITY)
                board.transform.Find("TextDurable").GetComponent<TextMeshProUGUI>().text = entry.durable_count.ToString();
            else
                board.transform.Find("TextDurable").GetComponent<TextMeshProUGUI>().text = "∞";

            board.transform.Find("StatusPanel/att1").GetComponent<TextMeshProUGUI>().text = entry.attack1.ToString();
            board.transform.Find("StatusPanel/att2").GetComponent<TextMeshProUGUI>().text = entry.attack2.ToString();
            board.transform.Find("StatusPanel/att3").GetComponent<TextMeshProUGUI>().text = entry.attack3.ToString();
            board.transform.Find("StatusPanel/spd").GetComponent<TextMeshProUGUI>().text = entry.speed.ToString();

            board.transform.Find("StatusPanel/def1").GetComponent<TextMeshProUGUI>().text = entry.defence1.ToString();
            board.transform.Find("StatusPanel/def2").GetComponent<TextMeshProUGUI>().text = entry.defence2.ToString();
            board.transform.Find("StatusPanel/def3").GetComponent<TextMeshProUGUI>().text = entry.defence3.ToString();
            board.transform.Find("StatusPanel/defX").GetComponent<TextMeshProUGUI>().text = entry.defenceX.ToString();

            //装備ボタン押下時イベントハンドラ
            if (objButtonEquip != null)
            {
                ButtonEquip = objButtonEquip.GetComponent<Button>();

                ButtonEquip.onClick.RemoveAllListeners();

                //ボタン表示初期化
                ButtonEquip.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_EQUIP");
                ButtonEquip.interactable = true;

                if (player_equip != null && player_equip.user_item_id == entry.user_item_id)
                {
                    //装備中の装備の場合は装備中と表示して合成不可
                    ButtonEquip.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_EQUIP_IN_EQUIP");
                    ButtonEquip.interactable = false;
                }
                else
                {
                    ButtonEquip.onClick.AddListener(() =>
                    {
                        onClickButtonEquip?.Invoke(this);
                        //doEquip(entry);
                    });
                }

                if (eqponly)
                {
                    //装備ボタンを真ん中
                    Vector3 hcv = objButtonEquip.transform.localPosition;
                    objButtonEquip.transform.localPosition = new Vector3(347f, hcv.y, hcv.z);
                }
            }

            //合成ボタン押下時イベントハンドラ
            if (objButtonSync != null)
            {
                if (eqponly)
                {
                    objButtonSync.gameObject.SetActive(false);
                    objButtonEvol.gameObject.SetActive(false);
                }
                else
                {
                    ButtonSync = objButtonSync.GetComponent<Button>();
                    ButtonSync.onClick.RemoveAllListeners();

                    ButtonEvol = objButtonEvol.GetComponent<Button>();
                    ButtonEvol.onClick.RemoveAllListeners();

                    objButtonSync.gameObject.SetActive(true);
                    objButtonEvol.gameObject.SetActive(false);

                    ButtonSync.interactable = true;

                    if (player_equip != null && player_equip.user_item_id == entry.user_item_id)
                    {
                        //装備中の装備の場合は合成不可
                        ButtonSync.interactable = false;
                    }
                    else if (player_equip == null || player_equip.item_id == 0)
                    {
                        //装備中の装備（ベース）が無い場合は合成不可
                        ButtonSync.interactable = false;
                    }
                    else if (player_equip.evolution == 1 && !entry.is_evol)
                    {
                        //ベースがすでに進化していて素材が進化対象でない場合は合成不可
                        ButtonSync.interactable = false;
                    }
                    else
                    {
                        if (entry.is_evol)
                        {
                            objButtonEvol.gameObject.SetActive(true);
                            objButtonSync.gameObject.SetActive(false);

                            ButtonEvol.onClick.AddListener(() =>
                            {
                                onClickButtonEvol?.Invoke(this);
                                //doEvol(entry);
                            });
                        }
                        else
                        {
                            if (entry.evolution == 1)
                            {
                                //素材がすでに進化してる場合は合成できない
                                ButtonSync.interactable = false;
                            }
                            else
                            {
                                ButtonSync.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_EQUIP_SYNCRO");
                                ButtonSync.onClick.AddListener(() =>
                                {
                                    onClickButtonSync?.Invoke(this);
                                    //doSync(entry);
                                });
                            }
                        }
                    }
                }
            }

            //捨てるボタン押下時イベントハンドラ
            if (objButtonDust != null)
            {
                if (eqponly)
                {
                    objButtonDust.gameObject.SetActive(false);
                }
                else { 
                    ButtonDust = objButtonDust.GetComponent<Button>();
                    ButtonDust.onClick.RemoveAllListeners();
                    ButtonDust.interactable = true;

                    if (entry.present_flg == 0)
                    {
                        //ButtonDust.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("ERROR_MSG_CANT_DISCARD");
                        ButtonDust.interactable = false;
                    }
                    else if (player_equip != null && player_equip.user_item_id == entry.user_item_id)
                    {
                        //装備中の場合
                        ButtonDust.interactable = false;
                    }
                    else
                    {
                        ButtonDust.onClick.AddListener(() =>
                        {
                            onClickButtonDust?.Invoke(this);
                            //doDust(entry);
                        });
                    }
                }
            }
        }
    }

    private void Awake()
    {
        //定数取得
        constants = APIConnectManager.Instance.login.constants;
    }
}
