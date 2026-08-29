public enum MainNavigationDestination
{
    None,
    Home,
    Quest,
    Gacha,
    Shop,
    Equip,
    Book,
    Profile
}

public enum MainProfileMode
{
    Self,
    Other
}

public enum FooterItemState
{
    Disabled,
    Normal,
    Hot
}

public enum MainOverlay
{
    None,
    Shop,
    Equip,
    Book,
    Profile
}

/// <summary>
/// Immutable final UI state for the Main UI runtime.
/// This model intentionally has no Unity, scene, API, or view dependencies.
/// </summary>
public sealed class MainUIState
{
    public bool FooterVisible { get; }
    public MainNavigationDestination SelectedDestination { get; }
    public MainProfileMode ProfileMode { get; }
    public FooterItemState HomeState { get; }
    public FooterItemState GachaState { get; }
    public FooterItemState ShopState { get; }
    public FooterItemState EquipState { get; }
    public FooterItemState BookState { get; }
    public FooterItemState ProfileState { get; }
    public MainOverlay Overlay { get; }

    public MainUIState(
        bool footerVisible,
        MainNavigationDestination selectedDestination,
        MainProfileMode profileMode,
        FooterItemState homeState,
        FooterItemState gachaState,
        FooterItemState shopState,
        FooterItemState equipState,
        FooterItemState bookState,
        FooterItemState profileState,
        MainOverlay overlay)
    {
        FooterVisible = footerVisible;
        SelectedDestination = selectedDestination;
        ProfileMode = profileMode;
        HomeState = homeState;
        GachaState = gachaState;
        ShopState = shopState;
        EquipState = equipState;
        BookState = bookState;
        ProfileState = profileState;
        Overlay = overlay;
    }

    public static MainUIState CreateDefault()
    {
        return new MainUIState(
            true,
            MainNavigationDestination.None,
            MainProfileMode.Self,
            FooterItemState.Normal,
            FooterItemState.Normal,
            FooterItemState.Normal,
            FooterItemState.Normal,
            FooterItemState.Normal,
            FooterItemState.Normal,
            MainOverlay.None);
    }
}
