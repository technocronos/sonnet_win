/// <summary>
/// Converts legacy Main/Footer data into a final MainUIState without touching Unity runtime objects.
/// </summary>
public sealed class MainUIRuntimeAdapter
{
    public MainUIState CreateState(
        HomeApi summary,
        string currentSceneName,
        MainProfileMode profileMode,
        bool footerVisible = true,
        MainOverlay overlay = MainOverlay.None)
    {
        FooterItemState homeState = summary == null ? FooterItemState.Normal : ConvertItemState(summary.menu0State);
        FooterItemState profileState = summary == null ? FooterItemState.Normal : ConvertItemState(summary.menu1State);
        FooterItemState equipState = summary == null ? FooterItemState.Normal : ConvertItemState(summary.menu2State);
        FooterItemState gachaState = summary == null ? FooterItemState.Normal : ConvertItemState(summary.menu3State);
        FooterItemState shopState = summary == null ? FooterItemState.Normal : ConvertItemState(summary.menu4State);
        FooterItemState bookState = summary == null ? FooterItemState.Normal : ConvertItemState(summary.menu5State);

        MainNavigationDestination selectedDestination = NormalizeSelectedDestination(
            ConvertDestination(currentSceneName),
            homeState,
            profileState,
            equipState,
            gachaState,
            shopState,
            bookState);

        MainOverlay normalizedOverlay = NormalizeOverlay(
            overlay,
            profileState,
            equipState,
            shopState,
            bookState);

        return new MainUIState(
            footerVisible,
            selectedDestination,
            profileMode,
            homeState,
            gachaState,
            shopState,
            equipState,
            bookState,
            profileState,
            normalizedOverlay);
    }

    private static FooterItemState ConvertItemState(string state)
    {
        if (state == "disable")
            return FooterItemState.Disabled;

        if (state == "hot")
            return FooterItemState.Hot;

        return FooterItemState.Normal;
    }

    private static MainNavigationDestination ConvertDestination(string sceneName)
    {
        switch (sceneName)
        {
            case "Home":
                return MainNavigationDestination.Home;
            case "MyPage":
            case "HisPage":
                return MainNavigationDestination.Profile;
            case "Equip":
                return MainNavigationDestination.Equip;
            case "Gacha":
                return MainNavigationDestination.Gacha;
            case "Shop":
                return MainNavigationDestination.Shop;
            case "Book":
                return MainNavigationDestination.Book;
            case "Quest":
                return MainNavigationDestination.Quest;
            default:
                return MainNavigationDestination.None;
        }
    }

    private static MainNavigationDestination NormalizeSelectedDestination(
        MainNavigationDestination destination,
        FooterItemState homeState,
        FooterItemState profileState,
        FooterItemState equipState,
        FooterItemState gachaState,
        FooterItemState shopState,
        FooterItemState bookState)
    {
        switch (destination)
        {
            case MainNavigationDestination.Home:
                return homeState == FooterItemState.Disabled ? MainNavigationDestination.None : destination;
            case MainNavigationDestination.Profile:
                return profileState == FooterItemState.Disabled ? MainNavigationDestination.None : destination;
            case MainNavigationDestination.Equip:
                return equipState == FooterItemState.Disabled ? MainNavigationDestination.None : destination;
            case MainNavigationDestination.Gacha:
                return gachaState == FooterItemState.Disabled ? MainNavigationDestination.None : destination;
            case MainNavigationDestination.Shop:
                return shopState == FooterItemState.Disabled ? MainNavigationDestination.None : destination;
            case MainNavigationDestination.Book:
                return bookState == FooterItemState.Disabled ? MainNavigationDestination.None : destination;
            default:
                return destination;
        }
    }

    private static MainOverlay NormalizeOverlay(
        MainOverlay overlay,
        FooterItemState profileState,
        FooterItemState equipState,
        FooterItemState shopState,
        FooterItemState bookState)
    {
        switch (overlay)
        {
            case MainOverlay.Profile:
                return profileState == FooterItemState.Disabled ? MainOverlay.None : overlay;
            case MainOverlay.Equip:
                return equipState == FooterItemState.Disabled ? MainOverlay.None : overlay;
            case MainOverlay.Shop:
                return shopState == FooterItemState.Disabled ? MainOverlay.None : overlay;
            case MainOverlay.Book:
                return bookState == FooterItemState.Disabled ? MainOverlay.None : overlay;
            case MainOverlay.None:
                return MainOverlay.None;
            default:
                return MainOverlay.None;
        }
    }
}
