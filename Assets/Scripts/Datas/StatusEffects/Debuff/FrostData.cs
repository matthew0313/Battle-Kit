using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Frost", menuName = "Scriptables/Status Effects/Debuffs/Frost", order = 0)]
public class FrostData : StatusEffectData
{
    [Header("Frost")]
    [SerializeField] internal float slowing = 0.6f;
    [SerializeField] internal int maxStack = 3;
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
    int stack = 1;
    public override void ReApply()
    {
        base.ReApply();
        if (stack < data.maxStack)
        {
            stack++;
            affected.moveSpeedMultiplier *= data.slowing;
            if (stack == data.maxStack) affected.canDash = false;
        }
    }
    public override void OnEffectAdd()
    {
        base.OnEffectAdd();
        affected.moveSpeedMultiplier *= data.slowing;
    }
    public override void OnEffectEnd()
    {
        base.OnEffectEnd();
        for(int i = 0; i < stack; i++)
        {
            affected.moveSpeedMultiplier /= data.slowing;
        }
        if(stack == data.maxStack) affected.canDash = true;
    }
}