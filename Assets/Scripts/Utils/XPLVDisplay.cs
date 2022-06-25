using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XPLVDisplay : MonoBehaviour
{

    public Text xpText;
    public Text lvText;
    public TMP_Text statsText;

    public PlayerMovement player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        xpText.text = "XP: " + player.currentXp.ToString();
        lvText.text = "Level: " + player.playerLevel.ToString();
        //statsText.text = "Força: " + player.strength.ToString();
    }
}
