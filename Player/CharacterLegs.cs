using UnityEngine;

public class CharacterLegs : MonoBehaviour
{
    [HideInInspector] public AudioSource source;
    public bool isPlayer;
    //
    public AudioClip[] footsteps;
    public Rigidbody[] thighs;
    public Rigidbody[] shins;
    [Space(10)]
    public Transform leadingFootTransform;
    //
    int legIndex;
    float legsCounter;
    int altlegIndex = 1;
    public int downForce;
    int holdDownForce = 150;
    public float legRate = 0.2f;
    public float legRateIncreaseByVelocity = 0.1f;
    [HideInInspector] public bool walking = false;
    [HideInInspector] public float moveForwardForce;
    //
    Rigidbody chestBody;
    Vector3 up = Vector3.up;
    Vector3 horizontalVelocity;
    Transform chestBodyTransform;
    //
    bool fourLegs;

    //

    void Start()
    {
        if (thighs.Length > 2) fourLegs = true;
        //
        StopWalking();
        for (int i = 0; i < shins.Length; i++)
        {
            thighs[i].maxAngularVelocity = shins[i].maxAngularVelocity = 20;
        }
        //
        if (isPlayer) source = GetComponent<AudioSource>();
        chestBody = GetComponent<Rigidbody>();
        chestBodyTransform = chestBody.transform;
    }
    //
    public void StopWalking()
    {
        walking = false;
        legsCounter = legRate * 0.99f;
    }
    //
    public void StartWalking()
    {
        ChangeLeg();
        walking = true;
    }
    //
    void FixedUpdate()
    {
        if (walking)
        {
            float speed = chestBody.velocity.sqrMagnitude;
            legsCounter += Time.deltaTime * (1 + speed * legRateIncreaseByVelocity);
            //
            horizontalVelocity = chestBodyTransform.forward;
            horizontalVelocity.y = 0;
            horizontalVelocity.Normalize();
            //
            if (legsCounter >= legRate) ChangeLeg();
            if (legsCounter > legRate * 0.75f) PlaceLeadingFootDown();
            else if (legsCounter > legRate * 0.5f) StraightenLeadingLeg();
            else if (legsCounter > legRate * 0.25f) BendKneeUp();
            else StartStep();
        }
        else HoldFeetOnGround();
    }
    //
    void StartStep()
    {
        PushThighForward();
        LiftAndPushLeadingFoot();
        HoldAndPullBackFollowingFoot();
    }
    //
    void BendKneeUp()
    {
        PushLeadingFootForward(0.4f);
        PullFollowingFootBack(0.4f);
        PullThighUp();
        PullShinBack();
        HoldFollowingFootOnGround();
    }
    //
    void StraightenLeadingLeg()
    {
        PushLeadingFootForward(0.4f);
        PullFollowingFootBack(0.4f);
        PullThighUp();
        LiftShinUp();
        //
        if (legsCounter > legRate * 0.66f && FootIsBehindChest()) legsCounter = legRate * 0.66f;
    }
    //
    void HoldAndPullBackFollowingFoot()
    {
        HoldFollowingFootOnGround();
        PullFollowingFootBack(1.0f);
    }
    //
    void PlaceLeadingFootDown()
    {
        LiftAndPushLeadingFoot();
        HoldLeadingFootOnGround();
        HoldAndPullBackFollowingFoot();
        if (isPlayer) PlayFootstepAudio();
    }
    //
    void HoldFeetOnGround()
    {
        HoldLeadingFootOnGround();
        HoldFollowingFootOnGround();
    }
    //
    void LiftAndPushLeadingFoot()
    {
        shins[legIndex].AddForce(up * Time.deltaTime, ForceMode.Impulse);
        PushLeadingFootForward(1.0f);
    }
    //
    void PushThighForward() => thighs[legIndex].AddForceAtPosition(up * Time.deltaTime, leadingFootTransform.TransformPoint(up * -2));
    void PullThighUp() => thighs[legIndex].AddForceAtPosition(up * Time.deltaTime, leadingFootTransform.TransformPoint(up * -2), ForceMode.Impulse);
    //
    void PullShinBack() => shins[legIndex].AddForceAtPosition(up * Time.deltaTime, leadingFootTransform.TransformPoint(up * -2), ForceMode.Impulse);
    void LiftShinUp()
    {
        shins[legIndex].AddForceAtPosition(up * Time.deltaTime * 0.5f, leadingFootTransform.TransformPoint(up * -2), ForceMode.Impulse);
        shins[legIndex].AddForceAtPosition(horizontalVelocity * moveForwardForce * Time.deltaTime, leadingFootTransform.TransformPoint(up * -2), ForceMode.Impulse);
    }
    //
    void PushLeadingFootForward(float multiplier) => shins[legIndex].AddForce(horizontalVelocity * moveForwardForce * multiplier * Time.deltaTime, ForceMode.Impulse);
    void PullFollowingFootBack(float multiplier) => shins[altlegIndex].AddForce(-horizontalVelocity * moveForwardForce * multiplier * Time.deltaTime, ForceMode.Impulse);
    //
    void HoldLeadingFootOnGround() => shins[legIndex].AddForce(-up * (holdDownForce * 0.5f) * Time.deltaTime, ForceMode.Impulse);
    void HoldFollowingFootOnGround() => shins[altlegIndex].AddForce(-up * holdDownForce * Time.deltaTime, ForceMode.Impulse);
    //
    void WiggleChest()
    {
        chestBody.AddForceAtPosition((chestBodyTransform.forward - Vector3.up * 2) * downForce * 0.66f * Time.deltaTime, chestBodyTransform.TransformPoint(Vector3.up * 2), ForceMode.Impulse);
        chestBody.AddForceAtPosition((chestBodyTransform.forward - Vector3.up * 2) * -downForce * 0.5f * Time.deltaTime, chestBodyTransform.TransformPoint(Vector3.up * -2), ForceMode.Impulse);
    }
    //
    bool FootIsBehindChest()
    {
        if (chestBodyTransform.InverseTransformPoint(shins[legIndex].transform.position).z < 0) return true;
        else return false;
    }
    //
    void PlayFootstepAudio()
    {
        int randChance = Random.Range(0, 100);
        if (randChance < 70) return;
        //
        source.pitch = Random.Range(0.80f, 0.85f);
        source.PlayOneShot(footsteps[Random.Range(0, footsteps.Length)], 0.2f);
    }
    //
    void ChangeLeg()
    {
        if (!fourLegs)
        {
            legIndex = (legIndex + 1) % 2;
            altlegIndex = 1 - legIndex;
        }
        else
        {
            legIndex = (legIndex + 1) % 4;
            altlegIndex = (legIndex + 2) % 4;
        }
        legsCounter -= legRate;
    }
}
