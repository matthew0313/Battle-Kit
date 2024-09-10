using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public abstract class StatusEffectData : ScriptableObject
{
    [SerializeField] internal StatusEffectEffect effect;
    [SerializeField] internal float duration;
    public void AddEffectToPlayer(Player affected)
    {
        GetEffect().AddToPlayer(affected);
    }
    protected abstract StatusEffect GetEffect();
    /*public void RefreshDuration(float duration)
    {
        durationLeft = Mathf.Max(duration, durationLeft);
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
        OnEffectRemove();
        PhotonNetwork.Destroy(gameObject);
    }
    public virtual void OnEffectRemove()
    {

    }*/
}
public abstract class StatusEffect
{
    public readonly StatusEffectData data;
    protected Player affected { get; private set; }
    protected float durationLeft;
    GameObject effect = null;
    public StatusEffect(StatusEffectData origin)
    {
        data = origin;
        durationLeft = data.duration;
    }
    public virtual void AddToPlayer(Player affected)
    {
        if (affected.FindStatusEffect(data, out StatusEffect found)) found.ReApply();
        else
        {
            this.affected = affected;
            affected.statusEffects.Add(this);
        }
        OnEffectAdd();
    }
    public virtual void ReApply()
    {
        durationLeft = data.duration;
    }
    public virtual void OnEffectAdd()
    {
        if(data.effect != null)
        {
            effect = PhotonNetwork.Instantiate(Path.Combine("Status Effects", "Effects", data.effect.gameObject.name), affected.transform.position, Quaternion.identity);
            effect.transform.SetParent(affected.transform);
        }
        affected.onDeath += EndEffect;
    }
    public virtual void OnEffectUpdate()
    {
        durationLeft -= Time.deltaTime;
        if(durationLeft <= 0)
        {
            EndEffect();
        }
    }
    bool ended = false;
    public void EndEffect()
    {
        if (ended) return;
        ended = true;
        affected.removeQueue.Add(this);
        affected.onDeath -= EndEffect;
        OnEffectEnd();
    }
    public virtual void OnEffectEnd()
    {
        if(effect != null) PhotonNetwork.Destroy(effect);
    }
}