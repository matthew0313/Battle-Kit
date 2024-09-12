using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Unity.VisualScripting;

[RequireComponent(typeof(PhotonView))]
public abstract class Projectile : MonoBehaviourPunCallbacks
{
    public Photon.Realtime.Player owner { get; protected set; }
    protected float damage;
    protected List<Player> hit = new();
    protected void Set(float damage)
    {
        owner = photonView.Owner;
        this.damage = damage;
    }
    public string inflictingEffect;
    public Func<Player, float, float> damageScaler = null;
    public Action<HitInfo> onHit;

    protected virtual bool ignoreOwner => false;
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;
        if (collision.CompareTag("Player"))
        {
            Player tmp = collision.GetComponent<Player>();
            if (!ignoreOwner && tmp.photonView.Owner == owner || hit.Contains(tmp)) return;
            hit.Add(tmp);
            Hit(tmp);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;
        if (collision.CompareTag("Player"))
        {
            Player tmp = collision.GetComponent<Player>();
            if (!ignoreOwner && tmp.photonView.Owner == owner || !hit.Contains(tmp)) return;
            hit.Remove(tmp);
        }
    }
    protected virtual void Hit(Player target)
    {
        float scaledDamage = GameManager.Instance.myPlayer.GetDamageValue(damage);
        if (damageScaler != null) scaledDamage = damageScaler.Invoke(target, scaledDamage);
        target.photonView.RPC("GetDamage", target.photonView.Owner, scaledDamage);
        GameManager.Instance.UIs.DamageText(scaledDamage, target.transform.position);
        if (inflictingEffect != null) target.photonView.RPC("AddStatusEffect", target.photonView.Owner, inflictingEffect);
        HitInfo info;
        info.hitPlayer = target;
        info.hitDamage = scaledDamage;
        onHit?.Invoke(info);
    }
    protected virtual void Despawn()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
public struct HitInfo
{
    public float hitDamage;
    public Player hitPlayer;
}