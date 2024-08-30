using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FollowingCam : MonoBehaviour
{
    public static FollowingCam cam { get; private set; }
    public FollowingCam()
    {
        cam = this;
    }
    [SerializeField] float lerpRate = 1.0f;
    Transform target;
    public void SetTarget(Transform target)
    {
        this.target = target;
    }
    private void Update()
    {
        if (target != null) transform.position = Vector2.Lerp(transform.position, target.position, lerpRate * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, transform.position.y, -10.0f);
    }
}
