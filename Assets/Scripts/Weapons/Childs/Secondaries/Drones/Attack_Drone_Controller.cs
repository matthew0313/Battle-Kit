using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

public class Attack_Drone_Controller : Drone_Controller
{
    [Header("Attack Drone Controller")]
    [SerializeField] internal float moveSpeed;
    [SerializeField] internal float fireRate, bulletDamage, bulletSpeed, bulletRange;
    [SerializeField] internal float autoMoveSpeed, autoMoveMinDist = 1.5f;
    [SerializeField] internal float autoFireRate, autoBulletDamage, autoBulletSpeed, autoBulletRange;
    [SerializeField] internal int ammo = 100;
    [SerializeField] internal bool automaticFire = true;
    [SerializeField] internal Bullet bullet;
    [SerializeField] internal Transform[] firePoints;
    TopLayer<Attack_Drone_Controller> topLayer;

    internal List<Player> targets = new();

    internal SkillEventPair autoManualSwitch, moveAttackSwitch;

    internal float counter = 0.0f;
    public override void Set(Player owner)
    {
        base.Set(owner);
        autoManualSwitch = new SkillEventPair(() => 1.0f);
        moveAttackSwitch = new SkillEventPair(() => 1.0f, () => "[E]: 조종 모드 전환");
        topLayer = new TopLayer(this);
        topLayer.OnStateEnter();
        skillList.Add(autoManualSwitch);
    }
    internal Func<string> ammoDesc = () => "--/--";
    protected override void Update()
    {
        base.Update();
        if (!photonView.IsMine) return;
        if (topLayer != null) topLayer.OnStateUpdate();
    }
    public override string AmmoDesc()
    {
        return ammoDesc.Invoke();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            targets.Add(collision.GetComponent<Player>());
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            targets.Remove(collision.GetComponent<Player>());
        }
    }
}
class TopLayer : TopLayer<Attack_Drone_Controller>
{
    public TopLayer(Attack_Drone_Controller origin) : base(origin)
    {
        defaultState = new Automatic(origin, this);
        AddState("Automatic", defaultState);
        AddState("Manual", new Manual(origin, this));
    }
}
class Automatic : State<Attack_Drone_Controller>
{
    public Automatic(Attack_Drone_Controller origin, Layer<Attack_Drone_Controller> parent) : base(origin, parent)
    {

    }
    public override void OnStateEnter()
    {
        base.OnStateEnter();
        origin.ammoDesc = () => $"자동 조종 (잔탄수: {origin.ammo})";
        origin.autoManualSwitch.skillDesc = () => "[F]: 수동 조종으로 전환";
        origin.owner.skillChange?.Invoke();
        origin.owner.ammoDescChange?.Invoke();
    }
    public override void OnStateUpdate()
    {
        base.OnStateUpdate();
        if (origin.equipped && Input.GetKeyDown(KeyCode.F))
        {
            parentLayer.ChangeState("Manual");
            return;
        }
        float dist = Vector2.Distance(origin.drone.position, origin.owner.transform.position);
        if (dist > origin.autoMoveMinDist)
        {
            origin.drone.Translate(Vector2.ClampMagnitude((origin.owner.transform.position - origin.drone.transform.position).normalized * Time.deltaTime * origin.autoMoveSpeed, dist), Space.World);
        }
        if (origin.counter < origin.autoFireRate) origin.counter += Time.deltaTime;
        if(origin.counter >= origin.autoFireRate && origin.targets.Count > 0)
        {
            origin.counter = 0;
            origin.targets.Sort((Player a, Player b) => Vector2.Distance(a.transform.position, origin.drone.transform.position).CompareTo(Vector2.Distance(b.transform.position, origin.drone.transform.position)));
            Vector2 targetPos = origin.targets[0].transform.position;
            origin.drone.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(targetPos.y - origin.drone.position.y, targetPos.x - origin.drone.position.x) * Mathf.Rad2Deg);
            if(origin.ammo > 0)
            {
                origin.ammo--;
                foreach (var i in origin.firePoints)
                {
                    PhotonNetwork.Instantiate(Path.Combine("Prefabs", origin.bullet.gameObject.name), i.position, i.rotation).GetComponent<Bullet>().Set(origin.autoBulletDamage, origin.autoBulletSpeed, origin.autoBulletRange, null, null);
                }
                origin.owner.ammoDescChange?.Invoke();
            }
        }
    }
}
class Manual : Layer<Attack_Drone_Controller>
{
    public Manual(Attack_Drone_Controller origin, Layer<Attack_Drone_Controller> parent) : base(origin, parent)
    {
        defaultState = new Manual_Moving(origin, this);
        AddState("Moving", defaultState);
        AddState("Attacking", new Manual_Attacking(origin, this));
    }
    public override void OnStateEnter()
    {
        base.OnStateEnter();
        origin.autoManualSwitch.skillDesc = () => "[F]: 자동 조종으로 전환";
        origin.skillList.Add(origin.moveAttackSwitch);
        origin.owner.skillChange?.Invoke();
    }
    public override void OnStateUpdate()
    {
        base.OnStateUpdate();
        if (!origin.equipped) return;
        if (Input.GetKeyDown(KeyCode.F))
        {
            parentLayer.ChangeState("Automatic");
            return;
        }
    }
    public override void OnStateExit()
    {
        base.OnStateExit();
        origin.skillList.Remove(origin.moveAttackSwitch);
        origin.owner.skillChange?.Invoke();
    }
}
class Manual_Moving : State<Attack_Drone_Controller>
{
    public Manual_Moving(Attack_Drone_Controller origin, Layer<Attack_Drone_Controller> parent) : base(origin, parent)
    {

    }
    Vector2 moveTo;
    public override void OnStateEnter()
    {
        base.OnStateEnter();
        origin.ammoDesc = () => $"이동 모드 (잔탄수: {origin.ammo})";
        origin.owner.ammoDescChange?.Invoke();
        moveTo = origin.drone.position;
    }
    public override void OnStateUpdate()
    {
        base.OnStateUpdate();
        float dist = Vector2.Distance(origin.drone.position, moveTo);
        if (dist > 0.1f)
        {
            origin.drone.Translate(Vector2.ClampMagnitude((moveTo - (Vector2)origin.drone.transform.position).normalized * Time.deltaTime * origin.moveSpeed, dist), Space.World);
        }
        if (!origin.equipped) return;
        if (Input.GetMouseButtonDown(0))
        {
            moveTo = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            parentLayer.ChangeState("Attacking");
        }
    }
}
class Manual_Attacking : State<Attack_Drone_Controller>
{
    public Manual_Attacking(Attack_Drone_Controller origin, Layer<Attack_Drone_Controller> parent) : base(origin, parent)
    {

    }
    public override void OnStateEnter()
    {
        base.OnStateEnter();
        origin.ammoDesc = () => $"공격 모드 (잔탄수: {origin.ammo})";
        origin.owner.ammoDescChange?.Invoke();
    }
    public override void OnStateUpdate()
    {
        base.OnStateUpdate();
        if (!origin.equipped) return;
        if (origin.counter < origin.fireRate) origin.counter += Time.deltaTime;
        if(origin.counter >= origin.fireRate)
        {
            if (Input.GetMouseButton(0))
            {
                origin.counter = 0;
                Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                origin.drone.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(targetPos.y - origin.drone.position.y, targetPos.x - origin.drone.position.x) * Mathf.Rad2Deg);
                if (origin.ammo > 0)
                {
                    origin.ammo--;
                    foreach (var i in origin.firePoints)
                    {
                        PhotonNetwork.Instantiate(Path.Combine("Prefabs", origin.bullet.gameObject.name), i.position, i.rotation).GetComponent<Bullet>().Set(origin.bulletDamage, origin.bulletSpeed, origin.bulletRange, null, null);
                    }
                    origin.owner.ammoDescChange?.Invoke();
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            parentLayer.ChangeState("Moving");
        }
    }
}