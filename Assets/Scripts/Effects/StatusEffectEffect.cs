using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(PhotonView), typeof(PhotonTransformView))]
public class StatusEffectEffect : MonoBehaviourPun
{
    [SerializeField] GameObject[] levels;
    private void Awake()
    {
        if (levels.Length > 0)
        {
            foreach (var i in levels) i.SetActive(false);
            levels[0].SetActive(true);
        }
    }
    public void Level(int level)
    {
        if(level < levels.Length)
        {
            photonView.RPC("EnableLevel", RpcTarget.All, level);
        }
    }
    [PunRPC] public void EnableLevel(int level) => levels[level].SetActive(true);
}