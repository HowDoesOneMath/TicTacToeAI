using System.Collections;
using System.Collections.Generic;
using TTT;
using UnityEngine;

public class RealPlayer : AbstractPlayer
{
    //camera for raycast
    public Camera mainCam;
    GamePiece hoverPiece;

    //Layer of the tic-tac-toe squares
    int catchMouseLayer;

    //To re-introduce transparency
    ClickableSquare lastHovered;


    bool hasClicked = false;

    private void Awake()
    {
        //The piece that will indicate where you can place
        hoverPiece = Instantiate(piece);

        //Only activate it while your cursor is over the board
        hoverPiece.gameObject.SetActive(false);

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

        while (!IsDoneMoving)
        {
            yield return new WaitForEndOfFrame();

            hoverPiece.gameObject.SetActive(false);

            //reset color change
            if (lastHovered != null)
                lastHovered.mr.material.color = new Color(1, 1, 1, 0);

            //check the mouse position and test if its colliding with a TicTacToe square
            //Also set the hovering piece to that location
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

            RaycastHit rch;
            if (Physics.Raycast(ray, out rch, 1000f, catchMouseLayer))
            {
                hoverPiece.gameObject.SetActive(true);

                hoverPiece.transform.position = rch.collider.transform.position + new Vector3(0, 0.1f, 0);

                ClickableSquare cs;

                if (rch.collider.TryGetComponent(out cs))
                {
                    //Color material so that players can tell where they are hovering over
                    lastHovered = cs;
                    cs.mr.material.color = new Color(1, 1, 1, 0.1f);

                    if (hasClicked)
                    {
                        hoverPiece.gameObject.SetActive(false);

                        chosenMove = new Vector2Int(cs.MySquareNumber / 3, cs.MySquareNumber % 3);
                        isDoneMoving = true;
                        hasClicked = false;

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
