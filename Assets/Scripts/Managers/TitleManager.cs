using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    static bool openingWatched = false;
    [SerializeField] Animator anim;
    private void Awake()
    {
        if (!openingWatched)
        {
            anim.Play("LobbyEnter");
            openingWatched = true;
        }
    }
}