using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class AIPlayer : BasePlayer
{
	private Vector3 _target;
	private Camera _cam;
	private NavMeshPath _path;
	private NavMeshAgent _agent;

	// public float JumpHeight;
	// public float JumpDuration;

	[SerializeField] private bool _drawPath;
	
	protected override void Awake()
	{
		base.Awake();
		
		_cam = Camera.main;
		_path = new NavMeshPath();
		_agent = GetComponent<NavMeshAgent>();
	}

	private void Start()
	{
		_target = transform.position;
	}

	protected override void Update()
	{
		base.Update();

		_agent.enabled = Grounded;
		
		if (Input.GetMouseButton(0) && _cam && _agent.enabled)
		{
			if (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out var hit, 100f, TileMask))
			{
				_target = hit.point;
				_agent.destination = hit.point;
			}
		}
		
		switch (_path.corners.Length)
		{
			case > 1:
				DesiredDirection = (_path.corners[1] - transform.position).normalized; break;
			case > 0:
				DesiredDirection = (_path.corners.Last() - transform.position).normalized; break;
		}
		

		
		NavMesh.CalculatePath(transform.position, _target, NavMesh.AllAreas, _path);
		if (_drawPath) DrawPath();
	}

	protected void FixedUpdate()
	{
		Move();
		
		if(_agent.isOnOffMeshLink)
		{
			StartCoroutine(RigidBodyJump());
		}
	}

	private IEnumerator RigidBodyJump()
	{
		var pos = transform.position;
		var data = _agent.currentOffMeshLinkData;
		var endPos = data.endPos;
		var direction = (new Vector3(endPos.x, pos.y, endPos.z) - pos).normalized;
	
		_agent.enabled = false;
		Rb.velocity = MoveSpeed * direction;
		Jump();
	
		yield return new WaitForSeconds(0.2f);
		yield return new WaitUntil(() => Grounded);
	
		_agent.CompleteOffMeshLink();
	}

	// Deprecated, use RigidBodyJump
	private IEnumerator ParabolaJump(float height, float duration)
	{
		OffMeshLinkData data = _agent.currentOffMeshLinkData;
		Vector3 startPos = _agent.transform.position;
		Vector3 endPos = data.endPos + Vector3.up * _agent.baseOffset;
		float normalizedTime = 0f;
		_agent.enabled = false;
		while (normalizedTime < 1f)
		{
			float yOffset = height * 4f * (normalizedTime - normalizedTime * normalizedTime);
			Rb.velocity = Vector3.Lerp(startPos, endPos, normalizedTime) + new Vector3(0, yOffset, 0);
			
			normalizedTime += Time.deltaTime / duration;
			yield return null;
		}
		
		_agent.enabled = true;
		_agent.CompleteOffMeshLink();
	}

	// No Longer needed with auto braking
	// private void CheckTarget()
	// {
	// 	if (_target == null) return;
	// 	
	// 	var pos = transform.position;
	// 	var target = (Vector3)_target;
	// 	var diff = (new Vector3(pos.x, 0, pos.z) - new Vector3(target.x, 0, target.z)).magnitude;
	// 	
	// 	if (diff <= 0.1f)
	// 	{
	// 		_target = null;
	// 	}
	// }
	
	private void DrawPath()
	{
		for (int i = 0; i < _path.corners.Length - 1; i++)
		{
			Debug.DrawLine(_path.corners[i], _path.corners[i + 1], MeshRenderer.material.color);
		}
	}
}
