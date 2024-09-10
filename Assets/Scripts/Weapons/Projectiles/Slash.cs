using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

[RequireComponent(typeof(PhotonView))]
public class Slash : Projectile
{
    [SerializeField] SpriteRenderer rend;
    float lifeTime;
    Color color;
    private void Awake()
    {
        color = rend.color;
    }
    public void Set(float damage, float lifeTime, Action<HitInfo> onHit, string inflictingEffect)
    {
        base.Set(damage, onHit, inflictingEffect);
        this.lifeTime = lifeTime;
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
            Despawn();
        }
    }
    [PunRPC]
    public void Dissolve(float newAlpha)
    {
        color.a = newAlpha;
        rend.color = color;
    }
}