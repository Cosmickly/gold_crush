using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _smoothTime;
    [SerializeField] private float _maxSpeed;

    private Vector3 _initialPos;
    private Vector3 _velocity;

    private void Awake()
    {
        _initialPos = transform.position;
    }

    private void FixedUpdate()
    {
        if (!_target) return;
        var currentPos = transform.position;
        if (!Physics.Raycast(currentPos, transform.forward, out var hitInfo, 500.0f))
        {
            transform.position = Vector3.SmoothDamp(currentPos, _initialPos,
                ref _velocity, _smoothTime, _maxSpeed);
            return;
        }

        var diff = _target.transform.position - hitInfo.point;

        transform.position = Vector3.SmoothDamp(currentPos,
            new Vector3(currentPos.x + diff.x, currentPos.y, currentPos.z + diff.z),
            ref _velocity, _smoothTime, _maxSpeed);
    }

    public void SetTarget(Transform followTarget)
    {
        _target = followTarget;
    }
}