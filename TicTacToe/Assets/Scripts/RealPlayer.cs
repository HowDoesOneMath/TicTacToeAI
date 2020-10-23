using System.Collections;
using System.Collections.Generic;
using TTT;
using UnityEngine;

//Derivative of abstract player, handles a single human user
public class RealPlayer : AbstractPlayer
{
    //camera for raycast
    public Camera mainCam;

    //This is used to indicate where to instantiate flippable game pieces, but should not get flipped itself - as such,
    //  it has its 'placed' value permanently set to false so it cannot get thrown.
    GamePiece hoverPiece;

    //Layer of the tic-tac-toe squares
    int catchMouseLayer;

    //Reference to the last square hovered over.
    //Important to reset alpha value of the piece after the player's mouse moves off it.

    ClickableSquare lastHovered;

    //Boolean to save Input.GetMouseButton to.
    bool hasClicked = false;

    private void Awake()
    {
        //The piece that will indicate where you can place
        hoverPiece = Instantiate(piece);

        //Only activate it while your cursor is over the board
        hoverPiece.gameObject.SetActive(false);

        //Layer of squares that must be tested, to make raycasts less expensive
        catchMouseLayer = (1 << LayerMask.NameToLayer("CatchPlayerMouse"));
        Debug.Log(catchMouseLayer);
    }

    private void Update()
    {
        hasClicked = Input.GetMouseButtonUp(0);
    }

    public override IEnumerator GetDecision(TTTBoard board)
    {
        isDoneMoving = false;

        //Keep doing the coroutine until a decision has been made
        while (!IsDoneMoving)
        {
            yield return new WaitForEndOfFrame();

            hoverPiece.gameObject.SetActive(false);

            //reset alpha value
            if (lastHovered != null)
                lastHovered.mr.material.color = new Color(1, 1, 1, 0);

            //check the mouse's raycast into the scene and test if its colliding with a TicTacToe square
            //Also set the hovering piece to that location
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

            RaycastHit rch;
            if (Physics.Raycast(ray, out rch, 1000f, catchMouseLayer))
            {
                //If it hits a square, set the player's hovering game piece over that square
                hoverPiece.gameObject.SetActive(true);

                hoverPiece.transform.position = rch.collider.transform.position + new Vector3(0, 0.1f, 0);

                ClickableSquare cs;

                //Get a reference to the square
                if (rch.collider.TryGetComponent(out cs))
                {
                    //Set alpha of square so that players can tell where they are hovering over
                    lastHovered = cs;
                    cs.mr.material.color = new Color(1, 1, 1, 0.1f);

                    //If the player has clicked that square, set the player's 'chosen move' to that square
                    //Deactivate the hovering game piece and set conditions to exit the while loop
                    if (hasClicked)
                    {
                        hoverPiece.gameObject.SetActive(false);

                        chosenMove = new Vector2Int(cs.MySquareNumber / 3, cs.MySquareNumber % 3);
                        isDoneMoving = true;
                        hasClicked = false;

                        //reset alpha values
                        if (lastHovered != null)
                            lastHovered.mr.material.color = new Color(1, 1, 1, 0);
                    }
                }
            }
        }
    }

    public override void ResetPlayer(SquareState playAs)
    {
        base.ResetPlayer(playAs);
    }
}
