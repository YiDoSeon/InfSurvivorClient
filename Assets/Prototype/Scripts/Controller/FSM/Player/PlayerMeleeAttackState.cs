using Shared.FSM;
using UnityEngine;

public class PlayerMeleeAttackState : PlayerStateBase
{
    public PlayerMeleeAttackState(LocalPlayerController entity, StateMachine<LocalPlayerController, PlayerState> stateMachine) : base(entity, stateMachine)
    {
    }

    public override void Enter()
    {
        entity.AnimationSetFloat(entity.ANIM_FLOAT_SPEED, 0f);
        entity.AnimationSetTrigger(entity.ANIM_TRIGGER_SWORD_ATTACK);
        entity.AnimationSetBool(entity.ANIM_BOOL_SWORD_ATTACKING, true);
        UpdateVelocity(PlayerState.MeleeAttack);
        lastMoveInput = InputHandler.MoveInput;
        entity.SetDirtySyncMove();
    }

    public override void Update()
    {
        if (InputHandler.FirePressed == false)
        {
            if (InputHandler.MoveInput == Vector2.zero)
            {
                stateMachine.ChangeState(PlayerState.Idle);
            }
            else
            {
                stateMachine.ChangeState(PlayerState.Move);
            }
        }
        else
        {
            CheckMoveInput(PlayerState.MeleeAttack);
        }
    }
    
    public override void FixedUpdate()
    {
    }

    public override void Exit()
    {
        entity.AnimationSetBool(entity.ANIM_BOOL_SWORD_ATTACKING, false);
    }
}
