using UnityEngine;
using UnityEngine.UI;
﻿using System.Collections;
using Kryz.CharacterStats;
using System.Collections.Generic;

[System.Serializable]
public class Item
{
    public float id;
    public Sprite icon;
    public string itemName;
    public int weaponArrayIndex;
    public string equipmentTypeName;
    //
    public ItemType type;
    public ItemRarity rarity;
    public ArmorType armorType;
    public HelmetType helmetType;
    public WeaponType weaponType;
    public ItemAffix[] affixes;
    public ItemAffix enchantmentAffix;
    //
    public int[] affixProbabilities;
    public int affixSumProbabilities;
    public List<ItemAffix> possibleAffixList;
    //
    public int maxAffixes;
    public bool hasEnchantment;
    public int craftingBaseCostConstant;
}
