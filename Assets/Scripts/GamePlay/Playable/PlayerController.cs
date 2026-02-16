using System;
using System.Collections.Generic;
using Gameplay.Core.StateMachine;
using GamePlay.Playable.Characters;
using GamePlay.Playable.Characters.State;
using GamePlay.Vehicle.Car.Weapons;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace GamePlay.Playable
{
    [RequireComponent(typeof(NetworkObject))]
    public class PlayerController : BaseCharacterController
    {
        [SerializeField]
        private GameObject _networkPanel;

        [SerializeField]
        private TextMeshPro _playerNameLabel;

        public PlayerInputData PlayerInput = new();
        public VehicleInputData VehicleInput = new();

        [SerializeField]
        private NetworkVariable<int> _playerNumber;

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                _networkPanel.SetActive(false);
            }
            else
            {
                _networkPanel.SetActive(true);
                _playerNameLabel.text = "Player" + OwnerClientId;
            }

            if (IsServer == false)
                return;


            States = new List<BaseState>()
            {
                new CharacterBaseState(this, Data, CharacterController, CharacterAnimationController,
                    PlayerInput),
                new CharacterEnterVehicleParamState(this),
                new CharacterExitVehicleParamState(this),
                new CharacterDrivingVehicleParamState(this, CharacterController, CharacterAnimationController,
                    VehicleInput),
                new CharacterSeatParamState(this, CharacterController, CharacterAnimationController,
                    VehicleInput),
                new CharacterChangeSeatParamState(this),
                new CharacterSeatMiniGunParamState(this,
                    CharacterController, CharacterAnimationController,
                    VehicleInput),
            };

            OnStateBeginChange.AddListener(OnStateBeginChanged);
            SwitchState<CharacterBaseState>();
        }

        private void Update()
        {
            if (TickableState != null)
                TickableState.Tick(Time.deltaTime);
        }

        private void OnStateBeginChanged(Type type, object param)
        {
            if (type == typeof(CharacterSeatMiniGunParamState))
            {
                if (param is CharacterSeatMiniGunParamState.OutData outData)
                {
                    CallChangeCameraRpc(CameraController.State.Minigun, outData.MiniGunController);
                }
            }
            else
            {
                CallChangeCameraRpc(CameraController.State.Default, this);
            }
        }

        [Rpc(SendTo.Owner)]
        private void CallChangeCameraRpc(CameraController.State state,
            NetworkBehaviourReference networkBehaviourReference)
        {
            if (networkBehaviourReference.TryGet(out NetworkBehaviour behaviour) == false)
                return;
            CameraController.Instance.Activate(state, behaviour.transform);
        }
    }
}