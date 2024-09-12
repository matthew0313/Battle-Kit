using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Frost", menuName = "Scriptables/Status Effects/Frost", order = 0)]
public class FrostData : StatusEffectData
{
    [Header("Slow")]
    [SerializeField] internal float slowing;
    protected override StatusEffect GetEffect()
    {
        return new Frost(this);
    }
}
public class Frost : StatusEffect
{
    public Frost(FrostData origin) : base(origin)
    {

    }
    new FrostData data { get { return base.data as FrostData; } }
    public override void OnEffectAdd()
    {
        base.OnEffectAdd();
        affected.moveSpeedMultiplier *= data.slowing;
    }
    protected override void OnStackAdd()
    {
        base.OnStackAdd();
        affected.moveSpeedMultiplier *= data.slowing;
        if (stack >= data.maxStack) affected.canDash = false;
    }
    protected override string EffectText()
    {
        return (stack >= data.maxStack) ? "얼어붙음" : base.EffectText();
    }
    public override void OnEffectEnd()
    {
        base.OnEffectEnd();
        for(int i = 0; i < stack; i++)
        {
            affected.moveSpeedMultiplier /= data.slowing;
        }
        if(stack >= data.maxStack) affected.canDash = true;
    }
}