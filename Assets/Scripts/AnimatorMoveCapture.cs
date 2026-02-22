using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorMoveCapture : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private string _stateName;

    [SerializeField]
    private List<Vector3> moveDeltas = new List<Vector3>();

    [SerializeField]
    private float _calculatedSpeed;

    private Vector3 _startPos;
    private Vector3 _endPos;
    
    [SerializeField]
    private bool _testEnd;

    private float _testTime;

    private void Awake()
    {
        _startPos = transform.position;
        _testTime = Time.time;
    }

    private void Update()
    {
        if (_testEnd)
            return;

        if (_animator.GetCurrentAnimatorStateInfo(0).IsName(_stateName) == false)
        {
            _endPos = transform.position;
            Vector3 calculatedPos = Vector3.zero;
            foreach (var moveDelta in moveDeltas)
            {
                calculatedPos += moveDelta;
            }

            var travelDistance = Vector3.Distance(_startPos, _endPos);
            var time = Time.time - _testTime;
            Debug.Log(
                $"Distance Trevel: {Vector3.Distance(_startPos, _endPos)} calculated distance: {calculatedPos} time: {Time.time - _testTime}");
            _calculatedSpeed = travelDistance / time;
            _testEnd = true;
        }
    }

    private void OnAnimatorMove()
    {
        if (_testEnd)
            return;

        moveDeltas.Add(_animator.deltaPosition);
        transform.position += _animator.deltaPosition;
    }
}