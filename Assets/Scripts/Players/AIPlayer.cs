using System;
using System.Collections;
using System.Linq;
using Interfaces;
using UnityEngine;
using UnityEngine.AI;

namespace Players
{
	public class AIPlayer : BasePlayer
	{
		[SerializeField] private float _searchRadius;

		[SerializeField] private Transform _target;
			
		[SerializeField] private float _distanceToTarget;
		private Camera _cam;
		private NavMeshPath _path;
		private NavMeshAgent _agent;
	
		private int GoldPieceMask => 1 << LayerMask.NameToLayer("GoldPiece");

		private Vector3Int _centerPos;

		private bool _active = true;

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
			_target = transform;
			StartCoroutine(AIPause(1f));
		}

		protected override void Update()
		{
			base.Update();

			// Try to get gold
			_target = GetNearestGoldPiece();

			// if no gold, but center, move to center
			if (_target == transform && TilemapManager.GetTile(_centerPos) is not null)
			{
				_target = TilemapManager.GetTile(_centerPos).transform;
			}

			// if no gold or center, stop
			if (_target)
				_distanceToTarget = Vector3.Distance(_target.position, transform.position);

			_agent.enabled = Grounded;
			
			if (!_agent.enabled) return; 
		
			if (Input.GetMouseButton(0) && _cam && _agent.enabled)
			{
				if (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out var hit, 100f, TileMask))
				{
					_target = hit.transform;
				}
			}

			_agent.destination = _target.position;
		
			switch (_path.corners.Length)
			{
				case > 1:
					DesiredDirection = (_path.corners[1] - transform.position).normalized; break;
				case > 0:
					DesiredDirection = (_path.corners.Last() - transform.position).normalized; break;
			}
			DesiredDirection.y = 0;
		
			
			NavMesh.CalculatePath(transform.position, _target.position, NavMesh.AllAreas, _path);
			if (_drawPath) DrawPath();
		}

		protected void FixedUpdate()
		{
			if (!Grounded) return;

			if (!_active) DesiredDirection = Vector3.zero;
			
			Move();
			Rotate(DesiredDirection);
		
			if(_agent.isOnOffMeshLink)
			{
				StartCoroutine(RigidBodyJump());
			}
		}

		private void OnCollisionStay(Collision other)
		{
			if (other.gameObject.TryGetComponent(out IHittable hittable))
			{
				SwingPickaxe();
			}
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
			Rotate(direction);
	
			_agent.enabled = false;
			Rb.velocity = MoveSpeed * direction;
			Jump();
	
			yield return new WaitForSeconds(0.2f);
			yield return new WaitUntil(() => Grounded);
	
			_agent.CompleteOffMeshLink();
		}
	
		private void DrawPath()
		{
			for (int i = 0; i < _path.corners.Length - 1; i++)
			{
				Debug.DrawLine(_path.corners[i], _path.corners[i + 1], MeshRenderer.material.color);
			}
		}

		private Transform GetNearestGoldPiece()
		{
			float closestDistance = float.MaxValue;
			Transform nextTarget = _target;
		
			int maxSearch = 10;
			Collider[] hits = new Collider[maxSearch];
			int numFound = Physics.OverlapSphereNonAlloc(transform.position, _searchRadius, hits, GoldPieceMask);
			for (int i = 0; i < numFound; i++)
			{
				var distance = Vector3.Distance(transform.position, hits[i].transform.position);
				if (distance < closestDistance)
				{
					closestDistance = distance;
					nextTarget = hits[i].transform;
				}
			}
		
			return nextTarget;
		}

		public override void AddGold()
		{
			base.AddGold();
			StartCoroutine(AIPause(0.25f));
		}


		private IEnumerator AIPause(float pauseTime)
		{
			_active = false;
			yield return new WaitForSeconds(pauseTime);
			_active = true;
		}

		public override void TogglePlayerEnabled(bool enable)
		{
			base.TogglePlayerEnabled(enable);
			_target = transform;
			if (enable)
				StartCoroutine(AIPause(1f));
		}
	}

	// Deprecated, use RigidBodyJump
	// private IEnumerator ParabolaJump(float height, float duration)
	// {
	// 	OffMeshLinkData data = _agent.currentOffMeshLinkData;
	// 	Vector3 startPos = _agent.transform.position;
	// 	Vector3 endPos = data.endPos + Vector3.up * _agent.baseOffset;
	// 	float normalizedTime = 0f;
	// 	_agent.enabled = false;
	// 	while (normalizedTime < 1f)
	// 	{
	// 		float yOffset = height * 4f * (normalizedTime - normalizedTime * normalizedTime);
	// 		Rb.velocity = Vector3.Lerp(startPos, endPos, normalizedTime) + new Vector3(0, yOffset, 0);
	//
	// 		normalizedTime += Time.deltaTime / duration;
	// 		yield return null;
	// 	}
	//
	// 	_agent.enabled = true;
	// 	_agent.CompleteOffMeshLink();
	// }
}
