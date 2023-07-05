using UnityEngine.UI;
﻿using UnityEngine;

[CreateAssetMenu(fileName = "New Item Object", menuName = "Items/Item Objects/New Item Object")]
public class ItemObject : ScriptableObject
{
    public string stringID;
    //
    [Space(10)]
    public Sprite itemIcon;
    public ItemType itemType;
    public ArmorType armorType;
    public ItemRarity itemRarity;
    public WeaponType weaponType;
    public HelmetType helmetType;
    public int weaponArrayIndex;
    //
    [Space(10)]
    public ItemAffix[] possibleAffixes;
    public int affixSumProbabilities;
}
