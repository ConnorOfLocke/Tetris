using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderboardUI_Entry : MonoBehaviour
{
    [SerializeField]
    private TMP_Text displayNameText;

    [SerializeField]
    private TMP_Text scoreText;

    [SerializeField]
    private TMP_Text placingText;


    public void Initialise(int placing, string displayName, string score)
    {
        placingText.text = placing.ToString();
        displayNameText.text = displayName;
        scoreText.text = score;
    }


}
