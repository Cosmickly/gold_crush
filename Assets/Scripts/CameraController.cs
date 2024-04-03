using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    public PlayerController target;
    // public TextMeshProUGUI tmp;
    // private Vector3 initPos;
    public float cameraSpeed;
    
    private void Start()
    {
        // initPos = transform.position;
    }

    private void LateUpdate()
    {
        var camTransform = transform;
        if (!Physics.Raycast(camTransform.position, camTransform.forward, out var hitInfo, 100.0f)) return;
        
        // Debug.DrawRay(camTransform.position, camTransform.forward * 100.0f, Color.yellow);
        var diff = Vector3.ClampMagnitude(target.transform.position - hitInfo.point, 1f);
        transform.Translate(new Vector3(diff.x, 0, diff.z) * (Time.deltaTime * cameraSpeed), Space.World);
        // tmp.text = "cam/player diff " + diff;
    }
}
