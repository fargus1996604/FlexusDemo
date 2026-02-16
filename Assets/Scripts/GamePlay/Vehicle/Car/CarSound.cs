using System;
using UnityEngine;

namespace GamePlay.Vehicle.Car
{
    [RequireComponent(typeof(CarEngine))]
    public class CarSound : MonoBehaviour
    {
        private CarEngine _engine;
        protected CarEngine Engine => _engine ??= GetComponent<CarEngine>();

        [SerializeField]
        private AudioSource _engineSource;

        [SerializeField]
        private float _minEngineVolume;
        
        [SerializeField]
        private float _minEnginePitch;

        [SerializeField]
        private float _maxEnginePich;

        [SerializeField]
        private float _smooth;
        
        private void Update()
        {
            float time = Engine.RPM.Value / Engine.MaxRPM;
            float volume = Mathf.Lerp(_minEngineVolume, 1, time);
            float pitch = Mathf.Lerp(_minEnginePitch, _maxEnginePich, time);
         
            _engineSource.volume = Mathf.Lerp(_engineSource.volume, volume, _smooth * Time.deltaTime);
            _engineSource.pitch = Mathf.Lerp(_engineSource.pitch, pitch, _smooth * Time.deltaTime);
        }
    }
}
