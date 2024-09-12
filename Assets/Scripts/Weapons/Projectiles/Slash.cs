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
    public void Set(float damage, float lifeTime)
    {
        base.Set(damage);
        this.lifeTime = lifeTime;
    }
    float counter = 0.0f;
    private void Update()
    {
        if(counter < lifeTime)
        {
            counter += Time.deltaTime;
            Dissolve(1.0f - counter / lifeTime);
        }
        else
        {
            if (photonView.IsMine) Despawn();
        }
    }
    public void Dissolve(float newAlpha)
    {
        color.a = newAlpha;
        rend.color = color;
    }
}