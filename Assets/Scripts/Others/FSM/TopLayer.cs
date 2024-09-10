using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TopLayer<T> : Layer<T>
{
    public Action onFSMChange;
    public TopLayer(T origin) : base(origin, null)
    {

    }
    public override void ChangeState(string stateName)
    {
        currentState.OnStateExit();
        currentState = states[stateName];
        currentState.OnStateEnter();
        AlertStateChange();
    }
    public override void AlertStateChange()
    {
        onFSMChange?.Invoke();
    }
    public override string GetCurrentFSM()
    {
        return $"Top->{currentState.GetCurrentFSM()}";
    }
}
