using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ClickableSquare : MonoBehaviour
{
    //Determines which of the 9 spaces the square corresponds to.
    public int MySquareNumber;

    //game pieces
    public List<GamePiece> myPieces;

    public MeshRenderer mr { get; private set; }

    //How far off-center the X or O can be placed
    public float moveSpread = 0.05f;

    private void Awake()
    {
        myPieces = new List<GamePiece>();
        mr = GetComponent<MeshRenderer>();

        mr.material = Instantiate(mr.material);

        //Force each square to position itself properly in the static list
        while (MainGame.squares.Count <= MySquareNumber)
        {
            MainGame.squares.Add(null);
        }
        MainGame.squares[MySquareNumber] = this;
    }

    //Add a physical playing piece to the board. Also checks if a piece already existed there.
    public void PlaceOnSquare(AbstractPlayer ap)
    {
        if (MainGame.currentPlayer != ap)
        {
            ap.lastValidity = MoveValidity.NOT_YOUR_TURN;
            return;
        }

        //Size of board - to account for modularity.
        int bSize = MainGame.gameBoard.boardLength;
        TTT.TTTSquare selectedSquare = MainGame.gameBoard.board[MySquareNumber / bSize][MySquareNumber % bSize];

        //Determines legality of move.
        //Ordinarily you'd deny the player to move illegally, but I found it more fun to let it happen
        //The game halts if you attempt an illegal move
        bool legal = (selectedSquare.state == TTT.SquareState.Empty);

        MainGame.gameBoard.SetSquareState(new Vector2Int(MySquareNumber / bSize, MySquareNumber % bSize), ap.playingAs);

        //Instantiate piece on the board square you click on
        GamePiece newPiece = Instantiate(ap.piece, transform.position + new Vector3(Random.Range(-moveSpread, moveSpread), myPieces.Count * 0.03f, Random.Range(-moveSpread, moveSpread)), transform.rotation);
        FlippableSymbol flippable = newPiece.GetComponent<FlippableSymbol>();
        newPiece.SetPlaced();
        if (flippable != null)
            flippable.placed = true;

        myPieces.Add(newPiece);

        //check legality
        if (legal)
            ap.lastValidity = MoveValidity.LEGAL;
        else
            ap.lastValidity = MoveValidity.SQUARE_ALREADY_OCCUPIED;
    }

    public void DumpPieces()
    {
        myPieces.Clear();
    }
}

public enum MoveValidity
{
    LEGAL,
    NOT_YOUR_TURN,
    SQUARE_ALREADY_OCCUPIED
}