using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(PhotonView), typeof(PhotonTransformView))]
public class StatusEffectText : MonoBehaviourPun
{
    [SerializeField] TMP_Text text;
    private void Awake()
    {
        gameObject.SetActive(false);
    }
    public void Set(string text, Color color)
    {
        gameObject.SetActive(true);
        this.text.color = color;
        this.text.text = text;
    }
    [PunRPC] public void SetText(string text) => this.text.text = text;
}