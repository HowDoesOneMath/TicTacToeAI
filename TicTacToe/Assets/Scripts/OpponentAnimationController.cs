using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used to control the table flip animation
[RequireComponent(typeof(Animator))]
public class OpponentAnimationController : MonoBehaviour
{
    //Animator attached
    public Animator anim { get; private set; }

    //List of objects that can be violently thrown or otherwise moved
    static List<Flippable> toFlip;
    public static List<Flippable> ToFlip
    {
        get { if (toFlip == null) { toFlip = new List<Flippable>(); } return toFlip; }
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    //Flips all flippable objects
    //This is called from an animation event
    public void YEET()
    {
        for (int i = 0; i < ToFlip.Count; ++i)
        {
            ToFlip[i].OnFlip();
        }
    }

    //Resets all flippable objects
    //This is called from the main game
    public void UN_YEET()
    {
        //reset animation tree
        anim.SetTrigger("New Game");

        for (int i = 0; i < ToFlip.Count; ++i)
        {
            ToFlip[i].OnUnFlip();
        }
    }
}
