using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State<T>
{
    protected T origin;
    public Layer<T> parentLayer { get; private set; }
    public State(T origin, Layer<T> parent)
    {
        this.origin = origin;
        parentLayer = parent;
    }
    public virtual void OnStateEnter() { }
    public virtual void OnStateUpdate() { }
    public virtual void OnStateFixedUpdate() { }
    public virtual void OnStateExit() { }
    public virtual string GetCurrentFSM()
    {
        return parentLayer.GetStateName(this);
    }
}
