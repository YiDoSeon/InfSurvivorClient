using Shared.FSM;
using Shared.Packet;
using UnityEngine;

namespace InfSurvivor.Runtime.Controller.FSM
{
    public class EnemyIdleState : EnemyStateBase
    {
        private float stateTime;
        public EnemyIdleState(EnemyController entity, StateMachine<EnemyController, EnemyState> stateMachine) : base(entity, stateMachine)
        {
        }
    
        public override void Enter()
        {
        }
    
        public override void FixedUpdate()
        {
        }
    }
}
