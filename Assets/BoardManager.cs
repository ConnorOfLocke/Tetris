using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class BoardManager : MonoBehaviour
{
    [SerializeField]
    private int BoardWidth = 10;

    [SerializeField]
    private int BoardHeight = 10;
    private int AdjBoardHeight => BoardHeight + 2;

    [SerializeField]
    private Transform prefabCellParent = null;

    [SerializeField]
    private GameObject prefabCellObject = null;

    [SerializeField]
    private float stepsPerMinute = 30.0f;

    private BoardCell[] boardCells = null;
    private GameObject[] boardCellObjects = null;

    private ShapeType activeShapeType;
    private RotationState activeShapeRotationState;
    private GameObject[] activeShapeObjectSet;
    private int[] activeShapeObjectIndexs;
    
    private float stepTimer = 0.0f;
    private bool isPlaying = true;

    private List<ShapeType> shapePool = new List<ShapeType>();

    public void Start()
    {
        ResetBoard();
    }

    public void ResetBoard()
    {
        //clear out the old board objects
        if (boardCellObjects != null)
        {
            for (int i = 0; i < boardCellObjects.Length; i++)
            {
                if (boardCellObjects[i] != null)
                {
                    Destroy(boardCellObjects[i]);
                    boardCellObjects[i] = null;
                }
            }
        }

        if (activeShapeObjectSet != null)
        {
            for (int i = 0; i < activeShapeObjectSet.Length; i++)
            {
                if (activeShapeObjectSet[i] != null)
                {
                    Destroy(activeShapeObjectSet[i]);
                    activeShapeObjectSet[i] = null;
                }
            }
        }

        //Make the board
        boardCells = new BoardCell[BoardWidth * AdjBoardHeight];
        boardCellObjects = new GameObject[BoardHeight * AdjBoardHeight];

        for (int cellIndex = 0; cellIndex < BoardWidth * AdjBoardHeight; cellIndex++)
        {
            boardCells[cellIndex] = new BoardCell(cellIndex);
        }

        //fill bottom row
        //for (int i = 0; i < BoardWidth - 3; i++)
        //{
        //    boardCells[i].isFilled = true;
        //    boardCellObjects[i] = GameObject.Instantiate(prefabCellObject, new Vector3(i, 0, 0), Quaternion.identity, prefabCellParent);
        //}

        stepTimer = 0;
        isPlaying = true;

        //Make the first Shape to start moving
        activeShapeType = GetNextShape();
        CreateNewShape(activeShapeType);
    }

    public ShapeType GetNextShape()
    {
        if (shapePool.Count <= 1)
        {
            List<ShapeType> nextShapes = new List<ShapeType>(ShapeData.shapeList);
            while (nextShapes.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, nextShapes.Count);
                shapePool.Add(nextShapes[index]);
                nextShapes.RemoveAt(index);
            }            
        }

        ShapeType returnShape = shapePool[0];
        shapePool.RemoveAt(0);
        return returnShape;
    }

    public bool CreateNewShape(ShapeType _shapeType)
    {
        activeShapeType = _shapeType;

        int[] dataset = null;
        switch (_shapeType)
        {
            //all 4x2 shapes
            case ShapeType.L_Shape: dataset = ShapeData.LShape; break;
            case ShapeType.J_Shape: dataset = ShapeData.JShape; break;
            case ShapeType.Z_Shape: dataset = ShapeData.ZShape; break;
            case ShapeType.S_Shape: dataset = ShapeData.SShape; break;
            case ShapeType.T_Shape: dataset = ShapeData.TShape; break;
            case ShapeType.O_Shape: dataset = ShapeData.OShape; break;
            case ShapeType.I_Shape: dataset = ShapeData.IShape; break;
        }

        activeShapeObjectIndexs = new int[8];

        int spawnIndex = (20 * BoardWidth) + 3;
        activeShapeObjectIndexs[0] = dataset[0] == 1 ? spawnIndex + 0 : -1;
        activeShapeObjectIndexs[1] = dataset[1] == 1 ? spawnIndex + 1 : -1;
        activeShapeObjectIndexs[2] = dataset[2] == 1 ? spawnIndex + 2 : -1;
        activeShapeObjectIndexs[3] = dataset[3] == 1 ? spawnIndex + 3 : -1;
        activeShapeObjectIndexs[4] = dataset[4] == 1 ? spawnIndex + 0 + BoardWidth : -1;
        activeShapeObjectIndexs[5] = dataset[5] == 1 ? spawnIndex + 1 + BoardWidth : -1;
        activeShapeObjectIndexs[6] = dataset[6] == 1 ? spawnIndex + 2 + BoardWidth : -1;
        activeShapeObjectIndexs[7] = dataset[7] == 1 ? spawnIndex + 3 + BoardWidth : -1;

        //check we can spawn an object there
        for (int i = 0; i < activeShapeObjectIndexs.Length; i++)
        {
            if (activeShapeObjectIndexs[i] != -1)
            {
                if (boardCells[activeShapeObjectIndexs[i]].isFilled)
                {
                    //Fail state, game over
                    return false;
                }
            }
        }

        activeShapeObjectSet = new GameObject[8];
        Vector3 spawnPoint = new Vector3(3, 20, 0);
        activeShapeObjectSet[0] = dataset[0] == 1 ? GameObject.Instantiate(prefabCellObject, spawnPoint + new Vector3(0, 0, 0), Quaternion.identity, prefabCellParent) : null;
        activeShapeObjectSet[1] = dataset[1] == 1 ? GameObject.Instantiate(prefabCellObject, spawnPoint + new Vector3(1, 0, 0), Quaternion.identity, prefabCellParent) : null;
        activeShapeObjectSet[2] = dataset[2] == 1 ? GameObject.Instantiate(prefabCellObject, spawnPoint + new Vector3(2, 0, 0), Quaternion.identity, prefabCellParent) : null;
        activeShapeObjectSet[3] = dataset[3] == 1 ? GameObject.Instantiate(prefabCellObject, spawnPoint + new Vector3(3, 0, 0), Quaternion.identity, prefabCellParent) : null;
        activeShapeObjectSet[4] = dataset[4] == 1 ? GameObject.Instantiate(prefabCellObject, spawnPoint + new Vector3(0, 1, 0), Quaternion.identity, prefabCellParent) : null;
        activeShapeObjectSet[5] = dataset[5] == 1 ? GameObject.Instantiate(prefabCellObject, spawnPoint + new Vector3(1, 1, 0), Quaternion.identity, prefabCellParent) : null;
        activeShapeObjectSet[6] = dataset[6] == 1 ? GameObject.Instantiate(prefabCellObject, spawnPoint + new Vector3(2, 1, 0), Quaternion.identity, prefabCellParent) : null;
        activeShapeObjectSet[7] = dataset[7] == 1 ? GameObject.Instantiate(prefabCellObject, spawnPoint + new Vector3(3, 1, 0), Quaternion.identity, prefabCellParent) : null;

        activeShapeRotationState = RotationState.Start;

        return true;
    }

    public void Update()
    {
        if (isPlaying)
        {
            UpdateSteps();
            UpdateInput();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ResetBoard();
        }
        
    }

    public void UpdateSteps()
    {
        if (stepTimer > 60.0f / stepsPerMinute)
        {
            stepTimer -= 60.0f / stepsPerMinute;

            //if we can't move down, place and solve
            if (!MoveDownSimple())
            {
                PlaceAndCheckState();
            }
        }

        stepTimer += Time.deltaTime;
    }

    public void UpdateInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveLeftSimple();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveRightSimple();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            MoveDownSimple();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            while (MoveDownSimple()) {}

            PlaceAndCheckState();
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            RotateLeft();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            RotateRight();
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            stepTimer += Time.deltaTime;
        }
    }

    public void MoveLeftSimple()
    {
        //Check if theres a thing in the way
        bool validMove = true;
        for (int i = 0; i < activeShapeObjectIndexs.Length; i++)
        {
            if (activeShapeObjectIndexs[i] != -1)
            {
                //check for edge of the board
                if (activeShapeObjectIndexs[i] % BoardWidth == 0)
                    validMove = false;

                //check for other cells
                if (activeShapeObjectIndexs[i] - 1 >= 0)
                {
                    if (boardCells[activeShapeObjectIndexs[i] - 1].isFilled)
                        validMove = false;
                }

                if (!validMove)
                    break;
            }
        }

        //Move the thing
        if (validMove)
        {
            for (int i = 0; i < activeShapeObjectSet.Length; i++)
            {
                if (activeShapeObjectSet[i] != null)
                    activeShapeObjectSet[i].transform.position += Vector3.left;

                if (activeShapeObjectIndexs[i] != -1)
                    activeShapeObjectIndexs[i] -= 1;
            }
        }
    }

    public void MoveRightSimple()
    {
        bool validMove = true;
        for (int i = 0; i < activeShapeObjectIndexs.Length; i++)
        {
            if (activeShapeObjectIndexs[i] != -1)
            {
                if (activeShapeObjectIndexs[i] % BoardWidth == BoardWidth - 1)
                    validMove = false;

                if (activeShapeObjectIndexs[i] + 1 < BoardWidth * AdjBoardHeight)
                {
                    if (boardCells[activeShapeObjectIndexs[i] + 1].isFilled)
                        validMove = false;
                }

                if (!validMove)
                    break;
            }
        }

        if (validMove)
        {
            for (int i = 0; i < activeShapeObjectSet.Length; i++)
            {
                if (activeShapeObjectSet[i] != null)
                    activeShapeObjectSet[i].transform.position += Vector3.right;

                if (activeShapeObjectIndexs[i] != -1)
                    activeShapeObjectIndexs[i] += 1;
            }
        }
    }

    public bool MoveDownSimple()
    {
        //check if something is in the way
        bool validMove = true;
        for (int i = 0; i < activeShapeObjectIndexs.Length; i++)
        {
            if (activeShapeObjectIndexs[i] != -1)
            {
                //bottom of the board
                if (activeShapeObjectIndexs[i] - BoardWidth < 0)
                    validMove = false;

                //another cell
                if (activeShapeObjectIndexs[i] - BoardWidth >= 0)
                {
                    if (boardCells[activeShapeObjectIndexs[i] - BoardWidth].isFilled)
                        validMove = false;
                }
            }
        }

        //move it down
        if (validMove)
        {
            for (int i = 0; i < activeShapeObjectSet.Length; i++)
            {
                if (activeShapeObjectSet[i] != null)
                    activeShapeObjectSet[i].transform.position += Vector3.down;

                if (activeShapeObjectIndexs[i] != -1)
                    activeShapeObjectIndexs[i] -= BoardWidth;
            }
        }

        return validMove;
    }

    public void PlaceAndCheckState()
    {
        //place and attach the objects
        for (int i = 0; i < activeShapeObjectIndexs.Length; i++)
        {
            if (activeShapeObjectIndexs[i] != -1)
            {
                boardCells[activeShapeObjectIndexs[i]].isFilled = true;
                boardCellObjects[activeShapeObjectIndexs[i]] = activeShapeObjectSet[i];
            }
        }

        int rowsCompleteThisSolve = 0;
        //Find any completed rows
        for (int rowIndex = 0; rowIndex < BoardHeight; rowIndex++)
        {
            bool rowComplete = true;
            for(int colIndex = 0; colIndex < BoardWidth; colIndex++)
            {
                if (!boardCells[rowIndex * BoardWidth + colIndex].isFilled)
                {
                    rowComplete = false;
                    break;
                }
            }
           
            if (rowComplete)
            {
                rowsCompleteThisSolve++;

                //remove stuff in row
                for (int colIndex = 0; colIndex < BoardWidth; colIndex++)
                {
                    boardCells[rowIndex * BoardWidth + colIndex].isFilled = false;
                    Destroy(boardCellObjects[rowIndex * BoardWidth + colIndex]);
                    boardCellObjects[rowIndex * BoardWidth + colIndex] = null;
                }

                //starting at the next row move every other row down
                for (int index = (rowIndex + 1) * BoardWidth; index < BoardWidth * AdjBoardHeight; index++)
                {
                    int belowIndex = index - BoardWidth;

                    boardCells[belowIndex].isFilled = boardCells[index].isFilled;

                    boardCellObjects[belowIndex] = boardCellObjects[index];
                    if (boardCellObjects[belowIndex] != null)
                        boardCellObjects[belowIndex].transform.position += Vector3.down;
                }

                //move back since we cleared a row
                rowIndex--;
            }
        }
        
        if (rowsCompleteThisSolve > 0)
        {
            Debug.Log($"Solved {rowsCompleteThisSolve} this round");
        }

        //Attempt to spawn a new object
        ShapeType nextShape = GetNextShape();
        if (!CreateNewShape(nextShape))
        {
            //Game Over
            Debug.Log($"GAME Over. Press Escape to start again");
            isPlaying = false;
        }
    }

    private bool CheckConflicts(int[] _indexs)
    {
        for (int i = 0; i < _indexs.Length; i++)
        {
            if (_indexs[i] != -1)
            {
                if (boardCells[_indexs[i]].isFilled)
                    return false;
            }
        }
        return true;
    }

    public bool RotateLeft()
    {
        bool validTurn = false;
        Vector3[] newStateOffsets = new Vector3[8];
        int[] newIndexs = new int[8];
        RotationState nextRotationState = (RotationState)(((int)activeShapeRotationState + 1) % 4);

        if (AttemptWallKick(activeShapeObjectIndexs,
                            activeShapeRotationState,
                            nextRotationState,
                            activeShapeType == ShapeType.I_Shape,
                            out newIndexs,
                            out newStateOffsets))
        {
            activeShapeObjectIndexs = newIndexs;
            activeShapeRotationState = nextRotationState;
            validTurn = true;

            for (int i = 0; i < activeShapeObjectSet.Length; i++)
            {
                if (activeShapeObjectSet[i] != null)
                    activeShapeObjectSet[i].transform.position += newStateOffsets[i];
            }
        }
       
        return validTurn;
    }

    public bool RotateRight()
    {
        bool validTurn = false;
        Vector3[] newStateOffsets = new Vector3[8];
        int[] newIndexs = new int[8];

        RotationState nextRotationState = (RotationState)(((int)activeShapeRotationState - 1) < 0 ? 3 : ((int)activeShapeRotationState - 1));
        if (AttemptWallKick(activeShapeObjectIndexs,
                            activeShapeRotationState,
                            nextRotationState,
                            activeShapeType == ShapeType.I_Shape,
                            out newIndexs,
                            out newStateOffsets))
        {
            activeShapeObjectIndexs = newIndexs;
            activeShapeRotationState = nextRotationState;
            validTurn = true;

            for (int i = 0; i < activeShapeObjectSet.Length; i++)
            {
                if (activeShapeObjectSet[i] != null)
                    activeShapeObjectSet[i].transform.position += newStateOffsets[i];
            }
        }
        
        return validTurn;


        if (validTurn)
        {
            RotationState prevthing = activeShapeRotationState;
            activeShapeRotationState = (RotationState)(((int)activeShapeRotationState - 1) < 0 ? 3 : ((int)activeShapeRotationState - 1));
            Debug.Log($"Moved from {prevthing} to {activeShapeRotationState}");
        }
        else
        {
            Debug.Log($"Failed to turn right from {activeShapeRotationState}");
        }
        
        return validTurn;
    }

    //Movement Functions
    bool MoveLeft(ref int[] _outState)
    {
        for (int i = 0; i < _outState.Length; i++)
        {
            if (_outState[i] % BoardWidth == 0)
                return false;
        }

        for (int i = 0; i < _outState.Length; i++)
        {
            if (_outState[i] != -1)
                _outState[i]--;
        }

        return true;
    }

    bool MoveRight(ref int[] _outState)
    {
        for (int i = 0; i < _outState.Length; i++)
        {
            if (_outState[i] % BoardWidth == BoardWidth - 1)
                return false;
        }

        for (int i = 0; i < _outState.Length; i++)
        {
            if (_outState[i] != -1)
                _outState[i]++;
        }

        return true;
    }

    bool MoveUp(ref int[] _outState)
    {
        for (int i = 0; i < _outState.Length; i++)
        {
            if (_outState[i] / BoardWidth == AdjBoardHeight - 1)
                return false;
        }

        for (int i = 0; i < _outState.Length; i++)
        {
            if (_outState[i] != -1)
                _outState[i] += BoardWidth;
        }

        return true;
    }

    bool MoveDown(ref int[] _outState)
    {
        for (int i = 0; i < _outState.Length; i++)
        {
            if (_outState[i] / BoardWidth == 0)
                return false;
        }

        for (int i = 0; i < _outState.Length; i++)
        {
            if (_outState[i] != -1)
                _outState[i] -= BoardWidth;
        }

        return true;
    }

    bool RotateLeft(ref int[] _outstate, ref Vector3[] _outOffset)
    {
        switch (activeShapeType)
        {
            case ShapeType.L_Shape:
                {
                    if (activeShapeRotationState == RotationState.Start)
                    {
                        //check down border
                        if (_outstate[1] - BoardWidth >= 0)
                        {
                            _outstate[0] += +1 - BoardWidth;
                            //_outstate[1] ; //pivot
                            _outstate[2] += -1 + BoardWidth;
                            _outstate[6] += -2;

                            _outOffset[0] += new Vector3(1, -1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(-1, 1, 0);
                            _outOffset[6] += new Vector3(-2, 0, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Left)
                    {
                        //check left border
                        if (_outstate[1] % BoardWidth != BoardWidth - 1)
                        {
                            _outstate[0] += +1 + BoardWidth;
                            //_outstate[1] =; //pivot
                            _outstate[2] += -1 - BoardWidth;
                            _outstate[6] += -BoardWidth * 2;

                            _outOffset[0] += new Vector3(1, 1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(-1, -1, 0);
                            _outOffset[6] += new Vector3(0, -2, 0);

                            return true;
                        }

                    }
                    else if (activeShapeRotationState == RotationState.Down)
                    {
                        //check top border
                        if (_outstate[1] + BoardWidth < BoardWidth * AdjBoardHeight)
                        {
                            _outstate[0] += -1 + BoardWidth;
                            //_outstate[1] ; //pivot 
                            _outstate[2] += +1 - BoardWidth;
                            _outstate[6] += +2;

                            _outOffset[0] += new Vector3(-1, 1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(1, -1, 0);
                            _outOffset[6] += new Vector3(2, 0, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Right)
                    {
                        //check bottom border
                        if (_outstate[1] % BoardWidth != 0)
                        {
                            _outstate[0] += -1 - BoardWidth;
                            //_outstate[1] =; //pivot
                            _outstate[2] += +1 + BoardWidth;
                            _outstate[6] += +2 * BoardWidth;

                            _outOffset[0] += new Vector3(-1, -1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(1, 1, 0);
                            _outOffset[6] += new Vector3(0, 2, 0);

                            return true;
                        }
                    }
                }
                break;
            case ShapeType.J_Shape:
                {
                    if (activeShapeRotationState == RotationState.Start)
                    {
                        //bottom border
                        if (_outstate[1] - BoardWidth >= 0)
                        {
                            _outstate[0] += +1 - BoardWidth;
                            //_outstate[1] =; //pivot
                            _outstate[2] += -1 + BoardWidth;
                            _outstate[4] += -2 * BoardWidth;

                            _outOffset[0] += new Vector3(1, -1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(-1, 1, 0);
                            _outOffset[4] += new Vector3(0, -2, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Left)
                    {
                        //right border
                        if (_outstate[1] % BoardWidth != BoardWidth - 1)
                        {
                            _outstate[0] += +1 + BoardWidth;
                            //_outstate[1] =;
                            _outstate[2] += -1 - BoardWidth;
                            _outstate[4] += +2;

                            _outOffset[0] += new Vector3(1, 1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(-1, -1, 0);
                            _outOffset[4] += new Vector3(2, 0, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Down)
                    {
                        //right border
                        if (_outstate[1] % BoardWidth != BoardWidth - 1)
                        {
                            _outstate[0] += -1 + BoardWidth;
                            //_outstate[1] =;
                            _outstate[2] += +1 - BoardWidth;
                            _outstate[4] += +BoardWidth * 2;

                            _outOffset[0] += new Vector3(-1, 1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(1, -1, 0);
                            _outOffset[4] += new Vector3(0, 2, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Right)
                    {
                        //left border
                        if (_outstate[1] % BoardWidth != 0)
                        {
                            _outstate[0] += -1 - BoardWidth;
                            //_outstate[1] =;
                            _outstate[2] += +1 + BoardWidth;
                            _outstate[4] += -2;

                            _outOffset[0] += new Vector3(-1, -1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(1, 1, 0);
                            _outOffset[4] += new Vector3(-2, 0, 0);

                            return true;
                        }
                    }
                }
                break;
            case ShapeType.Z_Shape:
                {
                    if (activeShapeRotationState == RotationState.Start)
                    {
                        //bottom border
                        if (_outstate[1] - BoardWidth >= 0)
                        {
                            //_outstate[1] =; //pivot
                            _outstate[2] += -1 + BoardWidth;
                            _outstate[4] += -BoardWidth * 2;
                            _outstate[5] += -1 - BoardWidth;

                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(-1, 1, 0);
                            _outOffset[4] += new Vector3(0, -2, 0);
                            _outOffset[5] += new Vector3(-1, -1, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Left)
                    {
                        //right border
                        if (_outstate[1] % BoardWidth != BoardWidth - 1)
                        {
                            //_outstate[1] =;
                            _outstate[2] += -1 - BoardWidth;
                            _outstate[4] += +2;
                            _outstate[5] += +1 - BoardWidth;

                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(-1, -1, 0);
                            _outOffset[4] += new Vector3(2, 0, 0);
                            _outOffset[5] += new Vector3(1, -1, 0);

                            return true;
                        }

                    }
                    else if (activeShapeRotationState == RotationState.Down)
                    {
                        //top border
                        if (_outstate[1] + BoardWidth < BoardWidth * AdjBoardHeight)
                        {
                            //_outstate[1] =;
                            _outstate[2] += +1 - BoardWidth;
                            _outstate[4] += +2 * BoardWidth;
                            _outstate[5] += +1 + BoardWidth;

                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(1, -1, 0);
                            _outOffset[4] += new Vector3(0, 2, 0);
                            _outOffset[5] += new Vector3(1, 1, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Right)
                    {
                        //left border
                        if (_outstate[1] % BoardWidth != 0)
                        {
                            //_outstate[1] =;
                            _outstate[2] += +1 + BoardWidth;
                            _outstate[4] += -2;
                            _outstate[5] += -1 + BoardWidth;

                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(1, 1, 0);
                            _outOffset[4] += new Vector3(-2, 0, 0);
                            _outOffset[5] += new Vector3(-1, 1, 0);

                            return true;
                        }
                    }
                }
                break;
            case ShapeType.S_Shape:
                {
                    if (activeShapeRotationState == RotationState.Start)
                    {
                        //bottom border
                        if (_outstate[1] - BoardWidth >= 0)
                        {
                            _outstate[0] += +1 - BoardWidth;
                            //_outstate[1] =; //pivot
                            _outstate[5] += -1 - BoardWidth;
                            _outstate[6] += -2;

                            _outOffset[0] += new Vector3(1, -1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[5] += new Vector3(-1, -1, 0);
                            _outOffset[6] += new Vector3(-2, 0, 0);


                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Left)
                    {
                        //left border
                        if (_outstate[1] % BoardWidth != BoardWidth - 1)
                        {
                            _outstate[0] += +1 + BoardWidth;
                            //_outstate[1] =
                            _outstate[5] += +1 - BoardWidth;
                            _outstate[6] += -BoardWidth * 2;

                            _outOffset[0] += new Vector3(1, 1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[5] += new Vector3(1, -1, 0);
                            _outOffset[6] += new Vector3(0, -2, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Down)
                    {
                        //top border
                        if (_outstate[1] + BoardWidth < BoardWidth * AdjBoardHeight)
                        {
                            _outstate[0] += -1 + BoardWidth;
                            //_outstate[1]= 
                            _outstate[5] += +1 + BoardWidth;
                            _outstate[6] += +2;

                            _outOffset[0] += new Vector3(-1, 1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[5] += new Vector3(1, 1, 0);
                            _outOffset[6] += new Vector3(2, 0, 0);


                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Right)
                    {
                        //left border
                        if (_outstate[1] % BoardWidth != 0)
                        {
                            _outstate[0] += -1 - BoardWidth;
                            //_outstate[1] = 
                            _outstate[5] += -1 + BoardWidth;
                            _outstate[6] += +2 * BoardWidth;

                            _outOffset[0] += new Vector3(-1, -1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[5] += new Vector3(-1, 1, 0);
                            _outOffset[6] += new Vector3(0, 2, 0);

                            return true;
                        }
                    }
                }
                break;
            case ShapeType.T_Shape:
                {
                    if (activeShapeRotationState == RotationState.Start)
                    {
                        //bottom border
                        if (_outstate[1] - BoardWidth >= 0)
                        {
                            _outstate[0] += +1 - BoardWidth;
                            //_outstate[1] = 
                            _outstate[2] += -1 + BoardWidth;
                            _outstate[5] += -1 - BoardWidth;

                            _outOffset[0] += new Vector3(1, -1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(-1, 1, 0);
                            _outOffset[5] += new Vector3(-1, -1, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Left)
                    {
                        //right border
                        if (_outstate[1] % BoardWidth != BoardWidth - 1)
                        {
                            _outstate[0] += +1 + BoardWidth;
                            //_outstate[1] =
                            _outstate[2] += -1 - BoardWidth;
                            _outstate[5] += +1 - BoardWidth;

                            _outOffset[0] += new Vector3(1, 1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(-1, -1, 0);
                            _outOffset[5] += new Vector3(1, -1, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Down)
                    {
                        //top border
                        if (_outstate[1] + BoardWidth < BoardWidth * AdjBoardHeight)
                        {
                            _outstate[0] += -1 + BoardWidth;
                            ///_outstate[1] =
                            _outstate[2] += +1 - BoardWidth;
                            _outstate[5] += +1 + BoardWidth;

                            _outOffset[0] += new Vector3(-1, 1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(1, -1, 0);
                            _outOffset[5] += new Vector3(1, 1, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Right)
                    {
                        //left border
                        if (_outstate[1] % BoardWidth != 0)
                        {
                            _outstate[0] += -1 - BoardWidth;
                            //_outstate[1] = 
                            _outstate[2] += +1 + BoardWidth;
                            _outstate[5] += -1 + BoardWidth;

                            _outOffset[0] += new Vector3(-1, -1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(1, 1, 0);
                            _outOffset[5] += new Vector3(-1, 1, 0);

                            return true;
                        }
                    }
                }
                break;
            case ShapeType.O_Shape:
                //Cube dont rotate silly
                return true;
                break;
            case ShapeType.I_Shape:
                {
                    if (activeShapeRotationState == RotationState.Start)
                    {
                        //bottom border * 2
                        if (_outstate[1] - 2 * BoardWidth > 0)
                        {
                            _outstate[0] += +1 - 2 * BoardWidth;
                            _outstate[1] += -BoardWidth;
                            _outstate[2] += -1;
                            _outstate[3] += -2 + BoardWidth;

                            _outOffset[0] += new Vector3(1, -2, 0);
                            _outOffset[1] += new Vector3(0, -1, 0);
                            _outOffset[2] += new Vector3(-1, 0, 0);
                            _outOffset[3] += new Vector3(-2, 1, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Left)
                    {
                        //right border
                        if (_outstate[1] % BoardWidth != 0 &&
                            _outstate[1] % BoardWidth < BoardWidth - 2)
                        {
                            _outstate[0] += +2 + BoardWidth;
                            _outstate[1] += +1;
                            _outstate[2] += -BoardWidth;
                            _outstate[3] += -1 - 2 * BoardWidth;

                            _outOffset[0] += new Vector3(2, 1, 0);
                            _outOffset[1] += new Vector3(1, 0, 0);
                            _outOffset[2] += new Vector3(0, -1, 0);
                            _outOffset[3] += new Vector3(-1, -2, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Down)
                    {
                        //top border
                        if (_outstate[1] + BoardWidth * 2 < BoardWidth * AdjBoardHeight)
                        {
                            _outstate[0] += -1 + 2 * BoardWidth;
                            _outstate[1] += +BoardWidth;
                            _outstate[2] += +1;
                            _outstate[3] += +2 - BoardWidth;

                            _outOffset[0] += new Vector3(-1, 2, 0);
                            _outOffset[1] += new Vector3(0, 1, 0);
                            _outOffset[2] += new Vector3(1, 0, 0);
                            _outOffset[3] += new Vector3(2, -1, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Right)
                    {
                        //left border
                        if (_outstate[1] % BoardWidth >= 2 &&
                            _outstate[1] % BoardWidth != BoardWidth - 1)
                        {
                            _outstate[0] += -2 - BoardWidth;
                            _outstate[1] += -1;
                            _outstate[2] += +BoardWidth;
                            _outstate[3] += +1 + 2 * BoardWidth;

                            _outOffset[0] += new Vector3(-2, -1, 0);
                            _outOffset[1] += new Vector3(-1, 0, 0);
                            _outOffset[2] += new Vector3(0, 1, 0);
                            _outOffset[3] += new Vector3(1, 2, 0);

                            return true;
                        }
                    }
                }
                break;
        }

        return false;
    }
    
    bool RotateRight(ref int[] _outstate, ref Vector3[] _outOffset)
    {
        switch (activeShapeType)
        {
            case ShapeType.L_Shape:
                {
                    if (activeShapeRotationState == RotationState.Start)
                    {
                        //check down border
                        if (_outstate[1] - BoardWidth >= 0)
                        {
                            _outstate[0] += +1 + BoardWidth;
                            //_outstate[1] =; //pivot
                            _outstate[2] += -1 - BoardWidth;
                            _outstate[6] += -BoardWidth * 2;

                            _outOffset[0] += new Vector3(1, 1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(-1, -1, 0);
                            _outOffset[6] += new Vector3(0, -2, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Left)
                    {
                        //right border
                        if (_outstate[1] % BoardWidth != BoardWidth - 1)
                        {
                            _outstate[0] += -1 + BoardWidth;
                            //_outstate[1] += ; //pivot
                            _outstate[2] += +1 - BoardWidth;
                            _outstate[6] += +2;

                            _outOffset[0] += new Vector3(-1, 1, 0);
                            //_outOffset[1]//pivot
                            _outOffset[2] += new Vector3(1, -1, 0);
                            _outOffset[6] += new Vector3(2, 0, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Down)
                    {
                        //top border
                        if (_outstate[1] + BoardWidth < BoardWidth * AdjBoardHeight)
                        {
                            _outstate[0] += -1 - BoardWidth;
                            //_outstate[1] +=; //pivot
                            _outstate[2] += +1 + BoardWidth;
                            _outstate[6] += +2 * BoardWidth;

                            _outOffset[0] += new Vector3(-1, -1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(1, 1, 0);
                            _outOffset[6] += new Vector3(0, 2, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Right)
                    {
                        //left border
                        if (_outstate[1] % BoardWidth != 0)
                        {
                            _outstate[0] += +1 - BoardWidth;
                            //_outstate[1] +=; //pivot
                            _outstate[2] += -1 + BoardWidth;
                            _outstate[6] += -2;

                            _outOffset[0] += new Vector3(1, -1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(-1, 1, 0);
                            _outOffset[6] += new Vector3(-2, 0, 0);

                            return true;
                        }
                    }
                }
                break;
            case ShapeType.J_Shape:
                {
                    if (activeShapeRotationState == RotationState.Start)
                    {
                        //bottom border
                        if (_outstate[1] - BoardWidth >= 0)
                        {
                            _outstate[0] += +1 + BoardWidth;
                            //_outstate[1] +=;
                            _outstate[2] += -1 - BoardWidth;
                            _outstate[4] += +2;

                            _outOffset[0] += new Vector3(1, 1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(-1, -1, 0);
                            _outOffset[4] += new Vector3(2, 0, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Left)
                    {
                        //right border
                        if (_outstate[1] % BoardWidth != BoardWidth - 1)
                        {
                            _outstate[0] += -1 + BoardWidth;
                            //_outstate[1] +=;
                            _outstate[2] += +1 - BoardWidth;
                            _outstate[4] += +2 * BoardWidth;

                            _outOffset[0] += new Vector3(-1, 1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(1, -1, 0);
                            _outOffset[4] += new Vector3(0, 2, 0);
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Down)
                    {
                        //up border
                        if (_outstate[1] + BoardWidth <= BoardWidth * AdjBoardHeight)
                        {
                            _outstate[0] += -1 - BoardWidth;
                            //_outstate[1] +=;
                            _outstate[2] += +1 + BoardWidth;
                            _outstate[4] += -2;

                            _outOffset[0] += new Vector3(-1, -1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(1, 1, 0);
                            _outOffset[4] += new Vector3(-2, 0, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Right)
                    {
                        //left border
                        if (_outstate[1] % BoardWidth != 0)
                        {
                            _outstate[0] += +1 - BoardWidth;
                            //_outstate[1] +=;
                            _outstate[2] += -1 + BoardWidth;
                            _outstate[4] += -2 * BoardWidth;

                            _outOffset[0] += new Vector3(1, -1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(-1, 1, 0);
                            _outOffset[4] += new Vector3(0, -2, 0);

                            return true;
                        }
                    }
                }
                break;
            case ShapeType.Z_Shape:
                {
                    if (activeShapeRotationState == RotationState.Start)
                    {
                        //bottom border
                        if (_outstate[1] - BoardWidth >= 0)
                        {
                            //_outstate[1] +=;
                            _outstate[2] += -1 - BoardWidth;
                            _outstate[4] += +2;
                            _outstate[5] += +1 - BoardWidth;

                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(-1, -1, 0);
                            _outOffset[4] += new Vector3(2, 0, 0);
                            _outOffset[5] += new Vector3(1, -1, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Left)
                    {
                        //right border
                        if (_outstate[1] % BoardWidth != BoardWidth - 1)
                        {
                            //_outstate[1] +=;
                            _outstate[2] += +1 - BoardWidth;
                            _outstate[4] += +2 * BoardWidth;
                            _outstate[5] += +1 + BoardWidth;

                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(1, -1, 0);
                            _outOffset[4] += new Vector3(0, 2, 0);
                            _outOffset[5] += new Vector3(1, 1, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Down)
                    {
                        //top border
                        if (_outstate[1] + BoardWidth < BoardWidth * AdjBoardHeight)
                        {
                            //_outstate[1] +=
                            _outstate[2] += +1 + BoardWidth;
                            _outstate[4] += -2;
                            _outstate[5] += -1 + BoardWidth;

                            //_outOffset[1]+=  //pivot
                            _outOffset[2] += new Vector3(1, 1, 0);
                            _outOffset[4] += new Vector3(-2, 0, 0);
                            _outOffset[5] += new Vector3(-1, 1, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Right)
                    {
                        //left border
                        if (_outstate[1] % BoardWidth != 0)
                        {
                            //_outstate[1] +=;
                            _outstate[2] += -1 + BoardWidth;
                            _outstate[4] += -2 * BoardWidth;
                            _outstate[5] += -1 - BoardWidth;

                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(-1, 1, 0);
                            _outOffset[4] += new Vector3(0, -2, 0);
                            _outOffset[5] += new Vector3(-1, -1, 0);

                            return true;
                        }
                    }
                }
                break;
            case ShapeType.S_Shape:
                {
                    if (activeShapeRotationState == RotationState.Start)
                    {
                        //bottom border
                        if (_outstate[1] - BoardWidth >= 0)
                        {
                            _outstate[0] += +1 + BoardWidth;
                            //_outstate[1] +=;
                            _outstate[5] += +1 - BoardWidth;
                            _outstate[6] += -2 * BoardWidth;

                            _outOffset[0] += new Vector3(1, 1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[5] += new Vector3(1, -1, 0);
                            _outOffset[6] += new Vector3(0, -2, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Left)
                    {
                        //right border
                        if (_outstate[1] % BoardWidth != BoardWidth - 1)
                        {
                            _outstate[0] += -1 + BoardWidth;
                            //_outstate[1] +=;
                            _outstate[5] += +1 + BoardWidth;
                            _outstate[6] += +2;

                            _outOffset[0] += new Vector3(-1, 1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[5] += new Vector3(1, 1, 0);
                            _outOffset[6] += new Vector3(2, 0, 0);

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Down)
                    {
                        //top border
                        if (_outstate[1] + BoardWidth < BoardWidth * AdjBoardHeight)
                        {
                            _outstate[0] += -1 - BoardWidth;
                            //_outstate[1] +=;
                            _outstate[5] += -1 + BoardWidth;
                            _outstate[6] += +2 * BoardWidth;

                            _outOffset[0] += new Vector3(-1, -1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[5] += new Vector3(-1, 1, 0);
                            _outOffset[6] += new Vector3(0, 2, 0);
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Right)
                    {
                        //right border
                        if (_outstate[1] % BoardWidth != 0)
                        {
                            _outstate[0] += +1 - BoardWidth;
                            //_outstate[1] +=;
                            _outstate[5] += -1 - BoardWidth;
                            _outstate[6] += -2;

                            _outOffset[0] += new Vector3(1, -1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[5] += new Vector3(-1, -1, 0);
                            _outOffset[6] += new Vector3(-2, 0, 0);
                        }
                    }
                }
                break;
            case ShapeType.T_Shape:
                {
                    if (activeShapeRotationState == RotationState.Start)
                    {
                        //bottom border
                        if (_outstate[1] - BoardWidth >= 0)
                        {
                            _outstate[0] += +1 + BoardWidth;
                            //_outstate[1] += 
                            _outstate[2] += -1 - BoardWidth;
                            _outstate[5] += +1 - BoardWidth;

                            _outOffset[0] += new Vector3(1, 1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(-1, -1, 0);
                            _outOffset[5] += new Vector3(1, -1, 0);
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Left)
                    {
                        //right border
                        if (_outstate[1] % BoardWidth != BoardWidth - 1)
                        {
                            _outstate[0] += -1 + BoardWidth;
                            //_outstate[1] += 
                            _outstate[2] += +1 - BoardWidth;
                            _outstate[5] += +1 + BoardWidth;

                            _outOffset[0] += new Vector3(-1, 1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(1, -1, 0);
                            _outOffset[5] += new Vector3(1, 1, 0);
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Down)
                    {
                        //top border
                        if (_outstate[1] + BoardWidth < BoardWidth * AdjBoardHeight)
                        {
                            _outstate[0] += -1 - BoardWidth;
                            //_outstate[1] +=;
                            _outstate[2] += +1 + BoardWidth;
                            _outstate[5] += -1 + BoardWidth;

                            _outOffset[0] += new Vector3(-1, -1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(1, 1, 0);
                            _outOffset[5] += new Vector3(-1, 1, 0);
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Right)
                    {
                        //left border
                        if (_outstate[1] % BoardWidth != 0)
                        {
                            _outstate[0] += +1 - BoardWidth;
                            //_outstate[1] +=;
                            _outstate[2] += -1 + BoardWidth;
                            _outstate[5] += -1 - BoardWidth;

                            _outOffset[0] += new Vector3(1, -1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(-1, 1, 0);
                            _outOffset[5] += new Vector3(-1, -1, 0);
                        }
                    }
                }
                break;
            case ShapeType.O_Shape:
                //Cube dont rotate silly                    
                break;
            case ShapeType.I_Shape:
                {
                    if (activeShapeRotationState == RotationState.Start)
                    {
                        //bottom border
                        if (_outstate[1] - BoardWidth * 2 > 0)
                        {
                            _outstate[0] += +2 + BoardWidth;
                            _outstate[1] += +1;
                            _outstate[2] += -BoardWidth;
                            _outstate[3] += -1 - 2 * BoardWidth;

                            _outOffset[0] += new Vector3(2, 1, 0);
                            _outOffset[1] += new Vector3(1, 0, 0);
                            _outOffset[2] += new Vector3(0, -1, 0);
                            _outOffset[3] += new Vector3(-1, -2, 0);
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Left)
                    {
                        //both borders
                        if (_outstate[1] % BoardWidth < BoardWidth - 2 &&
                            _outstate[1] % BoardWidth != 0)
                        {
                            _outstate[0] += -1 + 2 * BoardWidth;
                            _outstate[1] += +BoardWidth;
                            _outstate[2] += +1;
                            _outstate[3] += +2 - BoardWidth;

                            _outOffset[0] += new Vector3(-1, 2, 0);
                            _outOffset[1] += new Vector3(0, 1, 0);
                            _outOffset[2] += new Vector3(1, 0, 0);
                            _outOffset[3] += new Vector3(2, -1, 0);
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Down)
                    {
                        //top border
                        if (_outstate[1] + BoardWidth * 2 < BoardWidth * AdjBoardHeight)
                        {
                            _outstate[0] += -2 - BoardWidth;
                            _outstate[1] += -1;
                            _outstate[2] += +BoardWidth;
                            _outstate[3] += +1 + 2 * BoardWidth;

                            _outOffset[0] += new Vector3(-2, -1, 0);
                            _outOffset[1] += new Vector3(-1, 0, 0);
                            _outOffset[2] += new Vector3(0, 1, 0);
                            _outOffset[3] += new Vector3(1, 2, 0);
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Right)
                    {
                        //both borders
                        if (_outstate[1] % BoardWidth != BoardWidth - 1 &&
                            _outstate[1] % BoardWidth > 1)
                        {
                            _outstate[0] += +1 - 2 * BoardWidth;
                            _outstate[1] += -BoardWidth;
                            _outstate[2] += -1;
                            _outstate[3] += -2 + BoardWidth;

                            _outOffset[0] += new Vector3(1, -2, 0);
                            _outOffset[1] += new Vector3(0, -1, 0);
                            _outOffset[2] += new Vector3(-1, 0, 0);
                            _outOffset[3] += new Vector3(-2, 1, 0);
                        }
                    }
                }
                break;
        }
        return false;
    }

    public bool AttemptWallKick(int[] currentState, RotationState curState, RotationState nextState, bool is_IPiece, out int[] _returnState, out Vector3[] _returnOffsets)
    {
        int[] checkState = new int[8];
        Vector3[] checkOffset = new Vector3[8];

        void CopyState(ref int[] _outState, int[] inState)
        {
            for (int i = 0; i < inState.Length; i++)
            {
                _outState[i] = currentState[i];
            }
        }

        switch (curState)
        {
            case RotationState.Start:
                //right
                if (is_IPiece)
                {
                    if (nextState == RotationState.Right) //0>>1
                    {
                        //(0,  0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-2, 0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && MoveLeft(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-2, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1,  1)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && MoveUp(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(1, 1);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-2, -1)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && MoveLeft(ref checkState) && MoveDown(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-2, -1);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1,  2)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && MoveUp(ref checkState) && MoveUp(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(1, 2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                    }
                    else // 0>>3
                    {
                        //(0, 0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-1, 0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-1, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(2, 0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && MoveRight(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(2, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-1, 2)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && MoveUp(ref checkState) && MoveUp(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-1, 2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(2, -1)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && MoveRight(ref checkState) && MoveDown(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(2, -1);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                    }
                }
                else
                {
                    if (nextState == RotationState.Right)
                    {
                        //(0,  0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-1,  0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-1, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-1,  1)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && MoveUp(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-1, 1);
                            }
                            _returnOffsets = checkOffset;

                            return true;
                        }
                        //(0   -2)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveDown(ref checkState) && MoveDown(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(0, -2);
                            }

                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-1, -2)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && MoveDown(ref checkState) && MoveDown(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-1, -2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                    }
                    else //left
                    {
                        //(0, 0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1, 0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(1, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1, 1)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && MoveUp(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(1, 1);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(0, -2)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveDown(ref checkState) && MoveDown(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(0, -2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1, -2)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && MoveDown(ref checkState) && MoveDown(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(1, -2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                    }
                }
                break;
            case RotationState.Right: 
                if (is_IPiece)
                {
                    if (nextState == RotationState.Down) //1>>2
                    {
                        //(0,0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-1, 0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-1, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(2, 0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && MoveRight(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(2, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-1, 2)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && MoveUp(ref checkState) && MoveUp(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-1, 2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(2, -1)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && MoveRight(ref checkState) && MoveDown(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(2, -1);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                    }
                    else //start //1>>0
                    {
                        //(0,0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(2,0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && MoveRight(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(2, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-1,0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-1, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(2,1)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && MoveRight(ref checkState) && MoveUp(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(2, 1);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-1,-2)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && MoveDown(ref checkState) && MoveDown(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-1, -2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                    }
                }
                else
                {
                    if (nextState == RotationState.Down)
                    {
                        //(0, 0)
                        CopyState(ref checkState, currentState);
                        if (RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1, 0)
                        CopyState(ref checkState, currentState);
                        if (MoveRight(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(1, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1, -1)
                        CopyState(ref checkState, currentState);
                        if (MoveRight(ref checkState) && MoveDown(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(1, -1);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(0, 2)
                        CopyState(ref checkState, currentState);
                        if (MoveUp(ref checkState) && MoveUp(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(0, 2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1, 2)
                        CopyState(ref checkState, currentState);
                        if (MoveRight(ref checkState) && MoveUp(ref checkState) && MoveUp(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(1, 2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                    }
                    else //Start
                    {
                        //(0, 0)
                        CopyState(ref checkState, currentState);
                        if (RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1, 0)
                        CopyState(ref checkState, currentState);
                        if (MoveRight(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(1, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1, -1)
                        CopyState(ref checkState, currentState);
                        if (MoveRight(ref checkState) && MoveDown(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(1, -1);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(0, 2)
                        CopyState(ref checkState, currentState);
                        if (MoveUp(ref checkState) && MoveUp(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(0, 2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1, 2)
                        CopyState(ref checkState, currentState);
                        if (MoveRight(ref checkState) && MoveUp(ref checkState) && MoveUp(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(1, 2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                    }
                }
                break;
            case RotationState.Down:
                if (is_IPiece)
                {
                    if (nextState == RotationState.Left) //2>>3
                    {
                        //(0,0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(2,0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && MoveRight(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(2, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-1,0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-1, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(2, 1)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && MoveRight(ref checkState) && MoveUp(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(2, 1);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-1,-2)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && MoveDown(ref checkState) && MoveDown(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-1, -2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                    }
                    else //right //2>>1
                    {
                        //(0,0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1,  0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(1, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-2, 0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && MoveLeft(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-2, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1, -2)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && MoveDown(ref checkState) && MoveDown(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(1, -2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-2, 1)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && MoveLeft(ref checkState) && MoveDown(ref checkState) && MoveUp(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-2, 1);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                    }
                }
                else
                { 
                    if (nextState == RotationState.Left)
                {
                    //(0, 0)
                    CopyState(ref checkState, currentState);
                    if (RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                    {
                        _returnState = checkState;                        
                        _returnOffsets = checkOffset;
                        return true;
                    }
                    //(1, 0)
                    CopyState(ref checkState, currentState);
                    if (MoveRight(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                    {
                        _returnState = checkState;
                        for (int i = 0; i < 8; i++)
                        {
                            checkOffset[i] += new Vector3(1, 0);
                        }
                        _returnOffsets = checkOffset;
                        return true;
                    }
                    //(1, 1)
                    CopyState(ref checkState, currentState);
                    if (MoveRight(ref checkState) && MoveUp(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                    {
                        _returnState = checkState;
                        for (int i = 0; i < 8; i++)
                        {
                            checkOffset[i] += new Vector3(1, 1);
                        }
                        _returnOffsets = checkOffset;
                        return true;
                    }
                    //(0, -2)
                    CopyState(ref checkState, currentState);
                    if (MoveDown(ref checkState) && MoveDown(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                    {
                        _returnState = checkState;
                        for (int i = 0; i < 8; i++)
                        {
                            checkOffset[i] += new Vector3(0, -2);
                        }
                        _returnOffsets = checkOffset;
                        return true;
                    }
                    //(1, -2)
                    CopyState(ref checkState, currentState);
                    if (MoveRight(ref checkState) && MoveDown(ref checkState) && MoveDown(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                    {
                        _returnState = checkState;
                        for (int i = 0; i < 8; i++)
                        {
                            checkOffset[i] += new Vector3(1, -2);
                        }
                        _returnOffsets = checkOffset;
                        return true;
                    }
                }
                    else //right
                {
                    //(0, 0)
                    CopyState(ref checkState, currentState);
                    if (RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                    {
                        _returnState = checkState;
                        _returnOffsets = checkOffset;
                        return true;
                    }
                    //(-1, 0)
                    CopyState(ref checkState, currentState);
                    if (MoveLeft(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                    {
                        _returnState = checkState;
                        for (int i = 0; i < 8; i++)
                        {
                            checkOffset[i] += new Vector3(-1, 0);
                        }
                        _returnOffsets = checkOffset;
                        return true;
                    }
                    //(-1, 1)
                    CopyState(ref checkState, currentState);
                    if (MoveLeft(ref checkState) && MoveUp(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                    {
                        _returnState = checkState;
                        for (int i = 0; i < 8; i++)
                        {
                            checkOffset[i] += new Vector3(-1, 1);
                        }
                        _returnOffsets = checkOffset;
                        return true;
                    }
                    //(0, -2)
                    CopyState(ref checkState, currentState);
                    if (MoveDown(ref checkState) && MoveDown(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                    {
                        _returnState = checkState;
                        for (int i = 0; i < 8; i++)
                        {
                            checkOffset[i] += new Vector3(0, -2);
                        }
                        _returnOffsets = checkOffset;
                        return true;
                    }
                    //(-1, -2)
                    CopyState(ref checkState, currentState);
                    if (MoveLeft(ref checkState) && MoveDown(ref checkState) && MoveDown(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                    {
                        _returnState = checkState;
                        for (int i = 0; i < 8; i++)
                        {
                            checkOffset[i] += new Vector3(-1, -2);
                        }
                        _returnOffsets = checkOffset;
                        return true;
                    }
                }
                }
                break;
            case RotationState.Left:
                if (is_IPiece)
                {
                    if (nextState == RotationState.Start) //3>>0
                    {
                        //(0,0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1,0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(1, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-2, 0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && MoveLeft(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-2, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1, -2)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && MoveDown(ref checkState) && MoveDown(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(1, -2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-2, 1)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && MoveLeft(ref checkState) && MoveUp(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-2, 1);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                    } 
                    else //down //3>>2
                    {
                        //(0,0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-2,0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && MoveLeft(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-2, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1,0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(1, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-2,-1)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveLeft(ref checkState) && MoveLeft(ref checkState) && MoveDown(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-2, -1);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1,2)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (MoveRight(ref checkState) && MoveUp(ref checkState) && MoveUp(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(1, 2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                    }
                }
                else
                {
                    if (nextState == RotationState.Start)
                    {
                        //(0,  0)
                        CopyState(ref checkState, currentState);
                        if (RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-1,  0)
                        CopyState(ref checkState, currentState);
                        if (MoveLeft(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-1, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-1, -1)
                        CopyState(ref checkState, currentState);
                        if (MoveLeft(ref checkState) && MoveDown(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-1, -1);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(0,   2)
                        CopyState(ref checkState, currentState);
                        if (MoveUp(ref checkState) && MoveUp(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(0, 2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-1,  2)
                        CopyState(ref checkState, currentState);
                        if (MoveLeft(ref checkState) && MoveUp(ref checkState) && MoveUp(ref checkState) && RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-1, 2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                    }
                    else //down
                    {
                        //(0,  0)
                        CopyState(ref checkState, currentState);
                        if (RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            _returnOffsets = checkOffset;

                            return true;
                        }
                        //(-1,  0)
                        CopyState(ref checkState, currentState);
                        if (MoveLeft(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-1, 0);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-1, -1)
                        CopyState(ref checkState, currentState);
                        if (MoveLeft(ref checkState) && MoveDown(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-1, -1);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(0,  2)
                        CopyState(ref checkState, currentState);
                        if (MoveUp(ref checkState) && MoveUp(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(0, 2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(-1,  2)
                        CopyState(ref checkState, currentState);
                        if (MoveLeft(ref checkState) && MoveUp(ref checkState) && MoveUp(ref checkState) && RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            for (int i = 0; i < 8; i++)
                            {
                                checkOffset[i] += new Vector3(-1, 2);
                            }
                            _returnOffsets = checkOffset;
                            return true;
                        }
                    }
                }
                break;
        }

        _returnState = new int[8];
        _returnOffsets = new Vector3[8];
        return false;
    }

    public void OnDrawGizmos()
    {
        for (int i = 0; i < BoardWidth * AdjBoardHeight; i++)
        {
            if (i / BoardWidth > BoardHeight - 1)
                Gizmos.color = Color.grey;
            else
                Gizmos.color = Color.cyan;
            
            Gizmos.DrawWireCube(new Vector3(i % BoardWidth, i / BoardWidth, 0), Vector3.one * 0.95f);
        }

        if (activeShapeObjectIndexs != null)
        {
            Gizmos.color = Color.red;

            for (int i = 0; i < activeShapeObjectIndexs.Length; i++)
            {
                if (activeShapeObjectIndexs[i] != -1)
                    Gizmos.DrawWireSphere(new Vector3(activeShapeObjectIndexs[i] % BoardWidth, activeShapeObjectIndexs[i] / BoardWidth, 0), 0.5f );
            }
        }

        if (boardCells != null)
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < boardCells.Length; i++)
            {
                if (boardCells[i].isFilled)
                {
                    Gizmos.DrawWireSphere(new Vector3(i % BoardWidth, i / BoardWidth, 0), 0.5f);
                }
            }
        }
    }
    
    public struct BoardCell
    {
        public int cellIndex;
        public bool isFilled;
        
        public BoardCell(int _cellIndex)
        {
            cellIndex = _cellIndex;
            isFilled = false;
        }
    }
}

public enum ShapeType
{
    L_Shape,
    J_Shape,
    Z_Shape,
    S_Shape,
    T_Shape,
    O_Shape,
    I_Shape
}

public enum RotationState
{
    Start,
    Left,
    Down,
    Right,
}

public class ShapeData
{
    public static ShapeType[] shapeList = new ShapeType[7] { ShapeType.L_Shape, ShapeType.J_Shape, ShapeType.Z_Shape, ShapeType.S_Shape, ShapeType.T_Shape, ShapeType.O_Shape, ShapeType.I_Shape };

    // 0,0,1,0
    // 1,1,1,0
    public static int[] LShape = new int[8] {1,1,1,0, 0,0,1,0};

    // 1,0,0,0
    // 1,1,1,0
    public static int[] JShape = new int[8] {1,1,1,0, 1,0,0,0};  

    // 1,1,0,0
    // 0,1,1,0
    public static int[] ZShape = new int[8] {0,1,1,0, 1,1,0,0};

    // 0,1,1,0
    // 1,1,0,0
    public static int[] SShape = new int[8] {1,1,0,0, 0,1,1,0};

    // 0,1,0,0
    // 1,1,1,0
    public static int[] TShape = new int[8] {1,1,1,0, 0,1,0,0};

    // 0,1,1,0 
    // 0,1,1,0 
    public static int[] OShape = new int[8] {0,1,1,0, 0,1,1,0};

    // 0,0,0,0 
    // 1,1,1,1 
    public static int[] IShape = new int[8] {1,1,1,1, 0,0,0,0};
}

