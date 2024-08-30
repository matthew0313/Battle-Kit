using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviourPunCallbacks
{
    public void StartGame()
    {
        PhotonNetwork.JoinOrCreateRoom("Game", new RoomOptions() { MaxPlayers = 20, IsVisible = true, IsOpen = true }, TypedLobby.Default, null);
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Game");
    }
}
