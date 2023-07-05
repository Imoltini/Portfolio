using UnityEngine;

public class Morale : EnemyPassiveLogic, IEnemyCoroutineLogic
{
    int range = 5;
    int initDamage;
    int buffedDamage;
    Collider[] nearbyFriend = new Collider[2];
    //
    float initMoveSpeed;
    float buffedMoveSpeed;
    //
    LayerMask layer = 1 << 14;
    EnemyManager manager;
    Transform tForm;

    //

    public override void Init(EnemyManager m)
    {
        manager = m;
        extraExperience = 5;
        tForm = manager.eTransform;
        //
        initDamage = manager.runtimeDamage;
        buffedDamage = Mathf.CeilToInt(initDamage * 2.5f);
        initMoveSpeed = manager.stats.movementSpeed;
        buffedMoveSpeed = initMoveSpeed * 2.3f;
        //
        DoLogic();
    }
    //
    // run coroutine every second through manager as scriptable objects do not have access to coroutines
    public override void DoLogic() => manager.HandleCoroutineLogic(this, 1);
    public void CoroutineLogic()
    {
        int friends = Physics.OverlapSphereNonAlloc(tForm.position, range, nearbyFriend, layer);
        if (friends < 2) // Overlap will always add self in array as well, so friends needs to equal 2 for a real friend to be nearby
        {
            // if only found self keep initial speed and damage, otherwise buff self speed and damage
            manager.runtimeDamage = initDamage;
            manager.movement.SetMoveSpeed(initMoveSpeed);
        }
        else
        {
            manager.runtimeDamage = buffedDamage;
            manager.movement.SetMoveSpeed(buffedMoveSpeed);
        }
    }
}
