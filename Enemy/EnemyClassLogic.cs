using UnityEngine;

public abstract class EnemyClassLogic : ScriptableObject
{
    public int extraExperience;
    //
    public abstract void Init(EnemyManager m);
    public abstract void DoLogic();
}

public enum EnemyClassType
{
    Shaman,
    Wizard,
    Sorcerer,
    Mercenary,
    Annihilator,
    None = 99
}
