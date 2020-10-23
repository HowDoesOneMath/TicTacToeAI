using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGame : MonoBehaviour
{
    public static MainGame mainGame;

    public AbstractPlayer player1;
    public AbstractPlayer player2;

    public static List<GamePiece> pieces = new List<GamePiece>();
    public static List<ClickableSquare> squares = new List<ClickableSquare>();

    public AbstractPlayer winner { get; private set; }

    bool started = false;

    public static TTT.TTTBoard gameBoard;

    public static AbstractPlayer currentPlayer;

    public Queue<AbstractPlayer> playerTurns;

    public UnityEngine.UI.Text winLossText;
    public GameObject uiCanvas;

    public void StartGame(AbstractPlayer p1, AbstractPlayer p2)
    {
        //Deactivate the options to start a game
        uiCanvas.SetActive(false);
        winLossText.gameObject.SetActive(false);

        //Remove any previous board pieces
        for (int i = 0; i < squares.Count; ++i)
        {
            squares[i].DumpPieces();
        }

        for (int i = 0; i < pieces.Count; ++i)
        {
            Destroy(pieces[i].gameObject);
        }

        //Flush queue
        playerTurns = new Queue<AbstractPlayer>();
        winner = null;

        Debug.Log("RESETING");
        //Reset important data
        p1.ResetPlayer(TTT.SquareState.X);
        p2.ResetPlayer(TTT.SquareState.O);

        //Enqueue both players to make moves
        playerTurns.Enqueue(p1);
        playerTurns.Enqueue(p2);

        gameBoard = new TTT.TTTBoard(3, 3);

        started = true;

        //Undo previous table-flips
        OpponentAnimationController oac = null;

        if (player1.TryGetComponent(out oac))
        {
            oac.UN_YEET();
        }

        oac = null;

        if (player2.TryGetComponent(out oac))
        {
            oac.UN_YEET();
        }
    }

    private void Awake()
    {
        mainGame = this;
    }

    void Update()
    {
        if (!started)
            return;

        if (currentPlayer == null)
        {
            CycleTurns();
        }
        else if (currentPlayer.IsDoneMoving)
        {

            int bSize = MainGame.gameBoard.boardLength;
            squares[currentPlayer.ChosenMove.x * bSize+ currentPlayer.ChosenMove.y].PlaceOnSquare(currentPlayer);

            //end: cheater
            if (currentPlayer.lastValidity == MoveValidity.SQUARE_ALREADY_OCCUPIED)
            {
                EndGame(null, true);
                return;
            }

            TTT.SquareState checkForWin = gameBoard.CheckWin();

            //end: tie
            if (checkForWin == TTT.SquareState.Empty)
            {
                if (gameBoard.availableMoves > 0)
                {
                    //DebugBoard();
                    CycleTurns();
                }
                else
                {
                    EndGame(null, false);
                }
            }
            //end: win
            else
            {
                EndGame(currentPlayer, false);
            }
        }
    }

    void EndGame(AbstractPlayer theWinner, bool cheat)
    {
        winner = theWinner;

        playerTurns.Clear();
        currentPlayer = null;

        if (cheat)
        {
            //Check for a cheater
            winLossText.text = "YOU CHEATED!";
            OpponentAnimationController oac = null;

            //The fabled table flip animation
            if (player1.TryGetComponent(out oac))
            {
                oac.anim.SetTrigger("Flip");
            }

            oac = null;

            if (player2.TryGetComponent(out oac))
            {
                oac.anim.SetTrigger("Flip");
            }

            //UI is not immediately activated here, waits for animation event
        }
        else if (theWinner == null)
        {
            //Check for a tie
            winLossText.text = "NOBODY WINS!";

            ActivateUI();
        }
        else
        {
            //Check for a win
            winLossText.text = (theWinner.playingAs + " WINS!");

            ActivateUI();
        }

        started = false;

    }

    public void ActivateUI()
    {
        uiCanvas.SetActive(true);
        winLossText.gameObject.SetActive(true);
    }

    public void StartAsX()
    {
        StartGame(player2, player1);
    }

    public void StartAsO()
    {
        StartGame(player1, player2);
    }

    void DebugBoard()
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

    //Switches which player is playing. Coroutine runs until the player has decided on a move.
    void CycleTurns()
    {
        if (currentPlayer != null)
        {
            playerTurns.Enqueue(currentPlayer);
        }
        currentPlayer = playerTurns.Dequeue();

        StartCoroutine(currentPlayer.GetDecision(gameBoard));
    }

    public void EndGame()
    {
        Application.Quit();
    }
}
