﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait,
    move,
    win,
    lose,
    pause,
}

public enum TileKind
{
    Breakable,
    Blank,
    Normal,
}

[System.Serializable]
public class TileType
{
    public int x;
    public int y;
    public TileKind tileKind;
}

public class Board : MonoBehaviour
{
    [Header ("Scriptable Object Stuff")]
    public World world;
    public int level;

    public GameState currentState = GameState.move;

    [Header("Board Dimensions")]
    public int width;
    public int height;
    public int offset;

    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject breakableTilePrefab;
    public GameObject[] dots;
    public GameObject destroyEffect;

    [Header("Layout")]
    public TileType[] boardLayout;
    private bool[,] blankSpaces;
    private BackgroundTile[,] breakableTiles;
    public GameObject[,] allDots;
    public Dot currentDot;
    private FindMatches findMatches;
    // public int baseDamageValue = 1;
    private int streakValue = 1;
    private BattleManager battleManager;
    private SoundManager soundManager;
    private GoalManager goalManager;

    public float refillDelay = 0.5f;
    // public int[] damageGoals;

    public int countUncertainty = 0;
    public int countLight = 0;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("Current Level"))
        {
            level = PlayerPrefs.GetInt("Current Level");
        }
        if (world != null)
        {
            if (level < world.levels.Length)
            {
                if (world.levels[level] != null)
                {
                    width = world.levels[level].width;
                    height = world.levels[level].height;
                    dots = world.levels[level].dots;
                    boardLayout = world.levels[level].boardLayout;
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        goalManager = FindObjectOfType<GoalManager>();
        soundManager = FindObjectOfType<SoundManager>();
        battleManager = FindObjectOfType<BattleManager>();
        breakableTiles = new BackgroundTile[width, height];
        findMatches = FindObjectOfType<FindMatches>();
        blankSpaces = new bool[width, height];
        allDots = new GameObject[width, height];
        SetUp();
        currentState = GameState.pause;
    }

    public void GenerateBlankSpaces()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileKind == TileKind.Blank)
            {
                blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
            }
        }
    }

    public void GenerateBreakableTiles()
    {
        // Look at all the tiles in the layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            // If a tile is a "Jelly" tile
            if (boardLayout[i].tileKind == TileKind.Breakable)
            {
                // Create a "Jelly" tile at that position
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(breakableTilePrefab, tempPosition, Quaternion.identity);
                breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void SetUp()
    {
        GenerateBlankSpaces();
        GenerateBreakableTiles();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i,j])
                {
                    Vector2 tempPosition = new Vector2(i,j+offset);
                    Vector2 tilePosition = new Vector2(i,j);

                    // To do: moving down the backgroundTile
                    GameObject backgroundTile = Instantiate(tilePrefab,
                    tilePosition, Quaternion.identity) as GameObject;
                    backgroundTile.transform.parent = this.transform;
                    backgroundTile.name = "( " + i + ", " + j + " )";
                    int dotToUse = Random.Range(0, dots.Length);

                    int maxIterations = 0;
                    while(MatchesAt(i,j,dots[dotToUse]) && maxIterations < 100)
                    {
                        dotToUse = Random.Range(0, dots.Length);
                        maxIterations++;
                        Debug.Log(maxIterations);
                    }

                    GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    dot.GetComponent<Dot>().row = j;
                    dot.GetComponent<Dot>().column = i;
                    dot.transform.parent = this.transform;
                    dot.name = "( " + i + ", " + j + " )";
                    allDots[i,j] = dot;

                    UpdateTileCounter(dot.tag, 1);
                }
            }
        }
    }

    private bool MatchesAt(int column, int row, GameObject piece)
    {
        if (column > 1 && row > 1)
        {
            if (allDots[column-1, row] != null && allDots[column-2, row] != null)
            {
                if (allDots[column-1, row].tag == piece.tag
                && allDots[column-2, row].tag == piece.tag)
                {
                    return true;
                }
            }
            if (allDots[column, row-1] != null && allDots[column, row-2] != null)
            {
                if (allDots[column, row-1].tag == piece.tag
                && allDots[column, row-2].tag == piece.tag)
                {
                    return true;
                }
            }
        }
        else if (column <= 1 || row <= 1)
        {
            if (column > 1)
            {
                if (allDots[column-1, row] != null && allDots[column-2, row] != null)
                {
                    if (allDots[column-1, row].tag == piece.tag
                    && allDots[column-2, row].tag == piece.tag)
                    {
                        return true;
                    }
                }
            }
            if (row > 1)
            {
                if (allDots[column, row-1] != null && allDots[column, row-2] != null)
                {
                    if (allDots[column, row-1].tag == piece.tag
                    && allDots[column, row-2].tag == piece.tag)
                    {
                        return true;
                    }
                }
            }

        }
        return false;
    }

    private int ColumnOrRow()
    {
        // Make a copy of the current matches
        List<GameObject> matchCopy = findMatches.currentMatches as List<GameObject>;

        // Cycle through all of match copy and decide if a bomb needs to be created
        for (int i = 0; i < matchCopy.Count; i++)
        {
            // Store this dot
            Dot thisDot = matchCopy[i].GetComponent<Dot>();
            int column = thisDot.column;
            int row = thisDot.row;
            int columnMatch = 0;
            int rowMatch = 0;

            // Cycle through the rest of the pieces and compare
            for (int j = 0; j < matchCopy.Count; j++)
            {
                // Store the next dot
                Dot nextDot = matchCopy[j].GetComponent<Dot>();
                if (nextDot == thisDot)
                {
                    continue;
                }
                if (nextDot.column == thisDot.column && nextDot.CompareTag(thisDot.tag))
                {
                    columnMatch++;
                }
                if (nextDot.row == thisDot.row && nextDot.CompareTag(thisDot.tag))
                {
                    rowMatch++;
                }
            }

            // Return 3 if column or row match
            // Return 2 if adjacent
            // Return 1 if it's a color bomb
            if ( columnMatch == 4 || rowMatch == 4)
            {
                return 1;
            }
            if ( columnMatch == 2 && rowMatch == 2)
            {
                return 2;
            }
            if (columnMatch == 3 || rowMatch == 3)
            {
                return 3;
            }

        }

        return 0;
    }

    private void CheckToMakeBombs()
    {
        // How many objects are in findMatches currentMatches?
        if (findMatches.currentMatches.Count > 3)
        {
            // What type of match?
            int typeOfMatch = ColumnOrRow();
            if (typeOfMatch == 1)
            {
                // Make a color bomb
                // Debug.Log("Making a color bomb");
                // Is the current dot matched?
                if (currentDot != null)
                {
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isColorBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeColorBomb();
                        }
                    }
                    else
                    {
                        if (currentDot.otherDot != null)
                        {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (!otherDot.isColorBomb)
                            {
                                otherDot.isMatched = false;
                                otherDot.MakeColorBomb();
                            }
                        }
                    }
                }
            }
            else if (typeOfMatch == 2)
            {
                // Make an adjacent bomb
                // Debug.Log("Making an adjacent bomb");
                if (currentDot != null)
                {
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isAdjacentBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeAdjacentBomb();
                        }
                    }
                    else
                    {
                        if (currentDot.otherDot != null)
                        {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (!otherDot.isAdjacentBomb)
                            {
                                otherDot.isMatched = false;
                                otherDot.MakeAdjacentBomb();
                            }
                        }
                    }
                }
            }
            else if (typeOfMatch == 3)
            {
                findMatches.CheckBombs();
            }
        }
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if (allDots[column, row].GetComponent<Dot>().isMatched)
        {
            // How many elements are is the matched pieces list from findmatches?
            // if (findMatches.currentMatches.Count >= 4)
            // {
            //     CheckToMakeBombs();
            // }

            // Does a tile need to break?
            if (breakableTiles[column, row] != null)
            {
                // If it does, give one damage.
                breakableTiles[column, row].TakeDamage(1);
                if (breakableTiles[column, row].hitPoints <= 0)
                {
                    breakableTiles[column,row] = null;
                }
            }

            GameObject particle = Instantiate(destroyEffect,
              allDots[column,row].transform.position, Quaternion.identity);

            if (goalManager != null)
            {
                if (battleManager.GetCurrentHealth() > 0)
                {
                    goalManager.CompareGoal(allDots[column,row].tag.ToString());
                    goalManager.UpdateGoals();
                }
            }

            UpdateTileCounter(allDots[column, row].tag, -1);


            // if (allDots[column, row].tag == "Tile_Attack")
            // {
            //     // battleManager.DamageEnemy(baseDamageValue);
            //     if (soundManager != null)
            //     {
            //         soundManager.PlayDestroySpecial();
            //     }
            // }
            if (allDots[column, row].tag == "Tile_Energy") //Tile_Operator
            {
                battleManager.ConsumeEnergy(-1);
                if (soundManager != null)
                {
                    soundManager.PlayDestroySpecial();
                }
            }
            else if (allDots[column, row].tag == "Tile_NormalEvent") // Tile_SpecialEvent
            {
                AlchemyManager alchemyManager = FindObjectOfType<AlchemyManager>();

                List<GameObject> tilesFound = new List<GameObject>();
                tilesFound = alchemyManager.FindTilesByTag("Tile_Tree");
                if (tilesFound.Count > 0)
                {
                    if (alchemyManager.ModifyTile("Tile_SpecialEvent", "Tile_TreeBranch"))
                    {
                        countUncertainty--;
                    }
                }

                if (soundManager != null)
                {
                    soundManager.PlayDestroySpecial();
                }
            }
            else if (allDots[column, row].tag == "Tile_Tree")
            {
                AlchemyManager alchemyManager = FindObjectOfType<AlchemyManager>();
                if (alchemyManager.ModifyTile("Tile_Light", "Tile_SpecialEvent"))
                {
                    countUncertainty++;
                    countLight--;
                }

                if (soundManager != null)
                {
                    soundManager.PlayDestroySpecial();
                }
            }
            // else if (allDots[column, row].tag == "Tile_NormalEvent")
            // {
            //     battleManager.Restore();
            //     if (soundManager != null)
            //     {
            //         soundManager.PlayDestroySpecial();
            //     }
            // }
            else if (allDots[column, row].tag == "Tile_Light")
            {
                battleManager.ConsumeEnergy(-1);
                if (soundManager != null)
                {
                    soundManager.PlayDestroySpecial();
                }
            }
            else
            {
                if (soundManager != null)
                {
                    soundManager.PlayDestroyNormal();
                }
            }

            Destroy(particle, .5f);
            Destroy(allDots[column, row]);
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i,j] != null)
                {
                    DestroyMatchesAt(i,j);
                }
            }
        }

        AlchemyManager alchemyManager = FindObjectOfType<AlchemyManager>();
        alchemyManager.ResetEventPoints();

        findMatches.currentMatches.Clear();
        StartCoroutine(DecreaseRowCo2());
    }

    private IEnumerator DecreaseRowCo2()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // If the current spot isn't blank or is empty
                if (!blankSpaces[i,j] && allDots[i,j] == null)
                {
                    // Loop from the space above to the top of column
                    for (int k = j+1; k < height; k++)
                    {
                        // If a dot is found
                        if (allDots[i,k] != null)
                        {
                            // Move that dot to this empty space
                            allDots[i,k].GetComponent<Dot>().row = j;
                            // Set that spot to be null
                            allDots[i,k] = null;
                            // Break out of the loop;
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(refillDelay * 0.5f);
        StartCoroutine(FillBoardCo());
    }

    private IEnumerator DecreaseRowCo()
    {
        int nullCount = 0;
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (allDots[i,j] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    allDots[i,j].GetComponent<Dot>().row -= nullCount;
                    allDots[i,j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(refillDelay * 0.5f);
        StartCoroutine(FillBoardCo());
    }

    private void RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i,j] == null && !blankSpaces[i,j])
                {
                    Vector2 tempPosition = new Vector2(i,j+offset);
                    int dotToUse = Random.Range(0, dots.Length);

                    int maxIterations = 0;
                    while (MatchesAt(i,j, dots[dotToUse]))
                    {
                        maxIterations++;
                        dotToUse = Random.Range(0, dots.Length);
                        if (maxIterations > 100)
                        {
                            break;
                        }
                    }

                    GameObject piece = Instantiate(dots[dotToUse],
                      tempPosition, Quaternion.identity);
                    piece.GetComponent<Dot>().row = j;
                    piece.GetComponent<Dot>().column = i;
                    piece.transform.parent = this.transform;
                    piece.name = "( " + i + ", " + j + " )";
                    allDots[i,j] = piece;

                    UpdateTileCounter(piece.tag, 1);

                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
      for (int i = 0; i < width; i++)
      {
          for (int j = 0; j < height; j++)
          {
              if(allDots[i,j] != null)
              {
                  if (allDots[i,j].GetComponent<Dot>().isMatched)
                  {
                      return true;
                  }
              }
          }
      }
      return false;
    }

    private IEnumerator FillBoardCo()
    {
        yield return new WaitForSeconds(refillDelay);
        RefillBoard();

        while(MatchesOnBoard())
        {
            streakValue += 1;
            DestroyMatches(); // this is recursion
            yield return new WaitForSeconds(2 * refillDelay);
        }
        findMatches.currentMatches.Clear();
        currentDot = null;
        yield return new WaitForSeconds(refillDelay);

        if (IsDeadlocked())
        {
            ShuffleBoard();
            Debug.Log("Deadlocked!!!");
        }

        if (streakValue <= 1) // No action for the recursive procedure
        {
            // yield return battleManager.EnemyActionCo();
            AlchemyManager alchemyManager = FindObjectOfType<AlchemyManager>();
            alchemyManager.TriggerSpecialEvent();
        }

        currentState = GameState.move;
        streakValue = 1;
    }

    private void SwitchPieces(int column, int row, Vector2 direction)
    {
        // Take the first piece and save it in a holder
        GameObject holder = allDots[column + (int) direction.x, row + (int) direction.y] as GameObject;
        // Switching the first dot to be the second position
        allDots[column + (int) direction.x, row + (int) direction.y] = allDots[column, row];
        // Set the first dot to be the second dot
        allDots[column, row] = holder;
    }

    private bool CheckForMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i,j] != null)
                {
                    // Make sure that one and two to the right are in the board
                    if (i < width - 2)
                    {
                        // Check if the dots to the right exist
                        if (allDots[i+1,j]!=null && allDots[i+2,j]!=null)
                        {
                            if (allDots[i+1,j].tag == allDots[i,j].tag
                                && allDots[i+2,j].tag == allDots[i,j].tag)
                                {
                                    return true;
                                }
                        }
                    }

                    if (j < height - 2)
                    // Check if the dots above exist
                    if (allDots[i,j+1]!=null && allDots[i,j+2]!=null)
                    {
                        if (allDots[i,j+1].tag == allDots[i,j].tag
                            && allDots[i,j+2].tag == allDots[i,j].tag)
                            {
                                return true;
                            }
                    }
                }
            }
        }
        return false;
    }

    public bool SwitchAndCheck(int column, int row, Vector2 direction)
    {
        SwitchPieces(column, row, direction);
        if (CheckForMatches())
        {
            SwitchPieces(column, row, direction);
            return true;
        }
        SwitchPieces(column, row, direction);
        return false;
    }

    private bool IsDeadlocked()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i,j]!=null)
                {
                     if (i < width - 1)
                     {
                         if (SwitchAndCheck(i,j,Vector2.right))
                         {
                             return false;
                         }
                     }
                     if (j < height - 1)
                     {
                         if (SwitchAndCheck(i,j,Vector2.up))
                         {
                             return false;
                         }
                     }
                }
            }
        }
        return true;
    }

    private void ShuffleBoard()
    {
        // Create a list of game objects
        List<GameObject> newBoard = new List<GameObject>();
        // Add every piece to this list
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i,j] != null)
                {
                    newBoard.Add(allDots[i,j]);
                }
            }
        }
        // For every spot on the board
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // If this spot shouldn't be blank
                if (!blankSpaces[i,j])
                {
                    // pick a random number
                    int pieceToUse = Random.Range(0,newBoard.Count);

                    int maxIterations = 0;
                    while(MatchesAt(i,j,newBoard[pieceToUse]) && maxIterations < 100)
                    {
                        pieceToUse = Random.Range(0, newBoard.Count);
                        maxIterations++;
                        // Debug.Log(maxIterations);
                    }
                    // Make a container for the piece
                    Dot piece = newBoard[pieceToUse].GetComponent<Dot>();
                    // Assign the column/row to the piece
                    piece.column = i;
                    piece.row = j;
                    // Fill in the dots array with this new piece
                    allDots[i,j] = newBoard[pieceToUse];
                    // Remove it from the list
                    newBoard.Remove(newBoard[pieceToUse]);
                }
            }
        }
        // Check if it's still deadlocked
        if (IsDeadlocked())
        {
            ShuffleBoard();
        }
    }

    private void UpdateTileCounter(string thisTag, int delta)
    {
        switch (thisTag)
        {
            case "Tile_SpecialEvent":
                countUncertainty += delta;
                break;
            case "Tile_Light":
                countLight += delta;
                break;
        }
    }
}
