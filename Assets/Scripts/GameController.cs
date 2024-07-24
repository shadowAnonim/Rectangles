using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController S;

    private (int, int) curValues;

    private void Awake()
    {
        if (S == null)
            S = this;
        else
            Debug.LogError("Second instance of Game controller!");
    }

    public (int, int) RollDice()
    {
        curValues = (Random.Range(1, 7), Random.Range(1, 7));
        return curValues;
    }
}
