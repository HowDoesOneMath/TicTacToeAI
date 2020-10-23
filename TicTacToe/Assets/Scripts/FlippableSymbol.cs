using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlippableSymbol : Flippable
{
    public bool placed = false;

    public override void OnFlip()
    {
        if (placed)
            base.OnFlip();
    }

    public override void OnUnFlip()
    {
        if (placed)
            base.OnUnFlip();
    }
}
