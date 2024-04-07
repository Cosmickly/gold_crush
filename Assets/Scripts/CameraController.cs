using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _smoothTime;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private Vector3 _velocity;
    
    // public TextMeshProUGUI TMP;
    
    private void FixedUpdate()
    {
        if(!_target) return;
        var currentPos = transform.position;
        if (!Physics.Raycast(currentPos, transform.forward, out var hitInfo, 100.0f)) return;

        var diff = _target.transform.position - hitInfo.point;

        transform.position = Vector3.SmoothDamp(currentPos, 
            new Vector3(currentPos.x + diff.x, currentPos.y, currentPos.z + diff.z),
            ref _velocity, _smoothTime, _maxSpeed);
        
        // TMP.text = "SD vel " + _velocity;
    }

    public void SetTarget(Transform followTarget)
    {
        _target = followTarget;
    }
}
