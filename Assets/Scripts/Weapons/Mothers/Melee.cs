using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class Melee : Weapon
{
    [Header("Melee")]
    [SerializeField] protected float damage;
    [SerializeField] protected float slashRate;
    [SerializeField] Detector detector;
    [SerializeField] AudioClip slashSound, hitSound;
    float counter = 0.0f;
    private void Awake()
    {
        detector.onOneTimeDetect += Hit;
    }
    public override void OnWeaponUpdate()
    {
        base.OnWeaponUpdate();
        if(counter >= slashRate && Input.GetMouseButtonDown(0))
        {
            counter -= slashRate;
            Slash();
        }
    }
    List<Player> hit = new();
    protected virtual void Slash()
    {
        if(slashSound != null) GameManager.Instance.PlaySound(photonView, slashSound);
        hit.Clear();
        detector.Refresh();
        anim.SetTrigger("Slash");
    }
    protected virtual void Hit(Player player)
    {
        if (player == this) return;
        if (hitSound != null) GameManager.Instance.PlaySound(photonView, hitSound);
        float scaledDamage = owner.GetDamageValue(damage);
        GameManager.Instance.UIs.DamageText(scaledDamage, player.transform.position);
        player.photonView.RPC("GetDamage", player.photonView.Owner, scaledDamage);
    }
    protected override void Update()
    {
        base.Update();
        if (!photonView.IsMine) return;
        if (counter < slashRate) counter += Time.deltaTime;
    }
    public override float BarFill()
    {
        return Mathf.Max(0, 1.0f - counter / slashRate);
    }
}
