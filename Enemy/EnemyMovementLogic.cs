using UnityEngine;

public abstract class EnemyMovementLogic : ScriptableObject
{
    public abstract void Init(EnemyManager m);
    public abstract void DoLogic();
}

public enum EnemyMovementType
{
    ChaseAndJump,
    ChaseAndStop,
    ChaseAndFlee,
    ChaseJumpAndStop,
    EnemyTestMovement,
    Flee
}
