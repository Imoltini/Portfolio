using UnityEngine;

public abstract class EnemyPassiveLogic : ScriptableObject
{
    public int extraExperience = 1;
    //
    public abstract void Init(EnemyManager m);
    public abstract void DoLogic();
}

public enum EnemyPassiveType
{
    Bomber,
    Undead,
    Enrage,
    Marauder,
    Morale
}
