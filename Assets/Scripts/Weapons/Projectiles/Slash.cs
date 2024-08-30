using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Slash : MonoBehaviourPunCallbacks
{
    [SerializeField] SpriteRenderer rend;
    Photon.Realtime.Player owner;
    float damage, lifeTime;
    Color color;
    public void Set(float damage, float lifeTime)
    {
        owner = photonView.Owner;
        this.damage = damage;
        this.lifeTime = lifeTime;
        color = Color.white;
    }
    float counter = 0.0f;
    private void Update()
    {
        if (!photonView.IsMine) return;
        if(counter < lifeTime)
        {
            photonView.RPC("Dissolve", RpcTarget.All, 1.0f - counter / lifeTime);
            counter += Time.deltaTime;
        }
        else
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
    [PunRPC]
    public void Dissolve(float newAlpha)
    {
        color.a = newAlpha;
        rend.color = color;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;
        if (collision.CompareTag("Player"))
        {
            Player tmp = collision.GetComponent<Player>();
            if (tmp.photonView.Owner != owner) tmp.photonView.RPC("GetDamage", RpcTarget.All, damage);
        }
        else if (collision.CompareTag("Bullet"))
        {
            Bullet tmp = collision.GetComponent<Bullet>();
            if (tmp.photonView.IsMine == false)
            {
                collision.GetComponent<Bullet>().Reflect(transform.rotation, photonView.Owner);
            }
        }
    }
}