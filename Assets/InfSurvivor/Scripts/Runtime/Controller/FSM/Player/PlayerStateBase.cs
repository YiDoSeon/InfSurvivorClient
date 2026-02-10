using Shared.FSM;
using Shared.Packet;
using UnityEngine;

namespace InfSurvivor.Runtime.Controller.FSM
{
    public class PlayerStateBase : FiniteState<LocalPlayerController, PlayerState>
    {
        protected Vector2 lastMoveInput;
        public PlayerInputHandler InputHandler => entity.InputHandler;
        public PlayerStateBase(LocalPlayerController entity, StateMachine<LocalPlayerController, PlayerState> stateMachine) : base(entity, stateMachine)
        {
        }
    
        public override void Enter() { }
        public override void Update() { }
        public override void FixedUpdate() { }
        public override void Exit() { }
    
        protected void UpdateVelocity(PlayerState state)
        {
            entity.UpdateVelocity(state);
            entity.ApplyFacingDirection(InputHandler.MoveInput);        
        }
    
        protected void CheckMoveInput(PlayerState state)
        {
            UpdateVelocity(state);
            if (lastMoveInput != InputHandler.MoveInput)
            {
                lastMoveInput = InputHandler.MoveInput;
                entity.SetDirtySyncMove();
            }
        }
    }
    
}