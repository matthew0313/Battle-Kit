using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Shotgun : Gun
{
    [Header("Shotgun")]
    [SerializeField] protected int shotCount;
    [SerializeField] protected float spread;
    protected override void Fire()
    {
        if (fireSound != null) GameManager.Instance.PlaySound(photonView, fireSound);
        anim.SetTrigger("Fire");
        for(int i = 0; i < shotCount; i++) FireBullet(firePoint.position, firePoint.rotation * Quaternion.Euler(0, 0, Random.Range(-spread, spread)));
    }
}
