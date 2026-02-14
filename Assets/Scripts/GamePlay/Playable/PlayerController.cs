using System.Collections.Generic;
using Gameplay.Core;
using Gameplay.Core.StateMachine;
using GamePlay.Input;
using GamePlay.Playable.Characters;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Playable.Characters.State;
using UnityEngine;

namespace GamePlay.Playable
{
    public class PlayerController : BaseCharacterController
    {
        [SerializeField]
        private InputController _inputController;

        private VehicleInputHandler _vehicleInputHandler;

        private void Start()
        {
            _vehicleInputHandler = _inputController.GetVehicleInputHandler();
            States = new List<BaseState>()
            {
                new CharacterExploringState(this, Data, CharacterController, CharacterAnimationController,
                    _inputController.GetPlayerInputHandler(), Camera.main),
                new CharacterEnterVehicleParamState(this),
                new CharacterExitVehicleParamState(this),
                new CharacterDrivingVehicleParamState(this, CharacterController, CharacterAnimationController,
                    _vehicleInputHandler),
                new CharacterSeatParamState(this, CharacterController, CharacterAnimationController,
                    _vehicleInputHandler),
                new CharacterChangeSeatParamState(this)
            };

            SwitchState<CharacterExploringState>();
        }

        private void Update()
        {
            if (TickableState != null)
                TickableState.Tick(Time.deltaTime);
        }
    }
}