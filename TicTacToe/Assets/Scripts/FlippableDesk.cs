using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlippableDesk : Flippable
{
    public float strength;
    public Transform forcePosition;

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
