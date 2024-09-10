using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public abstract class TickDamageEffectData : StatusEffectData
{
    [Header("Tick Damage Effect")]
    [SerializeField] internal float tickDamage;
    [SerializeField] internal float tickRate;
    [SerializeField] internal int maxStack = 1;
    [SerializeField] internal Color damageColor = Color.cyan;
}
public abstract class TickDamageEffect : StatusEffect
{
    protected new TickDamageEffectData data { get { return base.data as TickDamageEffectData; } }
    public TickDamageEffect(TickDamageEffectData origin) : base(origin)
    {

    }
    protected int stack { get; private set; } = 1;
    public override void ReApply()
    {
        base.ReApply();
        if(stack < data.maxStack) stack++;
    }
    float counter = 0.0f;
    public override void OnEffectUpdate()
    {
        if (counter < data.tickRate) counter += Time.deltaTime;
        if(counter >= data.tickRate)
        {
            counter -= data.tickRate;
            GiveDamage();
        }
        base.OnEffectUpdate();
    }
    protected virtual void GiveDamage()
    {
        affected.photonView.RPC("GetDamageColor", RpcTarget.All, data.tickDamage * stack, new Vector3(data.damageColor.r, data.damageColor.g, data.damageColor.b));
    }
}