using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : BasePlayerController
{
	private Vector3 _target;
	private Camera _cam;

	// private NavMeshAgent _agent;

	protected override void Awake()
	{
		base.Awake();

		// _agent = GetComponent<NavMeshAgent>();
		// _agent.speed = MoveSpeed;
		_cam = Camera.main;
	}

	protected override void Update()
	{
		base.Update();

		if (Input.GetMouseButtonDown(0) && _cam)
		{
			if (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out var hit, 100f))
			{
				
			}

		}
		
		if (Input.GetKeyDown(KeyCode.Space))
		{
			Debug.Log("pressed space");
		}
	}
}
