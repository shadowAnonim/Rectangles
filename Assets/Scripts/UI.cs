using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public TMP_Text[] diceTexts;
    public Button diceButton;

    public void RollBtnClick()
    {
        StartCoroutine(AnimateDice());
    }

    IEnumerator AnimateDice()
    {
        diceButton.interactable = false;
        for (int i = 0; i < 10; i++)
        {
            diceTexts[0].text = Random.Range(1, 7).ToString();
            diceTexts[1].text = Random.Range(1, 7).ToString();
            yield return new WaitForSeconds(0.3f);

        }
        var values = GameController.S.RollDice();
        diceTexts[0].text = values.Item1.ToString();
        diceTexts[1].text = values.Item2.ToString();
        diceButton.interactable = true;
    }
}
