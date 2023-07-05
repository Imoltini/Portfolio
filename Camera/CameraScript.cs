using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
	public float heightFromPlayer;
	public float distanceFromPlayer;
	//
	[HideInInspector] public bool rotating;
	[HideInInspector] public bool interacting;
	[HideInInspector] public bool changedAngle;
	[HideInInspector] public Transform rotationAngle;
	[HideInInspector] public Transform originalRotation;
	//
	Vector3	velocity;
	float smoothTime;
	public Camera cam;
	Transform camForm;
	Vector3 originalOffset;
	//
	public Vector3 camOffset;
	public List<Transform> targets = new List<Transform>();

	//
	void Awake()
	{
		camOffset = new Vector3(0, heightFromPlayer, -distanceFromPlayer);
		originalRotation = camForm = transform;
		originalOffset = camOffset;
	}
	//
	void Start()
	{
		smoothTime = GM.i.atStart ? 2 : 0.35f;
		targets.Add(GM.i.pTransform);
		//
		if (GM.i.inArena) targets.Add(GM.i.pTwoTransform);
		else
		{
			if (GM.i.currentScene > 2 && GM.i.isMultiplayer)
			{
				camOffset = new Vector3(0, 9, -7.5f);
				targets.Add(GM.i.pTwoTransform);
				originalOffset = camOffset;
			}
		}
	}
	//
	void FixedUpdate()
	{
		if (rotating)
		{
			RotateCamera(rotationAngle);
			if (camForm.rotation == rotationAngle.rotation) rotating = false;
		}
	}
	//
	void LateUpdate()
	{
		if (targets.Count == 1) MoveCamera(targets[0].position);
		else if (targets.Count > 1) EncapsulateAndFollowPlayers();
	}
	//
	public void ClearTargets(bool allTargets, Transform targetToRemove = null)
	{
		if (allTargets) targets.Clear();
		else targets.Remove(targetToRemove);
	}
	//
	public void AddNewTarget(Transform newTarget) => targets.Add(newTarget);
	public void ChangeCameraAngle(bool changingAngle, Transform newCameraAngle, Vector3 newOffset)
	{
		if (changingAngle)
		{
			rotationAngle = newCameraAngle;
			camOffset = newOffset;
		}
		else
		{
			rotationAngle = originalRotation;
			camOffset = originalOffset;
		}
		//
		changedAngle = changingAngle;
		rotating = true;
	}
	//
	void MoveCamera(Vector3 newPosition) => camForm.position = Vector3.SmoothDamp(camForm.position, newPosition + camOffset, ref velocity, smoothTime);
	void EncapsulateAndFollowPlayers()
	{
		var bounds = new Bounds(targets[0].position, Vector3.zero);
		for (int i = 0; i < targets.Count; i++)
		{
			bounds.Encapsulate(targets[i].position);
		}
		//
		MoveCamera(bounds.center);
	}
	//
	void RotateCamera(Transform to)
	{
		camForm.rotation = Quaternion.Lerp(camForm.rotation, to.rotation, 0.004f);
		rotating = true;
	}
}
