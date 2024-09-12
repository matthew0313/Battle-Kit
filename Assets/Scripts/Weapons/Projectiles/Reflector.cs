using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Reflector : MonoBehaviour
{
    [SerializeField] PhotonView photonView;
    [SerializeField] Transform direction;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            Bullet tmp = collision.GetComponent<Bullet>();
            if (tmp.owner != photonView.Owner)
            {
                collision.GetComponent<Bullet>().Reflect(direction == null ? transform.rotation : direction.rotation, photonView.Owner);
            }
        }
    }
}