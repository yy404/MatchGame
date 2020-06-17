using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageManager : MonoBehaviour
{

    public Text damageText;
    public int totalDamage;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        damageText.text = "" + totalDamage; // .ToString()
    }

    public void DealDamage(int damageAmount)
    {
        totalDamage += damageAmount;
    }
}
