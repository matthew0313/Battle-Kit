using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

[RequireComponent(typeof(PhotonTransformView))]
public class Explosion : Projectile
{
    [SerializeField] SpriteRenderer rend;
    float lifeTime, knockback, counter = 0.0f;
    Color color;
    private void Awake()
    {
        color = rend.color;
    }
    protected override bool ignoreOwner => true;
    public void Set(float damage, float lifeTime, float knockback, float size)
    {
        base.Set(damage);
        this.lifeTime = lifeTime;
        this.knockback = knockback;
        transform.localScale = new Vector2(size * 2, size * 2);
    }
    private void Update()
    {
        if (!photonView.IsMine) return;
        if (counter < lifeTime)
        {
            counter += Time.deltaTime;
            photonView.RPC("Dissolve", RpcTarget.All, 1.0f - counter / lifeTime);
        }
        else
        {
            Despawn();
        }
    }
    /*private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;
        if (collision.CompareTag("Player"))
        {
            Debug.Log("HITTTT");
            Player tmp = collision.GetComponent<Player>();
            if (hit.Contains(tmp)) return;
            hit.Add(tmp);
            float scaledDamage = GameManager.Instance.myPlayer.GetDamageValue(damage);
            tmp.photonView.RPC("GetDamage", tmp.photonView.Owner, scaledDamage);
            tmp.photonView.RPC("Knockback", tmp.photonView.Owner, (Vector2)((tmp.transform.position - transform.position).normalized * knockback));
            HitInfo info;
            info.hitPlayer = tmp;
            onHit?.Invoke(scaledDamage);
        }
    }*/
    protected override void Hit(Player target)
    {
        base.Hit(target);
        target.photonView.RPC("Knockback", target.photonView.Owner, (Vector2)((target.transform.position - transform.position).normalized * knockback));
    }
    [PunRPC]
    public void Dissolve(float newAlpha)
    {
        color.a = newAlpha;
        rend.color = color;
    }
}