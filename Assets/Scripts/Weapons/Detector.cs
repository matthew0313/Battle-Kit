using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Detector : MonoBehaviour
{
    public List<Player> detected { get; } = new();
    public Action<Player> onDetect, onDetectEnd, onOneTimeDetect;
    List<Player> oneTimeHitList = new();
    public void Refresh() => oneTimeHitList.Clear();
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player tmp = collision.GetComponent<Player>();
            onDetect?.Invoke(tmp);
            detected.Add(collision.GetComponent<Player>());
            if (!oneTimeHitList.Contains(tmp))
            {
                oneTimeHitList.Add(tmp);
                onOneTimeDetect?.Invoke(tmp);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player tmp = collision.GetComponent<Player>();
            onDetectEnd?.Invoke(tmp);
            detected.Remove(collision.GetComponent<Player>());
        }
    }
}
