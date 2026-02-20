using GamePlay.Playable;
using GamePlay.Playable.Characters;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace GamePlay.Vehicle.Car.Seats
{
    public class Seat : NetworkBehaviour
    {
        [SerializeField]
        private Transform _pivot;
        public Transform Pivot => _pivot;

        [SerializeField]
        private Transform _doorPlayerPivot;

        [SerializeField]
        private NetworkObject _parentNetworkObject;

        [SerializeField]
        private BaseCharacterController _player;

        public BaseCharacterController Player => _player;

        public bool HasFree => _player == null;

        public bool TryAttach(BaseCharacterController player)
        {
            if (HasFree == false)
                return false;

            if (player.NetworkObject.TrySetParent(_parentNetworkObject))
            {
                player.transform.localPosition = _pivot.localPosition;
                player.transform.localRotation = _pivot.localRotation;
                _player = player;
                return true;
            }

            return false;
        }

        public bool TryDetach()
        {
            if (HasFree)
                return false;

            if (_player.NetworkObject.TryRemoveParent())
            {
                _player.transform.position = _doorPlayerPivot.position;
                _player = null;
                return true;
            }

            return false;
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