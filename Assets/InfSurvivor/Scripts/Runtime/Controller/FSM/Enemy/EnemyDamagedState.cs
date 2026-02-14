using Shared.FSM;
using Shared.Packet;
using UnityEngine;

namespace InfSurvivor.Runtime.Controller.FSM
{
    public class EnemyDamagedState : EnemyStateBase
    {
        private Vector2 predictedPosition;
        public EnemyDamagedState(EnemyController entity, StateMachine<EnemyController, EnemyState> stateMachine) : base(entity, stateMachine)
        {
        }

        public override void Enter()
        {
            entity.SetFlash(1f);
        }

        public override void FixedUpdate()
        {
            if ((entity.LogicalPos - entity.PredictedLogicalPos).sqrMagnitude < 0.01f)
            {
                stateMachine.ChangeState(EnemyState.Idle);
            }
        }
        
        public override void Exit()
        {
            entity.SetFlash(0f);
            entity.ResetKnockBackDir();
            entity.SyncTransform(entity.KnockBackSpeed);
        }
    }    
}