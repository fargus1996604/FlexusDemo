using System;
using Gameplay.Core;
using UnityEngine;

namespace GamePlay.Vehicle.Car.Weapons
{
    public class MiniGunSound : MonoBehaviour
    {
        [SerializeField]
        private AudioSource _startSource;

        [SerializeField]
        private AudioSource _loopSource;

        [SerializeField]
        private AudioSource _endSource;

        public void Tick(float deltaTime, MiniGunController controller)
        {
            if (controller.Data.Fire && controller.CanFire == false && _startSource.isPlaying == false)
            {
                _startSource.Play();
            }
            else if (controller.Data.Fire == false && _endSource.isPlaying == false && controller.Charge > 0.001f)
            {
                _endSource.time = Mathf.Lerp(0, _endSource.clip.length,  Mathf.Clamp01(1 - controller.Charge));
                _endSource.Play();
                _startSource.Stop();
            }

            if (controller.CanFire && _loopSource.isPlaying == false)
            {
                _loopSource.Play();
            }
            else if (controller.CanFire == false && _loopSource.isPlaying)
            {
                _loopSource.Stop();
            }
        }
        
        public void StopAllSounds()
        {
            _startSource.Stop();
            _loopSource.Stop();
            _endSource.Stop();
        }
    }
}