using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex : MonoBehaviour
{
    public Vector2 position;

    private void OnMouseEnter()
    {
        transform.localScale *= 1.5f;
    }

    private void OnMouseExit()
    {
        transform.localScale /= 1.5f;
    }

    private void OnMouseUpAsButton()
    {
        print(GameController.S.startPoint + " " +  position);
        if (GameController.S.startPoint is null)
            GameController.S.startPoint = position;
        else
        {
            Vector2 startPoint = GameController.S.startPoint.Value;
            GameController.S.DrawRectangle(
                (startPoint + position)/2, 
                Mathf.Abs((startPoint - position).x), 
                Mathf.Abs((startPoint - position).y));
        }
    }
}
