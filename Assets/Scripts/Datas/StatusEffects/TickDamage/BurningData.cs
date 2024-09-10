using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Burning", menuName = "Scriptables/Status Effects/Tick Damage/Burning", order = 0)]
public class BurningData : TickDamageEffectData
{
    protected override StatusEffect GetEffect()
    {
        return new Burning(this);
    }
}
public class Burning : TickDamageEffect
{
    public Burning(BurningData origin) : base(origin) { }
}