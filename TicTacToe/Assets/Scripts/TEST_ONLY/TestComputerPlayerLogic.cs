using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used for initial testing. Deprecated.
public class TestComputerPlayerLogic : MonoBehaviour
{
    public ComputerPlayer cpu;
    bool printed = false;

    void Start()
    {
        TTT.TTTBoard aBoard = new TTT.TTTBoard(3, 3);

        Debug.Log(aBoard.availableMoves);
        StartCoroutine(cpu.GetDecision(aBoard));
    }

    private void Update()
    {
        if (printed)
            return;

        if (!cpu.IsDoneMoving)
            return;

        printed = true;
        Debug.Log(cpu.ChosenMove + ", Expect net outcome of " + cpu.anticipatedResult.outcome + ", Game lasts for " + cpu.anticipatedResult.depth + " turns");
    }
}
