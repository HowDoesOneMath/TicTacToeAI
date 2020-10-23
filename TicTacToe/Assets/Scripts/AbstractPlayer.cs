using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TTT;

public abstract class AbstractPlayer : MonoBehaviour
{
    //X or O
    public SquareState playingAs;

    //Isdone variable due to using coroutines
    [SerializeField]
    protected bool isDoneMoving = true;
    public bool IsDoneMoving { get { return isDoneMoving; } }

    //chosen option
    protected Vector2Int chosenMove;
    public Vector2Int ChosenMove { get { return chosenMove; } }

    //Only used by computer, in order to display how it thinks the game will happen
    public MoveScore anticipatedResult;

    //Repeatedly called until the player moves
    //Makes a function call to AI's logic algorithm in ComputerPlayer
    public abstract IEnumerator GetDecision(TTTBoard board);

    public virtual void ResetPlayer(SquareState playAs)
    {
        //Set playing as variable
        playingAs = playAs;
        lastValidity = MoveValidity.LEGAL;

        //Set shader to display either an X or an O
        piece.GetComponent<MeshRenderer>().sharedMaterial.SetInt("isX", (playAs == SquareState.X) ? 1 : 0);

        Debug.Log(playingAs);
    }

    //The physical piece
    public GamePiece piece;

    //Check for if the last move was valid
    public MoveValidity lastValidity;

    //Debugging purposes
    protected void DebugBoard(TTTBoard gameBoard)
    {
        for (int i = 0; i < 3; ++i)
        {
            string line = "";

            for (int j = 0; j < 3; ++j)
            {
                switch (gameBoard.board[j][i].state)
                {
                    case TTT.SquareState.Empty:
                        line += " ___ ";
                        break;
                    default:
                        line += "  ";
                        line += gameBoard.board[j][i].state;
                        line += "  ";
                        break;
                }
            }

            Debug.Log(line);
        }
    }
}

//Used to provide more clarity than a Vector2Int
//Comes with a bool to check if it should be pruned by AlphaBeta
public class MoveScore
{
    public MoveScore(int score, int length)
    {
        outcome = score;
        depth = length;
    }

    public int outcome;
    public int depth;
    public bool invalid = false;
}
