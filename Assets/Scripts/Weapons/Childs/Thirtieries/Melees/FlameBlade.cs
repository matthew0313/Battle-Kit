using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
public class FlameBlade : Melee
{
    protected override void Hit(Player player)
    {
        base.Hit(player);
        player.photonView.RPC("AddStatusEffect", player.photonView.Owner, "FlameBlade_Burning");
    }
}