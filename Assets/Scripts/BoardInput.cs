using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardInput : MonoBehaviour
{
    private Action<Direction> onInputMove;
    private Action<bool> onInputRotate;
    private Action onInputSlam;

    private const float keyPressesPerSecond = 0.05f;

    private const float horizontalSwipeScreenPercentage = 0.075f; //in Screen space (swipeRation * ScreenWidth)
    private const float verticalSwipeScreenPercentage = 0.01f; //in Screen space (swipeRation * ScreenWidth)
    private float horizSwipeThreshold => horizontalSwipeScreenPercentage * Screen.width;
    private float vertSwipeThreshold => verticalSwipeScreenPercentage * Screen.height;

    private float tapTimeThreshold = 0.3f; //in seconds
    private float slamSwipeTimeAllowed = 0.2f; //In Seconds
    private float downMovesPerSecond = 32f; //Per Second

    private float keyPressTimer = keyPressesPerSecond;

    private DateTime timePressedDown;
    private Vector2 posisitionPressStart;
    private DateTime lastTimeMovedDown;

    private bool hasSwipedHorizontally = false;
    private bool hasSlammed = false;

    public void Initialise(Action<Direction> _onInputMove, Action<bool> _onInputRotate, Action _onInputSlam)
    {
        Debug.Log("[BoardInput] Disabling multitouch here");
        Input.multiTouchEnabled = false;

        onInputMove = _onInputMove;
        onInputRotate = _onInputRotate;
        onInputSlam = _onInputSlam;
    }

    public void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (keyPressTimer >= keyPressesPerSecond)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                onInputMove(Direction.Left);
                keyPressTimer = 0;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                onInputMove(Direction.Right);
                keyPressTimer = 0;

            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                onInputMove(Direction.Down);
            }

            if (Input.GetKey(KeyCode.Q))
            {
                onInputRotate(true);
                keyPressTimer = 0;
            }
            if (Input.GetKey(KeyCode.W))
            {
                onInputRotate(false);
                keyPressTimer = 0;
            }

            if (Input.GetKeyDown(KeyCode.Space))
                onInputSlam();
        }
        else
        {
            keyPressTimer += Time.deltaTime;
        }


#endif

#if UNITY_ANDROID || UNITY_IOS
        if (Input.touches.Length > 0)
        {
            Debug.Log($"[BoardInput] Touch {Input.touches[0].phase} {Input.touches[0].position}");
            Vector2 curPos = Input.touches[0].position;
            Vector2 swipeDelta = curPos - posisitionPressStart;

            if (Input.touches[0].phase == TouchPhase.Began)
            {
                timePressedDown = DateTime.Now;
                lastTimeMovedDown = DateTime.Now;

                posisitionPressStart = curPos;
                hasSwipedHorizontally = false;
                hasSlammed = false;
            }
            else if (Input.touches[0].phase == TouchPhase.Moved)
            {
                if (!hasSlammed)
                {
                    if (swipeDelta.x > horizSwipeThreshold)
                    {
                        onInputMove(Direction.Right);
                        hasSwipedHorizontally = true;
                        posisitionPressStart = curPos;
                    }
                    else if (swipeDelta.x < -horizSwipeThreshold)
                    {
                        onInputMove(Direction.Left);
                        hasSwipedHorizontally = true;
                        posisitionPressStart = curPos;
                    }
                    else if (swipeDelta.y < -vertSwipeThreshold)
                    {
                        if ((DateTime.Now - timePressedDown).TotalSeconds > slamSwipeTimeAllowed ||
                            hasSwipedHorizontally ||
                            swipeDelta.x > Mathf.Abs(horizSwipeThreshold) * 0.5f)
                        {
                            if ((DateTime.Now - lastTimeMovedDown).TotalSeconds > (1.0f / downMovesPerSecond))
                            {
                                onInputMove(Direction.Down);
                                lastTimeMovedDown = DateTime.Now.AddSeconds((1.0f / downMovesPerSecond));
                            }
                        }
                        else 
                        {
                            hasSlammed = true;
                            onInputSlam();
                            posisitionPressStart = curPos;
                        }
                    }
                }
            }
            else if (Input.touches[0].phase == TouchPhase.Ended)
            {
                if ((DateTime.Now - timePressedDown).TotalSeconds < tapTimeThreshold && !hasSlammed && !hasSwipedHorizontally)
                {
                    if (posisitionPressStart.x < Screen.width * 0.5f)
                    {
                        onInputRotate(true);   
                    }
                    else
                    {
                        onInputRotate(false);
                    }
                }
            }
            else if (Input.touches[0].phase == TouchPhase.Canceled)
            {
            }
        }
#endif
    }


}
