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

    public Vertex[,] grid;
    public (int, int) curValues;
    public int gridWidth, gridHeight;
    private Vertex startVertex = null;

    public Vertex StartVertex
    {
        get => startVertex;
        set
        {
            startVertex = value;
            ResetInteractableVertices();
            if (startVertex != null)
            {
                var vertices = SelectAvailableVertices();
                foreach (var vertex in vertices)
                    vertex.Interactable = true;
            }
        }
    }

    public Player CurrentPlayer
    {
        get => players[curPlayerIndex];
    }

    private void Awake()
    {
        if (S == null)
            S = this;
        else
            Debug.LogError("Second instance of \"Game controller\" class!");
    }

    private void Start()
    {
        CreateGrid(gridWidth, gridHeight);
        for (int i = 0; i < players.Length; i++)
            UI.S.playerPointsTexts[i].color = players[i].Color;
        NextTurn();
        UI.S.UpdateUI();
    }

    private void NextTurn()
    {
        startVertex = null;

        curPlayerIndex = (curPlayerIndex + 1) % (players.Length);
        ResetInteractableVertices();

        UI.S.UpdateUI();
        UI.S.ShowDice();
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
        grid = new Vertex[width, height];
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
            {
                var vertex = Instantiate(vertexPrefab, startPosition + new Vector2(i, j),
                    Quaternion.identity, gridTransform).GetComponent<Vertex>();
                vertex.position = new Vector2Int(i, j);
                vertex.Interactable = false;
                grid[i, j] = vertex;
            }

        Vertex cornerVertex = grid[0, height - 1];
        cornerVertex.Player = players[0];
        cornerVertex.state = VertexState.Edge;
        cornerVertex = grid[width - 1, 0];
        cornerVertex.Player = players[1];
        cornerVertex.state = VertexState.Edge;
    }

    #region vertices functions
    private Vertex[] GetAllVertices()
    {
        Vertex[] vertices = new Vertex[gridWidth * gridHeight];
        for (int i = 0; i < gridWidth;i++)
           for (int j = 0; j < gridHeight;j++)
                vertices[i * gridWidth + j] = grid[i,j];
        return vertices;
    }

    private int CountAdjacentFreeVertices(Vector2Int position)
    {
        int cnt = 0;
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;
                if (position.x + i >= 0 && position.x + i < gridWidth &&
                    position.y + j >= 0 && position.y + j < gridHeight)
                    if (grid[position.x + i, position.y + j].Player is null)
                        cnt++;
            }
        return cnt;
    }

    private void ResetInteractableVertices()
    {
        Vertex[] vertices = GetAllVertices().Where(v => v.Interactable).ToArray();
        foreach (var vertex in vertices)
            vertex.Interactable = false;

        vertices = GetAllVertices().Where(v => v.Player == CurrentPlayer &&
            v.state == VertexState.Edge).ToArray();
        foreach (var vertex in vertices)
            vertex.Interactable = true;
    }

    private List<Vertex> SelectAvailableVertices()
    {
        List<Vertex> availableVertices = new();
        Vertex vertex;
        int[] arr = { -1, 1 };
        for (int _ = 0; _ < 2; _++)
        {
            foreach (int i in arr)
                foreach (int j in arr)
                {
                    try
                    {
                        vertex = grid[startVertex.position.x + i * curValues.Item1,
                            startVertex.position.y + j * curValues.Item2];
                        if (vertex.state == VertexState.Free &&
                            IsRectangleFree((startVertex.transform.position + vertex.transform.position) / 2,
                                Mathf.Abs((startVertex.position - vertex.position).x),
                                Mathf.Abs((startVertex.position - vertex.position).y)))
                            availableVertices.Add(vertex);
                    }
                    catch (System.IndexOutOfRangeException) { }
                }
            curValues = (curValues.Item2, curValues.Item1);
        }

        return availableVertices;
    }
    #endregion

    #region rectangle functions

    public void DrawRectangle(Vector2 position, float width, float height)
    {
        gameMode = GameMode.Rolling;

        GameObject rect = Instantiate(rectanglePrefab, position, Quaternion.identity, gridTransform);

        Transform top, bottom, left, right;
        top = rect.transform.Find("Top");
        bottom = rect.transform.Find("Bottom");
        left = rect.transform.Find("Left");
        right = rect.transform.Find("Right");

        top.GetComponent<Renderer>().material.color = CurrentPlayer.Color;
        top.localScale = new Vector3(width, top.localScale.y);
        top.localPosition = Vector3.up * height / 2;

        bottom.GetComponent<Renderer>().material.color = CurrentPlayer.Color;
        bottom.localScale = new Vector3(width, bottom.localScale.y);
        bottom.localPosition = Vector3.down * height / 2;

        left.GetComponent<Renderer>().material.color = CurrentPlayer.Color;
        left.localScale = new Vector3(left.localScale.x, height);
        left.localPosition = Vector3.left * width / 2;

        right.GetComponent<Renderer>().material.color = CurrentPlayer.Color;
        right.localScale = new Vector3(right.localScale.x, height);
        right.localPosition = Vector3.right * width / 2;

        CurrentPlayer.points += (int)(width * height);
        Vector2Int startPos = GetStartCorner(position, width, height);
        Vertex vertex;
        for (int i = 0; i <= width; i++)
            for (int j = 0; j <= height; j++)
            {
                vertex = grid[startPos.x + i, startPos.y + j];
                vertex.Player = CurrentPlayer;
            }
        for (int i = 0; i <= width; i++)
            for (int j = 0; j <= height; j++)
            {
                vertex = grid[startPos.x + i, startPos.y + j];
                if (i == 0 || j == 0 || i == width || j == height)
                {
                    if (CountAdjacentFreeVertices(startPos + new Vector2Int(i, j)) != 0)
                        vertex.state = VertexState.Edge;
                    else
                        vertex.state = VertexState.Inner;
                }
                else
                    vertex.state = VertexState.Inner;
            }

        NextTurn();
    }

    private Vector2Int GetStartCorner(Vector2 position, float width, float height)
    {
        return startVertex.position + Vector2Int.RoundToInt(new Vector2(
            (position.x < startVertex.transform.position.x ? -width : 0),
            (position.y < startVertex.transform.position.y ? -height : 0)));
    }

    private bool IsRectangleFree(Vector2 position, int width, int height)
    {
        Vector2Int startPos = GetStartCorner(position, width, height);
        for (int i = 0; i <= width; i++)
            for (int j = 0; j <= height; j++)
            {
                if (grid[startPos.x + i, startPos.y + j].state == VertexState.Inner)
                    return false;
            }
        return true;
    }
    #endregion
}
