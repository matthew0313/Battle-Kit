using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

[RequireComponent(typeof(PhotonTransformView))]
public class Bullet : Projectile
{
    [SerializeField] bool reflectable = true;
    [SerializeField] bool pierce = false;
    protected float speed, range;
    public void Set(float damage, float speed, float range, Action<HitInfo> onHit, string inflictingEffect)
    {
        base.Set(damage, onHit, inflictingEffect);
        this.speed = speed;
        this.range = range;
    }
    float counter = 0.0f;
    private void Update()
    {
        if (!photonView.IsMine) return;
        if (counter * speed < range)
        {
            counter += Time.deltaTime;
            transform.Translate(transform.right * speed * Time.deltaTime, Space.World);
        }
        else
        {
            Despawn();
        }
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;
        base.OnTriggerEnter2D(collision);
        if (!IgnoreTarget(collision) && !pierce) Despawn();
    }
    bool IgnoreTarget(Collider2D collision)
    {
        if (collision.CompareTag("BulletIgnore") || collision.CompareTag("Bullet")) return true;
        if (collision.CompareTag("Player"))
        {
            if (collision.GetComponent<Player>().photonView.Owner == owner) return true;
        }
        return false;
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
        if (!reflectable) return;
        photonView.RPC("Redirect", photonView.Owner, reflectDir, newOwner);
    }
}