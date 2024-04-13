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
		
		if (Input.GetMouseButton(0) && _cam)
		{
			if (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out var hit, 100f, TileMask))
			{
				_target = hit.point;
			}
		}
		
		CheckTarget();
		
		switch (_path.corners.Length)
		{
			case > 1:
				DesiredDirection = (_path.corners[1] - transform.position).normalized;
				break;
			case > 0:
				DesiredDirection = (_path.corners.Last() - transform.position).normalized;
				break;
		}

		if (_target == null)
		{
			DesiredDirection = Vector3.zero;		
			return;
		}
		
		NavMesh.CalculatePath(transform.position, (Vector3) _target, NavMesh.AllAreas, _path);
		
		DrawPath();
	}

	protected void FixedUpdate()
	{
		Move();
	}

	private void CheckTarget()
	{
		if (_target == null) return;
		
		var pos = transform.position;
		var target = (Vector3)_target;
		var diff = (new Vector3(pos.x, 0, pos.z) - new Vector3(target.x, 0, target.z)).magnitude;
		
		if (diff <= 0.1f)
		{
			_target = null;
		}
	}
	
	private void DrawPath()
	{
		for (int i = 0; i < _path.corners.Length - 1; i++)
		{
			Debug.DrawLine(_path.corners[i], _path.corners[i + 1], Color.red);
		}
	}
}
