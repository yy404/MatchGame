using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindMatches : MonoBehaviour
{
    private Board board;
    public List<GameObject> currentMatches = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCo());
    }

    private IEnumerator FindAllMatchesCo()
    {
        yield return new WaitForSeconds(.2f);

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                GameObject currentDot = board.allDots[i,j];
                if (currentDot != null)
                {
                    if (i > 0 && i < board.width-1)
                    {
                        GameObject leftDot1 = board.allDots[i-1, j];
                        GameObject rightDot1 = board.allDots[i+1, j];

                        if (leftDot1 != null && rightDot1 != null)
                        {
                            if (leftDot1.tag == currentDot.tag && rightDot1.tag == currentDot.tag)
                            {
                                if (currentDot.GetComponent<Dot>().isRowBomb
                                    || leftDot1.GetComponent<Dot>().isRowBomb
                                    || rightDot1.GetComponent<Dot>().isRowBomb)
                                {
                                    currentMatches.Union(GetRowPieces(j));
                                }

                                if (currentDot.GetComponent<Dot>().isColumnBomb)
                                {
                                    currentMatches.Union(GetColumnPieces(i));
                                }
                                if (leftDot1.GetComponent<Dot>().isColumnBomb)
                                {
                                    currentMatches.Union(GetColumnPieces(i-1));
                                }
                                if (rightDot1.GetComponent<Dot>().isColumnBomb)
                                {
                                    currentMatches.Union(GetColumnPieces(i+1));
                                }

                                if (!currentMatches.Contains(leftDot1))
                                {
                                    currentMatches.Add(leftDot1);
                                }
                                leftDot1.GetComponent<Dot>().isMatched = true;
                                if (!currentMatches.Contains(rightDot1))
                                {
                                    currentMatches.Add(rightDot1);
                                }
                                rightDot1.GetComponent<Dot>().isMatched = true;
                                if (!currentMatches.Contains(currentDot))
                                {
                                    currentMatches.Add(currentDot);
                                }
                                currentDot.GetComponent<Dot>().isMatched = true;
                            }
                        }

                    }
                    if (j > 0 && j < board.height-1)
                    {
                        GameObject upDot1 = board.allDots[i, j+1];
                        GameObject downDot1 = board.allDots[i, j-1];

                        if (upDot1 != null && downDot1 != null)
                        {
                            if (upDot1.tag == currentDot.tag && downDot1.tag == currentDot.tag)
                            {
                                if (currentDot.GetComponent<Dot>().isColumnBomb
                                    || upDot1.GetComponent<Dot>().isColumnBomb
                                    || downDot1.GetComponent<Dot>().isColumnBomb)
                                {
                                    currentMatches.Union(GetColumnPieces(i));
                                }

                                if (currentDot.GetComponent<Dot>().isRowBomb)
                                {
                                    currentMatches.Union(GetRowPieces(j));
                                }
                                if (upDot1.GetComponent<Dot>().isRowBomb)
                                {
                                    currentMatches.Union(GetRowPieces(j+1));
                                }
                                if (downDot1.GetComponent<Dot>().isRowBomb)
                                {
                                    currentMatches.Union(GetRowPieces(j-1));
                                }

                                if (!currentMatches.Contains(upDot1))
                                {
                                    currentMatches.Add(upDot1);
                                }
                                upDot1.GetComponent<Dot>().isMatched = true;
                                if (!currentMatches.Contains(downDot1))
                                {
                                    currentMatches.Add(downDot1);
                                }
                                downDot1.GetComponent<Dot>().isMatched = true;
                                if (!currentMatches.Contains(currentDot))
                                {
                                    currentMatches.Add(currentDot);
                                }
                                currentDot.GetComponent<Dot>().isMatched = true;
                            }
                        }
                    }
                }
            }
        }
    }

    List<GameObject> GetColumnPieces(int column)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < board.height; i++)
        {
            if (board.allDots[column, i] != null)
            {
                dots.Add(board.allDots[column, i]);
                board.allDots[column,i].GetComponent<Dot>().isMatched = true;
            }
        }
        return dots;
    }

    List<GameObject> GetRowPieces(int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < board.width; i++)
        {
            if (board.allDots[i,row] != null)
            {
                dots.Add(board.allDots[i,row]);
                board.allDots[i,row].GetComponent<Dot>().isMatched = true;
            }
        }
        return dots;
    }

    public void CheckBombs()
    {
        // Did the player move something?
        if (board.currentDot != null)
        {
            // Is the piece they moved matched?
            if (board.currentDot.isMatched)
            {
                // Make it unmatched
                board.currentDot.isMatched = false;
                // Decide what kind of bomb to make
                int typeOfBomb = Random.Range(0,100);
                if (typeOfBomb < 50)
                {
                    // Make a row bomb
                    board.currentDot.MakeRowBomb();
                }
                else if (typeOfBomb >= 50)
                {
                    // Make a column bomb
                    board.currentDot.MakeColumnBomb();
                }
            }
            // Is the other piece matched?
            else if (board.currentDot.otherDot != null)
            {
                Dot otherDot = board.currentDot.otherDot.GetComponent<Dot>();
                // Is the other dot matched?
                if (otherDot.isMatched)
                {
                    // Make it unmatched
                    otherDot.isMatched = false;
                    // Decide what kind of bomb to make
                    int typeOfBomb = Random.Range(0,100);
                    if (typeOfBomb < 50)
                    {
                        // Make a row bomb
                        otherDot.MakeRowBomb();
                    }
                    else if (typeOfBomb >= 50)
                    {
                        // Make a column bomb
                        otherDot.MakeColumnBomb();
                    }
                }
            }
        }
    }
}
