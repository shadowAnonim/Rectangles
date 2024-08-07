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

    public Player CurrentPlayer
    {
        get => players[curPlayerIndex];
    }

    public Vertex StartVertex
    {
        get => startVertex;
        set
        {
            startVertex = value;
            ResetInteractableVertices();

            if (startVertex == null)
            {
                MakeVerticesInteractable(v => v.Player == CurrentPlayer &&
                    v.state == VertexState.Edge);
            }
            else
            {
                MakeVerticesInteractable(v => SelectAvailableVertices().Contains(v));
            }
        }
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
        MakeVerticesInteractable(v => v.Player == CurrentPlayer &&
            v.state == VertexState.Edge);

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

    #region vertices methods
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
    }

    private void MakeVerticesInteractable(System.Func<Vertex, bool> filter)
    {
        Vertex[] vertices = GetAllVertices().Where(filter).ToArray();
        foreach (var vertex in vertices)
            vertex.Interactable = true;
    }

    private List<Vertex> SelectAvailableVertices()
    {
        List<Vertex> availableVertices = new() {startVertex};
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
                        if (vertex.state != VertexState.Inner &&
                            IsRectangleFree(startVertex, vertex))
                            availableVertices.Add(vertex);
                    }
                    catch (System.IndexOutOfRangeException) { }
                }
            curValues = (curValues.Item2, curValues.Item1);
        }

        return availableVertices;
    }
    #endregion

    #region rectangle methods

    public void DrawRectangle(Vertex startVertex, Vertex endVertex)
    {
        gameMode = GameMode.Rolling;

        Vector2 rectPos = (startVertex.transform.position + endVertex.transform.position) / 2;
        GetRectangleWidthHeight(startVertex, endVertex, out int width, out int height);
        GameObject rect = Instantiate(rectanglePrefab, rectPos, Quaternion.identity, gridTransform);

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
        Vector2Int startPos = GetLeftBottomCorner(startVertex, endVertex);
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
                if (IsVertexOnEdgeOfRectangle(vertex, startVertex, endVertex))
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

    private void GetRectangleWidthHeight(Vertex startVertex, Vertex endVertex, out int width, out int height)
    {
        width = Mathf.Abs(startVertex.position.x - endVertex.position.x);
        height = Mathf.Abs(startVertex.position.y - endVertex.position.y);
    }    

    private bool IsVertexOnEdgeOfRectangle(Vertex vertex, Vertex startVertex, Vertex endVertex)
    {
        return vertex.position.x == startVertex.position.x || 
            vertex.position.y == startVertex.position.y ||
            vertex.position.x == endVertex.position.x ||
            vertex.position.y== endVertex.position.y;
    }


    //TODO: заменить методом, который меняет ссылки в правильном порядке
    private Vector2Int GetLeftBottomCorner(Vertex startVertex,  Vertex endVertex)
    {
        return new Vector2Int(
            Mathf.Min(startVertex.position.x, endVertex.position.x), 
            Mathf.Min(startVertex.position.y, endVertex.position.y));
    }

    private bool IsRectangleFree(Vertex startVertex, Vertex endVertex)
    {
        Vector2Int startPos = GetLeftBottomCorner(startVertex, endVertex);
        GetRectangleWidthHeight(startVertex, endVertex, out int width, out int height);
        for (int i = 0; i <= width; i++)
            for (int j = 0; j <= height; j++)
            {
                Vertex vertex = grid[startPos.x + i, startPos.y + j];
                if (vertex.state == VertexState.Inner ||
                    vertex.state == VertexState.Edge && !IsVertexOnEdgeOfRectangle(vertex, startVertex, endVertex))
                    return false;
            }
        return true;
    }
    #endregion
}
