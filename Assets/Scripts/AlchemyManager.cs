using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlchemyManager : MonoBehaviour
{
    public Sprite spriteTreeBranch;
    public Sprite spriteSpecialEvent;
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

    GameObject PickOneRandomly(string thisTag)
    {
        List<GameObject> tilesFound = new List<GameObject>();
        tilesFound = FindTilesByTag(thisTag);
        if (tilesFound.Count > 0)
        {
            int pieceToUse = Random.Range(0,tilesFound.Count);
            return tilesFound[pieceToUse];
        }
        return null;
    }

    public void ModifyTile(string thisTag, string thatTag)
    {
        // eventPoints++;
        if (true) //(eventPoints >= 3)
        {
            GameObject thisTile = PickOneRandomly(thisTag);
            if (thisTile != null)
            {
                SpriteRenderer rend = thisTile.GetComponent<SpriteRenderer>();

                if (thatTag == "Tile_TreeBranch")
                {
                    rend.sprite = spriteTreeBranch;
                    // rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, 0.5f);
                    thisTile.tag = "Tile_TreeBranch";
                }
                if (thatTag == "Tile_SpecialEvent")
                {
                    rend.sprite = spriteSpecialEvent;
                    rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, 0.2f);
                    thisTile.tag = "Tile_SpecialEvent";
                }
            }
        }
    }

    public void ResetEventPoints()
    {
        eventPoints = 0;
    }

    public void TriggerSpecialEvent()
    {
        if (board.countUncertainty >= 6)
        {
            ModifyTile("Tile_SpecialEvent", "Tile_TreeBranch");
            board.countUncertainty--;
        }
    }
}
