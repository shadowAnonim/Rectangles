using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VertexState { Free, Edge, Inner}

public class Vertex : MonoBehaviour
{
    public Vector2Int position;
    public VertexState state = VertexState.Free;
    
    private bool interactable;
    private Player player;
    private Vector3 normalScale;
#pragma warning disable CS0108
    private Renderer renderer;
#pragma warning restore
    private Animator animator;

    public Player Player
    {
        get => player;
        set
        {
            player = value;
            renderer.material.color = player.Color;
        }
    }

    public bool Interactable
    {
        get => interactable;
        set
        {
            interactable = value;
            animator.SetBool("Interactable", interactable);
            if (!interactable) ResetScale();
        }
    }

    private void Awake()
    {
        normalScale = transform.localScale;
        renderer = GetComponent<Renderer>();
        animator = GetComponent<Animator>();
    }

    private void OnMouseEnter()
    {
        IncreaseScale();
    }

    private void OnMouseExit()
    {
        ResetScale();
    }

    private void OnMouseUpAsButton()
    {
        if (!interactable)
            return;
        if (GameController.S.gameMode != GameMode.Drawing)
            return;

        Vertex startVertex = GameController.S.StartVertex;
        if (startVertex == null)
        {
            GameController.S.StartVertex = this;
        }
        else if (startVertex == this)
        {
            GameController.S.StartVertex = null;
        }
        else
        {
            GameController.S.DrawRectangle(startVertex, this);
        }
    }

    private void IncreaseScale()
    {
        transform.localScale *= 1.5f;
    }

    private void ResetScale()
    {
        transform.localScale = normalScale;
    }

}
