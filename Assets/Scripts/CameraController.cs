using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    public PlayerController target;
    public TextMeshProUGUI tmp;
    private Vector3 initPos;
    
    // Start is called before the first frame update
    void Start()
    {
        initPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        var camTransform = transform;
        if (Physics.Raycast(transform.position, transform.forward, out var hitInfo, 100.0f))
        {
            Debug.DrawRay(camTransform.position, camTransform.forward * 100.0f, Color.yellow);
            var diff = target.transform.position - hitInfo.point;
            tmp.text = diff.ToString();
            transform.Translate(diff.x, 0, diff.z,Space.World);
        }
    }
}
