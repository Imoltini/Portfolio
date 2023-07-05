using UnityEngine;

[CreateAssetMenu(fileName = "New Item Treasure Class", menuName = "Items/Treasure Classes/Item Treasure Class")]
public class ItemTreasureClass : ScriptableObject
{
    [Header("Treasure Class Variables")]
    public int tcLevel;
    public string tcName;
    //
    [Header("Items and Drop Chances")]
    public ItemObject[] itemObjects;
    public int[] itemPickChance;
    //
    [Header("Probability Sums")]
    public int sumItemPickProb;

    //

    void OnValidate()
    {
        tcName = name;
        if (itemPickChance.Length > 0) CalculateSumItemPickProbability();
    }
    //
    void CalculateSumItemPickProbability()
    {
        sumItemPickProb = 0;
        for (int i = 0; i < itemPickChance.Length; i++)
        {
            sumItemPickProb += itemPickChance[i];
        }
    }
}
