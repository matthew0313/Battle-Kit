using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

[RequireComponent(typeof(GameManager_UIs), typeof(PhotonView))]
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    public GameManager()
    {
        Instance = this;
    }
    public GameManager_UIs UIs { get; private set; }
    public Player myPlayer { get; private set; }
    private void Awake()
    {
        UIs = GetComponent<GameManager_UIs>();
        myPlayer = PhotonNetwork.Instantiate("Prefabs/Player", new Vector2(Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f)), Quaternion.identity).GetComponent<Player>();
        UIs.OnAwake();
    }
    private void Update()
    {
        if (!photonView.IsMine) return;
    }
}
