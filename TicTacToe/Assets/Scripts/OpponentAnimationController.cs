using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class OpponentAnimationController : MonoBehaviour
{
    public Animator anim { get; private set; }
    static List<Flippable> toFlip;
    public static List<Flippable> ToFlip
    {
        get { if (toFlip == null) { toFlip = new List<Flippable>(); } return toFlip; }
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void YEET()
    {
        for (int i = 0; i < ToFlip.Count; ++i)
        {
            ToFlip[i].OnFlip();
        }
    }

    public void UN_YEET()
    {
        anim.SetTrigger("New Game");

        for (int i = 0; i < ToFlip.Count; ++i)
        {
            ToFlip[i].OnUnFlip();
        }
    }
}
