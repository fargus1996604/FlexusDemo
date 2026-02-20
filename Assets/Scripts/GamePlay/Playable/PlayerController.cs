using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Core.StateMachine;
using GamePlay.Playable.Characters;
using GamePlay.Playable.Characters.State;
using GamePlay.Playable.Characters.State.Client;
using GamePlay.Playable.Characters.State.Server;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
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

            if (IsServer || IsHost)
            {
                States = new List<BaseState>()
                {  new ServerSeatMiniGunParamState(this, CharacterController, CharacterAnimationController,
                        VehicleInput),
                    new CharacterBaseState(this, Data, CharacterController, CharacterAnimationController,
                        PlayerInput.InteractPressed),
                    new CharacterEnterVehicleParamState(this),
                    new CharacterExitVehicleParamState(this),
                    new CharacterDrivingVehicleParamState(this, CharacterController, CharacterAnimationController,
                        VehicleInput),
                    new CharacterSeatParamState(this, CharacterController, CharacterAnimationController,
                        VehicleInput),
                    new CharacterChangeSeatParamState(this)
                  
                };
                OnStateBeginChange.AddListener(OnStateBeginChanged);
                SwitchState<BaseMovementState>();
                EnableNetworkTransformReplication();
            }
            else if (IsOwner)
            {
                States = new List<BaseState>()
                {
                    new ClientSeatMiniGunParamState(this,
                        CharacterController, CharacterAnimationController,
                        VehicleInput),
                    new ClientNoMoveState(this, CharacterController, NetworkAnimator),
                    new ClientMovementState(this, Data, CharacterController, CharacterAnimationController, PlayerInput,
                        NetworkAnimator)
                };
                SwitchState<BaseMovementState>();
                DisableNetworkTransformReplication();
            }
            else
            {
                EnableNetworkTransformReplication();
            }
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
            if (type == typeof(ServerSeatMiniGunParamState))
            {
                if (param is ServerSeatMiniGunParamState.SeatData seatData)
                {
                    CallChangeCameraRpc(CameraController.State.Minigun, seatData.MiniGunController);
                }
            }
            else
            {
                CallChangeCameraRpc(CameraController.State.Default, this);
            }

            if (type == typeof(CharacterBaseState))
            {
                CallPlayerControlSwitchClientRpc(true, default, default);
            }
            else if (type == typeof(CharacterDrivingVehicleParamState))
            {
                if (param is CharacterDrivingVehicleParamState.VehicleData vehicleData)
                {
                    var seatTransform = vehicleData.DriverSeat.transform;
                    CallPlayerControlSwitchClientRpc(false, seatTransform.localPosition, seatTransform.localRotation);
                }
            }
            else if (type == typeof(CharacterSeatParamState))
            {
                if (param is CharacterSeatParamState.VehicleData vehicleData)
                {
                    var seatTransform = vehicleData.Seat.transform;
                    CallPlayerControlSwitchClientRpc(false, seatTransform.localPosition, seatTransform.localRotation);
                }
            }
            else if (type == typeof(ServerSeatMiniGunParamState))
            {
                if (param is ServerSeatMiniGunParamState.SeatData seatData)
                {
                    var seatTransform = seatData.MiniGunSeat.Pivot;
                    CallPlayerControlSwitchClientRpc(false, seatTransform.localPosition, seatTransform.localRotation);
                }
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
            var state = States[index];
            Debug.Log($"Receive state {state.GetType()} count: {data.Length}");
            if (state is ParamBaseState)
            {
                Debug.Log("Enter");
                SwitchStateWithReferenceData(state.GetType(), data);
            }
        }

        [Rpc(SendTo.Owner)]
        private void CallPlayerControlSwitchClientRpc(bool canControl, Vector3 localPosition, Quaternion localRotation)
        {
            if (IsHost)
                return;

            if (canControl)
            {
                if (State is not BaseMovementState)
                {
                    SwitchState<BaseMovementState>();
                }
            }
            else
            {
                var anchor = new ClientNoMoveState.Anchor()
                {
                    Position = localPosition,
                    Rotation = localRotation
                };
                SwitchStateWithData<ClientNoMoveState, ClientNoMoveState.Anchor>(anchor);
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

            if (State is ClientMovementState clientMovementState)
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