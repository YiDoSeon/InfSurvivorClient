using Shared.FSM;
using Shared.Packet;
using UnityEngine;

namespace InfSurvivor.Runtime.Controller.FSM
{
    public class EnemyDamagedState : EnemyStateBase
    {
        private float time;
        public EnemyDamagedState(EnemyController entity, StateMachine<EnemyController, EnemyState> stateMachine) : base(entity, stateMachine)
        {
        }
    
        public override void Enter()
        {
            time = 0f;
            entity.SetFlash(1f);
        }
    
        public void ResetDuration()
        {
            time = 0f;
        }
    
        public override void FixedUpdate()
        {
            if (time >= 0.1f)
            {
                entity.SetFlash(0f);
                stateMachine.ChangeState(EnemyState.Idle);
            }
            else
            {
                if (entity.IsConfirmed == false)
                {
                    entity.KnockBack();                
                }
                time += Time.fixedDeltaTime;
            }
        }
    
        public override void Exit()
        {
            entity.SetFlash(0f);
            Debug.Log($"{entity.gameObject.name}: {entity.TargetMovePosition}");
        }
    }    
}