using Gameplay.Core.StateMachine;
using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Input;
using GamePlay.Playable.Characters;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Playable.Characters.State;
using GamePlay.Vehicle.Car;
using UnityEngine;

namespace GamePlay.Playable.Npc.State
{
    public class NpcIdleState : TickableBaseState
    {
        private BaseCharacterController.PlayerData _data;
        private CharacterAI.InteractionData _interactionData;
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;

        public NpcIdleState(IStateContext context, BaseCharacterController.PlayerData data,
            CharacterAI.InteractionData interactionData,
            CharacterController characterController,
            CharacterAnimationController characterAnimationController) : base(context)
        {
            _data = data;
            _interactionData = interactionData;
            _characterController = characterController;
            _characterAnimationController = characterAnimationController;
        }

        public override void Tick(float deltaTime)
        {
            if (_characterController.isGrounded && _data.Velocity.y < 0)
            {
                _data.Velocity.y = -2f;
            }

            _data.Velocity.y += _data.Gravity * Time.deltaTime;
            _characterController.Move(_characterAnimationController.DeltaPosition + (_data.Velocity * deltaTime));
            _characterAnimationController.Move(_interactionData.MoveDirection, _characterController.transform.forward);
            _characterAnimationController.SetDash(_interactionData.IsSprinting);
        }

        public override void Enter()
        {
            _characterController.enabled = true;
            _characterAnimationController.SwitchToBaseLayer();
            _interactionData.InteractPressed.AddListener(FindClosestVehicles);
        }

        public override void Exit()
        {
            _interactionData.InteractPressed.RemoveListener(FindClosestVehicles);
        }

        private void FindClosestVehicles()
        {
            var colliders =
                Physics.OverlapSphere(_characterController.transform.position, _data.VehicleDetectionRadius);
            if (colliders.Length == 0)
                return;

            CarVehicle nearVehicle = null;
            foreach (var collider in colliders)
            {
                if (collider.attachedRigidbody == null)
                    continue;

                if (collider.attachedRigidbody.TryGetComponent(out CarVehicle vehicle) == false)
                    continue;

                nearVehicle = vehicle;
            }

            if (nearVehicle == null)
                return;

            if (nearVehicle.HasFreeSeat())
            {
                Context.SwitchStateWithData<CharacterEnterVehicleParamState, CarVehicle>(nearVehicle);
            }
        }
    }
}