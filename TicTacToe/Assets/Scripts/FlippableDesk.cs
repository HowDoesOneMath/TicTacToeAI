using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Attached to anything that exists in the scene that we want to move
public class FlippableDesk : Flippable
{
    public float strength;
    public Transform forcePosition;

    //Option to throw the object in a direction once flipped if strength > 0
    public override void OnFlip()
    {
        base.OnFlip();

        rb.AddForceAtPosition(forcePosition.up * strength, forcePosition.position, ForceMode.Impulse);
    }

    public override void OnUnFlip()
    {
        base.OnUnFlip();
    }
}
