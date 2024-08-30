using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class Drone_Deployer : Weapon
{
    [SerializeField] Drone drone;
    [SerializeField] Transform spawnPoint;
    [SerializeField] float spawnCooldown;
    Drone deployed = null;
    float counter = 0.0f;
    public override void OnWeaponUpdate()
    {
        base.OnWeaponUpdate();
        if (counter < spawnCooldown) counter += Time.deltaTime;
        else if (Input.GetMouseButtonDown(0))
        {
            if (deployed == null)
            {
                deployed = PhotonNetwork.Instantiate(Path.Combine("Prefabs", drone.gameObject.name), spawnPoint.position, spawnPoint.rotation).GetComponent<Drone>();
                deployed.Set(owner);
            }
            else
            {
                PhotonNetwork.Destroy(deployed.gameObject);
                deployed = null;
            }
            owner.ammoDescChange?.Invoke();
            counter = 0.0f;
        }
    }
    public override string AmmoDesc()
    {
        return (deployed == null) ? "Not Deployed" : "Deployed";
    }
    public override float BarFill()
    {
        return 1.0f - counter / spawnCooldown;
    }
    private void OnDestroy()
    {
        if (deployed != null)
        {
            PhotonNetwork.Destroy(deployed.gameObject);
        }
    }
}
