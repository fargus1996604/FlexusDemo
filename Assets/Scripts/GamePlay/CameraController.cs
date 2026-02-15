using Core.Singleton;
using Unity.Cinemachine;
using UnityEngine;

namespace GamePlay
{
    public class CameraController : Singleton<CameraController>
    {
        [SerializeField]
        private CinemachineCamera _defaultCamera;

        [SerializeField]
        private CinemachineCamera _miniGunCamera;

        public void ActivateDefaultCamera(Transform target)
        {
            _defaultCamera.gameObject.SetActive(true);
            _miniGunCamera.gameObject.SetActive(false);
            _defaultCamera.Follow = target;
            _defaultCamera.LookAt = target;
        }
        
        public void ActivateMiniGunCamera(Transform target)
        {
            _defaultCamera.gameObject.SetActive(false);
            _miniGunCamera.gameObject.SetActive(true);
            _miniGunCamera.Follow = target;
            _miniGunCamera.LookAt = target;
        }
    }
}