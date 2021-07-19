using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCamera : MonoBehaviour
{
    [SerializeField]
    public Camera boardCamera;
    public GameObject backgroundQuad;

    public void SetupCamera(BoardManager _boardManager)
    {
        Vector3 camPosition = _boardManager.transform.position;

        camPosition += new Vector3(BoardManager.BoardWidth  * 0.5f   - 0.5f,
                                   BoardManager.AdjBoardHeight * 0.5f  + 0.5f,
                                   -70);

        //set position
        boardCamera.transform.position = camPosition;

        //move back quad to backface minus a bit
        float relativeClipPos = boardCamera.farClipPlane - 10f;
        backgroundQuad.transform.position = camPosition + boardCamera.transform.forward * relativeClipPos;

        //scale it relative to cam transform
        float h = Mathf.Tan(boardCamera.fieldOfView * Mathf.Deg2Rad * 0.5f) * relativeClipPos * 2.0f;
        backgroundQuad.transform.localScale = new Vector3(h * boardCamera.aspect, h, 1f);

    }
}
