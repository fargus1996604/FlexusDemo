using Gameplay.Core.StateMachine;
using Gameplay.Core.StateMachine.Interfaces;
using Unity.Netcode.Components;
using UnityEngine;

namespace GamePlay.Playable.Characters.State.Client
{
    public class ClientNoMoveState : TickableParamBaseState<ClientNoMoveState.Anchor>
    {
        public struct Anchor
        {
            public Vector3 Position;
            public Quaternion Rotation;
        }
        
        private PlayerController _playerController;
        private CharacterController _characterController;
        private NetworkAnimator _networkAnimator;

        public ClientNoMoveState(PlayerController context, CharacterController characterController,
            NetworkAnimator networkAnimator) : base(context)
        {
            _playerController = context;
            _characterController = characterController;
            _networkAnimator = networkAnimator;
        }

        public override void Enter()
        {
            _playerController.transform.localPosition = Data.Position;
            _playerController.transform.localRotation = Data.Rotation;
            _characterController.enabled = false;
            _networkAnimator.enabled = true;
        }

        public override void Exit()
        {
            _characterController.enabled = true;
        }

        public override void Tick(float deltaTime)
        {
        }
    }
}