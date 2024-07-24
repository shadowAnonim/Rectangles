using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController S;

    public int[,] grid;
    public (int, int) curValues;

    [SerializeField]
    private Transform gridTransform;
    [SerializeField]
    private GameObject vertexPrefab;

    private void Awake()
    {
        if (S == null)
            S = this;
        else
            Debug.LogError("Second instance of Game controller!");
    }

    private void Start()
    {
        CreateGrid(16, 16);
    }

    public (int, int) RollDice()
    {
        curValues = (Random.Range(1, 7), Random.Range(1, 7));
        return curValues;
    }

    public void CreateGrid(int width, int height)
    {
        Camera.main.orthographicSize = Mathf.Max(height, width / Camera.main.aspect) / 2 * 1.1f;
        print(Camera.main.aspect);
        Vector2 startPosition = new Vector2(-width / 2, -height / 2);
        grid = new int[width, height];
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
            {
                Instantiate(vertexPrefab, startPosition + new Vector2(i, j), Quaternion.identity, gridTransform);
            }
    }
}
