using KnightCrawlers.Extensions;
using UnityEngine.UI;
using UnityEngine;
using I2.Loc;
using System;
using TMPro;

public class EquipmentTooltipController : MonoBehaviour
{
    int pIndex;
    string prc = "prc";
    string bnc = "bnc";
    public bool isPlayerTwo;
    TooltipController tooltip;
    [HideInInspector] public bool onScreen;
    //
    public Image background;
    public RectTransform rect;
    public TextMeshProUGUI eName;
    public TextMeshProUGUI aType;
    public TextMeshProUGUI rarity;
    public TextMeshProUGUI[] stat;
    public TextMeshProUGUI enchantStat;
    //
    public Image icon;
    public Image iconFrame;
    //
    public GameObject wepDivider;
    public GameObject enchantDivider;
    public Vector2 multiplayerPosition;
    public Vector2 inventoryOpenPosition;
    Vector2 singlePlayerPosition = new Vector2(-634, -91);

    //

    void Awake() => GM.i.events.OnInventoryPressed += DetermineTooltipPosition;
    void Start()
    {
        pIndex = isPlayerTwo ? 1 : 0;
        tooltip = GM.i.tooltipManager.tooltipController[pIndex];
        //
        gameObject.SetActive(false);
    }
    //
    public void ShowTooltip(Item item, float tooltipOffset = 0)
    {
        if (!GM.i.ui.isMainCanvasOn) return;
        SetTextAndBackgroundColours((int)item.rarity);
        SetTooltipAffixes(item);
        //
        if (item.type == ItemType.Weapon) SetWeaponText(item);
        else SetArmorText(item);
        //
        if (item.hasEnchantment) SetUpEnchantmentTooltip(item.enchantmentAffix);
        else DisableEnchantmentExtras();
        //
        onScreen = true;
        icon.sprite = item.icon;
        CheckPosition(tooltipOffset);
        rect.gameObject.SetActive(true);
    }
    //
    void SetTextAndBackgroundColours(int index)
    {
        eName.color = GM.i.tooltipManager.itemRarityTextColor[index];
        rarity.color = GM.i.tooltipManager.itemRarityTextColor[index];
        iconFrame.color = GM.i.tooltipManager.itemRarityTextColor[index];
        background.color = GM.i.tooltipManager.tooltipBackgroundColourIndex[index];
        aType.color = index == 5 ? GM.i.tooltipManager.uniqueItemTypeColor : GM.i.tooltipManager.normalItemTypeColor;
    }
    //
    void SetWeaponText(Item item)
    {
        wepDivider.SetActive(true);
        eName.text = item.itemName.AllCaps();
        aType.text = item.equipmentTypeName.AllCaps();
        rarity.text = $"{item.rarity.ToString().AllCaps()} {item.weaponType.ToString().AllCaps()}";
    }
    //
    void SetArmorText(Item item)
    {
        if (item.type == ItemType.Offhand)
        {
            wepDivider.SetActive(true);
            eName.text = item.itemName.AllCaps();
            aType.text = item.equipmentTypeName.AllCaps();
            rarity.text = $"<color=#DCC>OFF-HAND</color> {item.weaponType.ToString().AllCaps()}";
        }
        else
        {
            wepDivider.SetActive(false);
            eName.text = item.itemName.AllCaps();
            aType.text = (int)item.rarity == 5 ? "SINFUL ARMOR" : item.equipmentTypeName.AllCaps();
            rarity.text = $"{item.rarity.ToString().AllCaps()} {item.armorType.ToString().AllCaps()}";
        }
    }
    //
    void SetUpEnchantmentTooltip(ItemAffix affix)
    {
        enchantDivider.SetActive(true);
        enchantStat.gameObject.SetActive(true);
        //
        LocalizedString localizedAffix = affix.affixIndex;
        enchantStat.text = affix.isPercent ? $"+{affix.actualValue}% {localizedAffix}" : $"+{affix.actualValue} {localizedAffix}";
    }
    //
    void DisableEnchantmentExtras()
    {
        enchantStat.gameObject.SetActive(false);
        enchantDivider.SetActive(false);
    }
    //
    void SetTooltipAffixes(Item item)
    {
        LocalizedString localizedAffix;
        int length = item.affixes.Length;
        for (int i = 0; i < stat.Length; i++)
        {
            if (i < length)
            {
                var aff = item.affixes[i];
                //
                localizedAffix = aff.affixIndex;
                stat[i].color = GM.i.tooltipManager.GetColourBasedOnAffixValue(aff);
                //
                if (aff.affixIndex.Equals(prc, StringComparison.Ordinal) || aff.affixIndex.Equals(bnc, StringComparison.Ordinal)) stat[i].text = localizedAffix;
                else
                {
                    if (aff.isPercent) stat[i].text = $"+{aff.actualValue}% {localizedAffix}";
                    else stat[i].text = $"+{aff.actualValue} {localizedAffix}";
                }
                stat[i].gameObject.SetActive(true);
            }
            //
            else stat[i].gameObject.SetActive(false);
        }
    }
    //
    void DetermineTooltipPosition(bool inventoryOnScreen, int pNumber)
    {
        if (pNumber != pIndex) return;
        //
        ChangePositionOnScreen(pNumber, inventoryOnScreen);
        if (!inventoryOnScreen && !GM.i.tooltipManager.CheckIfOtherItemsToShowAreNearby(false, pIndex)) gameObject.SetActive(false);
    }
    //
    public void ChangePositionOnScreen(int pNumber, bool invOnScreen, float offset = 0)
    {
        if (GM.i.isMultiplayer)
        {
            if (pNumber == pIndex) SetMultiplayerPosition(pNumber, invOnScreen, offset);
            return;
        }
        //
        if (!invOnScreen)
        {
            rect.pivot = Vector2.zero;
            Vector3 xOffset = new Vector3(tooltip.rect.rect.width + 35, 0, 0);
            rect.localPosition = tooltip.rect.localPosition + xOffset;
        }
        else
        {
            rect.pivot = Vector2.one;
            rect.anchoredPosition = singlePlayerPosition;
        }
    }
    //
    void SetMultiplayerPosition(int pNumber, bool invOnScreen, float offset)
    {
        float tooltipOffset = tooltip.rect.rect.height + 20;
        //
        if (invOnScreen)
        {
            if (tooltip.onScreen) rect.anchoredPosition = new Vector2(inventoryOpenPosition.x, inventoryOpenPosition.y + tooltipOffset);
            else rect.anchoredPosition = inventoryOpenPosition;
        }
        else rect.anchoredPosition = new Vector2(multiplayerPosition.x, multiplayerPosition.y + tooltipOffset);
    }
    //
    void CheckPosition(float offset)
    {
        if (offset == 0) Canvas.ForceUpdateCanvases();
        offset = GM.i.isMultiplayer ? rect.rect.height : rect.rect.width;
        ChangePositionOnScreen(pIndex, GM.i.ui.PlayerInMenu(pIndex), offset);
    }
    //
    public void HideTooltip()
    {
        onScreen = false;
        gameObject.SetActive(false);
    }
    //
    public void InitializeAnchorsAndScale()
    {
        if (GM.i.isMultiplayer)
        {
            Vector2 multiplayerAnchorPoint = isPlayerTwo ? new Vector2(1, 0) : Vector2.zero;
            rect.pivot = rect.anchorMin = rect.anchorMax = multiplayerAnchorPoint;
            rect.localScale = Vector3.one * 1.03f;
        }
        else
        {
            rect.pivot = rect.anchorMin = rect.anchorMax = Vector2.one;
            rect.localScale = Vector3.one * 1.05f;
        }
    }
    void OnDestroy() => GM.i.events.OnInventoryPressed -= DetermineTooltipPosition;
}
