using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TTT;

//Derivative of abstract player, handles computer users
public class ComputerPlayer : AbstractPlayer
{
    public int statesSampled = 0;
    public int culledStates = 0;

    //all free squares
    Queue<Vector2Int> moveChoices = new Queue<Vector2Int>();

    //all choices that are good choices for the AI
    Queue<Vector2Int> viableChoices = new Queue<Vector2Int>();

    //Determines how long a coroutine is allowed to take, so the game doesn't freeze while the AI is thinking
    public long millisecondsPerCycle = 17;

    //Allows the option to disable the coroutine's time limit
    public bool NonBlocking = true;

    System.DateTime currentTime;

    //Disable/Enable efficiency methods
    public bool AlphaBetaPrune = true;
    public bool SymmetryPrune = true;

    public override IEnumerator GetDecision(TTTBoard board)
    {
        currentTime = System.DateTime.Now;

        isDoneMoving = false;

        //Reset previous data
        anticipatedResult = new MoveScore(0, 0);
        statesSampled = 0;
        culledStates = 0;
        moveChoices.Clear();
        viableChoices.Clear();

        //Copy the board to avoid affecting the real one
        TTTBoard testBoard = new TTTBoard(board);

        //Get all free squares - the player may cheat, but the AI does not
        testBoard.PopulateMoveQueue(moveChoices);

        //Error checking for if we try to edit a board that's complete
        if (moveChoices.Count > 0)
        {
            chosenMove = moveChoices.Peek();
        }
        else
        {
            UnityEngine.Debug.LogError("TRYING TO PLAY ON ALREADY COMPLETE BOARD");
            isDoneMoving = true;
            chosenMove = Vector2Int.one * -1;
        }

        //Allows us to check whether or not the following coroutine has finished, by saving a reference to a bool
        BoolCheck newBc = new BoolCheck();
        StartCoroutine(GetBestMovesRecursive(testBoard, playingAs, 0, anticipatedResult, newBc, 2));

        //Keep waiting for previous functions to complete before moving on
        while (!newBc.complete)
        {
            yield return new WaitForEndOfFrame();
        }

        //Debug info
        Debug.Log("Computer player " + playingAs.ToString() + " processed " + statesSampled + " unique states, " +
            "best case scenario: " + anticipatedResult.outcome + " after " + anticipatedResult.depth + " turns");

        string tot = "";
        for (int i = 0; i < viableChoices.Count; ++i)
        {
            Vector2Int vChoice = viableChoices.Dequeue();
            viableChoices.Enqueue(vChoice);
            tot += vChoice;
            tot += " ";
        }

        Debug.Log("Choices: " + tot);
    }

    public override void ResetPlayer(SquareState playAs)
    {
        base.ResetPlayer(playAs);
    }

    private void LateUpdate()
    {

    }

    //Separate call to MainGame to activate UI, as bad timing with the table flip was bothering me.
    public void FlipTable()
    {
        MainGame.mainGame.ActivateUI();
    }

