using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardInput : MonoBehaviour
{
    Action<Direction> onInputMove;
    Action<bool> onInputRotate;
    Action onInputSlam;

    public void Initialise(Action<Direction> _onInputMove, Action<bool> _onInputRotate, Action _onInputSlam)
    {
        onInputMove = _onInputMove;
        onInputRotate = _onInputRotate;
        onInputSlam = _onInputSlam;
    }

    public void Update()
    {
        //#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            onInputMove(Direction.Left);
        if (Input.GetKeyDown(KeyCode.RightArrow))
            onInputMove(Direction.Right);
        if (Input.GetKeyDown(KeyCode.DownArrow))
            onInputMove(Direction.Down);

        if (Input.GetKeyDown(KeyCode.Q))
            onInputRotate(true);
        if (Input.GetKeyDown(KeyCode.W))
            onInputRotate(false);

        if (Input.GetKeyDown(KeyCode.Space))
            onInputSlam();
//#else

//#endif
    }


}
