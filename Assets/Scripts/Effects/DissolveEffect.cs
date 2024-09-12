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
        if (counter < duration)
        {
            counter += Time.deltaTime;
            Dissolve(1.0f - counter / duration);
        }
        else
        {
            if(photonView.IsMine) PhotonNetwork.Destroy(gameObject);
        }
    }
    public void Dissolve(float newAlpha)
    {
        color.a = newAlpha;
        rend.color = color;
    }
}
