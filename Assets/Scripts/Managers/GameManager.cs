using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.IO;
using System;
using UnityEngine.SceneManagement;

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
    public Dictionary<int, Player> players { get; } = new();
    public bool roundInProgress { get; private set; } = false;

    [SerializeField] float coinSpawnRate = 2.0f;
    [SerializeField] float roundStartTime = 5.0f;
    [SerializeField] float mapSize = 20.0f;

    Dictionary<string, AudioClip> clips = new();
    List<(GameObject, AudioSource)> sources = new();
    private void Awake()
    {
        UIs = GetComponent<GameManager_UIs>();
        myPlayer = PhotonNetwork.Instantiate(Path.Combine("Prefabs", "Player"), Vector2.zero, Quaternion.identity).GetComponent<Player>();
        myPlayer.gameObject.SetActive(false);
        UIs.OnAwake();
        foreach(var i in Resources.LoadAll<AudioClip>("Audio"))
        {
            clips.Add(i.name, i);
        }
    }
    [PunRPC] public void PlaySoundAll(int objectID, string clipName)
    {
        if (!clips.ContainsKey(clipName)) return;
        PhotonView target = PhotonView.Find(objectID);
        if (target == null) return;
        foreach(var i in sources)
        {
            if (!i.Item2.isPlaying)
            {
                i.Item1.transform.position = target.transform.position;
                i.Item2.clip = clips[clipName];
                i.Item2.Play();
                return;
            }
        }
        AudioSource tmp = new GameObject("AudioSource").AddComponent<AudioSource>();
        tmp.transform.position = target.transform.position;
        tmp.clip = clips[clipName];
        tmp.playOnAwake = false;
        tmp.Play();
        sources.Add((tmp.gameObject, tmp));
    }
    public void PlaySound(PhotonView target, AudioClip clip)
    {
        photonView.RPC("PlaySoundAll", RpcTarget.All, target.ViewID, clip.name);
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
        foreach (var i in sources) i.Item2.Stop();
        onRoundEnd?.Invoke();
    }
    public bool winner { get; private set; } = false;
    public void WinGame()
    {
        winner = true;
        photonView.RPC("EndRound", RpcTarget.All);
    }
    public void ReturnToLobby()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
