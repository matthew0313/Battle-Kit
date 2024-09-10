using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using System;

public abstract class Gun : Weapon
{
    [Header("Gun")]
    [SerializeField] protected bool auto = false;
    [SerializeField] protected float damage, fireRate, bulletSpeed, bulletRange, reloadTime;
    [SerializeField] protected int magSize, maxAmmo;
    [SerializeField] protected Bullet bullet;
    [SerializeField] protected Transform firePoint;
    protected float counter = 0.0f, reloadCounter = 0.0f;
    protected int ammo, mag;
    protected bool reloading = false;
    public override void Set(Player owner)
    {
        base.Set(owner);
        ammo = maxAmmo;
        mag = magSize;
    }
    public override void OnWeaponUpdate()
    {
        base.OnWeaponUpdate();
        if (!reloading && mag < magSize && ammo > 0 && Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
        else if(!reloading && counter >= fireRate && (auto ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0)))
        {
            counter -= fireRate;
            Fire();
        }
    }
    protected virtual void Reload()
    {
        reloading = true;
        reloadCounter = 0.0f;
        StartCoroutine(Reloading());
    }
    IEnumerator Reloading()
    {
        while(reloadCounter < reloadTime)
        {
            yield return null;
            reloadCounter += Time.deltaTime;
        }
        int tmp = Mathf.Min(ammo, magSize - mag);
        mag += tmp;
        ammo -= tmp;
        reloading = false;
        owner.ammoDescChange?.Invoke();
    }
    protected virtual void Fire()
    {
        if (mag <= 0) return;
        mag--;
        owner.ammoDescChange?.Invoke();
        if (anim != null) anim.SetTrigger("Fire");
        FireBullet(firePoint.position, firePoint.rotation);
    }
    protected virtual void FireBullet(Vector2 position, Quaternion rotation)
    {
        Bullet tmp = PhotonNetwork.Instantiate(Path.Combine("Prefabs", bullet.gameObject.name), position, rotation).GetComponent<Bullet>();
        tmp.Set(damage, bulletSpeed, bulletRange, null, null);
    }
    protected override void Update()
    {
        base.Update();
        if (!photonView.IsMine) return;
        if (counter < fireRate) counter += Time.deltaTime;
    }
    public override float BarFill()
    {
        if (!reloading) return 0.0f;
        else return 1.0f - reloadCounter / reloadTime;
    }
    public override string AmmoDesc()
    {
        return $"{mag}/{ammo}";
    }
}
