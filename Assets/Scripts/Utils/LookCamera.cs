using System;
using UnityEngine;

namespace Utils
{
    public class LookCamera : MonoBehaviour
    {
        private Camera _camera;
        
        private void OnEnable()
        {
            _camera = Camera.main;
        }

        private void UpdateData()
        {
            transform.forward = _camera.transform.position - transform.position;
        }

        private void Update()
        {
            UpdateData();
        }
    }
}