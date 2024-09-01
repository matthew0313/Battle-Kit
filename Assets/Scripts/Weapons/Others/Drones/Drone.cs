using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public abstract class Drone : MonoBehaviourPun
{
    protected Player owner;
    public virtual void Set(Player owner)
    {
        Debug.Log(owner);
        this.owner = owner;
    }
    [PunRPC]
    public void SetDroneActive(bool active)
    {
        gameObject.SetActive(active);
    }
    public virtual void OnDroneDeploy()
    {

    }
    public virtual void OnDroneUpdate()
    {

    }
    public virtual void OnDroneRetract()
    {

    }
    public virtual string AmmoDesc()
    {
        return "--/--";
    }
}
