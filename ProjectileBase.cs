using System.Collections;
using UnityEngine;
using System;

public abstract class ProjectileBase : PoolObject
{
    protected bool follow;
    protected float newHitID;
    protected float lastHitID;
    public bool hasTrailRenderer;
    protected int damageOverride;
    protected int remainingBounces;
    protected int remainingPierces;
    //
    Transform muzzleTransform;
    Transform impactTransform;
    protected Rigidbody projBody;
    protected Transform eTransform;
    protected Transform projTransform;
    protected SphereCollider projColl;
    //
    ParticleSystem trails;
    ParticleSystemRenderer rend;
    ParticleSystem projParticles;
    ParticleSystem muzzleParticles;
    ParticleSystem impactParticles;
    protected TrailRenderer trailRenderer;
    //
    AudioSource projAudioSource;
    AudioSource impactAudioSource;
    //
    int projLifeTime = 1;
    float lifetimeCounter;
    Vector3 zero = Vector3.zero;
    const int projectileSpeed = 40;
    protected bool triggeredAffinity;
    protected bool turnOnLifetimeCounter;
    protected WeaponManager weaponManager;
    WaitForSeconds waitToDisable = new WaitForSeconds(1f);

    //

    void Awake()
    {
        GetAndSetParticleSystems();
        GetAudioSources();
        //
        lastHitID = -1;
        projTransform = transform;
        projBody = GetComponent<Rigidbody>();
        projColl = GetComponent<SphereCollider>();
        rend = GetComponent<ParticleSystemRenderer>();
        projParticles = GetComponent<ParticleSystem>();
        weaponManager = pIndex == 1 ? GM.i.pTwoManager.weaponManager : GM.i.pManager.weaponManager;
    }
    //
    void Update()
    {
        if (turnOnLifetimeCounter)
        {
            lifetimeCounter -= Time.deltaTime;
            if (lifetimeCounter <= 0)
            {
                if (weaponManager.hasAffinity)
                {
                    if (triggeredAffinity) TurnProjectileOff();
                    else ReturnProjectileToPlayer();
                }
                else TurnProjectileOff();
            }
        }
    }
    //
    void GetAndSetParticleSystems()
    {
        muzzleTransform = transform.GetChild(1).transform;
        impactTransform = transform.GetChild(2).transform;
        trails = transform.GetChild(0).gameObject.GetComponent<ParticleSystem>();
        muzzleParticles = muzzleTransform.gameObject.GetComponent<ParticleSystem>();
        impactParticles = impactTransform.gameObject.GetComponent<ParticleSystem>();
        if (hasTrailRenderer) trailRenderer = transform.GetChild(0).GetChild(0).gameObject.GetComponent<TrailRenderer>();
        //
        muzzleTransform.parent = GM.i.poolManager.poolsHolder.transform;
        impactTransform.parent = GM.i.poolManager.poolsHolder.transform;
    }
    //
    void GetAudioSources()
    {
        projAudioSource = GetComponent<AudioSource>();
        impactAudioSource = impactTransform.gameObject.GetComponent<AudioSource>();
    }
    //
    void SetMuzzleBeforeShot(Vector3 pos)
    {
        muzzleTransform.position = pos;
        impactTransform.rotation *= Quaternion.Euler(-90f, 0f, 0f);
    }
    //
    protected void HandleCollisionWithObject(Vector3 position, bool pierce = false)
    {
        turnOnLifetimeCounter = false;
        lifetimeCounter = projLifeTime;
        //
        if (!pierce) TurnProjectileOff();
        else
        {
            SetImpactParticle(position);
            PlayRandomlyPitchedAudio(impactAudioSource);
        }
    }
    //
    void SetImpactParticle(Vector3 position)
    {
        impactTransform.position = position;
        impactTransform.rotation *= Quaternion.Euler(-90f, 0f, 0f);
        impactParticles.Play();
    }
    //
    public override void OnProjectileReuse(ProjectileReuseData data)
    {
        projBody.velocity = zero;
        lifetimeCounter = projLifeTime;
        damageOverride = data.damageOverride;
        //
        rend.enabled = true;
        projColl.enabled = true;
        turnOnLifetimeCounter = true;
        remainingBounces = pIndex == 1 ? GM.i.pTwoManager.weaponManager.bounceMult : GM.i.pManager.weaponManager.bounceMult;
        remainingPierces = pIndex == 1 ? GM.i.pTwoManager.weaponManager.pierceMult : GM.i.pManager.weaponManager.pierceMult;
        if (!GM.i.inArena) remainingPierces += pIndex == 1 ? GM.i.pTwoManager.dominionManager.puncturePierce : GM.i.pManager.dominionManager.puncturePierce;
        //
        SetMuzzleBeforeShot(data.startPosition);
        SetParticleSystemsBeforeUse();
        //
        PlayRandomlyPitchedAudio(projAudioSource);
        SetProjectileSpeedAndDirection(data.speed, data.target, data.angle);
    }
    //
    void SetParticleSystemsBeforeUse()
    {
        trails.Play();
        projParticles.Play();
        muzzleParticles.Play();
        impactParticles.Stop();
    }
    //
    public virtual void SetProjectileSpeedAndDirection(int speed, Vector3 target, float angle = 0)
    {
        projTransform.LookAt(target);
        var force = projTransform.forward;
        //
        if (angle != 0) projBody.AddForce(speed * (force + (projTransform.right * angle)), ForceMode.Impulse);
        else projBody.AddForce(speed * force, ForceMode.Impulse);
    }
    //
    public void BounceProjectile(GameObject hitEnemy, float instanceID)
    {
        Collider[] enemies = Physics.OverlapSphere(hitEnemy.transform.position, 15, GM.i.pManager.actions.enemyLayerOnly);
        if (enemies.Length == 0 || remainingBounces <= 0) TurnProjectileOff();
        else
        {
            remainingBounces--;
            projBody.velocity = zero;
            turnOnLifetimeCounter = true;
            //
            int randEnemy = UnityEngine.Random.Range(0, enemies.Length);
            while (enemies[randEnemy].gameObject.GetInstanceID() == instanceID)
            {
                randEnemy = UnityEngine.Random.Range(0, enemies.Length);
            }
            //
            SetProjectileSpeedAndDirection(projectileSpeed, enemies[randEnemy].gameObject.transform.position);
        }
    }
    //
    protected void ReturnProjectileToPlayer()
    {
        triggeredAffinity = true;
        projBody.velocity = zero;
        lifetimeCounter = 3;
        lastHitID = -1;
        //
        SetProjectileSpeedAndDirection(projectileSpeed, weaponManager.manager.movement.chestBodyTransform.position);
        remainingPierces = 200;
    }
    //
    void PlayRandomlyPitchedAudio(AudioSource source)
    {
        source.pitch = UnityEngine.Random.Range(0.9f, 1.11f);
        source.Play();
    }
    //
    void TurnProjectileOff()
    {
        trails.Stop();
        follow = false;
        lastHitID = -1;
        projParticles.Stop();
        rend.enabled = false;
        projColl.enabled = false;
        projBody.velocity = zero;
        triggeredAffinity = false;
        eTransform = GM.i.pManager.projectiles.forward;
        if (hasTrailRenderer) trailRenderer.emitting = false;
    }
}
