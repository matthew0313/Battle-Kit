using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Detector : MonoBehaviour
{
    Collider2D m_collider;
    Collider2D _collider
    {
        get
        {
            if(m_collider == null) m_collider = GetComponent<Collider2D>();
            return m_collider;
        }
    }
    private void OnEnable()
    {
        _collider.enabled = true;
    }
    private void OnDisable()
    {
        _collider.enabled = false;
    }
    public List<Player> detected { get; } = new();
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            detected.Add(collision.GetComponent<Player>());
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            detected.Remove(collision.GetComponent<Player>());
        }
    }
}
