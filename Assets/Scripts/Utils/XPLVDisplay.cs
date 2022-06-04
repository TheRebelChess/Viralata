using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class XPLVDisplay : MonoBehaviour
{

    public TMP_Text xpText;
    public TMP_Text lvText;
    public TMP_Text statsText;

    public PlayerMovement player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        xpText.text = "Experiência: " + player.currentXp.ToString();
        lvText.text = "Nível: " + player.playerLevel.ToString();
        statsText.text = "Força: " + player.strength.ToString();
    }
}
