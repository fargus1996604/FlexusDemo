using System.Collections.Generic;
using GamePlay;
using GamePlay.Input;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace Test
{
    public class TestPrediction : NetworkBehaviour
    {
        [System.Serializable]
        public struct InputData : INetworkSerializable
        {
            public int Tick;
            public bool Move;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref Tick);
                serializer.SerializeValue(ref Move);
            }
        }

        [System.Serializable]
        public struct MoveDataState : INetworkSerializable
        {
            public int Tick;
            public Vector3 Position;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref Tick);
                serializer.SerializeValue(ref Position);
            }
        }

        [SerializeField]
        private float _speed;

        private InputData _inputData;
        private MoveDataState _currentState;
        private MoveDataState _serverState;
        private Queue<InputData> _serverInputQueue;
        private CircularBuffer<InputData> _inputBuffer;
        private CircularBuffer<MoveDataState> _stateBuffer;
        private UserInputSystem _inputSystem;
        private NetworkTimer _networkTimer;

        private int _tickRate = 60;
        private float _tickDelta => 1f / _tickRate;


        public override void OnNetworkSpawn()
        {
            _networkTimer = new NetworkTimer();
            _inputBuffer = new CircularBuffer<InputData>(1024);
            _stateBuffer = new CircularBuffer<MoveDataState>(1024);
            _serverInputQueue = new Queue<InputData>();
            if (IsOwner)
            {
                CameraController.Instance.Activate(CameraController.State.Default, transform);
                _inputSystem = new UserInputSystem();
                _inputSystem.Enable();
            }

            _currentState.Position = transform.position;
            _serverState.Position = transform.position;
        }

        private void Update()
        {
            _networkTimer.Update(Time.deltaTime);
            if (_networkTimer.ShouldTick())
            {
                HandleClient();
                HandleServer();
            }
        }

        private void LateUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, _currentState.Position, 0.8f);
        }

        private void HandleClient()
        {
            if (!IsOwner || !IsClient)
                return;

            _inputData = new InputData()
            {
                Tick = _networkTimer.CurrentTick,
                Move = _inputSystem.Player.Sprint.ReadValue<float>() > 0
            };

            _inputBuffer.Add(_inputData, _networkTimer.CurrentTick);
            _currentState = ProcessMovement(_currentState, _inputData, _tickDelta);
            _stateBuffer.Add(_currentState, _networkTimer.CurrentTick);
            SendMoveServerRpc(_inputData);
        }

        private void HandleServer()
        {
            if (!IsServer)
                return;

            while (_serverInputQueue.Count > 0)
            {
                var inputData = _serverInputQueue.Dequeue();
                _serverState = ProcessMovement(_serverState, inputData, _tickDelta);
            }

            SendClientRpc(_serverState);
        }

        [ServerRpc]
        public void SendMoveServerRpc(InputData inputData)
        {
            _serverInputQueue.Enqueue(inputData);
        }

        [ClientRpc]
        public void SendClientRpc(MoveDataState dataState)
        {
            Reconcile(dataState);
        }

        private void Reconcile(MoveDataState serverState)
        {
            if (IsServer && IsOwner)
                return;

            float error = Vector3.Distance(serverState.Position, _stateBuffer.Get(serverState.Tick).Position);

            if (error < 0.1f)
                return;

            _currentState = serverState;
            _stateBuffer.Add(serverState, serverState.Tick);

            int tick = serverState.Tick + 1;
            while (tick < _networkTimer.CurrentTick)
            {
                _currentState = ProcessMovement(_currentState, _inputBuffer.Get(tick), _tickDelta);
                _stateBuffer.Add(_currentState, tick);
                tick++;
            }
        }

        private MoveDataState ProcessMovement(MoveDataState stete, InputData inputData, float tickDelta)
        {
            if (inputData.Move)
            {
                stete.Position += (Vector3.forward * _speed) * tickDelta;
            }

            stete.Tick = inputData.Tick;
            return stete;
        }
    }
}