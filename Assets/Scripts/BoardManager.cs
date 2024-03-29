﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class BoardManager : MonoBehaviour
{
    public const int BoardWidth = 10;
    public const int BoardHeight = 20;
    public const float LineClearPause = 1.0f;
    public static int AdjBoardHeight => BoardHeight + 2;
    public static int StartingLevel = 0;
    public static int LookAheadSteps = 3;
    public static bool ShowShapeShadow = true;
    public static bool SendScoreAtEndOfRound = true;

    public static bool Paused = false;

    [SerializeField]
    private Transform prefabCellParent = null;

    [SerializeField]
    private BoardCell prefabCellObject = null;
    [SerializeField]
    private GameObject prefabEmptyCellObject = null;
    [SerializeField]
    private GameObject prefabShadowObject = null;

    [SerializeField]
    private BoardCamera boardCamera;

    [SerializeField]
    private BoardUI boardUI;

    [SerializeField]
    private GameOverUI gameOverUI;

    [SerializeField]
    private PauseUI pauseUI;

    [SerializeField]
    private BoardInput boardInput;

    [SerializeField]
    MaterialSet materialSet;

    [SerializeField]
    LevelInfo levelInfo;

    private BoardCellInfo[] boardCells = null;
    private BoardCell[] boardCellObjects = null;
    private GameObject[] boardCellEmptyObjects = null;
    private GameObject[] shadowObjects = null;

    private ShapeType activeShapeType;
    private RotationState activeShapeRotationState;
    private BoardCell[] activeShapeObjectSet = null;
    private int[] activeShapeObjectIndexs = null;

    private NextShape[] nextShapes = null;

    struct NextShape
    {
        public ShapeType nextObjectShapeType;
        public BoardCell[] nextObjectSet;
        public int[] nextShapeObjectIndexs;
    }

    private float stepTimer = 0.0f;
    private float lineClearPause = 0.0f;
    private bool isPlaying = true;

    private long score = 0;
    private int linesCleared = 0;
    private int curLevel => StartingLevel + linesCleared / 10;

    private List<ShapeType> shapePool = new List<ShapeType>();

    private Vector3 nextShapeOffset = new Vector3(8,0,0);
    private Vector3 nextShapeVertOffset = new Vector3(0, -4, 0);

    public long Score => score;
    public long CurLevel => curLevel;
       
    public void Start()
    {
        ResetBoard();

        boardCamera.SetupCamera(this);
        boardUI.Initialise(this);
        boardInput.Initialise(OnInputMove, OnInputRotate, OnInputSlam);
    }

    public void ResetBoard()
    {
        score = 0;
        linesCleared = 0;

        //clear out the old board objects, active shapes and next objects
        if (boardCellObjects != null)
        {
            for (int i = 0; i < boardCellObjects.Length; i++)
            {
                if (boardCellObjects[i] != null)
                {
                    Destroy(boardCellObjects[i].gameObject);
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
                    Destroy(activeShapeObjectSet[i].gameObject);
                    activeShapeObjectSet[i] = null;
                }
            }
        }

        if (nextShapes != null)
        {
            for (int shapeIndex = 0; shapeIndex < LookAheadSteps; shapeIndex++)
            {
                for (int i = 0; i < nextShapes[shapeIndex].nextObjectSet.Length; i++)
                {
                    if (nextShapes[shapeIndex].nextObjectSet[i] != null)
                    {
                        Destroy(nextShapes[shapeIndex].nextObjectSet[i].gameObject);
                        nextShapes[shapeIndex].nextObjectSet[i] = null;
                    }
                }
            }

        }

        //Create empty cells if they have not been already
        if (boardCellEmptyObjects == null)
        {
            boardCellEmptyObjects = new GameObject[BoardWidth * AdjBoardHeight];
            for (int i = 0; i < boardCellEmptyObjects.Length; i++)
            {
                boardCellEmptyObjects[i] = Instantiate(prefabEmptyCellObject,
                                                        new Vector3(i % BoardWidth, i / BoardWidth),
                                                        Quaternion.identity,
                                                        prefabCellParent);                
            }
        }
        else
        {
            for (int i = 0; i < boardCellEmptyObjects.Length; i++)
            {
                boardCellEmptyObjects[i].SetActive(true);
            }
        }

        //Create look aheads
        if (shadowObjects == null)
        {
            shadowObjects = new GameObject[8];
            for (int i = 0; i < 8; i++)
            {
                shadowObjects[i] = Instantiate(prefabShadowObject, prefabCellParent);
            }
        }

        //Make the board
        boardCells = new BoardCellInfo[BoardWidth * AdjBoardHeight];
        boardCellObjects = new BoardCell[BoardHeight * AdjBoardHeight];

        for (int cellIndex = 0; cellIndex < BoardWidth * AdjBoardHeight; cellIndex++)
        {
            boardCells[cellIndex] = new BoardCellInfo(cellIndex);
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
        CreateNewShape(activeShapeType, out activeShapeObjectIndexs, out activeShapeObjectSet);
        activeShapeRotationState = RotationState.Start;

        //Make the next shapes ready for action
        nextShapes = new NextShape[LookAheadSteps];

        for (int shapeIndex = 0; shapeIndex < nextShapes.Length; shapeIndex++)
        {
            nextShapes[shapeIndex].nextObjectShapeType = GetNextShape();
            CreateNewShape(nextShapes[shapeIndex].nextObjectShapeType, out nextShapes[shapeIndex].nextShapeObjectIndexs, out nextShapes[shapeIndex].nextObjectSet);
            for (int i = 0; i < 8; i++)
            {
                if (nextShapes[shapeIndex].nextObjectSet[i] != null)
                    nextShapes[shapeIndex].nextObjectSet[i].transform.position += nextShapeOffset + nextShapeVertOffset * shapeIndex;
            }
        }

        UpdateEmptyCells();
        UpdateShadow();
    }

    public void UpdateEmptyCells()
    {
        //emplaced cells
        for (int i = 0; i < BoardWidth * AdjBoardHeight; i++ )
        {
            if (boardCellObjects[i] != null)
                boardCellEmptyObjects[i].SetActive(false);
            else
                boardCellEmptyObjects[i].SetActive(true);
        }

        //active objects
        for (int i = 0; i < 8; i++)
        {
            if (activeShapeObjectIndexs[i] != -1)
                boardCellEmptyObjects[activeShapeObjectIndexs[i]].SetActive(false);
        }

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

    public bool CreateNewShape(ShapeType _shapeType, out int[] indexes, out BoardCell[] cellObjects)
    {
        int[] dataset = null;
        Material shapeMaterial = null;
        switch (_shapeType)
        {
            //all 4x2 shapes
            case ShapeType.L_Shape: 
                dataset = ShapeData.LShape;
                shapeMaterial = materialSet.LPieceMaterial;
                break;
            case ShapeType.J_Shape: 
                dataset = ShapeData.JShape;
                shapeMaterial = materialSet.JPieceMaterial;
                break;
            case ShapeType.Z_Shape:
                dataset = ShapeData.ZShape;
                shapeMaterial = materialSet.ZPieceMaterial;
                break;
            case ShapeType.S_Shape:
                dataset = ShapeData.SShape;
                shapeMaterial = materialSet.SPieceMaterial;
                break;
            case ShapeType.T_Shape:
                dataset = ShapeData.TShape;
                shapeMaterial = materialSet.TPieceMaterial;
                break;
            case ShapeType.O_Shape: 
                dataset = ShapeData.OShape;
                shapeMaterial = materialSet.OPieceMaterial;
                break;
            case ShapeType.I_Shape: 
                dataset = ShapeData.IShape;
                shapeMaterial = materialSet.IPieceMaterial;
                break;
        }

        indexes = new int[8];

        int spawnIndex = (20 * BoardWidth) + 3;
        indexes[0] = dataset[0] == 1 ? spawnIndex + 0 : -1;
        indexes[1] = dataset[1] == 1 ? spawnIndex + 1 : -1;
        indexes[2] = dataset[2] == 1 ? spawnIndex + 2 : -1;
        indexes[3] = dataset[3] == 1 ? spawnIndex + 3 : -1;
        indexes[4] = dataset[4] == 1 ? spawnIndex + 0 + BoardWidth : -1;
        indexes[5] = dataset[5] == 1 ? spawnIndex + 1 + BoardWidth : -1;
        indexes[6] = dataset[6] == 1 ? spawnIndex + 2 + BoardWidth : -1;
        indexes[7] = dataset[7] == 1 ? spawnIndex + 3 + BoardWidth : -1;

        BoardCell MakeNewCellObject(Vector3 _spawnPoint, int x, int y)
        {
            BoardCell _returnObject = Instantiate(prefabCellObject, _spawnPoint + new Vector3(x, y, 0), Quaternion.identity, prefabCellParent);
            Renderer _renderer = _returnObject.GetComponent<Renderer>();
            if (_renderer != null)
                _renderer.material = shapeMaterial;
            return _returnObject;
        }

        cellObjects = new BoardCell[8];
        Vector3 spawnPoint = new Vector3(3, 20, 0);
        cellObjects[0] = dataset[0] == 1 ? MakeNewCellObject(spawnPoint, 0, 0) : null;
        cellObjects[1] = dataset[1] == 1 ? MakeNewCellObject(spawnPoint, 1, 0) : null;
        cellObjects[2] = dataset[2] == 1 ? MakeNewCellObject(spawnPoint, 2, 0) : null;
        cellObjects[3] = dataset[3] == 1 ? MakeNewCellObject(spawnPoint, 3, 0) : null;
        cellObjects[4] = dataset[4] == 1 ? MakeNewCellObject(spawnPoint, 0, 1) : null;
        cellObjects[5] = dataset[5] == 1 ? MakeNewCellObject(spawnPoint, 1, 1) : null;
        cellObjects[6] = dataset[6] == 1 ? MakeNewCellObject(spawnPoint, 2, 1) : null;
        cellObjects[7] = dataset[7] == 1 ? MakeNewCellObject(spawnPoint, 3, 1) : null;

        //check we can spawn an object there
        for (int i = 0; i < indexes.Length; i++)
        {
            if (indexes[i] != -1)
            {
                if (boardCells[indexes[i]].isFilled)
                {
                    //Fail state, game over
                    return false;
                }
            }
        }

        return true;
    }

    public void UpdateShadow()
    {
        int movesDownAllowed = 0;

        int[] checkState = new int[8];
        CopyState(ref checkState, activeShapeObjectIndexs);

        while (MoveDown(ref checkState) && CheckConflicts(checkState))
        {
            movesDownAllowed++;
        }

        for (int i = 0; i < 8; i++)
        {
            if (activeShapeObjectIndexs[i] >= 0 && movesDownAllowed > 0 && lineClearPause <= 0 && ShowShapeShadow)
            {
                shadowObjects[i].SetActive(true);
                shadowObjects[i].transform.position = activeShapeObjectSet[i].transform.position;
                shadowObjects[i].transform.position += Vector3.down * movesDownAllowed;
            }
            else
            {
                shadowObjects[i].SetActive(false);
            }
        }
    }


    public void Update()
    {
        if (isPlaying && !Paused)
        {
            UpdateSteps();
        }
    }

    public void PauseGame(bool pauseState)
    {
        if (pauseState)
        {
            Paused = true;
            pauseUI.ShowPauseUI();
        }
        else
        {
            Paused = false;
            pauseUI.ShowPauseUI();
        }
    }

    public void UpdateSteps()
    {
        if (lineClearPause > 0)
        {
            lineClearPause -= Time.deltaTime;
        }
        else
        {
            float stepsPerMinute = 60.0f / levelInfo.GetStepsPerLevel(curLevel);

            if (stepTimer > 1.0f / stepsPerMinute)
            {
                stepTimer -= 1.0f / stepsPerMinute;

                //if we can't move down, place and solve
                if (!MoveDownSimple())
                {
                    PlaceAndCheckState();
                }
            }

            stepTimer += Time.deltaTime;
        }
    }   

    public void OnInputMove(Direction direction)
    {
        if (!isPlaying || lineClearPause > 0 || Paused)
            return;

        switch (direction)
        {
            case Direction.Left:
                MoveLeftSimple();
                break;
            case Direction.Right:
                MoveRightSimple();
                break;
            case Direction.Up:
                //nutin
                break;
            case Direction.Down:
                if (!MoveDownSimple())
                {
                    PlaceAndCheckState();
                }
                break;
        }
    }

    public void OnInputRotate(bool isLeft)
    {
        if (!isPlaying || lineClearPause > 0 || Paused)
            return;

        if (isLeft)        
            RotateLeft();        
        else        
            RotateRight();        
    }

    public void OnInputSlam()
    {
        if (!isPlaying || lineClearPause > 0 || Paused)
            return;

        while (MoveDownSimple()) { }

        PlaceAndCheckState();
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

            UpdateEmptyCells();
            UpdateShadow();
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

            UpdateEmptyCells();
            UpdateShadow();
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

            UpdateEmptyCells();
            UpdateShadow();
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
                    //Destroy(boardCellObjects[rowIndex * BoardWidth + colIndex]);
                    boardCellObjects[rowIndex * BoardWidth + colIndex].FireDestroyAnim();
                    boardCellObjects[rowIndex * BoardWidth + colIndex].DestroyAfterDelay(LineClearPause);
                    boardCellObjects[rowIndex * BoardWidth + colIndex] = null;
                }

                //starting at the next row move every other row down
                for (int index = (rowIndex + 1) * BoardWidth; index < BoardWidth * AdjBoardHeight; index++)
                {
                    int belowIndex = index - BoardWidth;

                    boardCells[belowIndex].isFilled = boardCells[index].isFilled;

                    boardCellObjects[belowIndex] = boardCellObjects[index];
                    if (boardCellObjects[belowIndex] != null)
                    {
                        boardCellObjects[belowIndex].AddPositionAfterDelay(Vector3.down, LineClearPause);
                    }
                }

                //move back since we cleared a row
                rowIndex--;
            }
        }
        
        if (rowsCompleteThisSolve > 0)
        {
            switch (rowsCompleteThisSolve)
            {
                case 1:
                    score += 40 * (curLevel + 1); break;
                case 2:
                    score += 100 * (curLevel + 1); break;
                case 3:
                    score += 300 * (curLevel + 1); break;
                case 4:
                    score += 1200 * (curLevel + 1); break;
            }
            Debug.Log($"Solved {rowsCompleteThisSolve} this round");
            lineClearPause = LineClearPause;
        }
        linesCleared += rowsCompleteThisSolve;

        //Attempt to spawn the saved object 
        if (LookAheadSteps == 0)
        {
            activeShapeType = GetNextShape();
            CreateNewShape(activeShapeType, out activeShapeObjectIndexs, out activeShapeObjectSet);
            activeShapeRotationState = RotationState.Start;
        }
        else
        {
            activeShapeObjectIndexs = nextShapes[0].nextShapeObjectIndexs;
            activeShapeObjectSet = nextShapes[0].nextObjectSet;
            activeShapeRotationState = RotationState.Start;
            activeShapeType = nextShapes[0].nextObjectShapeType;
        }

        if (!CheckConflicts(activeShapeObjectIndexs))
        {
            OnGameOver();
        }
        else
        {
            if (LookAheadSteps > 0)
            {
                //move the objects to the spawn point
                for (int i = 0; i < 8; i++)
                {
                    if (activeShapeObjectSet[i] != null)
                        activeShapeObjectSet[i].transform.position -= nextShapeOffset;
                }

                //shuffle them forward
                for (int shapeIndex = 1; shapeIndex < nextShapes.Length; shapeIndex++)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (nextShapes[shapeIndex].nextObjectSet[i] != null)
                            nextShapes[shapeIndex].nextObjectSet[i].transform.position -= nextShapeVertOffset;
                    }

                    nextShapes[shapeIndex - 1] = nextShapes[shapeIndex];
                }

                nextShapes[nextShapes.Length - 1].nextObjectShapeType = GetNextShape();
                CreateNewShape(nextShapes[nextShapes.Length - 1].nextObjectShapeType,
                                out nextShapes[nextShapes.Length - 1].nextShapeObjectIndexs,
                                out nextShapes[nextShapes.Length - 1].nextObjectSet);

                for (int i = 0; i < 8; i++)
                {
                    if (nextShapes[nextShapes.Length - 1].nextObjectSet[i] != null)
                        nextShapes[nextShapes.Length - 1].nextObjectSet[i].transform.position += nextShapeOffset + nextShapeVertOffset * (nextShapes.Length - 1);
                }
            }

            UpdateEmptyCells();
            UpdateShadow();
        }
    }

    private void OnGameOver()
    {
        //Game Over
        isPlaying = false;

        gameOverUI.InitialiseAndShow(Score, linesCleared, curLevel, BoardManager.SendScoreAtEndOfRound, () => {
            ResetBoard();
        });
    }

    private bool CheckConflicts(int[] _indexs)
    {
        for (int i = 0; i < _indexs.Length; i++)
        {
            if (_indexs[i] != -1)
            {
                if (_indexs[i] < 0)
                    Debug.Log("SCREM");

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
            UpdateEmptyCells();
            UpdateShadow();
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

            UpdateEmptyCells();
            UpdateShadow();
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
            if (_outState[i] != -1)
            {
                if (_outState[i] / BoardWidth == AdjBoardHeight - 1)
                    return false;
            }
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
            if (_outState[i] != -1)
            {
                if (_outState[i] / BoardWidth == 0)
                    return false;
            }
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
                //break;
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

                            return true;
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
                        
                            return true;
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
                            _outstate[0] += +1 + BoardWidth;
                            //_outstate[1] += 
                            _outstate[2] += -1 - BoardWidth;
                            _outstate[5] += +1 - BoardWidth;

                            _outOffset[0] += new Vector3(1, 1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(-1, -1, 0);
                            _outOffset[5] += new Vector3(1, -1, 0);
                            
                            return true;
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
                            _outstate[2] += +1 + BoardWidth;
                            _outstate[5] += -1 + BoardWidth;

                            _outOffset[0] += new Vector3(-1, -1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(1, 1, 0);
                            _outOffset[5] += new Vector3(-1, 1, 0);
                            
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
                            _outstate[5] += -1 - BoardWidth;

                            _outOffset[0] += new Vector3(1, -1, 0);
                            //_outOffset[1] //pivot
                            _outOffset[2] += new Vector3(-1, 1, 0);
                            _outOffset[5] += new Vector3(-1, -1, 0);
                        
                            return true;
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
                            
                            return true;
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

                            return true;
                        }
                    }
                    else if (activeShapeRotationState == RotationState.Down)
                    {
                        //check up 2 blocks and down 1 block
                        if (_outstate[1] + BoardWidth * 2 < BoardWidth * AdjBoardHeight &&
                            _outstate[1] - BoardWidth >= 0)
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
                        
                            return true;
                        }
                    }
                }
                break;
        }
        return false;
    }

    void CopyState(ref int[] _outState, int[] inState)
    {
        for (int i = 0; i < inState.Length; i++)
        {
            _outState[i] = inState[i];
        }
    }

    public bool AttemptWallKick(int[] currentState, RotationState curState, RotationState nextState, bool is_IPiece, out int[] _returnState, out Vector3[] _returnOffsets)
    {
        int[] checkState = new int[8];
        Vector3[] checkOffset = new Vector3[8];

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
                        checkOffset = new Vector3[8];
                        if (RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1, 0)
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
                        //(1, -1)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
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
                        checkOffset = new Vector3[8];
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
                    else //Start
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
                        //(1, -1)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
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
                        checkOffset = new Vector3[8];
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
                        checkOffset = new Vector3[8];
                        if (RotateRight(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;                        
                            _returnOffsets = checkOffset;
                            return true;
                        }
                        //(1, 0)
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
                        //(1, 1)
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
                        //(0, -2)
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
                    }
                    else //right
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
                        //(-1, 1)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
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
                        //(-1, -2)
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
                        //(-1, -1)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
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
                        checkOffset = new Vector3[8];
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
                    }
                    else //down
                    {
                        //(0,  0)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
                        if (RotateLeft(ref checkState, ref checkOffset) && CheckConflicts(checkState))
                        {
                            _returnState = checkState;
                            _returnOffsets = checkOffset;

                            return true;
                        }
                        //(-1,  0)
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
                        //(-1, -1)
                        CopyState(ref checkState, currentState);
                        checkOffset = new Vector3[8];
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
                        checkOffset = new Vector3[8];
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
    
    public struct BoardCellInfo
    {
        public int cellIndex;
        public bool isFilled;
        
        public BoardCellInfo(int _cellIndex)
        {
            cellIndex = _cellIndex;
            isFilled = false;
        }
    }
}

[Serializable]
public struct MaterialSet
{    
    [SerializeField] public Material LPieceMaterial;
    [SerializeField] public Material JPieceMaterial;
    [SerializeField] public Material ZPieceMaterial;
    [SerializeField] public Material SPieceMaterial;
    [SerializeField] public Material TPieceMaterial;
    [SerializeField] public Material OPieceMaterial;
    [SerializeField] public Material IPieceMaterial;
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

public enum Direction
{
    Up,
    Down,
    Left,
    Right
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

