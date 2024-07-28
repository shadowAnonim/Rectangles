using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex : MonoBehaviour
{
    public Vector2 position;

    
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

        if (GameController.S.startVertex == null)
            GameController.S.startVertex = this;
        else
        {
            Vertex startVertex = GameController.S.startVertex;
            GameController.S.DrawRectangle(
                (startVertex.transform.position + transform.position) / 2,
                Mathf.Abs((startVertex.position - position).x),
                Mathf.Abs((startVertex.position - position).y));
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
