using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using System;

public abstract class CountLimitedWeapon : Weapon
{
    [Header("Use Count Limited")]
    [SerializeField] protected bool auto = false;
    [SerializeField] protected int useCount;
    [SerializeField] protected float useRate;
    protected int useLeft;
    float counter = 0.0f;
    private void Awake()
    {
        useLeft = useCount;
    }
    protected override void Update()
    {
        base.Update();
        if (counter < useRate) counter += Time.deltaTime;
    }
    public override void OnWeaponUpdate()
    {
        base.OnWeaponUpdate();
        if(counter >= useRate && (auto ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0)))
        {
            counter -= useRate;
            AttemptUse();
        }
    }
    protected virtual void AttemptUse()
    {
        if (useLeft <= 0) return;
        useLeft--;
        OnUse();
    }
    protected virtual void OnUse()
    {

    }
}