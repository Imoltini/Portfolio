/*
This is an editor script used to create scriptable objects for items from a csv file.
Objects are created and saved in directories related to their item or base type.
Performance is not a priority as this is used to create a large amount of scriptable objects
without needing to manually copy and paste values from a database.
*/

using System.Collections.Generic;
using KnightCrawlers.Data;
﻿using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;

public static class ItemBaseSOGenerator
{
    // Lists
    static int probHolder = 0;
    static List<int> tempProbabilityHolder = new List<int>();
    static List<ItemAffix> tempAffixHolder = new List<ItemAffix>();

    // Unique CSV Path
    static string uniqueEquipmentCSVPath = "Assets/Utility/CSVs/ItemCSVs/Uniques.csv";

    // Base Armor CSV Paths
    static string shouldersCSVPath = "Assets/Utility/CSVs/ItemCSVs/Shoulders.csv";
    static string shieldsCSVPath = "Assets/Utility/CSVs/ItemCSVs/Shields.csv";
    static string weaponsCSVPath = "Assets/Utility/CSVs/ItemCSVs/Weapons.csv";
    static string helmetsCSVPath = "Assets/Utility/CSVs/ItemCSVs/Helms.csv";
    static string chestsCSVPath = "Assets/Utility/CSVs/ItemCSVs/Chests.csv";
    static string glovesCSVPath = "Assets/Utility/CSVs/ItemCSVs/Gloves.csv";
    static string bootsCSVPath = "Assets/Utility/CSVs/ItemCSVs/Boots.csv";
    static string ringsCSVPath = "Assets/Utility/CSVs/ItemCSVs/Rings.csv";
    static string pantsCSVPath = "Assets/Utility/CSVs/ItemCSVs/Pants.csv";

    [MenuItem("Knight Crawlers/Generate Items/Generate Boots")] public static void GenerateBoots() => GenerateBaseItems(bootsCSVPath, 1);
    [MenuItem("Knight Crawlers/Generate Items/Generate Rings")] public static void GenerateRings() => GenerateBaseItems(ringsCSVPath, 4);
    [MenuItem("Knight Crawlers/Generate Items/Generate Pants")] public static void GeneratePants() => GenerateBaseItems(pantsCSVPath, 0);
    [MenuItem("Knight Crawlers/Generate Items/Generate Gloves")] public static void GenerateGloves() => GenerateBaseItems(glovesCSVPath, 2);
    [MenuItem("Knight Crawlers/Generate Items/Generate Chests")] public static void GenerateChests() => GenerateBaseItems(chestsCSVPath, 6);
    [MenuItem("Knight Crawlers/Generate Items/Generate Shields")] public static void GenerateShields() => GenerateBaseItems(shieldsCSVPath, 7);
    [MenuItem("Knight Crawlers/Generate Items/Generate Helmets")] public static void GenerateHelmets() => GenerateBaseItems(helmetsCSVPath, 3);
    [MenuItem("Knight Crawlers/Generate Items/Generate Shoulders")] public static void GenerateShoulders() => GenerateBaseItems(shouldersCSVPath, 5);
    //
    [MenuItem("Knight Crawlers/Generate Unique Items")] public static void GenerateUniques() => GenerateUniqueItems();
    [MenuItem("Knight Crawlers/Generate Items/Generate Weapons")] public static void GenerateWeapons() => GenerateWeaponItems();

    //

