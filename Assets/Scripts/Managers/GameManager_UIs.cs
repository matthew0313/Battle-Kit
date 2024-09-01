using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.IO;

public class GameManager_UIs : MonoBehaviourPunCallbacks
{
    GameManager origin;
    [SerializeField] Animator anim;
    [SerializeField] Transform weaponBar;
    [SerializeField] Text weaponName, ammoDesc, extraDesc;
    [SerializeField] DamageText damageText;
    [SerializeField] Transform skillUIParent;
    [SerializeField] SkillUIElement skillUIElement;
    List<SkillUIElement> skillUIs = new();
    public void OnAwake()
    {
        origin = GetComponent<GameManager>();
        origin.myPlayer.weaponChange += WeaponName;
        origin.myPlayer.weaponChange += AmmoDesc;
        origin.myPlayer.weaponChange += ExtraDesc;
        origin.myPlayer.weaponChange += SetSkillBar;
        origin.myPlayer.ammoDescChange += AmmoDesc;
        origin.myPlayer.skillDescChange += SetSkillDesc;
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
        SkillBar();
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
    void SetSkillBar()
    {
        int i = 0;
        for(; i < origin.myPlayer.skillList.Count; i++)
        {
            if(skillUIs.Count <= i)
            {
                skillUIs.Add(Instantiate(skillUIElement, skillUIParent));
            }
            else
            {
                skillUIs[i].gameObject.SetActive(true);
            }
        }
        for(; i < skillUIs.Count; i++)
        {
            skillUIs[i].gameObject.SetActive(false);
        }
        SetSkillDesc();
    }
    void SetSkillDesc()
    {
        for(int i = 0; i < origin.myPlayer.skillList.Count; i++)
        {
            skillUIs[i].skillName.text = origin.myPlayer.skillList[i].skillDesc.Invoke();
        }
    }
    void SkillBar()
    {
        for(int i = 0; i < origin.myPlayer.skillList.Count; i++)
        {
            skillUIs[i].skillBar.localScale = new Vector2(origin.myPlayer.skillList[i].skillBarFill.Invoke(), 1.0f);
        }
    }
    public void DamageText(float damage, Vector2 position)
    {
        DamageText tmp = PhotonNetwork.Instantiate(Path.Combine("Effects", damageText.gameObject.name), position, Quaternion.identity).GetComponent<DamageText>();
        if (damage >= 40.0f)
        {
            tmp.Set(damage.ToString() + "!!", new Color(1, 0, 0), new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized);
        }
        else if(damage >= 20.0f)
        {
            tmp.Set(damage.ToString() + "!", new Color(1, 0.5f, 0), new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized);
        }
        else
        {
            tmp.Set(damage.ToString(), new Color(1, 1, 0), new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized);
        }
    }
    void ExtraDesc()
    {
        if (origin.myPlayer.extraDesc == null) extraDesc.text = "";
        else extraDesc.text = origin.myPlayer.extraDesc.Invoke();
    }
}
