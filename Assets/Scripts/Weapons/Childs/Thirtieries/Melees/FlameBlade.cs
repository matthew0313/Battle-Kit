using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
public class FlameBlade : Melee
{
    protected override void Slash()
    {
        if (anim != null) anim.SetTrigger("Slash");
        PhotonNetwork.Instantiate(Path.Combine("Prefabs", slash.gameObject.name), slashPoint.position, slashPoint.rotation).GetComponent<Slash>().Set(damage, slashLifetime, null, "FlameBlade_Burning");
    }
}