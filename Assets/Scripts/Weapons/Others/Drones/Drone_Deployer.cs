using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class Drone_Deployer : Weapon
{
    [SerializeField] Drone drone;
    [SerializeField] Transform spawnPoint;
    Drone deployed = null;
    public override void OnWeaponEquip()
    {
        base.OnWeaponEquip();
        if(deployed == null)
        {
            deployed = PhotonNetwork.Instantiate(Path.Combine("Prefabs", drone.gameObject.name), spawnPoint.position, spawnPoint.rotation).GetComponent<Drone>();
            deployed.Set(owner);
        }
        else
        {
            deployed.transform.position = spawnPoint.position;
            deployed.transform.rotation = spawnPoint.rotation;
            deployed.photonView.RPC("SetDroneActive", RpcTarget.All, true);
        }
        deployed.OnDroneDeploy();
        owner.extraDesc += ExtraDesc;
    }
    public override void OnWeaponUpdate()
    {
        deployed.OnDroneUpdate();
    }
    public override void OnWeaponUnequip()
    {
        base.OnWeaponUnequip();
        deployed.OnDroneRetract();
        deployed.photonView.RPC("SetDroneActive", RpcTarget.All, false);
        owner.extraDesc -= ExtraDesc;
    }
    public override string AmmoDesc()
    {
        return deployed.AmmoDesc();
    }
    private void OnDestroy()
    {
        if (deployed != null) PhotonNetwork.Destroy(deployed.gameObject);
    }
    protected virtual string ExtraDesc() => "Press E to switch between move and attack.";
}
