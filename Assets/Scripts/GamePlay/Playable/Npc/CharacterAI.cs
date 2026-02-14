using System;
using System.Collections.Generic;
using Gameplay.Core.StateMachine;
using GamePlay.Playable.Characters;
using GamePlay.Playable.Characters.State;
using GamePlay.Playable.Npc.State;
using UnityEngine;
using UnityEngine.Events;

namespace GamePlay.Playable.Npc
{
    public class CharacterAI : BaseCharacterController
    {
        [System.Serializable]
        public class InteractionData
        {
            public UnityEvent InteractPressed;
            public Vector2 MoveDirection;
            public bool IsSprinting;
        }

        [SerializeField]
        private InteractionData _test;

        private void Start()
        {
            States = new List<BaseState>()
            {
                new NpcIdleState(this, Data, _test, CharacterController, CharacterAnimationController)
            };

            SwitchState<NpcIdleState>();
        }

        private void Update()
        {
            if (TickableState != null)
                TickableState.Tick(Time.deltaTime);
        }
    }
}