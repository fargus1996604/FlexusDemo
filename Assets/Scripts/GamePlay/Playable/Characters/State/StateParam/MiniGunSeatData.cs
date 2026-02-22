using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using GamePlay.Vehicle.Car.Weapons;
using Unity.Netcode;
using UnityEngine;

namespace GamePlay.Playable.Characters.State.StateParam
{
    public struct MiniGunSeatData : IStateNetworkData
    {
        public CarVehicle Vehicle;
        public MiniGunSeat MiniGunSeat;
        public MiniGunController MiniGunController;

        public void Boxing(NetworkBehaviourReference[] references)
        {
            references[0].TryGet(out Vehicle);
            references[1].TryGet(out MiniGunSeat);
            references[2].TryGet(out MiniGunController);
        }

        public NetworkBehaviourReference[] Unboxing()
        {
            return new NetworkBehaviourReference[]
            {
                Vehicle,
                MiniGunSeat,
                MiniGunController
            };
        }

        public bool IsValid()
        {
            return Vehicle != null && MiniGunSeat != null && MiniGunController != null;
        }
    }
}
