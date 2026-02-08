using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    public PlayerIdleState(LocalPlayerController entity, StateMachine<LocalPlayerController, PlayerState> stateMachine) : base(entity, stateMachine)
    {
    }

    public override void Enter()
    {
        entity.UpdateVelocity(PlayerState.Idle);
        entity.SetDirtySyncMove();
    }

    public override void Update()
    {
        if (InputHandler.FirePressed)
        {
            stateMachine.ChangeState(PlayerState.MeleeAttack);
        }
        else if (InputHandler.MoveInput.sqrMagnitude > 0.01f)
        {
            stateMachine.ChangeState(PlayerState.Move);
        }
    }

    public override void Exit()
    {
    }
}
