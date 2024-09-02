using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using System.IO;

[RequireComponent(typeof(PhotonView))]
public class Player : MonoBehaviourPunCallbacks
{
    [Header("Components")]
    [SerializeField] GameObject model;
    [SerializeField] BoxCollider2D hitbox;
    [Header("Statistics")]
    [SerializeField] float maxHp;
    float hp;

    [Header("Movement")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Transform rotator;
    [SerializeField] float moveSpeed = 1.0f;

    [Header("Dashing")]
    [SerializeField] float dashSpeed = 20.0f;
    [SerializeField] float dashDist = 3.0f;
    [SerializeField] float dashEffectRate = 0.05f;
    [SerializeField] float dashCooldown = 1.0f;

    [Header("Weapons")]
    [SerializeField] Transform weaponAnchor;
    [SerializeField]
    Weapon[] primaries, secondaries, melees;
    [SerializeField] List<Weapon> weapons = new();
    public Weapon heldWeapon { get; private set; }
    int heldWeaponIndex = 0;

    [Header("Display")]
    [SerializeField] Transform hpScaler;
    [SerializeField] Transform dashCooldownScaler;

    public List<SkillEventPair> skillList { get; } = new();
    public Func<string> ammoDesc, extraDesc;
    public Func<float> barFill;
    public Action ammoDescChange, skillDescChange;
    public Action weaponChange;

    public Action<float> onDamageDeal;

    int cantMove = 0, cantDash = 0, cantAttack = 0, m_invincibility = 0;
    public bool canMove { get { return cantMove <= 0; } set { if (value == false) cantMove++; else cantMove--; } }
    public bool canDash { get { return cantDash <= 0; } set { if (value == false) cantDash++; else cantDash--; } }
    public bool canAttack { get { return cantAttack <= 0; } set { if (value == false) cantAttack++; else cantAttack--; } }
    public bool invincibility { get { return m_invincibility > 0; } set { if (value == false) m_invincibility--; else m_invincibility++; } }

    float moveSpeedMultiplier = 1.0f, attackDamageMultiplier = 1.0f, receivedDamageMultiplier = 1.0f;
    private void Awake()
    {
        hp = maxHp;
        if (photonView.IsMine)
        {
            FollowingCam.cam.SetTarget(transform);
            RandomizeLoadout();
        }
    }
    void RandomizeLoadout()
    {
        List<Weapon> loadout = new()
        {
            primaries[UnityEngine.Random.Range(0, primaries.Length)],
            secondaries[UnityEngine.Random.Range(0, secondaries.Length)],
            melees[UnityEngine.Random.Range(0, melees.Length)]
        };
        SetWeapons(loadout);
    }
    public void SetWeapons(List<Weapon> weapons)
    {
        while(this.weapons.Count > 0)
        {
            PhotonNetwork.Destroy(this.weapons[0].gameObject);
            this.weapons.RemoveAt(0);
        }
        for(int i = 0; i < weapons.Count; i++)
        {
            Weapon tmp = PhotonNetwork.Instantiate(Path.Combine("Prefabs", weapons[i].gameObject.name), weaponAnchor.position, weaponAnchor.rotation).GetComponent<Weapon>();
            tmp.transform.SetParent(weaponAnchor);
            tmp.Set(this);
            this.weapons.Add(tmp);
        }
        heldWeapon = this.weapons[0];
        heldWeapon.OnWeaponEquip();
        weaponChange?.Invoke();
    }
    private void Update()
    {
        StatusEffectUpdate();
        if (!photonView.IsMine || dead) return;
        Move();
        Rotate();
        WeaponUpdate();
    }
    bool dashing = false;
    Vector2 dashDir = Vector2.zero;
    float dashCounter = 0.0f, dashCooldownCounter = 0.0f;
    IEnumerator dashEffect = null;
    void Move()
    {
        if (rb.velocity.magnitude > 2.0f || canMove == false)
        {
            return;
        }
        else rb.velocity = Vector2.zero;
        if (!dashing)
        {
            Vector2 move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            transform.Translate(move * moveSpeed * moveSpeedMultiplier * Time.deltaTime, Space.World);
            if(dashCooldownCounter < dashCooldown)
            {
                photonView.RPC("SetDashCooldown", RpcTarget.All, Mathf.Min(dashCooldown, dashCooldownCounter + Time.deltaTime));
            }
            else
            {
                if (canDash && Input.GetKeyDown(KeyCode.LeftShift))
                {
                    photonView.RPC("SetDashCooldown", RpcTarget.All, 0.0f);
                    dashDir = move.normalized;
                    dashing = true;
                    invincibility = true;
                    if (dashEffect == null) dashEffect = DashEffect();
                    StartCoroutine(dashEffect);
                }
            }
        }
        else
        {
            if(dashCounter * dashSpeed < dashDist)
            {
                dashCounter += Time.deltaTime;
                transform.Translate(dashDir * dashSpeed * Time.deltaTime, Space.World);
            }
            else
            {
                dashCounter = 0.0f;
                dashing = false;
                invincibility = false;
                StopCoroutine(dashEffect);
            }
        }
    }
    IEnumerator DashEffect()
    {
        while (true)
        {
            PhotonNetwork.Instantiate(Path.Combine("Effects", "DashEffect"), transform.position, rotator.rotation);
            yield return new WaitForSeconds(dashEffectRate);
        }
    }
    void Rotate()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        rotator.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(rotator.position.y - mousePos.y, rotator.position.x - mousePos.x) * Mathf.Rad2Deg);
        if(heldWeapon != null)
        {
            if (mousePos.x > rotator.position.x) heldWeapon.transform.localScale = new Vector2(1.0f, -1.0f);
            else heldWeapon.transform.localScale = new Vector2(1.0f, 1.0f);
        }
    }
    void WeaponUpdate()
    {
        for(int i = 0; i < weapons.Count; i++)
        {
            if (heldWeaponIndex != i &&  Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                heldWeapon.OnWeaponUnequip();
                heldWeapon = weapons[i];
                heldWeaponIndex = i;
                heldWeapon.OnWeaponEquip();
                weaponChange?.Invoke();
            }
        }
        if(canAttack) heldWeapon.OnWeaponUpdate();
    }
    public float GetDamageValue(float damage)
    {
        return damage * attackDamageMultiplier;
    }
    public List<StatusEffect> statusEffects { get; } = new();
    public List<StatusEffect> statusEffectRemoveQueue { get; } = new();
    void StatusEffectUpdate()
    {
        foreach (var i in statusEffects) i.OnEffectUpdate();
        statusEffects.RemoveAll((StatusEffect effect) => statusEffectRemoveQueue.Contains(effect) );
        statusEffectRemoveQueue.Clear();
    }
    [PunRPC]
    public void SetDashCooldown(float dashCooldownCounter)
    {
        this.dashCooldownCounter = dashCooldownCounter;
        dashCooldownScaler.localScale = new Vector2(dashCooldownCounter / dashCooldown, 1.0f);
    }
    [PunRPC]
    public void SetHp(float hp)
    {
        this.hp = hp;
        hpScaler.localScale = new Vector2(hp/maxHp, 1.0f);
    }
    [PunRPC]
    public void GetDamage(float damage)
    {
        if (invincibility || hp == 0) return;
        if (photonView.IsMine)
        {
            GameManager.Instance.UIs.DamageText(damage, transform.position);
        }
        hp = Mathf.Max(0, hp - damage);
        hpScaler.localScale = new Vector2(hp / maxHp, 1.0f);
        if (hp <= 0 && photonView.IsMine) Death();
    }
    [PunRPC]
    public void SetPlayerVisibility(bool playerActive)
    {
        model.SetActive(playerActive);
        hitbox.enabled = playerActive;
    }
    [PunRPC]
    public void Knockback(Vector2 knockback)
    {
        rb.AddForce(knockback, ForceMode2D.Impulse);
    }
    [PunRPC] public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
    }
    [PunRPC] public void SetCanDash(bool canDash)
    {
        this.canDash = canDash;
    }
    [PunRPC] public void SetCanAttack(bool canAttack)
    {
        this.canAttack = canAttack;
    }
    [PunRPC] public void MultiplyMoveSpeed(float multiplier)
    {
        moveSpeedMultiplier *= multiplier;
    }
    [PunRPC] public void DivideMoveSpeed(float divided)
    {
        moveSpeedMultiplier /= divided;
    }
    [PunRPC] public void MultiplyAttackDamage(float multiplier)
    {
        attackDamageMultiplier *= multiplier;
    }
    [PunRPC] public void DivideAttackDamage(float divided)
    {
        attackDamageMultiplier /= divided;
    }
    [PunRPC] public void MultiplyReceivedDamage(float multiplier)
    {
        receivedDamageMultiplier *= multiplier;
    }
    [PunRPC] public void DivideReceivedDamage(float divided)
    {
        receivedDamageMultiplier /= divided;
    }
    public bool dead { get; private set; } = false;
    public Action onDeath, onRevive;
    public void Death()
    {
        onDeath?.Invoke();
        dead = true;
        while (weapons.Count > 0)
        {
            PhotonNetwork.Destroy(weapons[0].gameObject);
            weapons.RemoveAt(0);
        }
        photonView.RPC("SetPlayerVisibility", RpcTarget.All, false);
        //if(killer != null) FollowingCam.cam.SetTarget((killer as Player).transform);
        StartCoroutine(DeathCounter());
    }
    IEnumerator DeathCounter()
    {
        yield return new WaitForSeconds(3.0f);
        onRevive?.Invoke();
        RandomizeLoadout();
        dead = false;
        photonView.RPC("SetHp", RpcTarget.All, maxHp);
        transform.position = new Vector2(UnityEngine.Random.Range(-5.0f, 5.0f), UnityEngine.Random.Range(-5.0f, 5.0f));
        photonView.RPC("SetPlayerVisibility", RpcTarget.All, true);
        FollowingCam.cam.SetTarget(transform);
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("SetHp", newPlayer, hp);
            if (dead) photonView.RPC("SetPlayerActive", newPlayer, false);
        }
    }
}
public class SkillEventPair
{
    public SkillEventPair(Func<float> skillBarFill, Func<string> skillDesc)
    {
        this.skillBarFill = skillBarFill;
        this.skillDesc = skillDesc;
    }
    public readonly Func<float> skillBarFill;
    public readonly Func<string> skillDesc;
}
