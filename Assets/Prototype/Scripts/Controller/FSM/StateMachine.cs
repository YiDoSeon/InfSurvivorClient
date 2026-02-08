using System.Collections.Generic;
using UnityEngine;

public class StateMachine<TEntity, TEnum> where TEnum : System.Enum
{
    private TEntity entity;
    private Dictionary<TEnum, FiniteState<TEntity, TEnum>> stateCache = new Dictionary<TEnum, FiniteState<TEntity, TEnum>>();
    public FiniteState<TEntity, TEnum> CurrentState { get; private set; }
    public TEnum CurrentStateId { get; private set; }

    public StateMachine(TEntity entity)
    {
        this.entity = entity;
    }

    public void AddState(TEnum id, FiniteState<TEntity, TEnum> state)
    {
        stateCache[id] = state;
    }

    public void Initialize(TEnum initialId)
    {
        CurrentStateId = initialId;
        CurrentState = stateCache[initialId];
        CurrentState?.Enter();
    }

    public void ChangeState(TEnum newId)
    {
        if (EqualityComparer<TEnum>.Default.Equals(CurrentStateId, newId))
        {
            return;
        }
        CurrentState?.Exit();
        CurrentStateId = newId;
        CurrentState = stateCache[newId];
        CurrentState?.Enter();
    }

    public void Update()
    {
        CurrentState?.Update();
    }
    
    public void FixedUpdate()
    {
        CurrentState?.FixedUpdate();
    }
}
