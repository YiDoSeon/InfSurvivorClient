using Shared.FSM;
using Shared.Packet;
using UnityEngine;

public class EnemyIdleState : EnemyStateBase
{
    private float stateTime;
    public EnemyIdleState(EnemyController entity, StateMachine<EnemyController, EnemyState> stateMachine) : base(entity, stateMachine)
    {
    }

    public override void Enter()
    {
        stateTime = Time.time + Random.Range(0.5f, 1f);
    }

    public override void Update()
    {
        if (stateTime < Time.time)
        {
            stateMachine.ChangeState(EnemyState.Move);
        }
    }
}
