using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Transform _mainCameraTransform;

    void Start()
    {
        // Cache the camera transform for better performance
        if (Camera.main != null)
        {
            _mainCameraTransform = Camera.main.transform;
        }
    }

    // LateUpdate is best for camera-related movement to prevent stuttering
    void LateUpdate()
    {
        if (_mainCameraTransform != null)
        {
            // Makes the UI face the same direction as the camera
            transform.rotation = _mainCameraTransform.rotation;
        }
    }
}
