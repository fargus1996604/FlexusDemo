using GamePlay.Playable;
using UnityEngine;

namespace GamePlay.Vehicle.Car.Seats
{
    public class Seat : MonoBehaviour
    {
        [SerializeField]
        private Transform _pivot;

        [SerializeField]
        private Transform _doorPlayerPivot;

        [SerializeField]
        private PlayerController _player;

        public bool HasFree => _player == null;

        public bool TryAttach(PlayerController player)
        {
            if (HasFree == false)
                return false;

            player.transform.SetParent(_pivot);
            player.transform.localPosition = Vector3.zero;
            player.transform.localRotation = Quaternion.identity;
            _player = player;
            return true;
        }

        public void Detach()
        {
            if (HasFree)
                return;
            
            _player.transform.SetParent(null);
            _player.transform.position = _doorPlayerPivot.position;
            //_player.transform.rotation = Quaternion.identity;
            _player = null;
        }
    }
}