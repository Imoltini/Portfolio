using UnityEngine;

public abstract class OrbitalBase : MonoBehaviour
{
    Transform orb;
    Vector3 direction;
    Vector3 orbitPosition;
    protected float newHitID;
    protected float lastHitID;
    //
    float x;
    float z;
    float orbitAngle;
    public float speed;
    public float orbitRadius;
    //
    public AudioClip[] hitSFX;
    public AudioSource source;
    //
    protected int hitVFXID;
    public GameObject hitVFX;

    //
    void Start()
    {
        orb = transform;
        hitVFXID = hitVFX.GetInstanceID();
        StartCoroutine(GM.i.poolManager.CreateSlowPool(hitVFX, 50));
    }
    //
    void LateUpdate()
    {
        orbitAngle += speed * Time.deltaTime;
        //
        x = orbitRadius * Mathf.Cos(orbitAngle);
        z = orbitRadius * Mathf.Sin(orbitAngle);
        orbitPosition = new Vector3(x, 0.5f, z);
        //
        // orbit around player
        orb.position = orbitPosition + GM.i.pTransform.position;
    }
}
