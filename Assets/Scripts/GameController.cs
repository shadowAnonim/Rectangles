using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public enum GameMode { Rolling, Drawing }

public class GameController : MonoBehaviour
{
    public static GameController S;

    [SerializeField]
    private Transform gridTransform;
    [SerializeField]
    private GameObject vertexPrefab;
    [SerializeField]
    private GameObject rectanglePrefab;

    public GameMode gameMode = GameMode.Rolling;
    public Player[] players;
    public int curPlayerIndex = -1;

    public Vertex[] grid;
    public (int, int) curValues;
    public Vertex startVertex = null;

    private void Awake()
    {
        if (S == null)
            S = this;
        else
            Debug.LogError("Second instance of \"Game controller\" class!");
    }

    private void Start()
    {
        CreateGrid(32, 32);
        for (int i = 0; i < players.Length; i++)
            UI.S.playerPointsTexts[i].color = players[i].Color;
        NextTurn();
        UI.S.UpdateUI();
    }

    public (int, int) RollDice()
    {
        curValues = (Random.Range(1, 7), Random.Range(1, 7));
        return curValues;
    }

    public void CreateGrid(int width, int height)
    {
        Camera.main.orthographicSize = Mathf.Max(height, width / Camera.main.aspect) / 2 * 1.33f;
        Vector2 startPosition = new(-width / 2, -height / 2);
        grid = new Vertex[width * height];
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
            {
                var vertex = Instantiate(vertexPrefab, startPosition + new Vector2(i, j),
                    Quaternion.identity, gridTransform).GetComponent<Vertex>();
                vertex.position = new Vector2(i, j);
                vertex.Interactable = false;
                grid[i * width + j] = vertex;
            }

        grid.First(v => v.position == new Vector2(0, height - 1)).Player = players[0];
        grid.First(v => v.position == new Vector2(width - 1, 0)).Player = players[1];
    }

    public void DrawRectangle(Vector2 pos, float width, float height)
    {
        gameMode = GameMode.Rolling;

        GameObject rect = Instantiate(rectanglePrefab, pos, Quaternion.identity, gridTransform);

        Transform top, bottom, left, right;
        top = rect.transform.Find("Top");
        bottom = rect.transform.Find("Bottom");
        left = rect.transform.Find("Left");
        right = rect.transform.Find("Right");

        top.GetComponent<Renderer>().material.color = players[curPlayerIndex].Color;
        top.localScale = new Vector3(width, top.localScale.y);
        top.localPosition = Vector3.up * height / 2;

        bottom.GetComponent<Renderer>().material.color = players[curPlayerIndex].Color;
        bottom.localScale = new Vector3(width, bottom.localScale.y);
        bottom.localPosition = Vector3.down * height / 2;

        left.GetComponent<Renderer>().material.color = players[curPlayerIndex].Color;
        left.localScale = new Vector3(left.localScale.x, height);
        left.localPosition = Vector3.left * width / 2;

        right.GetComponent<Renderer>().material.color = players[curPlayerIndex].Color;
        right.localScale = new Vector3(right.localScale.x, height);
        right.localPosition = Vector3.right * width / 2;

        players[curPlayerIndex].points += (int)(width * height);
        NextTurn();
    }

    private void NextTurn()
    {
        startVertex = null;

        if (curPlayerIndex != -1)
            grid.Where(v => v.Player == players[curPlayerIndex]).Select(v => v.Interactable = false).ToList();
        curPlayerIndex = (curPlayerIndex + 1) % (players.Length);
        grid.Where(v => v.Player == players[curPlayerIndex]).Select(v => v.Interactable = true).ToList();

        UI.S.UpdateUI();
        UI.S.ShowDice();
    }
}
