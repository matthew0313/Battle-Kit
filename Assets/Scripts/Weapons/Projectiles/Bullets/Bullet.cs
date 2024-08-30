using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviourPunCallbacks
{
    Photon.Realtime.Player owner;
    protected float damage, speed, range;
    public void Set(float damage, float speed, float range)
    {
        owner = photonView.Owner;
        this.damage = damage;
        this.speed = speed;
        this.range = range;
    }
    float counter = 0.0f;
    private void Update()
    {
        if (!photonView.IsMine) return;
        if(counter * speed < range)
        {
            counter += Time.deltaTime;
            transform.Translate(transform.right * speed * Time.deltaTime, Space.World);
        }
        else
        {
            Despawn();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;
        if (collision.CompareTag("Bullet") || collision.CompareTag("Slash")) return;
        if (collision.CompareTag("Player"))
        {
            Player tmp = collision.GetComponent<Player>();
            if (tmp.photonView.Owner != owner) Hit(tmp);
            else return;
        }
        Despawn();
    }
    protected virtual void Hit(Player target)
    {
        target.photonView.RPC("GetDamage", RpcTarget.All, damage);
    }
    protected virtual void Despawn()
    {
        PhotonNetwork.Destroy(gameObject);
    }
    [PunRPC]
    public void Redirect(Quaternion reflectDir, Photon.Realtime.Player newOwner)
    {
        counter = 0.0f;
        owner = newOwner;
        transform.rotation = reflectDir;
    }
    public void Reflect(Quaternion reflectDir, Photon.Realtime.Player newOwner)
    {
        photonView.RPC("Redirect", RpcTarget.All, reflectDir, newOwner);
    }
}