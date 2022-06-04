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
        xpText.text = "Experi�ncia: " + player.currentXp.ToString();
        lvText.text = "N�vel: " + player.playerLevel.ToString();
        statsText.text = "For�a: " + player.strength.ToString();
    }
}
