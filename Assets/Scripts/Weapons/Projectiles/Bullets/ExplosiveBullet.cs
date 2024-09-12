using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using System;

public class ExplosiveBullet : Bullet
{
    [SerializeField] Explosion explosion;
    [SerializeField] float explosionLifeTime, explosionKnockback, explosionSize;

    protected override void Hit(Player target)
    {
        Despawn();
    }
    protected override void Despawn()
    {
        Explosion tmp = PhotonNetwork.Instantiate(Path.Combine("Prefabs", explosion.gameObject.name), transform.position, Quaternion.identity).GetComponent<Explosion>();
        tmp.Set(damage, explosionLifeTime, explosionKnockback, explosionSize);
        tmp.onHit = onHit;
        tmp.inflictingEffect = inflictingEffect;
        base.Despawn();
    }
}