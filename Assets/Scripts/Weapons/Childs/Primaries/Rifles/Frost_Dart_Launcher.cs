using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class Frost_Dart_Launcher : Gun
{
    protected override void FireBullet(Vector2 position, Quaternion rotation)
    {
        Bullet tmp = PhotonNetwork.Instantiate(Path.Combine("Prefabs", bullet.gameObject.name), position, rotation).GetComponent<Bullet>();
        tmp.Set(damage, bulletSpeed, bulletRange, null, "FrostDartLauncher_Frost");
    }
}