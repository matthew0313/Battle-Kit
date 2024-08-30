using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Attack_Drone : Drone
{
    [SerializeField] float damage, fireRate, bulletSpeed, bulletRange;
    [SerializeField] Bullet bullet;
    [SerializeField] Transform rotator;
    [SerializeField] Transform[] firePoints;
    List<Player> targets = new();
    float counter = 0.0f;
    protected override void Update()
    {
        if (!photonView.IsMine) return;
        base.Update();
        if (counter < fireRate) counter += Time.deltaTime;
        else if(targets.Count > 0)
        {
            counter = 0.0f;
            rotator.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(rotator.position.y - targets[0].transform.position.y, rotator.position.x - targets[0].transform.position.x) * Mathf.Rad2Deg);
            Fire();
        }
    }
    protected virtual void Fire()
    {
        foreach(var i in firePoints)
        {
            PhotonNetwork.Instantiate(Path.Combine("Prefabs", bullet.gameObject.name), i.position, i.rotation).GetComponent<Bullet>().Set(damage, bulletSpeed, bulletRange);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;
        if (collision.CompareTag("Player"))
        {
            Player tmp = collision.GetComponent<Player>();
            if(tmp != owner)
            {
                targets.Add(tmp);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;
        if (collision.CompareTag("Player"))
        {
            Player tmp = collision.GetComponent<Player>();
            if (targets.Contains(tmp)) targets.Remove(tmp);
        }
    }
}
