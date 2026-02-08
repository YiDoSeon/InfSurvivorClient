using UnityEngine;

public abstract class FiniteState<T, TEnum> where TEnum : System.Enum
{
    protected T entity;
    protected StateMachine<T, TEnum> stateMachine;

    public FiniteState(T entity, StateMachine<T, TEnum> stateMachine)
    {
        this.entity = entity;
        this.stateMachine = stateMachine;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void Exit();
}
