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
    [Header("Attack Drone")]
    [SerializeField] float moveSpeed;
    [SerializeField] float damage, fireRate, bulletSpeed, bulletRange;
    [SerializeField] Bullet bullet;
    [SerializeField] Transform[] firePoints;
    [SerializeField] int ammo;
    float counter = 0.0f;
    DroneMode mode = DroneMode.Move;
    Vector2 destination = Vector2.zero;
    enum DroneMode
    {
        Move,
        Attack
    }
    public override void OnDroneDeploy()
    {
        base.OnDroneDeploy();
        destination = transform.position;
    }
    public override void OnDroneUpdate()
    {
        base.OnDroneUpdate();
        if (counter < fireRate) counter += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (mode == DroneMode.Move) mode = DroneMode.Attack;
            else
            {
                destination = transform.position;
                mode = DroneMode.Move;
            }
            owner.ammoDescChange?.Invoke();
        }
        if(mode == DroneMode.Move)
        {
            if (Input.GetMouseButtonDown(0))
            {
                destination = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            float dist = Vector2.Distance(destination, transform.position);
            if (dist > 0.1f)
            {
                transform.Translate(Vector2.ClampMagnitude((destination - (Vector2)transform.position).normalized * Time.deltaTime * moveSpeed, dist), Space.World);
            }
        }
        else if (mode == DroneMode.Attack)
        {
            if (Input.GetMouseButton(0))
            {
                if (counter >= fireRate)
                {
                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    counter -= fireRate;
                    transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x) * Mathf.Rad2Deg);
                    if (ammo > 0)
                    {
                        ammo--;
                        owner.ammoDescChange?.Invoke();
                        foreach (var i in firePoints)
                        {
                            PhotonNetwork.Instantiate(Path.Combine("Prefabs", bullet.gameObject.name), i.position, i.rotation).GetComponent<Bullet>().Set(damage, bulletSpeed, bulletRange);
                        }
                    }
                }
            }
        }
    }
    public override string AmmoDesc()
    {
        return (mode == DroneMode.Move) ? "Mode: Moving" : $"Mode: Attacking (ammo: {ammo})";
    }
    /*[SerializeField] float damage, fireRate, bulletSpeed, bulletRange;
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
    }*/
}