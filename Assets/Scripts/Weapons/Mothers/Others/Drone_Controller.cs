using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Drone_Controller : Weapon
{
    [SerializeField] Transform m_drone;
    [SerializeField] AudioClip deploySound;
    public Transform drone { get { return m_drone; } }
    private void Awake()
    {
        if(deploySound != null) GameManager.Instance.PlaySound(photonView, deploySound);
        drone.SetParent(null);
    }
    [PunRPC] public void SetDroneActive(bool active)
    {
        drone.gameObject.SetActive(active);
    }
    private void OnDestroy()
    {
        Destroy(drone.gameObject);
    }
}
