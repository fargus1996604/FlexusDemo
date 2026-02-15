using System.Collections.Generic;
using Gameplay.Core;
using Gameplay.Core.StateMachine;
using GamePlay.Input;
using GamePlay.Input.InputHandler;
using GamePlay.Playable.Characters;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Playable.Characters.State;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace GamePlay.Playable
{
    public class PlayerController : BaseCharacterController
    {
        [SerializeField]
        private GameObject _networkPanel;
        
        [SerializeField]
        private TextMeshPro _playerNameLabel;

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                _networkPanel.SetActive(false);
            }
            else
            {
                _networkPanel.SetActive(true);
                _playerNameLabel.text = "Player"+OwnerClientId;
            }
        }

        private void Start()
        {
            if(IsOwner == false)
                return;
            
            var inputController = InputController.Instance;
            var cameraController = CameraController.Instance;
            
            var vehicleInputHandler = inputController.GetVehicleInputHandler();
            var playerInputHandler = inputController.GetPlayerInputHandler();
            
            States = new List<BaseState>()
            {
                new CharacterBaseState(this, Data, CharacterController, CharacterAnimationController,
                    playerInputHandler, cameraController, Camera.main),
                new CharacterEnterVehicleParamState(this),
                new CharacterExitVehicleParamState(this),
                new CharacterDrivingVehicleParamState(this, CharacterController, CharacterAnimationController,
                    vehicleInputHandler, cameraController),
                new CharacterSeatParamState(this, CharacterController, CharacterAnimationController,
                    vehicleInputHandler),
                new CharacterChangeSeatParamState(this),
                new CharacterSeatMiniGunParamState(this,
                    CharacterController, CharacterAnimationController,
                    vehicleInputHandler, cameraController),
            };

            SwitchState<CharacterBaseState>();
        }

        private void Update()
        {
            if (TickableState != null)
                TickableState.Tick(Time.deltaTime);
        }
    }
}