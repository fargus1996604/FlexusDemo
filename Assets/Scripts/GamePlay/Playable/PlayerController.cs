using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Core.StateMachine;
using GamePlay.Playable.Characters;
using GamePlay.Playable.Characters.State;
using TMPro;
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

            if (IsServer || IsOwner == false)
            {
                EnableNetworkTransformReplication();
                OnStateBeginChange.AddListener(OnStateBeginChanged);
            }
            else
            {
                DisableNetworkTransformReplication();
            }

            
            if (IsServer == false && IsOwner == false)
                return;

            States = new List<BaseState>()
            {
                new CharacterBaseState(this, Data, CharacterController, CharacterAnimationController, NetworkAnimator,
                    PlayerInput.InteractPressed),
                new CharacterEnterVehicleParamState(this),
                new CharacterExitVehicleParamState(this),
                new CharacterDrivingVehicleParamState(this, CharacterController, CharacterAnimationController,
                    NetworkAnimator,
                    VehicleInput),
                new CharacterSeatParamState(this, CharacterController, CharacterAnimationController, NetworkAnimator,
                    VehicleInput),
                new CharacterChangeSeatParamState(this),
                new CharacterSeatMiniGunParamState(this, CharacterController, CharacterAnimationController,
                    NetworkAnimator,
                    VehicleInput),
            };

            SwitchState<BaseMovementState>();
        }

        private void Update()
        {
            if (TickableState != null)
                TickableState.Tick(Time.deltaTime);
        }

        private void EnableNetworkTransformReplication()
        {
            NetworkTransform.SyncPositionX = true;
            NetworkTransform.SyncPositionY = true;
            NetworkTransform.SyncPositionZ = true;
            NetworkTransform.SyncRotAngleX = true;
            NetworkTransform.SyncRotAngleY = true;
            NetworkTransform.SyncRotAngleZ = true;
        }

        private void DisableNetworkTransformReplication()
        {
            NetworkTransform.SyncPositionX = false;
            NetworkTransform.SyncPositionY = false;
            NetworkTransform.SyncPositionZ = false;
            NetworkTransform.SyncRotAngleX = false;
            NetworkTransform.SyncRotAngleY = false;
            NetworkTransform.SyncRotAngleZ = false;
        }

        private void OnStateBeginChanged(Type type, object param)
        {
            if (type == typeof(CharacterSeatMiniGunParamState))
            {
                if (param is CharacterSeatMiniGunParamState.SeatData seatData)
                {
                    CallChangeCameraRpc(CameraController.State.Minigun, seatData.MiniGunController);
                }
            }
            else
            {
                CallChangeCameraRpc(CameraController.State.Default, this);
            }
        }

        protected override void OnStateChangedForNetwork<T, TD>(TD data)
        {
            var state = States.OfType<T>().FirstOrDefault();
            int index = States.IndexOf(state);
            if (data is IStateNetworkData networkData)
            {
                SendStateChangedRpc(index, networkData.Unboxing());
            }
        }

        [Rpc(SendTo.Owner)]
        private void SendStateChangedRpc(int index, NetworkBehaviourReference[] data)
        {
            if(IsServer)
                return;
            
            var state = States[index];
            Debug.Log($"Receive state {state.GetType()} count: {data.Length}");
            if (state is ParamBaseState)
            {
                Debug.Log("Enter");
                SwitchStateWithReferenceData(state.GetType(), data);
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

        [Rpc(SendTo.Owner)]
        public void SendMoveClientRpc(MovementState movementState)
        {
            if (IsHost)
                return;

            if (State is CharacterBaseState clientMovementState)
            {
                clientMovementState.Reconcile(movementState);
            }
        }

        [ServerRpc]
        public void SendInputServerRpc(PlayerInputData.State state)
        {
            if (State is CharacterBaseState characterBaseState)
            {
                characterBaseState.ServerInputStateQueue.Enqueue(state);
            }
        }
    }

    public interface IStateNetworkData
    {
        public void Boxing(NetworkBehaviourReference[] references);
        public NetworkBehaviourReference[] Unboxing();
        public bool IsValid();
    }
}