using UnityEngine;

public class Annihilator : EnemyClassLogic, IEnemyCoroutineLogic
{
    EnemyManager manager;
    //
    int backMult = 50;
    float waitTime = 0.7f;
    WaitForSeconds waitToAnnihilate = new WaitForSeconds(0.7f);

    //

    public override void Init(EnemyManager m) => manager = m;
    public override void DoLogic()
    {
        if (manager.attackCooldown > 0) return;
        //
        if (!manager.actions.inAir)
        {
            GM.i.globalEnemyManager.SpawnAnnihilatorEffect(manager.eTransform.position, waitTime);
            manager.HandleLogicAfterDelay(this, waitTime);
        }
        //
        manager.attackCooldown = manager.runtimeAttackSpeed;
    }
    //
    public void CoroutineLogic()
    {
        // as scriptable objects do not have access to Coroutines, this is run after a delay from a Monobehavior derived manager
        Collider[] objects = Physics.OverlapSphere(manager.eTransform.position, 3, GM.i.globalEnemyManager.damageables);
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].gameObject.TryGetComponent(out IDamageable damageable)) damageable.TakeDamage(manager.runtimeDamage, DamageType.Normal);
            PlayerManager pManager = objects[i].gameObject.GetComponentInParent<PlayerManager>();
            //
            if (pManager)
            {
                if (pManager.popped || pManager.actions.inAir) return;
                //
                Vector3 direction = (pManager.movement.chestBodyTransform.position - manager.eTransform.position).normalized;
                pManager.movement.chestBody.velocity = Vector3.zero; // kill velocity before adding new forces so that the force applied is predictable
                //
                pManager.movement.chestBody.AddForce(direction * backMult, ForceMode.Impulse);
                pManager.TogglePopped();
            }
        }
    }
}
