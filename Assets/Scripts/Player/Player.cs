using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public List<Weapon> weapons { get; } = new();
    public Weapon heldWeapon { get; private set; }
    int heldWeaponIndex = 0;

    [Header("Display")]
    [SerializeField] GameObject forceField;
    [SerializeField] Transform hpScaler;
    [SerializeField] Transform dashCooldownScaler;
    [SerializeField] Text coinText;

    [Header("Score")]
    [SerializeField] int coinsForWin = 30;
    
    public List<SkillEventPair> skillList = new();
    public Func<string> ammoDesc, extraDesc;
    public Func<float> barFill;
    public Action ammoDescChange, skillChange;
    public Action weaponChange, loadoutChange;

    public Action<float> onDamageDeal;

    int cantMove = 0, cantRotate = 0, cantDash = 0, cantAttack = 0, cantSwapWeapon = 0, m_invincibility = 0;
    public bool canMove { get { return cantMove <= 0; } set { if (value == false) cantMove++; else cantMove--; } }
    public bool canRotate { get { return cantRotate <= 0; } set { if (value == false) cantRotate++; else cantRotate--; } }
    public bool canDash { get { return cantDash <= 0; } set { if (value == false) cantDash++; else cantDash--; } }
    public bool canAttack { get { return cantAttack <= 0; } set { if (value == false) cantAttack++; else cantAttack--; } }
    public bool canSwapWeapon { get { return cantSwapWeapon <= 0; } set { if (value == false) cantSwapWeapon++; else cantSwapWeapon--; } }
    public bool invincibility { get { return m_invincibility > 0; } set { if (value == false) m_invincibility--; else m_invincibility++; } }

    public float moveSpeedMultiplier = 1.0f, attackDamageMultiplier = 1.0f, receivedDamageMultiplier = 1.0f;
    private void Awake()
    {
        hp = maxHp;
    }
    public void RoundStart()
    {
        model.SetActive(true);
        onRevive?.Invoke();
        RandomizeLoadout();
        dead = false;
        photonView.RPC("SetHp", RpcTarget.All, maxHp);
        float randomAngle = UnityEngine.Random.Range(0.0f, 360.0f);
        float randomDiameter = UnityEngine.Random.Range(3.0f, 5.0f);
        transform.position = new Vector2(Mathf.Sin(randomAngle) * randomDiameter, Mathf.Cos(randomAngle) * randomDiameter);
        photonView.RPC("SetPlayerVisibility", RpcTarget.All, true);
        photonView.RPC("SetCoin", RpcTarget.All, 0);
        StartCoroutine(ForceField());
        FollowingCam.cam.SetTarget(transform);
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
        heldWeaponIndex = 0;
        weaponChange?.Invoke();
        loadoutChange?.Invoke();
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
        if (!canRotate) return;
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
        if (canSwapWeapon)
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                if (heldWeaponIndex != i && Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    heldWeapon.OnWeaponUnequip();
                    heldWeapon = weapons[i];
                    heldWeaponIndex = i;
                    heldWeapon.OnWeaponEquip();
                    weaponChange?.Invoke();
                    break;
                }
            }
        }
        if(canAttack) heldWeapon.OnWeaponUpdate();
    }
    public float GetDamageValue(float damage)
    {
        return damage * attackDamageMultiplier;
    }
    public List<StatusEffect> statusEffects { get; } = new();
    public List<StatusEffect> removeQueue { get; } = new();
    [PunRPC] public void AddStatusEffect(string effectName)
    {
        if (dead || invincibility) return;
        Resources.Load<StatusEffectData>(Path.Combine("Status Effects", "Datas", effectName)).AddEffectToPlayer(this);
    }
    public bool FindStatusEffect<T>(out T found) where T : StatusEffect
    {
        foreach(var i in statusEffects)
        {
            if (i is T)
            {
                found = i as T;
                return true;
            }
        }
        found = null;
        return false;
    }
    public bool FindStatusEffect(StatusEffectData data, out StatusEffect found)
    {
        foreach (var i in statusEffects)
        {
            if (i.data == data)
            {
                found = i;
                return true;
            }
        }
        found = null;
        return false;
    }
    void StatusEffectUpdate()
    {
        foreach (var i in statusEffects) i.OnEffectUpdate();
        if(removeQueue.Count > 0)
        {
            foreach (var i in removeQueue)
            {
                statusEffects.Remove(i);
            }
            removeQueue.Clear();
        }
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
    public Action<float> onDamage;
    [PunRPC]
    public void GetDamage(float damage)
    {
        if (dead) return;
        if (invincibility)
        {
            GameManager.Instance.UIs.DamageText(0.0f, transform.position, Color.black);
            return;
        }
        GameManager.Instance.UIs.DamageText(damage, transform.position);
        photonView.RPC("SetHp", RpcTarget.All, Mathf.Max(0, hp - damage));
        onDamage?.Invoke(damage);
        if (hp <= 0) Death();
    }
    [PunRPC]
    public void GetDamageColor(float damage, Vector3 color)
    {
        if (dead) return;
        if (invincibility)
        {
            GameManager.Instance.UIs.DamageText(0.0f, transform.position, Color.black);
            return;
        }
        GameManager.Instance.UIs.DamageText(damage, transform.position, new Color(color.x, color.y, color.z));
        photonView.RPC("SetHp", RpcTarget.All, Mathf.Max(0, hp - damage));
        onDamage?.Invoke(damage);
        if (hp <= 0) Death();
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
        if (dead || invincibility) return;
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

    public int coins { get; private set; } = 0;
    public Action onCoinEdit;
    public void EarnCoin()
    {
        photonView.RPC("SetCoin", RpcTarget.All, ++coins);
        onCoinEdit?.Invoke();
        if(coins >= coinsForWin)
        {
            GameManager.Instance.WinGame();
        }
    }
    [PunRPC] public void SetCoin(int coins)
    {
        this.coins = coins;
        coinText.text = coins.ToString();
    }
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
        int droppedCoins = coins / 2;
        for(int i = 0; i < droppedCoins; i++)
        {
            float deg = UnityEngine.Random.Range(0.0f, 360.0f);
            float diam = UnityEngine.Random.Range(0.0f, 3.0f);
            GameManager.Instance.photonView.RPC("SpawnCoin", RpcTarget.MasterClient, (Vector2)transform.position + new Vector2(Mathf.Cos(deg), Mathf.Sin(deg)) * diam);
        }
        photonView.RPC("SetCoin", RpcTarget.All, coins / 2 + coins % 2);
        //if(killer != null) FollowingCam.cam.SetTarget((killer as Player).transform);
        StartCoroutine(DeathCounter());
    }
    IEnumerator DeathCounter()
    {
        yield return new WaitForSeconds(3.0f);
        model.SetActive(true);
        onRevive?.Invoke();
        RandomizeLoadout();
        dead = false;
        photonView.RPC("SetHp", RpcTarget.All, maxHp);
        float randomAngle = UnityEngine.Random.Range(0.0f, 360.0f);
        float randomDiameter = UnityEngine.Random.Range(3.0f, 5.0f);
        transform.position = new Vector2(Mathf.Sin(randomAngle) * randomDiameter, Mathf.Cos(randomAngle) * randomDiameter);
        photonView.RPC("SetPlayerVisibility", RpcTarget.All, true);
        StartCoroutine(ForceField());
        FollowingCam.cam.SetTarget(transform);
    }
    WaitForSeconds forceFieldTime = new WaitForSeconds(5.0f);
    bool forceFieldOn = false;
    IEnumerator ForceField()
    {
        forceFieldOn = true;
        invincibility = true;
        photonView.RPC("SetForceFieldVisibility", RpcTarget.All, true);
        yield return forceFieldTime;
        forceFieldOn = false;
        invincibility = false;
        photonView.RPC("SetForceFieldVisibility", RpcTarget.All, false);
    }
    [PunRPC] public void SetForceFieldVisibility(bool visibility) => forceField.SetActive(visibility);
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("SetHp", newPlayer, hp);
            if (dead) photonView.RPC("SetPlayerActive", newPlayer, false);
            if (forceFieldOn) photonView.RPC("SetForceFieldVisibility", newPlayer, true);
        }
    }
}
public class SkillEventPair
{
    public SkillEventPair()
    {

    }
    public SkillEventPair(Func<float> skillBarFill)
    {
        this.skillBarFill = skillBarFill;
    }
    public SkillEventPair(Func<float> skillBarFill, Func<string> skillDesc)
    {
        this.skillBarFill = skillBarFill;
        this.skillDesc = skillDesc;
    }
    public Func<float> skillBarFill;
    public Func<string> skillDesc;
}
