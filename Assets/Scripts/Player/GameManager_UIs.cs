using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GameManager_UIs : MonoBehaviourPunCallbacks
{
    GameManager origin;
    [SerializeField] Animator anim;
    [SerializeField] Transform weaponBar;
    [SerializeField] Text weaponName, ammoDesc;
    public void OnAwake()
    {
        origin = GetComponent<GameManager>();
        origin.myPlayer.weaponChange += WeaponName;
        origin.myPlayer.weaponChange += AmmoDesc;
        origin.myPlayer.ammoDescChange += AmmoDesc;
        origin.myPlayer.onDeath += () => { anim.SetBool("Dead", true); };
        origin.myPlayer.onRevive += () => { anim.SetBool("Dead", false); };
    }
    private void Start()
    {
        WeaponName();
        AmmoDesc();
    }
    private void Update()
    {
        WeaponBar();
    }
    void AmmoDesc()
    {
        ammoDesc.text = (origin.myPlayer.ammoDesc == null) ? "--/--" : origin.myPlayer.ammoDesc.Invoke();
    }
    void WeaponName()
    {
        if (origin.myPlayer.heldWeapon == null)
        {
            weaponName.text = "--";
        }
        else
        {
            string tmp = origin.myPlayer.heldWeapon.name;
            weaponName.text = tmp.Replace("(Clone)", "").Trim();
        }
    }
    void WeaponBar()
    {
        weaponBar.localScale = new Vector2((origin.myPlayer.barFill == null) ? 0.0f : origin.myPlayer.barFill.Invoke(), 1.0f);
    }
}
