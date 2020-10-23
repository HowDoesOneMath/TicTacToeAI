using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used for the X and O markers in the game
public class FlippableSymbol : Flippable
{
    public bool placed = false;

    //Only ones that are placed on the board will get thrown.
    //This is to avoid an edge case mentioned in RealPlayer
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
