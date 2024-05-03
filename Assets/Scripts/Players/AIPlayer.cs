using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Players
{
	public class AIPlayer : BasePlayer
	{
		[SerializeField] private float _searchRadius;
		[SerializeField] private Vector3 _target;
		[SerializeField] private float _distanceToTarget;
		private Camera _cam;
		private NavMeshPath _path;
		private NavMeshAgent _agent;
	
		private int GoldPieceMask => 1 << LayerMask.NameToLayer("GoldPiece");

		private Vector3Int _centerPos;

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
			var size = TilemapManager.TilemapSize;
			_centerPos = new Vector3Int(size.x / 2, 0, size.y / 2);
			
			_target = transform.position;
		}

		protected override void Update()
		{
			base.Update();
		
			_target = GetNearestGoldPiece();
			
			_agent.enabled = Grounded;
			
			if (!_agent.enabled) return; 
		
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
			DesiredDirection.y = 0;
		
			
			NavMesh.CalculatePath(transform.position, _target, NavMesh.AllAreas, _path);
			if (_drawPath) DrawPath();
		}

		protected override void FixedUpdate()
		{
			base.FixedUpdate();
			Move();
			
			Rotate();
		
			if(_agent.isOnOffMeshLink)
			{
				StartCoroutine(RigidBodyJump());
			}

			_distanceToTarget = Vector3.Distance(_target, transform.position);
			if (_distanceToTarget < 1.5f) _target = _centerPos;
			
			if (Rb.velocity.magnitude < 1.5f && _distanceToTarget > 1.5f) SwingPickaxe();
		}

		private void OnDisable()
		{
			_agent.enabled = true;
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

		private Vector3 GetNearestGoldPiece()
		{
			float closestDistance = float.MaxValue;
			Vector3 closestPosition = _target;
		
			int maxSearch = 10;
			Collider[] hits = new Collider[maxSearch];
			int numFound = Physics.OverlapSphereNonAlloc(transform.position, _searchRadius, hits, GoldPieceMask);
			for (int i = 0; i < numFound; i++)
			{
				var distance = Vector3.Distance(transform.position, hits[i].transform.position);
				if (distance < closestDistance)
				{
					closestDistance = distance;
					closestPosition = hits[i].transform.position;
				}
			}
		
			return closestPosition;
		}
	}
}