    //Takes in the board, the type of symbol (X, O) to simulate being placed, the level of recursion,
    //  a reference to the outcome to return, a check to say if it completed or was interrupted, and a previous best for AlphaBeta pruning
    private IEnumerator GetBestMovesRecursive(TTTBoard board, SquareState toPlace, int functionDepth, MoveScore returnOutcome, BoolCheck bc, int previousBest)
    {
        statesSampled += 1;
        //Outcome determines if the end result is a win, loss or tie.
        //It gets set to be the state we expect to get overriden, with 1 meaning a win and -1 meaning a loss
        //Because we expect the AI to only make moves that improve its odds of winning, we initialize it to -2 if it's the AI's turn
        //That way, it will get overriden immediately
        //The opposite is true for if it's the player's turn
        int outcome = ((toPlace == playingAs) ? -2 : 2);

        //Estimate of how long before the end of the game
        int movesUntilEndgame = 0;

        //Used to determine if the rest of the loop is garbage, due to alpha-beta pruning
        //The reason this is used as opposed to break; is to ensure that the list of move choices is kept consistent
        //The queue should be unaltered once the function passes to a higher recursion
        bool skipRestOfLoop = false;

        for (int i = 0; i < moveChoices.Count; ++i)
        {
            //For if alpha-beta determines all proceeding tests to be obsolete
            if (skipRestOfLoop)
            {
                //cycles through the queue until it returns to its starting arrangement
                moveChoices.Enqueue(moveChoices.Dequeue());
                continue;
            }

            //Take a move from the queue to test
            Vector2Int testMove = moveChoices.Dequeue();

            //Checks if the square is worth testing according to symmetry pruning - if it's not, skip the square and continue
            if (SymmetryPrune)
            {
                if (board.board[testMove.x][testMove.y].CheckIfCulled())
                {
                    moveChoices.Enqueue(testMove);
                    culledStates += 1;
                    continue;
                }
            }

            //Sets the square of the simulated board
            board.SetSquareState(testMove, toPlace);

            MoveScore moveScore = new MoveScore(0, 0);

            SquareState isVictory = board.CheckWin();

            if (isVictory == SquareState.Empty)
            {
                //Nobody one, continue simulation if possible
                if (moveChoices.Count > 0)
                {
                    //Set move score, which indicates (GameResult, GameLength).
                    //Passes the current best outcome to conduct AlphaBeta
                    BoolCheck newBc = new BoolCheck();
                    IEnumerator getReturn = GetBestMovesRecursive(board, (toPlace == SquareState.O) ? SquareState.X : SquareState.O, functionDepth + 1, moveScore, newBc, outcome);
                    StartCoroutine(getReturn);

                    //Do not continue until the inner function has succeeded
                    while (!newBc.complete)
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    //If moveScore is marked as invalid, it means to prune it
                    //Move to the next iteration
                    if (moveScore.invalid)
                    {
                        moveChoices.Enqueue(testMove);
                        board.SetSquareState(testMove, SquareState.Empty);
                        continue;
                    }

                    //Keeps track of how long the game will last, with the leaf node starting at 1 and counting up
                    moveScore.depth += 1;
                }
                else
                {
                    //Tie
                    moveScore = new MoveScore(0, 1);
                }
            }
            else if (isVictory == playingAs)
            {
                //you won
                moveScore = new MoveScore(1, 1);
            }
            else
            {
                //Opponent won
                moveScore = new MoveScore(-1, 1);
            }

            moveChoices.Enqueue(testMove);
            board.SetSquareState(testMove, SquareState.Empty);

            if (AlphaBetaPrune && functionDepth > 0)
            {
                //AlphaBeta
                //The root function should not alpha-beta prune, only receive a series of possible moves
                if (playingAs == toPlace)
                {
                    //If it's simulating your move and you can do better than the current best, your opponent will move to stop it
                    //Mark the return as invalid to tell the outer function to skip it
                    if (previousBest < moveScore.outcome)
                    {
                        culledStates += 1;
                        returnOutcome.invalid = true;
                        skipRestOfLoop = true;
                        continue;
                    }
                }
                else
                {
                    //Opposite logic for opponent's turn
                    if (previousBest > moveScore.outcome)
                    {
                        culledStates += 1;
                        returnOutcome.invalid = true;
                        skipRestOfLoop = true;
                        continue;
                    }
                }
            }

            if (playingAs == toPlace)
            {
                //If the move proposes a better result, then we overwrite our existing best move and options
                //If the result is net positive, we want a smaller MovesUntilEndgame. If net negative, we'll take the bigger.
                //If it's a tie, we don't care due to the fact that all tie games take the same amount of moves
                if (moveScore.outcome > outcome)
                {
                    outcome = moveScore.outcome;
                    movesUntilEndgame = moveScore.depth;

                    if (functionDepth == 0)
                    {
                        viableChoices.Clear();
                        Debug.Log("VIABLE CHOICE: " + testMove + " AT DEPTH " + moveScore.depth + " RESULTING IN BETTER SCORE OF " + moveScore.outcome);
                        viableChoices.Enqueue(testMove);
                    }

                }
                else if (moveScore.outcome == outcome)
                {
                    //Delay the game if losing, quicken the game if winning
                    if (outcome > 0)
                    {
                        if (moveScore.depth < movesUntilEndgame)
                        {
                            movesUntilEndgame = moveScore.depth;

                            if (functionDepth == 0)
                            {
                                viableChoices.Clear();
                                Debug.Log("VIABLE CHOICE: " + testMove + " AT DEPTH " + moveScore.depth + " RESULTING IN SCORE OF " + moveScore.outcome);
                                viableChoices.Enqueue(testMove);
                            }
                        }
                        else if (moveScore.depth == movesUntilEndgame)
                        {
                            if (functionDepth == 0)
                            {
                                Debug.Log("VIABLE CHOICE: " + testMove + " AT DEPTH " + moveScore.depth + " RESULTING IN SCORE OF " + moveScore.outcome);
                                viableChoices.Enqueue(testMove);
                            }
                        }
                    }
                    else
                    {
                        if (moveScore.depth > movesUntilEndgame)
                        {
                            movesUntilEndgame = moveScore.depth;

                            if (functionDepth == 0)
                            {
                                viableChoices.Clear();
                                Debug.Log("VIABLE CHOICE: " + testMove + " AT DEPTH " + moveScore.depth + " RESULTING IN SCORE OF " + moveScore.outcome);
                                viableChoices.Enqueue(testMove);
                            }
                        }
                        else if (moveScore.depth == movesUntilEndgame)
                        {
                            if (functionDepth == 0)
                            {
                                Debug.Log("VIABLE CHOICE: " + testMove + " AT DEPTH " + moveScore.depth + " RESULTING IN SCORE OF " + moveScore.outcome);
                                viableChoices.Enqueue(testMove);
                            }
                        }
                    }
                }
            }
            else
            {
                //Invert the checks during the Opponent's turn
                //The recursive function always starts on the AI's turn, so we can remove the isRoot checks during the opponent's logic
                if (moveScore.outcome < outcome)
                {
                    outcome = moveScore.outcome;
                    movesUntilEndgame = moveScore.depth;
                }
                else if (moveScore.outcome == outcome)
                {
                    //Delay the game if losing, quicken the game if winning - but for the Opponent
                    if (outcome < 0)
                    {
                        if (moveScore.depth < movesUntilEndgame)
                        {
                            movesUntilEndgame = moveScore.depth;
                        }
                    }
                    else
                    {
                        if (moveScore.depth > movesUntilEndgame)
                        {
                            movesUntilEndgame = moveScore.depth;
                        }
                    }
                }
            }
        }

        //Return the best options we saved
        returnOutcome.outcome = outcome;
        returnOutcome.depth = movesUntilEndgame;

        //forces the program to stop for now, if the coroutine is taking too long to respond
        if (NonBlocking && (System.DateTime.Now - currentTime).Milliseconds >= millisecondsPerCycle)
        {
            yield return new WaitForEndOfFrame();
            currentTime = System.DateTime.Now;
        }

        //Tell the outer function that you have finished
        bc.complete = true;

        //Choose a square from the viable options once all the functions have completed
        if (functionDepth == 0)
        {
            isDoneMoving = true;
            currentTime = System.DateTime.Now;

            int toChoose = Random.Range(0, viableChoices.Count);

            for (int i = 0; i < toChoose; ++i)
            {
                viableChoices.Enqueue(viableChoices.Dequeue());
            }

            chosenMove = viableChoices.Peek();
        }
    }
}

//Used to tell coroutines if it's safe to continue
//Coroutines are executed in a specific order that does not take into account recursion
//As such, it becomes important to check whether or not the inner function is complete before the outer function can continue.
public class BoolCheck
{
    public bool complete = false;
}