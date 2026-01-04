using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    public MyPage MyPage;
    public Equip Equip;
    public Shop Shop;
    public Book Book;
    public Quest Quest;

    public Button MyPageButton;
    public Button EquipButton;
    public Button ShopButton;
    public Button BookButton;

    // Start is called before the first frame update
    void Start()
    {
        MyPage.gameObject.SetActive(false);
        Equip.gameObject.SetActive(false);
        Shop.gameObject.SetActive(false);
        Book.gameObject.SetActive(false);

        MyPageButton.onClick.AddListener(onClickMyPage);
        EquipButton.onClick.AddListener(onClickEquip);
        ShopButton.onClick.AddListener(onClickShop);
        BookButton.onClick.AddListener(onClickBook);
    }

    public void onClickMyPage()
    {
        AudioManager.Instance.PlaySE("se_btn");
        
        MyPage.gameObject.SetActive(true);
        MyPage.show();
    }
    public void onClickEquip()
    {
        AudioManager.Instance.PlaySE("se_btn");

        Equip.gameObject.SetActive(true);
        Equip.show();
    }
    public void onClickShop()
    {
        AudioManager.Instance.PlaySE("se_btn");
        Shop.gameObject.SetActive(true);
        Shop.show();
    }
    public void onClickBook()
    {
        AudioManager.Instance.PlaySE("se_btn");

        Book.gameObject.SetActive(true);
        Book.show();
    }
}
