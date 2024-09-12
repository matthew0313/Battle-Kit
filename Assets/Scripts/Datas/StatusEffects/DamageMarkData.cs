using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Fate", menuName = "Scriptables/Status Effects/Fate", order = 1)]
public class DamageMarkData : StatusEffectData
{
    [Header("Damage Mark")]
    [SerializeField] internal float markDamage;
    protected override StatusEffect GetEffect()
    {
        return new DamageMark(this);
    }
}
public class DamageMark : StatusEffect
{
    public DamageMark(DamageMarkData origin) : base(origin)
    {

    }
    new DamageMarkData data { get { return base.data as DamageMarkData; } }
    protected override void OnStackAdd()
    {
        base.OnStackAdd();
        if (stack >= data.maxStack)
        {
            GameManager.Instance.UIs.DamageText(data.markDamage, affected.transform.position, data.debuffColor.ToVector());
            affected.photonView.RPC("GetDamage", affected.photonView.Owner, data.markDamage);
            EndEffect();
        }
    }
}