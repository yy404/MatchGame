using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlchemyManager : MonoBehaviour
{
    public Sprite newSprite;
    private Board board;
    private int eventPoints;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        eventPoints = 0;
    }

    // // Update is called once per frame
    // void Update()
    // {
    // }

    List<GameObject> FindTilesByTag(string thisTag)
    {
        List<GameObject> tilesFound = new List<GameObject>();
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.allDots[i,j]!=null)
                {
                     if (board.allDots[i,j].tag == thisTag)
                     {
                        tilesFound.Add(board.allDots[i,j]);
                     }
                }
            }
        }

        return tilesFound;
    }

    GameObject PickOneRandomly()
    {
        List<GameObject> tilesFound = new List<GameObject>();
        tilesFound = FindTilesByTag("Tile_Tree");
        if (tilesFound.Count > 0)
        {
            int pieceToUse = Random.Range(0,tilesFound.Count);
            return tilesFound[pieceToUse];
        }
        return null;
    }

    public void ModifyTile()
    {
        eventPoints++;
        if (eventPoints >= 3)
        {
            GameObject thisTile = PickOneRandomly();
            if (thisTile != null)
            {
                thisTile.GetComponent<SpriteRenderer>().sprite = newSprite;
                thisTile.tag = "Tile_TreeBranch";
            }
            eventPoints = 0;
        }
    }
}
