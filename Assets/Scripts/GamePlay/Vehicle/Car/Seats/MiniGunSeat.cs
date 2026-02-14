using GamePlay.Vehicle.Car.Weapons;
using UnityEngine;

namespace GamePlay.Vehicle.Car.Seats
{
    public class MiniGunSeat : Seat
    {
        [SerializeField]
        private MiniGunController _controller;
        public MiniGunController Controller => _controller;
    }
}