    static void GenerateBaseItems(string path, int armorType)
    {
        int rarity = 0;
        tempAffixHolder.Clear();
        tempProbabilityHolder.Clear();
        string[] file = File.ReadAllLines(path);
        int length = file.Length;
        //
        // for loop starts at 1 as the item's name is in the first column of the CSV and is not needed
        for (int i = 1; i < length; i++)
        {
            ItemObject itemObject = ScriptableObject.CreateInstance<ItemObject>();
            string[] data = file[i].Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
            //
            SetItemVariables(itemObject, data, armorType, rarity);
            CreateItem(itemObject, data[1]);
            SetAffixVariables(itemObject, data);
            OrderAndSetAffixArray(itemObject);
            EditorUtility.SetDirty(itemObject);
            rarity++;
            //
            // Item bases have four rarities and therefore every fourth line in the CSV
            // file contains a different item base and the holders need to be cleared for a new base
            if (i % 4 == 0)
            {
                rarity = 0;
                probHolder = 0;
                tempAffixHolder.Clear();
                tempProbabilityHolder.Clear();
            }
        }
        //
        AssetDatabase.SaveAssets();
    }
    //
    static void GenerateWeaponItems()
    {
        tempAffixHolder.Clear();
        tempProbabilityHolder.Clear();
        string[] file = File.ReadAllLines(weaponsCSVPath);
        int length = file.Length;
        //
        for (int i = 1; i < length; i++)
        {
            ItemObject itemObject = ScriptableObject.CreateInstance<ItemObject>();
            string[] data = file[i].Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
            //
            SetWeaponItemVariables(itemObject, data);
            CreateWeaponItem(itemObject, data[1]);
            SetAffixVariables(itemObject, data, true);
            EditorUtility.SetDirty(itemObject);
        }
        //
        AssetDatabase.SaveAssets();
    }
    //
    static void GenerateUniqueItems()
    {
        tempAffixHolder.Clear();
        tempProbabilityHolder.Clear();
        string[] s = File.ReadAllLines(uniqueEquipmentCSVPath);
        int length = s.Length;
        //
        for (int i = 1; i < length; i++)
        {
            ItemObject itemObject = ScriptableObject.CreateInstance<ItemObject>();
            string[] data = s[i].Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
            //
            SetUniqueItemVariables(itemObject, data);
            CreateUniqueItem(itemObject, data[1]);
            SetAffixVariables(itemObject, data, true);
            EditorUtility.SetDirty(itemObject);
        }
        //
        AssetDatabase.SaveAssets();
    }
    //
    static void SetWeaponItemVariables(ItemObject itemObject, string[] data)
    {
        itemObject.stringID = data[0];
        itemObject.itemType = ItemType.Weapon;
        itemObject.itemRarity = ItemRarity.Unique;
        itemObject.weaponType = (WeaponType)int.Parse(data[2]);
        itemObject.itemIcon = SetWeaponSprite(itemObject.stringID);
        //
        itemObject.possibleAffixes = new ItemAffix[int.Parse(data[3])];
    }
    //
    static void SetUniqueItemVariables(ItemObject itemObject, string[] data)
    {
        itemObject.stringID = data[0];
        itemObject.itemType = ItemType.Equipment;
        itemObject.itemRarity = ItemRarity.Unique;
        itemObject.armorType = (ArmorType)int.Parse(data[2]);
        itemObject.itemIcon = SetSprite(itemObject.armorType.ToString(), itemObject.stringID);
        //
        itemObject.possibleAffixes = new ItemAffix[int.Parse(data[3])];
    }
    //
    static void SetItemVariables(ItemObject itemObject, string[] data, int armorType, int rarity)
    {
        itemObject.stringID = data[0];
        itemObject.itemType = ItemType.Equipment;
        //
        // ArmorType and Rarity are saved in the database as integers instead of strings
        // as integer comparison is faster and strings can be misspelt
        itemObject.armorType = (ArmorType)armorType;
        itemObject.itemRarity = (ItemRarity)rarity;
        itemObject.itemIcon = SetSprite(itemObject.armorType.ToString(), itemObject.stringID);
        //
        itemObject.possibleAffixes = new ItemAffix[SetPossibleAffixArraySize(int.Parse(data[2]))];
    }
    //
    static Sprite SetSprite(string armorType, string equipmentType) => Resources.Load<Sprite>($"Items/Icons/{armorType}/{equipmentType}");
    static Sprite SetWeaponSprite(string iconName) => Resources.Load<Sprite>($"Items/Icons/Weapons/{iconName}");
    static int SetPossibleAffixArraySize(int size) => tempAffixHolder.Count > 0 ? size + tempAffixHolder.Count : size;
    static void SetAffixVariables(ItemObject itemObject, string[] data, bool isUnique = false)
    {
        if (tempAffixHolder.Count > 0) PopulateAffixesFromHolderList(itemObject);
        int index = tempAffixHolder.Count > 0 ? tempAffixHolder.Count : 0;
        //
        // item affixes in the CSV file start at index 3 for non uniques and index 4 for uniques
        int startingIndex = isUnique ? 4 : 3;
        for (int i = startingIndex; i < data.Length - 2; i += 4)
        {
            var affix = itemObject.possibleAffixes[index];
            //
            affix.affixIndex = data[i];
            affix.affixName = AffixDescHolder.GetItemAffixString(affix.affixIndex);
            affix.isPercent = CheckIfIsPercent(affix.affixIndex);
            affix.minValue = int.Parse(data[i+1]);
            affix.maxValue = int.Parse(data[i+2]);
            affix.probability = int.Parse(data[i+3]);
            index++;
            //
            if (!isUnique)
            {
                tempAffixHolder.Add(affix);
                probHolder += affix.probability;
                tempProbabilityHolder.Add(affix.probability);
            }
        }
        //
        if (!isUnique) itemObject.affixSumProbabilities = probHolder;
    }
    //
    static bool CheckIfIsPercent(string n)
    {
        if (n.StartsWith("e", StringComparison.Ordinal) || n.StartsWith("c", StringComparison.Ordinal)) return true;
        else if (n.StartsWith("d", StringComparison.Ordinal))
        {
            if (n.Equals("dmg", StringComparison.Ordinal) || n.Equals("dmgl", StringComparison.Ordinal) || n.Equals("def", StringComparison.Ordinal) || n.Equals("defp", StringComparison.Ordinal) || n.Equals("defh", StringComparison.Ordinal) || n.Equals("defl", StringComparison.Ordinal)) return false;
            else return true;
        }
        else if (n.Equals("mf", StringComparison.Ordinal) || n.Equals("rcc", StringComparison.Ordinal) || n.Equals("xpm", StringComparison.Ordinal)) return true;
        else return false;
    }
    //
    static void PopulateAffixesFromHolderList(ItemObject itemObject)
    {
        var array = itemObject.possibleAffixes;
        //
        for (int i = 0; i < tempAffixHolder.Count; i++)
        {
            array[i].probability = tempProbabilityHolder[i];
            array[i].affixIndex = tempAffixHolder[i].affixIndex;
            array[i].affixName = tempAffixHolder[i].affixName;
            array[i].isPercent = tempAffixHolder[i].isPercent;
            array[i].minValue = tempAffixHolder[i].minValue;
            array[i].maxValue = tempAffixHolder[i].maxValue;
        }
    }
    //
    static void OrderAndSetAffixArray(ItemObject itemObject)
    {
        List<ItemAffix> tempAffixList = new List<ItemAffix>();
        for (int i = 0; i < itemObject.possibleAffixes.Length; i++)
        {
            tempAffixList.Add(itemObject.possibleAffixes[i]);
        }
        //
        tempAffixList = tempAffixList.OrderByDescending(affix => affix.probability).ToList();
        itemObject.possibleAffixes = new ItemAffix[tempAffixList.Count];
        //
        for (int i = 0; i < tempAffixList.Count; i++)
        {
            itemObject.possibleAffixes[i] = tempAffixList[i];
        }
        //
        tempAffixList = null;
    }
    //
    static void CreateItem(ItemObject itemObject, string index)
    {
        string assetName = $"{index}_{itemObject.itemRarity.ToString()}_{itemObject.stringID.ToString()}";
        string pathName = $"Assets/Resources/Items/Base/{itemObject.armorType.ToString()}";
        string folderName = itemObject.stringID.ToString();
        //
        if (!AssetDatabase.IsValidFolder($"{pathName}/{folderName}")) AssetDatabase.CreateFolder(pathName, folderName);
        AssetDatabase.CreateAsset(itemObject, $"{pathName}/{folderName}/{assetName}.asset");
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(itemObject));
    }
    //
    static void CreateUniqueItem(ItemObject itemObject, string index)
    {
        string assetName = $"{index}_{itemObject.stringID.ToString()}";
        string pathName = $"Assets/Resources/Items";
        string folderName = "Uniques";
        //
        AssetDatabase.CreateAsset(itemObject, $"{pathName}/{folderName}/{assetName}.asset");
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(itemObject));
    }
    //
    static void CreateWeaponItem(ItemObject itemObject, string index)
    {
        string assetName = $"{index}_{itemObject.stringID.ToString()}_{itemObject.weaponType.ToString()}";
        string folderName = itemObject.weaponType.ToString();
        string pathName = $"Assets/Resources/Items/Weapons";
        //
        if (!AssetDatabase.IsValidFolder($"{pathName}/{folderName}")) AssetDatabase.CreateFolder(pathName, folderName);
        AssetDatabase.CreateAsset(itemObject, $"{pathName}/{folderName}/{assetName}.asset");
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(itemObject));
    }
}
