using System;
using GamePlay.Playable.Characters.Animation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GamePlay.Playable.Characters
{
    public class CharacterSound : MonoBehaviour
    {
        [SerializeField]
        private CharacterAnimationController _characterAnimationController;

        [SerializeField]
        private AudioSource _audioSource;

        [SerializeField]
        private AudioClip[] _stepsClips;

        private int _stepIndex;

        private float _lastStepTime;
        
        private void Start()
        {
            _characterAnimationController.OnFootStep.AddListener(OnFootStep);
        }

        private void OnFootStep()
        {
            float movingMagnitude = _characterAnimationController.DeltaPosition.magnitude;
            if(movingMagnitude < 0.01f)
                return;
            
            _audioSource.volume = movingMagnitude < 0.02f ? 0.5f : 1f;
            
            if(Time.time - _lastStepTime < 0.1f)
                return;

            _audioSource.PlayOneShot(_stepsClips[_stepIndex]);
            _lastStepTime = Time.time;
            _stepIndex = Random.Range(0, _stepsClips.Length);
        }
    }
}