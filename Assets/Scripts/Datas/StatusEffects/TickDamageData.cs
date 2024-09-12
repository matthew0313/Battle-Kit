using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "Tick Damage", menuName = "Scriptables/Status Effects/Tick Damage", order = 0)]
public class TickDamageData : StatusEffectData
{
    [Header("Tick Damage")]
    [SerializeField] internal float tickDamage;
    [SerializeField] internal float tickRate;
    protected override StatusEffect GetEffect()
    {
        return new TickDamage(this);
    }
}
public class TickDamage : StatusEffect
{
    protected new TickDamageData data { get { return base.data as TickDamageData; } }
    public TickDamage(TickDamageData origin) : base(origin)
    {

    }
    float counter = 0.0f;
    public override void OnEffectUpdate()
    {
        base.OnEffectUpdate();
        if (counter < data.tickRate) counter += Time.deltaTime;
        if(counter >= data.tickRate)
        {
            counter -= data.tickRate;
            GiveDamage();
        }
    }
    protected virtual void GiveDamage()
    {
        GameManager.Instance.UIs.DamageText(data.tickDamage * stack, affected.transform.position, data.debuffColor.ToVector());
        affected.photonView.RPC("GetDamage", affected.photonView.Owner, data.tickDamage * stack);
    }
}