using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Projects a final MainUIState onto the existing Main scene footer hierarchy.
/// This class owns no navigation, API, preference, scene, or animation behavior.
/// </summary>
public sealed class FooterView
{
    private sealed class Slot
    {
        public readonly GameObject Root;
        public readonly Button Button;
        public readonly Image Background;
        public readonly Image BackgroundSelect;
        public readonly Image BackgroundDisabled;
        public readonly GameObject Batch;

        public Slot(
            GameObject root,
            Button button,
            Image background,
            Image backgroundSelect,
            Image backgroundDisabled,
            GameObject batch)
        {
            Root = root;
            Button = button;
            Background = background;
            BackgroundSelect = backgroundSelect;
            BackgroundDisabled = backgroundDisabled;
            Batch = batch;
        }
    }

    private readonly GameObject footerRoot;
    private readonly Slot home;
    private readonly Slot myPage;
    private readonly Slot hisPage;
    private readonly Slot equip;
    private readonly Slot gacha;
    private readonly Slot shop;
    private readonly Slot book;

    public FooterView(Footer footer)
    {
        if (footer == null)
            throw new ArgumentNullException(nameof(footer));

        footerRoot = footer.FooterBase;
        if (footerRoot == null)
            throw new InvalidOperationException("Footer.FooterBase is not assigned.");

        home = CreateSlot("Home", footer.Menu0);
        myPage = CreateSlot("MyPage", footer.Menu1);
        hisPage = CreateSlot("HisPage", FindButton("HisPage"));
        equip = CreateSlot("Equip", footer.Menu2);
        gacha = CreateSlot("Gacha", footer.Menu3);
        shop = CreateSlot("Shop", footer.Menu4);
        book = CreateSlot("Book", footer.Menu5);
    }

    /// <summary>
    /// Applies the complete footer presentation for a final UI state.
    /// </summary>
    public void Apply(MainUIState state)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));

        SetPhysicalSlotsActive(state.ProfileMode);

        Render(home, state.HomeState, state.SelectedDestination == MainNavigationDestination.Home);
        Render(myPage, state.ProfileState,
            state.ProfileMode == MainProfileMode.Self && state.SelectedDestination == MainNavigationDestination.Profile);
        Render(hisPage, state.ProfileState,
            state.ProfileMode == MainProfileMode.Other && state.SelectedDestination == MainNavigationDestination.Profile);
        Render(equip, state.EquipState, state.SelectedDestination == MainNavigationDestination.Equip);
        Render(gacha, state.GachaState, state.SelectedDestination == MainNavigationDestination.Gacha);
        Render(shop, state.ShopState, state.SelectedDestination == MainNavigationDestination.Shop);
        Render(book, state.BookState, state.SelectedDestination == MainNavigationDestination.Book);

        // FooterView is a regular C# class, so this root can safely be reactivated by a later Apply call.
        footerRoot.SetActive(state.FooterVisible);
    }

    private void SetPhysicalSlotsActive(MainProfileMode profileMode)
    {
        home.Root.SetActive(true);
        myPage.Root.SetActive(profileMode == MainProfileMode.Self);
        hisPage.Root.SetActive(profileMode == MainProfileMode.Other);
        equip.Root.SetActive(true);
        gacha.Root.SetActive(true);
        shop.Root.SetActive(true);
        book.Root.SetActive(true);
    }

    private static void Render(Slot slot, FooterItemState itemState, bool selected)
    {
        bool disabled = itemState == FooterItemState.Disabled;
        bool selectedVisible = !disabled && selected;

        slot.Button.interactable = !disabled;
        slot.BackgroundDisabled.gameObject.SetActive(disabled);
        slot.BackgroundSelect.gameObject.SetActive(selectedVisible);
        slot.Background.gameObject.SetActive(!disabled && !selectedVisible);
        slot.Batch.SetActive(!disabled && itemState == FooterItemState.Hot);
        slot.Button.targetGraphic = disabled
            ? slot.BackgroundDisabled
            : selectedVisible
                ? slot.BackgroundSelect
                : slot.Background;
    }

    private Button FindButton(string slotName)
    {
        Transform slotTransform = footerRoot.transform.Find(slotName);
        if (slotTransform == null)
            throw new InvalidOperationException("FooterBase is missing the '" + slotName + "' slot.");

        Button button = slotTransform.GetComponent<Button>();
        if (button == null)
            throw new InvalidOperationException("Footer slot '" + slotName + "' is missing its Button component.");

        return button;
    }

    private Slot CreateSlot(string slotName, Button button)
    {
        if (button == null)
            throw new InvalidOperationException("Footer slot '" + slotName + "' is not assigned.");

        if (button.transform.parent != footerRoot.transform)
            throw new InvalidOperationException("Footer slot '" + slotName + "' is not a direct child of FooterBase.");

        return new Slot(
            button.gameObject,
            button,
            FindImage(button.transform, "Background", slotName),
            FindImage(button.transform, "BackgroundSelect", slotName),
            FindImage(button.transform, "BackgroundDisabled", slotName),
            FindGameObject(button.transform, "Batch", slotName));
    }

    private static Image FindImage(Transform slotTransform, string childName, string slotName)
    {
        Transform child = FindChild(slotTransform, childName, slotName);
        Image image = child.GetComponent<Image>();
        if (image == null)
            throw new InvalidOperationException("Footer slot '" + slotName + "/" + childName + "' is missing its Image component.");

        return image;
    }

    private static GameObject FindGameObject(Transform slotTransform, string childName, string slotName)
    {
        return FindChild(slotTransform, childName, slotName).gameObject;
    }

    private static Transform FindChild(Transform slotTransform, string childName, string slotName)
    {
        Transform child = slotTransform.Find(childName);
        if (child == null)
            throw new InvalidOperationException("Footer slot '" + slotName + "' is missing '" + childName + "'.");

        return child;
    }
}
