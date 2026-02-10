using Shared.FSM;
using Shared.Packet;
using UnityEngine;

public class EnemyMoveState : EnemyStateBase
{
    private float stateTime;
    public EnemyMoveState(EnemyController entity, StateMachine<EnemyController, EnemyState> stateMachine) : base(entity, stateMachine)
    {
    }

    public override void Enter()
    {
    }

    public override void FixedUpdate()
    {
    }
}
