using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
public class Melee : Weapon
{
    [Header("Melee")]
    [SerializeField] float damage, slashRate, slashLifetime;
    [SerializeField] Slash slash;
    [SerializeField] Transform slashPoint;
    float counter = 0.0f;
    public override void OnWeaponUpdate()
    {
        base.OnWeaponUpdate();
        if(counter >= slashRate && Input.GetMouseButtonDown(0))
        {
            counter -= slashRate;
            Slash();
        }
    }
    protected virtual void Slash()
    {
        if (anim != null) anim.SetTrigger("Slash");
        PhotonNetwork.Instantiate(Path.Combine("Prefabs", slash.gameObject.name), slashPoint.position, slashPoint.rotation).GetComponent<Slash>().Set(damage, slashLifetime);
    }
    protected override void Update()
    {
        base.Update();
        if (!photonView.IsMine) return;
        if (counter < slashRate) counter += Time.deltaTime;
    }
    public override float BarFill()
    {
        return Mathf.Max(0, 1.0f - counter / slashRate);
    }
}
