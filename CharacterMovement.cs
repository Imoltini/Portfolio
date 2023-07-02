using UnityEngine.EventSystems;
using System.Collections;
﻿using UnityEngine;
using System;

public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Variables")]
    public float maxSpeed;
    public int acceleration;
    public float speedFactor;
    public int maxAccelerationForce;
    public float maxAccelerationForceFactor;
    public AnimationCurve accelerationCurve;
    public AnimationCurve maxAccelerationCurve;
    [HideInInspector] public float initMoveSpeed;
    //
    Vector3 forceScale = new Vector3(1,0,1);
    Vector3 goalVel = Vector3.zero;
    Vector3 zero = Vector3.zero;
    //
    float faceForce;
    int speedLimit = 19;
    float deadZone = 0.3f;
    bool toggledHeightChange;
    [HideInInspector] public bool moving;
    [HideInInspector] public bool playerBlocked;
    //
    [HideInInspector] public Vector3 currentFacing = Vector3.zero;
    [HideInInspector] public Vector3 inputDirection = Vector3.zero;
    //
    [HideInInspector] public CharacterLegs legs;
    [HideInInspector] public Rigidbody chestBody;
    [HideInInspector] public Transform chestBodyTransform;
    [HideInInspector] public CharacterUpright chestUpright;
    [HideInInspector] public CharacterFaceDirection faceDirection;
    [HideInInspector] public CharacterMaintainHeight maintainHeight;
    //
    PlayerManager manager;

    //

    void Awake()
    {
        legs = GetComponent<CharacterLegs>();
        chestBody = GetComponent<Rigidbody>();
        chestUpright = GetComponent<CharacterUpright>();
        faceDirection = GetComponent<CharacterFaceDirection>();
        maintainHeight = GetComponent<CharacterMaintainHeight>();
        //
        chestBody.maxAngularVelocity = 40.0f;
        faceForce = faceDirection.facingForce;
        chestBodyTransform = chestBody.transform;
        //
        manager = GetComponentInParent<PlayerManager>();
        SetMoveSpeed(maxSpeed, true);
        initMoveSpeed = maxSpeed;
    }
    //
    void Start() => TurnPlayerPhysicsOff();
    void Update()
    {
        if (playerBlocked || manager.playerDied || manager.actions.tornadoPowerUp || manager.actions.inAir) return;
        inputDirection = zero;
        HandleMovement();
    }
    //
    void FixedUpdate()
    {
        if (!moving || manager.actions.inAir) return;
        AddForceToPlayerChest();
    }
    //
    void HandleMovement()
    {
        if (manager.input.GetAxis(3) >= deadZone) inputDirection += Vector3.right;
        if (manager.input.GetAxis(3) <= -deadZone) inputDirection += Vector3.left;
        if (manager.input.GetAxis(4) >= deadZone) inputDirection += Vector3.forward;
        if (manager.input.GetAxis(4) <= -deadZone) inputDirection += Vector3.back;
        //
        if (inputDirection != Vector3.zero)
        {
            if (!legs.walking)
            {
                toggledHeightChange = false;
                legs.StartWalking();
            }
            NormalizeMovement();
            ToggleMaintainHeight(false);
            //
            faceDirection.LookAt(inputDirection);
        }
        //
        else
        {
            StopMoving();
            ToggleMaintainHeight(true);
        }
    }
    //
    void AddForceToPlayerChest()
    {
        float velDot = Vector3.Dot(inputDirection, goalVel.normalized);
        float accel = acceleration * accelerationCurve.Evaluate(velDot);
        Vector3 goalVelocity = inputDirection * maxSpeed * speedFactor;
        goalVel = Vector3.MoveTowards(goalVel, goalVelocity, accel * Time.deltaTime);
        Vector3 neededAccel = (goalVel - chestBody.velocity) / Time.deltaTime;
        float maxAccel = maxAccelerationForce * maxAccelerationCurve.Evaluate(velDot) * maxAccelerationForceFactor;
        neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);
        chestBody.AddForce(Vector3.Scale(neededAccel * chestBody.mass, forceScale));
    }
    //
    void NormalizeMovement()
    {
        moving = true;
        inputDirection.Normalize();
        ToggleMaintainHeight(false);
        //
        currentFacing = chestBodyTransform.forward;
        currentFacing.y = 0;
        currentFacing.Normalize();
    }
    //
    void StopMoving()
    {
        moving = false;
        if (legs.walking)
        {
            legs.StopWalking();
            toggledHeightChange = false;
        }
    }
    //
    public void ToggleBlockedPlayer(bool isBlocked)
    {
        if (isBlocked)
        {
            legs.StopWalking();
            moving = false;
        }
        else legs.StartWalking();
        //
        inputDirection = zero;
        playerBlocked = isBlocked;
        manager.actions.canJump = !isBlocked;
    }
    //
    public void TurnPlayerPhysicsOff()
    {
        legs.StopWalking();
        legs.walking = false;
        legs.enabled = false;
        inputDirection = zero;
        chestUpright.enabled = false;
        faceDirection.enabled = false;
        maintainHeight.enabled = false;
        manager.actions.currentForwardFacing = chestBodyTransform.forward;
    }
    //
    public void TurnPlayerPhysicsOn()
    {
        legs.enabled = true;
        inputDirection = zero;
        chestUpright.enabled = true;
        faceDirection.enabled = true;
        maintainHeight.enabled = true;
    }
    //
    public void SetSpecificMoveSpeed(float s)
    {
        maxSpeed = s;
        legs.moveForwardForce = maxSpeed * 6;
    }
    public void SetMoveSpeed(float s, bool init = false)
    {
        maxSpeed = init ? s : maxSpeed += s;
        if (maxSpeed >= speedLimit) maxSpeed = speedLimit;
        legs.moveForwardForce = maxSpeed * 6;
    }
    //
    void ToggleMaintainHeight(bool pullHigher)
    {
        if (toggledHeightChange) return;
        //
        maintainHeight.TogglePullUpForce(pullHigher);
        toggledHeightChange = true;
    }
    //
    public void ChangeMaxSpinVelocity(float amount) => chestBody.maxAngularVelocity = amount;
}
