using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PhotonView))]
public class Coin : MonoBehaviourPunCallbacks
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform == GameManager.Instance.myPlayer.transform)
        {
            GameManager.Instance.myPlayer.EarnCoin();
            gameObject.SetActive(false);
            photonView.RPC("DestroyCoin", photonView.Owner);
        }
    }
    [PunRPC] public void DestroyCoin()
    {
        PhotonNetwork.Destroy(gameObject);
    }
    private void Awake()
    {
        GameManager.Instance.onRoundEnd += Check;
    }
    private void OnDestroy()
    {
        GameManager.Instance.onRoundEnd -= Check;
    }
    void Check()
    {
        if (!photonView.IsMine) return;
        PhotonNetwork.Destroy(gameObject);
    }
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        photonView.TransferOwnership(newMasterClient);
    }
}