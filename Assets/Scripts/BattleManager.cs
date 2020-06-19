using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public int totalHealthEnemy;
    public int totalHealthPlayer;
    public Text healthTextEnemy;
    public Text healthTextPlayer;
    public Image healthBarEnemy;
    public Image healthBarPlayer;

    public GameObject tilePrefab;
    public GameObject placeHolderEnemy;

    private Board board;
    private int currHealthEnemy;
    private int currHealthPlayer;
    private float enemyActionDelay = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();

        currHealthEnemy = totalHealthEnemy;
        currHealthPlayer = totalHealthPlayer;
    }

    // Update is called once per frame
    void Update()
    {
        healthTextEnemy.text = "Enemy health: " + currHealthEnemy; // .ToString()
        healthTextPlayer.text = "Health: " + currHealthPlayer;
    }

    public void DamageEnemy(int damageAmount)
    {
        currHealthEnemy -= damageAmount;
        if (board != null && healthBarEnemy != null)
        {
            healthBarEnemy.fillAmount = (float) currHealthEnemy / (float) totalHealthEnemy;
        }
    }
    public void DamagePlayer(int damageAmount)
    {
        currHealthPlayer -= damageAmount;
        if (board != null && healthBarPlayer != null)
        {
            healthBarPlayer.fillAmount = (float) currHealthPlayer / (float) totalHealthPlayer;
        }
    }

    public IEnumerator EnemyActionCo()
    {
        int i = Random.Range(0, board.width);
        int j = Random.Range(0, board.height);
        Debug.Log(i+", "+j);
        if (board.allDots[i,j] != null)
        {
            GameObject marker = Instantiate(tilePrefab,
                board.allDots[i,j].transform.position,
                Quaternion.identity);
            SpriteRenderer rend = marker.GetComponent<SpriteRenderer>();
            Color tempColor = Color.black;
            tempColor.a = 0.5f;
            rend.color = tempColor;
            rend.sortingOrder = 1;

            placeHolderEnemy.GetComponent<Image>().sprite
                = board.allDots[i,j].GetComponent<SpriteRenderer>().sprite;
            placeHolderEnemy.SetActive(true);

            yield return new WaitForSeconds(enemyActionDelay);

            if (board.allDots[i,j].tag == "Tile_Heart")
            {
                DamageEnemy(-1);
            }
            else if (board.allDots[i,j].tag == "Tile_Attack")
            {
                DamagePlayer(1);
            }

            Destroy(marker);
            placeHolderEnemy.SetActive(false);
        }

    }
}
