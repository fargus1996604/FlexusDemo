using System;
using System.Collections.Generic;
using Core.Singleton;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamePlay
{
    public class CameraController : Singleton<CameraController>
    {
        [System.Serializable]
        public enum State
        {
            Default,
            Vehicle,
            Minigun
        }

        [System.Serializable]
        public struct CameraData
        {
            public State State;
            public CinemachineCamera Camera;
        }


        [SerializeField]
        private List<CameraData> _cameras;

        public void Activate(State state, Transform follow)
        {
            Activate(state, follow, follow);
        }

        public void Activate(State state, Transform follow, Transform lookAt)
        {
            foreach (var cameraData in _cameras)
            {
                if (cameraData.State == state)
                {
                    cameraData.Camera.Follow = follow;
                    cameraData.Camera.LookAt = lookAt;
                    cameraData.Camera.gameObject.SetActive(true);
                }
                else
                {
                    cameraData.Camera.gameObject.SetActive(false);
                }
            }
        }
    }
}