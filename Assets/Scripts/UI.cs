using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static UI S;

    private Animator diceAnimator;
    [SerializeField]
    private Sprite[] diceSprites;

    public Button[] dice;
    public Button diceButton;

    public TMP_Text[] playerPointsTexts;
    public TMP_Text currentPlayerText;

    private void Awake()
    {
        if (S == null)
            S = this;
        else
            Debug.LogError("Second instance of \"UI\" class!");

        diceAnimator = diceButton.transform.parent.GetComponent<Animator>();
    }

    public void UpdateUI()
    {
        var players = GameController.S.players;
        for (int i = 0; i < players.Length; i++)
            playerPointsTexts[i].text = $"{players[i].Name}: {players[i].points}";
        currentPlayerText.text = $"Ходит {players[GameController.S.curPlayerIndex].Name}";
        currentPlayerText.color = players[GameController.S.curPlayerIndex].Color;
    }

    public void HideDice()
    {
        diceAnimator.SetBool("Hidden", true);
    }

    public void ShowDice()
    {
        diceAnimator.SetBool("Hidden", false);
    }

    public void RollBtnClick()
    {
        StartCoroutine(AnimateDice());
    }

    private IEnumerator AnimateDice()
    {
        diceButton.interactable = false;
        for (int i = 0; i < 10; i++)
        {
            dice[0].image.sprite = diceSprites[Random.Range(1, 7) - 1];
            dice[1].image.sprite = diceSprites[Random.Range(1, 7) - 1];
            yield return new WaitForSeconds(0.1f);

        }
        var values = GameController.S.RollDice();
        dice[0].image.sprite = diceSprites[values.Item1 - 1];
        dice[1].image.sprite = diceSprites[values.Item2 - 1];
        HideDice();
        GameController.S.gameMode = GameMode.Drawing;
        diceButton.interactable = true;
    }
}
