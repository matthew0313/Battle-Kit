using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffect
{
    protected float durationLeft = 0.0f;
    protected Player affected;
    public StatusEffect(float duration)
    {
        durationLeft = duration;
    }
    public void RefreshDuration(float duration)
    {
        durationLeft = Mathf.Max(duration, durationLeft);
    }
    public virtual void EffectAdd(Player affected)
    {
        affected.statusEffects.Add(this);
        this.affected = affected;
        OnEffectAdd();
    }
    public virtual void OnEffectAdd()
    {

    }
    public virtual void OnEffectUpdate()
    {
        if (durationLeft > 0.0f) durationLeft -= Time.deltaTime;
        else
        {
            RemoveEffect();
        }
    }
    bool removed = false;
    public void RemoveEffect()
    {
        if (removed) return;
        removed = true;
        affected.statusEffects.Remove(this);
        OnEffectRemove();
    }
    public virtual void OnEffectRemove()
    {

    }
}