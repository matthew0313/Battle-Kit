using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class Reflector : MonoBehaviourPun
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            Bullet tmp = collision.GetComponent<Bullet>();
            if (tmp.owner != photonView.Owner)
            {
                collision.GetComponent<Bullet>().Reflect(transform.rotation, photonView.Owner);
            }
        }
    }
}