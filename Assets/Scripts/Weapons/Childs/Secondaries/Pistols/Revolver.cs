using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class Revolver : Gun
{
    [Header("Revolver")]
    [SerializeField] float highNoonDamage;
    [SerializeField] float highNoonCooldown, highNoonStartup;
    [SerializeField] float highNoonRequiredShot;
    [SerializeField] Detector highNoonRange;
    [SerializeField] GameObject highNoonRangeIndicator;
    [SerializeField] Text highNoonText;
    [SerializeField] float highNoonMoveSpeed = 0.25f;
    float highNoonCounter = 0.0f;
    int hitCount = 0;
    private void Awake()
    {
        highNoonText.canvas.transform.SetParent(null, false);
    }
    public override void Set(Player owner)
    {
        base.Set(owner);
        Debug.Log(owner.transform);
        highNoonText.canvas.transform.SetParent(owner.transform, false);
        skillList.Add(new SkillEventPair(HighNoonBarScale, HighNoonSkillDesc));
    }
    protected override Bullet FireBullet(Vector2 position, Quaternion rotation)
    {
        Bullet tmp = base.FireBullet(position, rotation);
        tmp.onHit += OnBulletHit;
        return tmp;
    }
    IEnumerator highNoon = null;
    public override void OnWeaponUpdate()
    {
        base.OnWeaponUpdate();
        if(highNoon == null)
        {
            if(!reloading && hitCount >= highNoonRequiredShot && highNoonCounter >= highNoonCooldown && Input.GetKeyDown(KeyCode.E))
            {
                hitCount = 0;
                highNoonCounter = 0;
                highNoon = HighNoon();
                StartCoroutine(highNoon);
                owner.skillChange?.Invoke();
            }
        }
    }
    protected override void Update()
    {
        base.Update();
        if (!photonView.IsMine) return;
        if (highNoon == null && highNoonCounter < highNoonCooldown) highNoonCounter += Time.deltaTime;
    }
    WaitForSeconds interval = new WaitForSeconds(0.2f);
    IEnumerator HighNoon()
    {
        owner.canAttack = false;
        owner.canDash = false;
        owner.canSwapWeapon = false;
        owner.moveSpeedMultiplier *= highNoonMoveSpeed;
        photonView.RPC("SetIndicatorActive", RpcTarget.All, true);
        photonView.RPC("HighNoonText", RpcTarget.All);
        yield return new WaitForSeconds(highNoonStartup);
        photonView.RPC("SetIndicatorActive", RpcTarget.All, false);
        List<Player> victims = new();
        foreach (var i in highNoonRange.detected) victims.Add(i);
        foreach(var i in victims)
        {
            if (mag <= 0) break;
            mag--;
            Debug.Log(ammo);
            owner.ammoDescChange?.Invoke();
            anim.SetTrigger("Fire");
            float scaledDamage = owner.GetDamageValue(highNoonDamage);
            i.photonView.RPC("GetDamage", i.photonView.Owner, scaledDamage);
            GameManager.Instance.UIs.DamageText(scaledDamage, i.transform.position);
            yield return interval;
        }
        owner.canAttack = true;
        owner.canDash = true;
        owner.canSwapWeapon = true;
        owner.moveSpeedMultiplier /= highNoonMoveSpeed;
        highNoon = null;
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        if (highNoon != null) photonView.RPC("SetIndicatorActive", newPlayer, true);
    }
    [PunRPC] public void SetIndicatorActive(bool active) => highNoonRangeIndicator.SetActive(active);
    [PunRPC] public void HighNoonText()
    {
        StartCoroutine(HighNoonTexting());
    }
    const string highNoonTextContent = "석양이 진다....";
    WaitForSeconds waitSec = new WaitForSeconds(0.15f);
    WaitForSeconds lastWaitSec = new WaitForSeconds(0.5f);
    IEnumerator HighNoonTexting()
    {
        for(int i = 0; i < highNoonTextContent.Length; i++)
        {
            highNoonText.text += highNoonTextContent[i];
            yield return waitSec;
        }
        yield return lastWaitSec;
        highNoonText.text = string.Empty;
    }
    void OnBulletHit(HitInfo info)
    {
        if(hitCount < highNoonRequiredShot)
        {
            hitCount++;
            owner.skillChange?.Invoke();
        }
    }
    float HighNoonBarScale()
    {
        return highNoonCounter / highNoonCooldown;
    }
    string HighNoonSkillDesc()
    {
        return (hitCount < highNoonRequiredShot) ? $"명중 횟수: {hitCount}/{highNoonRequiredShot}" : "[E]: 석양이 진다";
    }
    private void OnDestroy()
    {
        if(photonView.IsMine) PhotonNetwork.Destroy(highNoonText.canvas.gameObject);
    }
}