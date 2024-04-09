using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Player;
using UnityEngine;
using UnityEngine.AI;

public class AIController : BasePlayerController
{
	private Vector3? _target;
	private Camera _cam;
	private NavMeshPath _path;

	protected override void Awake()
	{
		base.Awake();
		
		_path = new NavMeshPath();
		_cam = Camera.main;
	}

	protected override void Update()
	{
		base.Update();
		
		if (Input.GetKeyDown(KeyCode.R))
		{
			Rb.AddForce(5f * transform.right, ForceMode.Impulse);
		}
		
		if (Input.GetMouseButtonDown(0) && _cam)
		{
			if (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out var hit, 100f, TileMask))
			{
				_target = hit.transform.position;
			}
		}
		
		CheckTarget();
		
		if (_target == null) return;
		
		NavMesh.CalculatePath(transform.position, (Vector3) _target, NavMesh.AllAreas, _path);
		
		DrawPath();

		switch (_path.corners.Length)
		{
			case > 1:
				Move(_path.corners[1]);
				break;
			case > 0:
				Move(_path.corners.Last());
				break;
		}
	}

	private void CheckTarget()
	{
		if (_target == null) return;
		
		var pos = transform.position;
		var flatPos = new Vector3(pos.x, 0, pos.z);
		var diff = (flatPos - (Vector3)_target).magnitude;
		
		if (diff > 0.01f) return;
		
		_target = null;
	}


	private void Move(Vector3 movePoint)
	{
		var direction = (movePoint - transform.position).normalized;		
		Rb.velocity = Vector3.Lerp(Rb.velocity, MoveSpeed * direction, Time.deltaTime);
	}
	
	private void DrawPath()
	{
		for (int i = 0; i < _path.corners.Length - 1; i++)
		{
			Debug.DrawLine(_path.corners[i], _path.corners[i + 1], Color.red);
		}
	}
}
