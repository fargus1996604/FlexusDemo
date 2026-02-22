using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using Unity.Netcode;

namespace GamePlay.Playable.Characters.State.StateParam
{
    public struct VehicleSeatData : IStateNetworkData
    {
        public CarVehicle Vehicle;
        public DriverSeat DriverSeat;

        public void Boxing(NetworkBehaviourReference[] references)
        {
            references[0].TryGet(out Vehicle);
            references[1].TryGet(out DriverSeat);
        }

        public NetworkBehaviourReference[] Unboxing()
        {
            return new NetworkBehaviourReference[]
            {
                Vehicle,
                DriverSeat
            };
        }

        public bool IsValid()
        {
            return Vehicle != null && DriverSeat != null;
        }
    }
}