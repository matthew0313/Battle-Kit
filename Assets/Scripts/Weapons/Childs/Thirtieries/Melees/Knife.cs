using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
public class Knife : Melee
{
    [Header("Knife")]
    [SerializeField] float flyingSlashDamage;
    [SerializeField] float flyingSlashFireRate, flyingSlashSpeed, flyingSlashRange;
    [SerializeField] Bullet flyingSlash;
    [SerializeField] Transform firePoint;
    private static SkillEventPair flyingSlashSkill;
    float flyingSlashCounter = 0.0f;
    public override void Set(Player owner)
    {
        base.Set(owner);
        flyingSlashSkill = new SkillEventPair(FlyingSlashBarFill, FlyingSlashSkillDesc);
    }
    public override void OnWeaponEquip()
    {
        base.OnWeaponEquip();
        owner.skillList.Add(flyingSlashSkill);
    }
    public override void OnWeaponUpdate()
    {
        base.OnWeaponUpdate();
        if(flyingSlashCounter >= flyingSlashFireRate)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                flyingSlashCounter -= flyingSlashFireRate;
                anim.SetTrigger("Slash");
                owner.skillDescChange?.Invoke();
                PhotonNetwork.Instantiate(Path.Combine("Prefabs", flyingSlash.gameObject.name), firePoint.position, firePoint.rotation).GetComponent<Bullet>().Set(flyingSlashDamage, flyingSlashSpeed, flyingSlashRange);
            }
        }
    }
    protected override void Update()
    {
        base.Update();
        if (flyingSlashCounter < flyingSlashFireRate)
        {
            flyingSlashCounter += Time.deltaTime;
            if (flyingSlashCounter >= flyingSlashFireRate && equipped) owner.skillDescChange?.Invoke();
        }
    }
    public override void OnWeaponUnequip()
    {
        base.OnWeaponUnequip();
        owner.skillList.Remove(flyingSlashSkill);
    }
    float FlyingSlashBarFill()
    {
        return flyingSlashCounter / flyingSlashFireRate;
    }
    string FlyingSlashSkillDesc()
    {
        return (flyingSlashCounter >= flyingSlashFireRate) ? "E: Flying Slash" : "Charging...";
    }
}