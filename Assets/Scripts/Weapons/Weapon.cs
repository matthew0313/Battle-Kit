using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public abstract class Weapon : MonoBehaviourPunCallbacks
{
    protected Player owner;

    [Header("Player")]
    [SerializeField] protected GameObject model;
    [SerializeField] protected Animator anim;
    protected bool equipped = false;

    private void Awake()
    {
        if (!photonView.IsMine) model.SetActive(false);
    }
    public virtual void Set(Player owner)
    {
        this.owner = owner;
        model.SetActive(false);
        transform.localScale = Vector2.one;
    }
    public virtual void OnWeaponEquip()
    {
        equipped = true;
        photonView.RPC("ModelVisibility", RpcTarget.All, true);
        owner.barFill += BarFill;
        owner.ammoDesc += AmmoDesc;
    }
    public virtual void OnWeaponUpdate()
    {

    }
    protected virtual void Update()
    {

    }
    public virtual void OnWeaponUnequip()
    {
        equipped = false;
        photonView.RPC("ModelVisibility", RpcTarget.All, false);
        owner.barFill -= BarFill;
        owner.ammoDesc -= AmmoDesc;
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (photonView.IsMine)
        {
            if (equipped) photonView.RPC("ModelVisibility", newPlayer, true);
            else photonView.RPC("ModelVisibility", newPlayer, false);
        }
        
    }
    [PunRPC]
    public void ModelVisibility(bool visible)
    {
        model.SetActive(visible);
    }
    public virtual float BarFill()
    {
        return 0.0f;
    }
    public virtual string AmmoDesc()
    {
        return "--/--";
    }
    private void OnDestroy()
    {
        if (photonView.IsMine && equipped) OnWeaponUnequip();
    }
}
