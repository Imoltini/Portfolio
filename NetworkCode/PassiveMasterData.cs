using UnityEngine;
using System.IO;

public enum PassiveType { Defensive = 0, Offensive, Unique, Hybrid }

[CreateAssetMenu(fileName = "PassiveMasterData", menuName = "ScriptableObjects/PassiveMasterData")]
public class PassiveMasterData : GenericScriptableObject<PassiveMasterData>
{
    public SerializableDictionary<int, PassiveData> passiveDataDict = new();

    public override void GenerateList(string filepath, int skiprows)
    {
        passiveDataDict = new SerializableDictionary<int, PassiveData>();

        if (!string.IsNullOrEmpty(filepath))
        {
            filePath = filepath;
        }

        string[] lines = File.ReadAllLines(Application.dataPath + filePath);
        for (int i = skiprows; i < lines.Length; i++)
        {
            string[] data = lines[i].Split("\t");

            PassiveData passiveData = new()
            {
                passiveID = int.Parse(data[0]),
                passiveName = data[1],
                description = data[2],
                unlockLevel = int.Parse(data[3]),
                passiveType = (PassiveType)int.Parse(data[4]),
                iconKey = data[5],
                param1 = string.IsNullOrEmpty(data[6]) ? 0 : float.Parse(data[6]),
                param2 = string.IsNullOrEmpty(data[7]) ? 0 : float.Parse(data[7])
            };

            if (!passiveDataDict.ContainsKey(passiveData.passiveID))
                passiveDataDict.Add(passiveData.passiveID, passiveData);
            else
                passiveDataDict[passiveData.passiveID] = passiveData;
        }

        Debug.Log("Done converting CSV to SO from: " + filePath);
    }
}

[System.Serializable]
public class PassiveData
{
    public int passiveID;
    public string passiveName;
    public string description;
    public int unlockLevel;
    public PassiveType passiveType;
    public string iconKey;
    public float param1, param2;
}
