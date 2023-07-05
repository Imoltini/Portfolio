using System.Collections.Generic;
﻿using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;
using I2.Loc;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    int equipAction = 0;
    int extractAction = 19;
    int equipActionAlt = 23;
    //
    float currentID;
    float pTwocurrentID;
    bool everLootedUnique;
    bool everLootedOffhand;
    [HideInInspector] public Item currentShownItem;
    [HideInInspector] public Item pTwocurrentShownItem;
    // COLOURS
    public Color minStatValueColor;
    public Color maxStatValueColor;
    public Color normalItemTypeColor;
    public Color uniqueItemTypeColor;
    public Color defaultStatValueColor;
    public Color[] itemRarityTextColor;
    public Color[] tooltipBackgroundColourIndex;
    //
    [HideInInspector] public bool recentlyExtracted;
    [HideInInspector] public bool recentlyPickedUpItem;
    [HideInInspector] public int currentShownGeneralItem;
    [HideInInspector] public bool generalTooltipOnScreen;
    //
    public TooltipController[] tooltipController;
    public EquipmentTooltipController[] equipmentController;
    //
    WaitForSeconds itemWait = new WaitForSeconds(0.5f);
    public Dictionary<float, Item> pTwoNearbyItems = new Dictionary<float, Item>();
    public Dictionary<float, Item> listOfNearbyItems = new Dictionary<float, Item>();

    //

    void Awake() => generalTooltipObject.SetActive(false);
    void Start() => GM.i.events.OnPlayerDied += HideAllTooltips;
    void Update()
    {
        if (GM.i.atStart || GM.i.inSanctuary || !GM.i.isFocused) return;
        //
        if (GM.i.systemInput.GetButtonDown(equipAction) || GM.i.systemInput.GetButtonDown(equipActionAlt)) EquipGroundItem(0);
        else if (GM.i.systemInput.GetButtonDown(extractAction)) SpawnEssences(0);
        //
        if (GM.i.isMultiplayer)
        {
            if (GM.i.pTwoInput.GetButtonDown(equipAction) || GM.i.pTwoInput.GetButtonDown(equipActionAlt)) EquipGroundItem(1);
            else if (GM.i.pTwoInput.GetButtonDown(extractAction)) SpawnEssences(1);
        }
    }
    //
    void EquipGroundItem(int pIndex)
    {
        if (!tooltipController[pIndex].onScreen) return;
        Item shownItem = pIndex == 0 ? currentShownItem : pTwocurrentShownItem;
        PlayerManager playerManager = pIndex == 0 ? GM.i.pManager : GM.i.pTwoManager;
        //
        if (shownItem.rarity == ItemRarity.Unique && !everLootedUnique) UnlockUniqueAchievement();
        if (shownItem.type == ItemType.Weapon)
        {
            // change player visuals through equipment manager
            if ((int)shownItem.weaponType <= 2) playerManager.equipment.EquipWeapon(shownItem);
            else
            {
                //offhand weapons are treated as shields and so are equipped as armor
                if (!everLootedOffhand) UnlockOffHandAchievement();
                playerManager.equipment.EquipArmor(shownItem);
            }
        }
        else
        {
            if (shownItem.armorType == ArmorType.Shield && !everLootedOffhand) UnlockOffHandAchievement();
            playerManager.equipment.EquipArmor(shownItem);
        }
        //
        // Equip item object in player inventory and apply its stats
        GM.i.events.EquippedGroundItem(shownItem.id);
        if (pIndex == 0) GM.i.ui.playerInventory.EquipItem(shownItem);
        else GM.i.ui.pTwoInventory.EquipItem(shownItem);
        //
        if (equipmentController[pIndex].onScreen) equipmentController[pIndex].HideTooltip();
        CheckIfOtherItemsToShowAreNearby(true, pIndex);
        StartCoroutine(ChangeRecentItemBool());
    }
    //
    public void FlagRecentlyPickedUpItem() => StartCoroutine(ChangeRecentItemBool());
    public IEnumerator ChangeRecentItemBool()
    {
        recentlyPickedUpItem = true;
        yield return itemWait;
        recentlyPickedUpItem = false;
    }
    //
    public void RecentlyExtractedGlyph() => StartCoroutine(ChangeExtractBools());
    void SpawnEssences(int pIndex)
    {
        if (!tooltipController[pIndex].onScreen) return;
        //
        if (equipmentController[pIndex].onScreen) equipmentController[pIndex].HideTooltip();
        GM.i.events.ExtractedEssence(currentShownItem.id);
        CheckIfOtherItemsToShowAreNearby(true, pIndex);
        StartCoroutine(ChangeExtractBools());
    }
    //
    IEnumerator ChangeExtractBools()
    {
        recentlyExtracted = true;
        yield return itemWait;
        recentlyExtracted = false;
    }
    //
    public void HideTooltip(int pNumber)
    {
        tooltipController[pNumber].HideTooltip();
        RemoveShownItemID(pNumber);
    }
    //
    public void AddCurrentShownItemID(Item item, bool isPlayerTwo)
    {
        if (isPlayerTwo)
        {
            pTwocurrentShownItem = item;
            pTwocurrentID = item.id;
        }
        else
        {
            currentShownItem = item;
            currentID = item.id;
        }
    }
    //
    void RemoveShownItemID(int pNumber)
    {
        if (pNumber == 0)
        {
            currentShownItem = null;
            currentID = -9119; // id set to an integer that will never be found naturally
        }
        else
        {
            pTwocurrentShownItem = null;
            pTwocurrentID = -1919;
        }
    }
    //
    public void RemoveItemFromList(float id, int pNumber)
    {
        equipmentController[pNumber].HideTooltip();
        listOfNearbyItems.Remove(id);
        HideTooltip(pNumber);
    }
    //
    public void CheckIfSlotHasItem(ItemType iType, ArmorType aType, bool isPlayerTwo, float tooltipOffset = 0)
    {
        var slot = isPlayerTwo ? GM.i.ui.pTwoInventory : GM.i.ui.playerInventory;
        if (iType == ItemType.Weapon)
        {
            if (aType == ArmorType.Shield) ShowEquipmentTooltip(slot.GetOffHandWeapon(), isPlayerTwo, tooltipOffset);
            else ShowEquipmentTooltip(slot.GetMainHandWeapon(), isPlayerTwo, tooltipOffset);
        }
        else
        {
            if (slot.IsTaken(aType)) ShowEquipmentTooltip(slot.GetEquippedItem(aType), isPlayerTwo, tooltipOffset);
        }
    }
    //
    public void ShowEquipmentTooltip(Item item, bool isPlayerTwo, float tooltipOffset = 0)
    {
        if (isPlayerTwo) equipmentController[1].ShowTooltip(item, tooltipOffset);
        else equipmentController[0].ShowTooltip(item, tooltipOffset);
    }
    //
    public void CheckIfItemCanBeShown(Item item, int pNumber)
    {
        float id = pNumber == 0 ? pTwocurrentID : currentID;
        if (item.id == id) return;
        //
        var itemList = pIndex == 0 ? listOfNearbyItems : pTwoNearbyItems;
        if (!itemList.ContainsKey(item.id)) itemList.Add(item.id, item);
        //
        tooltipController[pNumber].ShowTooltip(item);
    }
    //
    public bool CheckIfOtherItemsToShowAreNearby(bool removeItemFromList, int pIndex)
    {
        HideTooltip(pIndex);
        var itemList = pIndex == 0 ? listOfNearbyItems : pTwoNearbyItems;
        if (removeItemFromList) itemList.Remove(pIndex == 0 ? currentShownItem.id : pTwocurrentShownItem.id);
        //
        if (itemList.Count > 0)
        {
            foreach (Item item in itemList.Values)
            {
                tooltipController[pIndex].ShowTooltip(item);
                break;
            }
            //
            return true;
        }
        else return false;
    }
    //
    public Color GetColourBasedOnAffixValue(ItemAffix affix)
    {
        if (affix.actualValue >= affix.maxValue) return maxStatValueColor;
        else if (affix.actualValue == affix.minValue) return minStatValueColor;
        else return defaultStatValueColor;
    }
    //
    public void HideAllTooltips()
    {
        HideTooltip(0);
        HideTooltip(1);
        equipmentController[0].HideTooltip();
        equipmentController[1].HideTooltip();
    }
    //
    void UnlockOffHandAchievement()
    {
        everLootedOffhand = true;
        GM.i.steamManager.UnlockAchievement("ACH_LOOT_OFFHAND");
    }
    //
    void UnlockUniqueAchievement()
    {
        everLootedUnique = true;
        GM.i.steamManager.UnlockAchievement("ACH_LOOT_UNIQUE");
    }
    //
    public void InitializeTooltips()
    {
        currentID = -9119;
        pTwocurrentID = -1919;
        //
        tooltipController[0].InitializeAnchorsAndScale();
        equipmentController[0].InitializeAnchorsAndScale();
        //
        if (GM.i.isMultiplayer)
        {
            tooltipController[1].InitializeAnchorsAndScale();
            equipmentController[1].InitializeAnchorsAndScale();
        }
    }
    //
    void OnDisable() => GM.i.events.OnPlayerDied -= HideAllTooltips;
}
