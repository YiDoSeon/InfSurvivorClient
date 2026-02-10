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
        stateTime = Time.time + Random.Range(2f, 5f);
        Vector2 velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        entity.SetVelocity(velocity);
    }

    public override void Update()
    {
        if (stateTime < Time.time)
        {
            stateMachine.ChangeState(EnemyState.Idle);
        }
    }
}
