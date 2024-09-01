using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class DissolveEffect : MonoBehaviourPun
{
    [SerializeField] SpriteRenderer rend;
    [SerializeField] float duration = 1.0f;
    float counter = 0.0f;
    Color color;
    private void Awake()
    {
        color = rend.color;
    }
    private void Update()
    {
        if (!photonView.IsMine) return;
        if (counter < duration)
        {
            photonView.RPC("Dissolve", RpcTarget.All, (duration - counter) / duration);
            counter += Time.deltaTime;
        }
        else PhotonNetwork.Destroy(gameObject);
    }
    [PunRPC]
    public void Dissolve(float newAlpha)
    {
        color.a = newAlpha;
        rend.color = color;
    }
}
