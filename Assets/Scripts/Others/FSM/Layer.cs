using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Layer<T> : State<T>
{
    protected State<T> currentState, defaultState;
    protected Dictionary<string, State<T>> states = new();
    protected Dictionary<State<T>, string> stateNames = new();
    public Layer(T origin, Layer<T> parent) : base(origin, parent)
    {

    }
    protected void AddState(string name, State<T> state)
    {
        states.Add(name, state);
        stateNames.Add(state, name);
    }
    public string GetStateName(State<T> state) => stateNames[state];
    public override void OnStateEnter()
    {
        base.OnStateEnter();
        currentState = defaultState;
        currentState.OnStateEnter();
    }
    public override void OnStateUpdate()
    {
        base.OnStateUpdate();
        currentState.OnStateUpdate();
    }
    public override void OnStateFixedUpdate()
    {
        base.OnStateFixedUpdate();
        currentState.OnStateFixedUpdate();
    }
    public override void OnStateExit()
    {
        base.OnStateExit();
        currentState.OnStateExit();
    }
    public virtual void ChangeState(string stateName)
    {
        currentState.OnStateExit();
        currentState = states[stateName];
        currentState.OnStateEnter();
        parentLayer.AlertStateChange();
    }
    public virtual void AlertStateChange()
    {
        parentLayer.AlertStateChange();
    }
    public override string GetCurrentFSM()
    {
        return $"{base.GetCurrentFSM()}->{currentState.GetCurrentFSM()}";
    }
}
