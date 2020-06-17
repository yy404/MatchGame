using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageManager : MonoBehaviour
{

    public Text damageText;
    public int totalDamage;
    public Image healthBar;

    private Board board;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
    }

    // Update is called once per frame
    void Update()
    {
        damageText.text = "" + totalDamage; // .ToString()
    }

    public void DealDamage(int damageAmount)
    {
        totalDamage += damageAmount;
        if (board != null && healthBar != null)
        {
            int length = board.damageGoals.Length;
            healthBar.fillAmount = 1.0f - ((float) totalDamage / (float) board.damageGoals[length-1]);
            Debug.Log(healthBar.fillAmount);
        }
    }
}
