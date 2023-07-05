using UnityEditor;
﻿using UnityEngine;

[CreateAssetMenu(fileName = "NewTreasureClass", menuName = "Items/Treasure Classes/Treasure Class")]
public class LootTable : ScriptableObject
{
    [Header("Treasure Class Variables")]
    public int tcLevel;
    public string tcName;
    //
    [Header("Items and Drop Chances")]
    public ItemTreasureClass[] itemTreasureClass;
    public int[] itemTreasureClassPickChance;
    //
    [Header("Calculated Values : Do Not Change")]
    public int noDropValue;
    public int maxProbability;
    //
    [Header("Value To Fill In")]
    public int noDropChance;

    //

#if UNITY_EDITOR
    //
    void CalculateValues()
    {
        int itcChance = CalculateSumItemClassPickChance();
        noDropValue = CalculateNoDropValue(itcChance);
        maxProbability = itcChance + noDropValue;
    }
    //
    int CalculateSumItemClassPickChance()
    {
        int chance = 0;
        for (int i = 0; i < itemTreasureClassPickChance.Length; i++)
        {
            chance += itemTreasureClassPickChance[i];
        }
        return chance;
    }
    //
    int CalculateNoDropValue(int itcChance)
    {
        float noDropProb = (float)noDropChance / 100;
        float denominator = 1 - noDropProb;
        //
        float newValue = (itcChance / denominator) * noDropProb;
        return (int)newValue;
    }
    //
    void OnValidate()
    {
        tcName = name;
        if (noDropChance > 0) CalculateValues();
    }
    //
#endif
}
