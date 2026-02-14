using GamePlay.Playable;
using GamePlay.Playable.Characters;
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
        private BaseCharacterController _player;

        public BaseCharacterController Player => _player;

        [SerializeField]
        private bool _test;

        public bool HasFree => _test == false && _player == null;

        public bool TryAttach(BaseCharacterController player)
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
            if (_player == null)
                return;

            _player.transform.SetParent(null);
            _player.transform.position = _doorPlayerPivot.position;
            _player = null;
        }

        public bool TransferTo(Seat seat)
        {
            if (seat == null || HasFree)
                return false;

            if (seat.TryAttach(_player) == false)
                return false;

            _player = null;

            return true;
        }
    }
}