using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Playing piece for the game - serves only as a marker, not related to the algorithm
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(Rigidbody))]
public class GamePiece : MonoBehaviour
{
    public MeshRenderer mr { get; private set; }
    public MeshFilter mf { get; private set; }
    public Rigidbody rb { get; private set; }

    public bool placed = false;

    private void Awake()
    {
        mr = GetComponent<MeshRenderer>();
        mf = GetComponent<MeshFilter>();
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
    }

    //Registers pieces on the main game board
    public void SetPlaced()
    {
        placed = true;

        MainGame.pieces.Add(this);
    }

    //Only if placed will it remove itself from the list
    private void OnDestroy()
    {
        if (placed)
            MainGame.pieces.Remove(this);
    }
}
