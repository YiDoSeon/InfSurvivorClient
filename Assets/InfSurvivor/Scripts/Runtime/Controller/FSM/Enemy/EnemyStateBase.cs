using Shared.FSM;
using Shared.Packet;

namespace InfSurvivor.Runtime.Controller.FSM
{
    public class EnemyStateBase : FiniteState<EnemyController, EnemyState>
    {
        public EnemyStateBase(EnemyController entity, StateMachine<EnemyController, EnemyState> stateMachine) : base(entity, stateMachine)
        {
        }
    
        public override void Enter() { }
    
        public override void Exit() { }
    
        public override void FixedUpdate() { }
    
        public override void Update() { }
    }    
}