using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

}
