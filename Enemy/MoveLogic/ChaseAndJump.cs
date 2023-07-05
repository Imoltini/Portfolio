using UnityEngine;

public class ChaseAndJump : EnemyMovementLogic
{
    float anticipationDelay = 0.4f;
    float anticipationCooldown;
    bool shownWarning;
    bool anticipating;
    //
    EnemyManager manager;
    float rand;

    //

    public override void Init(EnemyManager m)
    {
        manager = m;
        rand = manager.stats.jumpTriggerDistance + Random.Range(3, 7);
    }
    //
    public override void DoLogic()
    {
        manager.actions.FindPlayer();
        manager.actions.Move();
        //
        if (manager.actions.playerDistance > rand || !manager.actions.canJump) return;
        if (!shownWarning) ShowAttackWarning();
        if (anticipating)
        {
            anticipationCooldown -= Time.deltaTime;
            if (anticipationCooldown <= 0) PerformAttack();
        }
    }
    //
    void ShowAttackWarning()
    {
        manager.dangerWarning.gameObject.SetActive(true);
        anticipationCooldown = anticipationDelay;
        manager.dangerWarning.Play();
        shownWarning = true;
        anticipating = true;
    }
    //
    void PerformAttack()
    {
        manager.dangerWarning.gameObject.SetActive(false);
        manager.actions.Jump();
        shownWarning = false;
        anticipating = false;
    }
}
