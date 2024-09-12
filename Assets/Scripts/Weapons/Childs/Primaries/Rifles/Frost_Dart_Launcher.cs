using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class Frost_Dart_Launcher : Gun
{
    protected override Bullet FireBullet(Vector2 position, Quaternion rotation)
    {
        Bullet tmp = base.FireBullet(position, rotation);
        tmp.inflictingEffect = "FrostDartLauncher_Frost";
        return tmp;
    }
}