using UnityEditor;
using UnityEngine;

namespace Test
{
    public class MoveTest : MonoBehaviour
    {
        [SerializeField]
        private AnimationCurve _curve;
    
        [SerializeField]
        private Vector3[] _moveDeltas;

        [SerializeField]
        private float _duration;
    
        private void Update()
        {
            float speed = _curve.Evaluate(_curve.keys[Time.frameCount % _curve.length].value);
            Debug.Log(speed);
            transform.position += Vector3.forward * speed;
        }

        [ContextMenu("Bake")]
        public void Bake()
        {
            _curve.ClearKeys();
            float time = _duration / _moveDeltas.Length;
            for (int i = 0; i < _moveDeltas.Length; i++)
            {
                _curve.AddKey(time * i, _moveDeltas[i].z);
            }
            EditorUtility.SetDirty(this);
        }
    }
}
