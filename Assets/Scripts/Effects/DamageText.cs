using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class DamageText : MonoBehaviourPun
{
    [SerializeField] TMP_Text text;
    const float duration = 1.0f, moveSpeed = 2.0f;
    float counter = 0.0f;
    Color color;
    Vector2 moveDir;
    public void Set(string content, Vector3 color, Vector2 dir)
    {
        photonView.RPC("SetDefaults",RpcTarget.All, content, color);
        moveDir = dir;
    }
    private void Update()
    {
        if (!photonView.IsMine) return;
        if (counter < duration)
        {
            photonView.RPC("Dissolve", RpcTarget.All, (duration - counter) / duration);
            counter += Time.deltaTime;
            transform.Translate(moveDir * (1.0f - counter) * moveSpeed * Time.deltaTime);
        }
        else PhotonNetwork.Destroy(gameObject);
    }
    [PunRPC]
    public void Dissolve(float newAlpha)
    {
        color.a = newAlpha;
        text.color = color;
    }
    [PunRPC]
    public void SetDefaults(string text, Vector3 color)
    {
        this.text.text = text;
        this.color = color.ToColor();
        this.text.color = this.color;
    }
}
