public class InventoryController
{
    string closeSFX = "closeBag";
    string openSFX = "openBag";
    bool pOneInInventory;
    bool pTwoInInventory;
    UIManager manager;

    //

    public InventoryController(UIManager m) => manager = m;
    public void OpenPlayerInventory(int pNumber)
    {
        if (manager.pauseOnScreen || GM.i.pManager.playerDied || manager.PlayerInMenu(pNumber)) return;
        //
        SwitchControlsAndPlayerMovement(pNumber, true, ControlType.UI);
        GM.i.audioManager.Play(GM.i.audioManager.uiSounds[openSFX]);
        TogglePlayerInventory(pNumber, true);
        //
        if (!GM.i.isMultiplayer)
        {
            manager.invOnScreen = true;
            manager.inSubMenu = true;
        }
    }
    //
    public void ClosePlayerInventory(int pNumber)
    {
        if (PlayerIsntInInventory(pNumber)) return;
        //
        SwitchControlsAndPlayerMovement(pNumber, false, ControlType.Gameplay);
        GM.i.audioManager.Play(GM.i.audioManager.uiSounds[closeSFX]);
        TogglePlayerInventory(pNumber, false);
        //
        if (!GM.i.isMultiplayer)
        {
            manager.invOnScreen = false;
            manager.inSubMenu = false;
        }
    }
    //
    void SwitchControlsAndPlayerMovement(int pNumber, bool status, ControlType controlType)
    {
        if (pNumber == 0) GM.i.pManager.movement.ToggleBlockedPlayer(status);
        else GM.i.pTwoManager.movement.ToggleBlockedPlayer(status);
        GM.i.ToggleControls(controlType, pNumber);
    }
    //
    void TogglePlayerInventory(int pNumber, bool on)
    {
        var inventory = pNumber == 0 ? manager.playerInventory : manager.pTwoInventory;
        if (on) inventory.OpenInventoryWindow();
        else inventory.CloseInventoryWindow();
        //
        if (pNumber == 0)
        {
            manager.playerInventory.canScroll = on;
            manager.inventoryCanvas.enabled = on;
            manager.pOneInMenu = on;
            pOneInInventory = on;
        }
        else
        {
            manager.pTwoInventoryCanvas.enabled = on;
            manager.pTwoInventory.canScroll = on;
            manager.pTwoInMenu = on;
            pTwoInInventory = on;
        }
        //
        if (!GM.i.isMultiplayer) ChangeDepthOfField(on);
        GM.i.events.PressedInventory(on, pNumber);
    }
    //
    void ChangeDepthOfField(bool on)
    {
        if (on)
        {
            GM.i.camScript.ChangeCameraAngle(true, GM.i.camScript.originalRotation, manager.inventoryCameraOffset);
            if (!GM.i.inSanctuary) GM.i.ChangeDepthOfField(3.5f);
        }
        else
        {
            GM.i.camScript.ChangeCameraAngle(false, GM.i.camScript.originalRotation, manager.inventoryCameraOffset);
            if (!GM.i.inSanctuary) GM.i.ResetDepthOfField(7.5f);
        }
    }
    //
    bool PlayerIsntInInventory(int pNumber)
    {
        if (pNumber == 0)
        {
            if (pOneInInventory) return false;
            else return true;
        }
        else
        {
            if (pTwoInInventory) return false;
            else return true;
        }
    }
}
