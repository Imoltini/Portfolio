using System.Collections.Generic;
﻿using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;
using I2.Loc;
using TMPro;

public class TooltipManager : MonoBehaviour
{
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
    [Header("General Tooltip Variables")]
    public Image generalItemIcon;
    public GameObject glyphExtractPrompt;
    public GameObject generalTooltipObject;
    public TextMeshProUGUI generalItemName;
    public TextMeshProUGUI generalItemType;
    public TextMeshProUGUI generalItemDescription;
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
        if (GM.i.systemInput.GetButtonDown(0) || GM.i.systemInput.GetButtonDown(23)) EquipGroundItem(0);
        if (GM.i.systemInput.GetButtonDown(19)) SpawnEssences(0);
        //
        if (GM.i.isMultiplayer)
        {
            if (GM.i.systemInput.GetButtonDown(0) || GM.i.systemInput.GetButtonDown(23)) EquipGroundItem(1);
            if (GM.i.systemInput.GetButtonDown(19)) SpawnEssences(1);
        }
    }
    //
    void EquipGroundItem(int pIndex)
    {
        if (!tooltipController[pIndex].onScreen) return;
        Item shownItem = pIndex == 0 ? currentShownItem : pTwocurrentShownItem;
        //
        if (currentShownItem.rarity == ItemRarity.Unique && !everLootedUnique) UnlockUniqueAchievement();
        if (shownItem.type == ItemType.Weapon)
        {
            if ((int)currentShownItem.weaponType <= 2) GM.i.pManager.equipment.EquipWeapon(currentShownItem);
            else
            {
                //offhand weapons are treated as shields and so are equipped as armor
                GM.i.pManager.equipment.EquipArmor(currentShownItem);
                if (!everLootedOffhand) UnlockOffHandAchievement();
            }
        }
        else
        {
            GM.i.pManager.equipment.EquipArmor(currentShownItem);
            if (currentShownItem.armorType == ArmorType.Shield && !everLootedOffhand) UnlockOffHandAchievement();
        }
        //
        GM.i.events.EquippedGroundItem(currentShownItem.id);
        GM.i.ui.playerInventory.EquipItem(currentShownItem);
        //
        if (equipmentController[0].onScreen && !GM.i.ui.invOnScreen) equipmentController[0].HideTooltip();
        StartCoroutine(ChangeRecentItemBool());
        CheckIfOtherItemsToShowAreNearby();
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
        Item shownItem = pIndex == 0 ? currentShownItem : pTwocurrentShownItem;
        //
        if (equipmentController[pIndex].onScreen) equipmentController[pIndex].HideTooltip();
        GM.i.events.ExtractedEssence(showItem.id);
        StartCoroutine(ChangeExtractBools());
        CheckIfOtherItemsToShowAreNearby();
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
    public void CheckIfInventorySlotHasItem(ItemType iType, ArmorType aType, bool isPlayerTwo, float tooltipOffset = 0)
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
        if (pNumber == 0 && !listOfNearbyItems.ContainsKey(item.id)) listOfNearbyItems.Add(item.id, item);
        if (pNumber == 1 && !pTwoNearbyItems.ContainsKey(item.id)) pTwoNearbyItems.Add(item.id, item);
        //
        tooltipController[pNumber].ShowTooltip(item);
    }
    //
    public bool CheckIfOtherItemsToShowAreNearby(bool removeItemFromList = true)
    {
        if (removeItemFromList) listOfNearbyItems.Remove(currentShownItem.id);
        //
        HideTooltip(0);
        if (listOfNearbyItems.Count > 0)
        {
            foreach (Item item in listOfNearbyItems.Values)
            {
                tooltipController[0].ShowTooltip(item);
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
    public void ShowGeneralTooltip(GeneralItemObject generalItem, int itemID)
    {
        if (!GM.i.ui.isMainCanvasOn) return;
        //
        currentShownGeneralItem = itemID;
        glyphExtractPrompt.SetActive(false);
        generalItemName.text = generalItem.itemName;
        generalItemIcon.sprite = generalItem.itemIcon;
        generalItemDescription.text = LocalizationManager.GetTranslation(generalItem.localizedTerm);
        if (generalItem.itemType == ItemType.Artifact) generalItemType.text = LocalizationManager.GetTranslation("artifact");
        else if (generalItem.itemType == ItemType.DominationRune) generalItemType.text = LocalizationManager.GetTranslation("artifact");
        else if (generalItem.itemType == ItemType.Cosmetics)
        {
            if (generalItem.cosmeticsType == CosmeticsType.Head) generalItemType.text = LocalizationManager.GetTranslation("attachment");
            else if (generalItem.cosmeticsType == CosmeticsType.Wings) generalItemType.text = LocalizationManager.GetTranslation("wings");
            else if (generalItem.cosmeticsType == CosmeticsType.Outfit)
            {
                generalItemType.text = LocalizationManager.GetTranslation("outfit");
                if (LocalizationManager.CurrentLanguageCode.Equals("en", StringComparison.Ordinal)) generalItemDescription.text = generalItem.itemDescription;
            }
            else if (generalItem.cosmeticsType == CosmeticsType.Sanctuary) generalItemType.text = LocalizationManager.GetTranslation("sanctuary");
            else if (generalItem.cosmeticsType == CosmeticsType.Companion) generalItemType.text = LocalizationManager.GetTranslation("companion");
        }
        else
        {
            string val = generalItem.glyphAffix.actualValue.ToString();
            string aff = LocalizationManager.GetTranslation(generalItem.glyphAffix.affixIndex);
            string translation = generalItem.glyphAffix.isPercent ? LocalizationManager.GetTranslation("glyphPercent") : LocalizationManager.GetTranslation("glyphAffix");
            string formattedTranslation = string.Format(translation, val, aff);
            generalItemDescription.text = formattedTranslation;
            //
            glyphExtractPrompt.SetActive(true);
            generalItemType.text = "ENCHANTING";
        }
        //
        generalTooltipObject.SetActive(true);
        generalTooltipOnScreen = true;
        //
        equipmentController[0].HideTooltip();
        HideTooltip(0);
    }
    //
    public void HideGeneralTooltip()
    {
        DisableGeneralTooltip();
        //
        if (listOfNearbyItems.Count > 0)
        {
            foreach (Item item in listOfNearbyItems.Values)
            {
                tooltipController[0].ShowTooltip(item);
                break;
            }
        }
    }
    //
    public void DisableGeneralTooltip()
    {
        currentShownGeneralItem = -1;
        generalTooltipOnScreen = false;
        generalTooltipObject.SetActive(false);
    }
    //
    public void HideAllTooltips()
    {
        HideTooltip(0);
        DisableGeneralTooltip();
        equipmentController[0].HideTooltip();
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
