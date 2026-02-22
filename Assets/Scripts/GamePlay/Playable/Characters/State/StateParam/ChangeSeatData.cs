using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using Unity.Netcode;
using UnityEngine;

namespace GamePlay.Playable.Characters.State.StateParam
{
    public struct ChangeSeatData : IStateNetworkData
    {
        public CarVehicle Vehicle;
        public Seat Seat;

        public void Boxing(NetworkBehaviourReference[] references)
        {
            references[0].TryGet(out Vehicle);
            references[1].TryGet(out Seat);
        }
        
        public NetworkBehaviourReference[] Unboxing()
        {
            return new NetworkBehaviourReference[]
            {
                Vehicle,
                Seat
            };
        }

        public bool IsValid()
        {
            return Vehicle != null && Seat != null;
        }
    }
}
