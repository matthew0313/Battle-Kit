using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class Drone : MonoBehaviourPun
{
    [SerializeField] float minOwnerDist, moveSpeed;
    protected Player owner;
    public virtual void Set(Player owner)
    {
        Debug.Log(owner);
        this.owner = owner;
    }
    protected virtual void Update()
    {
        if (!photonView.IsMine) return;
        if (Vector2.Distance(owner.transform.position, transform.position) > minOwnerDist)
        {
            transform.Translate((owner.transform.position - transform.position).normalized * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}
