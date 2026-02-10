using Shared.FSM;
using Shared.Packet;
using UnityEngine;

public class EnemyDamagedState : EnemyStateBase
{
    private float time;
    public EnemyDamagedState(EnemyController entity, StateMachine<EnemyController, EnemyState> stateMachine) : base(entity, stateMachine)
    {
    }

    public override void Enter()
    {
        time = Time.time + 0.1f;
        entity.SetFlash(1f);
    }

    public override void Update()
    {
        if (time < Time.time)
        {
            entity.SetFlash(0f);
            stateMachine.ChangeState(EnemyState.Idle);
        }
    }

    public override void FixedUpdate()
    {
        entity.KnockBack();
    }
}
