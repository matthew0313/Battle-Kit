using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
public class Sword : Melee
{
    [Header("Knife")]
    [SerializeField] float flyingSlashDamage;
    [SerializeField] float flyingSlashFireRate, flyingSlashSpeed, flyingSlashRange;
    [SerializeField] Bullet flyingSlash;
    [SerializeField] Transform firePoint;
    SkillEventPair flyingSlashSkill;
    float flyingSlashCounter = 0.0f;
    public override void Set(Player owner)
    {
        base.Set(owner);
        //Flying Slash
        skillList.Add(new SkillEventPair(FlyingSlashBarFill, () => "[E]: 검기 날리기"));
    }
    public override void OnWeaponUpdate()
    {
        base.OnWeaponUpdate();
        if(flyingSlashCounter >= flyingSlashFireRate)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                flyingSlashCounter -= flyingSlashFireRate;
                anim.SetTrigger("FlyingSlash");
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
}