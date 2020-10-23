using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TTT
{
    //Holds all relevant board information
    //This was initially going to be far more modular, with options to set board size
    //Time constraints made expanding on this impossible, so all boards are just treated as 3x3

    public class TTTBoard
    {
        public int boardLength { get; private set; }
        public int amountForWin { get; private set; }
        public TTTSquare[][] board { get; private set; }
        public int availableMoves { get; private set; }

        public List<TTTWinCondition> winConditions { get; private set; }
        public List<TTTBoardSymmetryCheck> symmetryTypes { get; private set; }

        //Constructor to duplicate boards, such that the AI does not modify the existing board
        public TTTBoard(TTTBoard toDuplicate)
        {
            amountForWin = toDuplicate.amountForWin;
            boardLength = toDuplicate.boardLength;
            board = new TTTSquare[toDuplicate.board.Length][];

            //Sets the board squares to be the same as the previous board
            for (int i = 0; i < boardLength; ++i)
            {
                board[i] = new TTTSquare[boardLength];

                for (int j = 0; j < boardLength; ++j)
                {
                    board[i][j] = new TTTSquare(toDuplicate.board[i][j]);
                }
            }

            availableMoves = toDuplicate.availableMoves;

            SetWinConditions();

            SetSymmetries();
        }

        //Constructor for making a new board
        public TTTBoard(int lengthOfBoard, int winAmnt)
        {
            amountForWin = winAmnt;
            boardLength = lengthOfBoard;
            board = new TTTSquare[boardLength][];

            //List of random priorities to distribute to the board squares
            List<int> randomPriorities = new List<int>();
            for (int i = 0; i < boardLength * boardLength; ++i)
            {
                randomPriorities.Add(i);
            }

            for (int i = 0; i < boardLength; ++i)
            {
                board[i] = new TTTSquare[boardLength];

                for (int j = 0; j < boardLength; ++j)
                {
                    //Priorities are randomly given to allow for greater board diversity
                    int priority = randomPriorities[Random.Range(0, randomPriorities.Count)];
                    board[i][j] = new TTTSquare(priority);
                    randomPriorities.Remove(priority);
                }
            }

            availableMoves = lengthOfBoard * lengthOfBoard;

            SetWinConditions();

            SetSymmetries();
        }

        //Win conditions are updated every time a relevant square is filled
        void SetWinConditions()
        {
            winConditions = new List<TTTWinCondition>();

            //Win conditions for horizontal and vertical sets are handled at once
            for (int i = 0; i < boardLength; ++i)
            {
                TTTWinCondition vertical = new TTTWinCondition(amountForWin);
                TTTWinCondition horizontal = new TTTWinCondition(amountForWin);

                for (int j = 0; j < boardLength; ++j)
                {
                    vertical.AddCheck(board[i][j]);
                    horizontal.AddCheck(board[j][i]);
                }

                winConditions.Add(vertical);
                winConditions.Add(horizontal);
            }

            //The 2 diagonals are dealt with after
            TTTWinCondition diagonalBackSlash = new TTTWinCondition(amountForWin);
            TTTWinCondition diagonalForwardSlash = new TTTWinCondition(amountForWin);

            for (int i = 0; i < boardLength; ++i)
            {
                diagonalBackSlash.AddCheck(board[i][i]);
                diagonalForwardSlash.AddCheck(board[i][boardLength - 1 - i]);
            }

            winConditions.Add(diagonalBackSlash);
            winConditions.Add(diagonalForwardSlash);
        }

        //register all types of symmetries the board can have
        void SetSymmetries()
        {
            TTTBoardSymmetryCheck XaxisSymmetry = new TTTBoardSymmetryCheck();
            TTTBoardSymmetryCheck YaxisSymmetry = new TTTBoardSymmetryCheck();
            TTTBoardSymmetryCheck DiagonalBackSlashSymmetry = new TTTBoardSymmetryCheck();
            TTTBoardSymmetryCheck DiagonalForwardSlashSymmetry = new TTTBoardSymmetryCheck();
            TTTBoardSymmetryCheck QuarterRotationalSymmetry = new TTTBoardSymmetryCheck();
            TTTBoardSymmetryCheck HalfRotationalSymmetry = new TTTBoardSymmetryCheck();

            TTTIndividualSymmetry sym;

            //All the extra set up is taken care of in the TTTIndividualSymmetry constructor, so we only have to worry about sets of squares here
            for (int i = 0; i < boardLength; ++i)
            {
                for (int j = 0; j < boardLength / 2; ++j)
                {
                    sym = new TTTIndividualSymmetry(new TTTSquare[]{ board[i][j], board[i][boardLength - 1 - j]}, XaxisSymmetry);
                }
            }

            for (int i = 0; i < boardLength / 2; ++i)
            {
                for (int j = 0; j < boardLength; ++j)
                {
                    sym = new TTTIndividualSymmetry(new TTTSquare[] { board[i][j], board[boardLength - 1 - i][j] }, YaxisSymmetry);
                }
            }

            for (int i = 0; i < boardLength; ++i)
            {
                for (int j = 0; j < i; ++j)
                {
                    sym = new TTTIndividualSymmetry(new TTTSquare[] { board[i][j], board[j][i] }, DiagonalForwardSlashSymmetry);
                }
            }

            for (int i = 0; i < boardLength; ++i)
            {
                for (int j = boardLength - 1; j >= boardLength - i; --j)
                {
                    sym = new TTTIndividualSymmetry(new TTTSquare[] { board[i][j], board[boardLength - 1 - j][boardLength - 1 - i] }, DiagonalBackSlashSymmetry);
                }
            }

            for (int i = 0; i < (boardLength + 1) / 2; ++i)
            {
                for (int j = 0; j < boardLength / 2; ++j)
                {
                    sym = new TTTIndividualSymmetry(
                        new TTTSquare[] { board[i][j], board[boardLength - 1 - j][i], board[boardLength - 1 - i][boardLength - 1 - j], board[j][boardLength - 1 -i] }, 
                        QuarterRotationalSymmetry
                        );
                }
            }

            for (int i = 0; i < boardLength; ++i)
            {
                for (int j = 0; j < boardLength; ++j)
                {
                    if (i == boardLength - 1 - i && j == boardLength - 1 - j)
                        continue;
                    sym = new TTTIndividualSymmetry(new TTTSquare[] { board[i][j], board[boardLength - 1 - i][boardLength - 1 - j] }, HalfRotationalSymmetry);
                }
            }
        }

        //Gets a queue of every square that may be played upon
        //The AI will then take this queue and loop through it to find the best moves
        public void PopulateMoveQueue(Queue<Vector2Int> toPopulate)
        {
            for (int i = 0; i < boardLength; ++i)
            {
                for (int j = 0; j < boardLength; ++j)
                {
                    if (board[i][j].state != SquareState.Empty)
                    {
                        continue;
                    }

                    toPopulate.Enqueue(new Vector2Int(i, j));
                }
            }
        }

        //Sets a square of the board, but also sets additional information
        public void SetSquareState(Vector2Int loc, SquareState state)
        {
            if (board[loc.x][loc.y].state == state)
            {
                return;
            }

            board[loc.x][loc.y].SetState(state);

            if (state == SquareState.Empty)
            {
                availableMoves += 1;
            }
            else
            {
                availableMoves -= 1;
            }
        }

        //Checks all win conditions. Returning a value of Empty means nobody has won yet.
        public SquareState CheckWin()
        {
            SquareState winner = SquareState.Empty;

            for (int i = 0; i < winConditions.Count; ++i)
            {
                winner = winConditions[i].CheckForWin();

                if (winner != SquareState.Empty)
                {
                    break;
                }
            }

            return winner;
        }
    }

    //Governs a single square of the board
    public class TTTSquare
    {
        //Determines which win conditions this square is a part of
        //These will be updated once a square has been set/reset
        public List<TTTWinCondition> affectedConditions { get; private set; }

        public List<TTTIndividualSymmetry> pairSymmetries { get; private set; }

        public SquareState state { get; private set; }

        //Priority is used to help find out which square should be chosen in the event of a symmetric board
        public int priority { get; private set; }

        public int flaggedBit { get; private set; } = 0;

        public TTTSquare(int squarePriority)
        {
            state = SquareState.Empty;
            priority = squarePriority;

            affectedConditions = new List<TTTWinCondition>();
            pairSymmetries = new List<TTTIndividualSymmetry>();
        }

        public TTTSquare(TTTSquare duplicateSquare)
        {
            state = duplicateSquare.state;
            priority = duplicateSquare.priority;

            affectedConditions = new List<TTTWinCondition>();
            pairSymmetries = new List<TTTIndividualSymmetry>();
        }

        //Upon setting the state, the affected win conditions and symmetries are updated
        public void SetState(SquareState newState)
        {
            for (int i = 0; i < affectedConditions.Count; ++i)
            {
                affectedConditions[i].amountPer[state] -= 1;
                affectedConditions[i].amountPer[newState] += 1;
            }

            state = newState;

            for (int i = 0; i < pairSymmetries.Count; ++i)
            {
                pairSymmetries[i].UpdateSymmetry();
            }
        }

        //Check to see if the board has any symmetry, and the square should not be polled
        //If so, return true only if this square has the highest priority of all its symmetrical squares
        public bool CheckIfCulled()
        {
            for (int i = 0; i < pairSymmetries.Count; ++i)
            {
                if (pairSymmetries[i].mainOwner.isSymmetric && pairSymmetries[i].symmetryPriority > priority)
                {
                    return true;
                }
            }

            return false;
        }
    }

    //All possible square states
    public enum SquareState
    {
        Empty,
        X,
        O
    }

    //A group of symmetrical squares
    //Symmetries are stored using bitwise operations for efficiency
    //If all bits in bitwiseSymmetryCheck are 0, then the board is symmetrical
    public class TTTBoardSymmetryCheck
    {
        public TTTBoardSymmetryCheck()
        {
            checks = new Dictionary<TTTIndividualSymmetry, int>();
        }

        public int bitwiseSymmetryCheck { get; private set; } = 0;

        public bool isSymmetric { get; private set; } = false;

        public Dictionary<TTTIndividualSymmetry, int> checks { get; private set; }

        public void AddSymmetry(TTTIndividualSymmetry symmetry)
        {
            checks.Add(symmetry, checks.Count);
        }

        //bitwise operation - places a '1' at any bit that's not symmetric
        public void UpdateSymmetry(TTTIndividualSymmetry symmetry)
        {
            if (!symmetry.isSymmetric)
                bitwiseSymmetryCheck |= (1 << checks[symmetry]);
            else
                bitwiseSymmetryCheck &= ~(1 << checks[symmetry]);

            //If all bits are 0, there's symmetry
            isSymmetric = (bitwiseSymmetryCheck == 0);
        }
    }

    //Individual symmetry class
    //Checks a small array of squares to see if they're all equal
    public class TTTIndividualSymmetry
    {
        public bool isSymmetric { get; protected set; } = false;
        public int symmetryPriority { get; protected set; } = 0;

        public TTTSquare[] squares { get; protected set; }
        public TTTBoardSymmetryCheck mainOwner { get; protected set; }

        //A priority is chosen based on the highest priority of all squares 
        //Assuming the board is symmetrical, only the highest priority square will get chosen by the AI
        public TTTIndividualSymmetry(TTTSquare[] testedSquares, TTTBoardSymmetryCheck owner)
        {
            squares = testedSquares;
            mainOwner = owner;

            for (int i = 0; i < squares.Length; ++i)
            {
                squares[i].pairSymmetries.Add(this);
                if (symmetryPriority < squares[i].priority)
                    symmetryPriority = squares[i].priority;
            }

            owner.AddSymmetry(this);
            UpdateSymmetry();
        }

        //Repeatedly tests all elements of the array for symmetry
        public void UpdateSymmetry()
        {
            for (int i = 0; i < squares.Length - 1; ++i)
            {
                if (squares[i].state != squares[i + 1].state)
                {
                    isSymmetric = false;
                    mainOwner.UpdateSymmetry(this);
                    return;
                }
            }

            isSymmetric = true;
            mainOwner.UpdateSymmetry(this);
        }
    }

    public class TTTWinCondition
    {
        public List<TTTSquare> checks { get; private set; }
        //Relevant squares that can determine a win
        //For instance, if this were a main diagonal on this board:
        //
        //1 2 3
        //4 5 6
        //7 8 9
        //
        //it could have squares 1, 5, 9 in here

        //amountPer gets how many squares exist in a given state for the win condition
        public Dictionary<SquareState, int> amountPer { get; private set; }

        public int amountForWin { get; private set; }

        public TTTWinCondition(int winAmount)
        {
            checks = new List<TTTSquare>();
            amountForWin = winAmount;
            amountPer = new Dictionary<SquareState, int>();

            //Creates one dictionary element per square state, initialized to 0
            SquareState[] allValues = (SquareState[])System.Enum.GetValues(typeof(SquareState));

            for (int i = 0; i < allValues.Length; ++i)
            {
                amountPer.Add(allValues[i], 0);
            }
        }

        //Upon adding a square to check, it also tells the square to add itself to that square's affectedConditions list
        public void AddCheck(TTTSquare square)
        {
            checks.Add(square);
            square.affectedConditions.Add(this);
            amountPer[square.state] += 1;
        }

        //Checks if the number of any given state is equal to the 'amount for win' (3 in this case, as its a 3x3 board)
        //Returns Empty if there is no winner yet
        public SquareState CheckForWin()
        {
            SquareState win = SquareState.Empty;

            foreach(KeyValuePair<SquareState, int> kvp in amountPer)
            {
                if (kvp.Value >= amountForWin)
                {
                    return kvp.Key;
                }
            }

            return win;
        }
    }
}