using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D defaultCursor;
    public Texture2D clickedCursor;

    private Vector2 _defaultSpot = new Vector2(65, 40);
    // private Vector2 _clickedSpot = new Vector2(40, 55);

    void Start()
    {
        defaultCursor = Resources.Load<Texture2D>("Prefabs/UI/Cursor/default_cursor");
        clickedCursor = Resources.Load<Texture2D>("Prefabs/UI/Cursor/clicked_cursor");
        // Debug.Log(defaultCursor.width);
        // Debug.Log(defaultCursor.height);

        Cursor.SetCursor(defaultCursor, _defaultSpot, CursorMode.Auto);

    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Cursor.SetCursor(clickedCursor, _defaultSpot, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(defaultCursor, _defaultSpot, CursorMode.Auto);
        }
    }

    // public void OnMouseUp()
    // {
    //     Cursor.SetCursor(defaultCursor, new Vector2(0, 0), CursorMode.Auto);
    // }

    // public void OnMouseDown()
    // {
    //     Cursor.SetCursor(clickedCursor, new Vector2(0, 0), CursorMode.Auto);
    // }
}
