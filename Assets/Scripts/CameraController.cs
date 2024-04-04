using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    public float cameraSpeed;

    private void LateUpdate()
    {
        var camTransform = transform;
        if (!Physics.Raycast(camTransform.position, camTransform.forward, out var hitInfo, 100.0f)) return;
        
        var diff = Vector3.ClampMagnitude(target.transform.position - hitInfo.point, 1f);
        transform.Translate(new Vector3(diff.x, 0, diff.z) * (Time.deltaTime * cameraSpeed), Space.World);
    }

    public void SetTarget(Transform followTarget)
    {
        target = followTarget;
    }
}
