using UnityEngine;

public class PlayerMoveState : PlayerStateBase
{
    private Vector2 lastMoveInput;
    public PlayerMoveState(LocalPlayerController entity, StateMachine<LocalPlayerController, PlayerState> stateMachine) : base(entity, stateMachine)
    {
    }

    public override void Enter()
    {
        entity.AnimationSetFloat(entity.ANIM_FLOAT_SPEED, entity.LastVelocity.sqrMagnitude);
        entity.ApplyFacingDirection(InputHandler.MoveInput);
        lastMoveInput = InputHandler.MoveInput;
        entity.SetDirtySyncMove();
    }

    public override void Update()
    {
        if (InputHandler.FirePressed)
        {
            stateMachine.ChangeState(PlayerState.MeleeAttack);
        }
        else if (InputHandler.MoveInput == Vector2.zero)
        {
            stateMachine.ChangeState(PlayerState.Idle);
        }
        else
        {
            CheckMoveInput(PlayerState.Move);
        }
    }

    public override void FixedUpdate()
    {
    }

    public override void Exit()
    {
        entity.AnimationSetFloat(entity.ANIM_FLOAT_SPEED, 0f);
    }
}
