using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class Explosion : MonoBehaviourPun
{
    [SerializeField] SpriteRenderer rend;
    float damage, lifeTime, knockback, counter = 0.0f;
    Color color;
    public void Set(float damage, float lifeTime, float knockback, float size)
    {
        this.damage = damage;
        this.lifeTime = lifeTime;
        this.knockback = knockback;
        photonView.RPC("SetDefaults", RpcTarget.All, size);
        color = rend.color;
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
            PhotonNetwork.Destroy(gameObject);
        }
    }
    List<Player> hit = new();
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;
        if (collision.CompareTag("Player"))
        {
            Player tmp = collision.GetComponent<Player>();
            if (hit.Contains(tmp)) return;
            hit.Add(tmp);
            tmp.photonView.RPC("GetDamage", RpcTarget.All, GameManager.Instance.myPlayer.GetDamageValue(damage));
            tmp.photonView.RPC("Knockback", tmp.photonView.Owner, (Vector2)((tmp.transform.position - transform.position).normalized * knockback));
        }
    }
    [PunRPC]
    public void Dissolve(float newAlpha)
    {
        color.a = newAlpha;
        rend.color = color;
    }
    [PunRPC]
    public void SetDefaults(float scale)
    {
        transform.localScale = new Vector2(scale * 2, scale * 2);
        color = rend.color;
    }
}