using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.IO;
using System;

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
    public bool roundInProgress { get; private set; } = false;

    [SerializeField] float coinSpawnRate = 2.0f;
    [SerializeField] float roundStartTime = 5.0f;
    [SerializeField] float mapSize = 20.0f;
    private void Awake()
    {
        UIs = GetComponent<GameManager_UIs>();
        myPlayer = PhotonNetwork.Instantiate(Path.Combine("Prefabs", "Player"), Vector2.zero, Quaternion.identity).GetComponent<Player>();
        myPlayer.gameObject.SetActive(false);
        UIs.OnAwake();
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SetInfos", newPlayer, roundInProgress);
        }
    }
    float counter = 0.0f;
    float coinCounter = 0.0f;
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (!roundInProgress)
            {
                if (counter < roundStartTime) counter += Time.deltaTime;
                if(counter >= roundStartTime && PhotonNetwork.CurrentRoom.PlayerCount > 1)
                {
                    photonView.RPC("StartRound", RpcTarget.All);
                }
            }
            else
            {
                if (coinCounter < coinSpawnRate) coinCounter += Time.deltaTime;
                if(coinCounter >= coinSpawnRate)
                {
                    coinCounter -= coinSpawnRate;
                    SpawnCoin(new Vector2(UnityEngine.Random.Range(-mapSize, mapSize), UnityEngine.Random.Range(-mapSize, mapSize)));
                }
            }
        }
    }
    [PunRPC] public void SpawnCoin(Vector2 pos)
    {
        PhotonNetwork.Instantiate(Path.Combine("Prefabs", "Coin"), pos, Quaternion.identity);
    }
    public Action onRoundStateSet, onRoundStart, onRoundEnd;
    [PunRPC] public void SetInfos(bool roundInProgress)
    {
        this.roundInProgress = roundInProgress;
        myPlayer.gameObject.SetActive(roundInProgress);
        if(roundInProgress) myPlayer.RoundStart();
        onRoundStateSet?.Invoke();
    }
    [PunRPC] public void StartRound()
    {
        if (roundInProgress == true) return;
        winner = false;
        roundInProgress = true;
        myPlayer.gameObject.SetActive(true);
        myPlayer.RoundStart();
        onRoundStart?.Invoke();
    }
    [PunRPC] public void EndRound()
    {
        if (roundInProgress == false) return;
        roundInProgress = false;
        myPlayer.gameObject.SetActive(false);
        counter = 0.0f;
        onRoundEnd?.Invoke();
    }
    public bool winner { get; private set; } = false;
    public void WinGame()
    {
        winner = true;
        photonView.RPC("EndRound", RpcTarget.All);
    }
}
