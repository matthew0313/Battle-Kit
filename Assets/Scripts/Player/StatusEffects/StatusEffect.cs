using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffect
{
    float durationLeft = 0.0f;
    public StatusEffect(float duration)
    {
        durationLeft = duration;
    }
    public void RefreshDuration(float duration)
    {
        durationLeft = Mathf.Max(duration, durationLeft);
    }
    public abstract void EffectAdd(Player affected);
    public virtual void OnEffectAdd()
    {

    }
}