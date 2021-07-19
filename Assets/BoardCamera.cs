using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCamera : MonoBehaviour
{
    [SerializeField]
    public Camera boardCamera;

    public void SetupCamera(BoardManager _boardManager)
    {
        Vector3 camPosition = _boardManager.transform.position;

        camPosition += new Vector3(BoardManager.BoardWidth  * 0.5f   - 0.5f,
                                   BoardManager.AdjBoardHeight * 0.5f  + 0.5f,
                                   -60);

        //set position
        boardCamera.transform.position = camPosition;
    }
}
