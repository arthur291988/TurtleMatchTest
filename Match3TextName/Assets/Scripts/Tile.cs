using System;
using UnityEngine;

public class Tile : MonoBehaviour
{

    //private static Tile selected; // 1 
    private SpriteRenderer Renderer; // 2
    //[NonSerialized]
    public Vector2 Position;
    //[NonSerialized]
    public Vector2 MoveToPosition;

    [NonSerialized]
    public Transform _transform;
    private Vector2 touchStartPos;
    private Vector2 touchDragPos;

    private float maxSwipeDistance = 0.9f;
    private float maxAxeDeviation = 0.4f;

    [NonSerialized]
    public GameObject _gameObject;

    //[NonSerialized]
    public int row;
    //[NonSerialized]
    public int column;

    private bool swipeSessionStarted;


    [NonSerialized]
    public SpriteRenderer _spriteRenderer;

    [NonSerialized]
    public bool isMoving;

    [NonSerialized]
    public bool isMatched;

    [NonSerialized]
    public int spriteNumber;

    [NonSerialized]
    public bool isControlTile;

    [NonSerialized]
    public bool toRemove;

    private float moveSpeed;

    //private void OnEnable()
    //{
    //    isMoving=false;
    //    isMatched=false;
    //}


    private void OnEnable()
    {
        toRemove = false;
        moveSpeed = 0.15f; // 0.15
        Renderer = GetComponent<SpriteRenderer>();
        _transform = transform;
        _gameObject = _transform.gameObject;
        swipeSessionStarted = false;
        isMatched = false;
    }


    public void Select() // 4
    {
        Renderer.color = Color.grey;
    }

    public void Unselect() // 5 
    {
        Renderer.color = Color.white;
    }

    public void DisactivateTile()
    {
        isMoving = false;
        isControlTile = false;
        _gameObject.SetActive(false);
    }

    private void OnMouseDown() //6
    {
        touchStartPos = CommonData.Instance._camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        swipeSessionStarted = true;
    }

    private void OnMouseDrag()
    {
        if (!GridManager.Instance.isSwiping && swipeSessionStarted)
        {
            touchDragPos = CommonData.Instance._camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            if ((touchDragPos - touchStartPos).magnitude > maxSwipeDistance)
            {
                if (Mathf.Abs(touchDragPos.y - touchStartPos.y) < maxAxeDeviation)
                {
                    if (touchDragPos.x < touchStartPos.x)
                    {
                        //GridManager.Instance.SwapTiles(new Vector2Int((int)(Position.x - GridManager.Instance.Distance),Position.y), Position);


                        GridManager.Instance.swipeAnimation(this, GridManager.Instance.Grid[column - 1, row]);

                    }
                    if (touchDragPos.x > touchStartPos.x)
                    {
                        //GridManager.Instance.SwapTiles(new Vector2Int((int)(Position.x + GridManager.Instance.Distance), Position.y), Position);

                        GridManager.Instance.swipeAnimation(this, GridManager.Instance.Grid[column + 1, row]);
                    }
                    swipeSessionStarted = false;

                }
                if (Mathf.Abs(touchDragPos.x - touchStartPos.x) < maxAxeDeviation)
                {


                    if (touchDragPos.y < touchStartPos.y)
                    {
                        //GridManager.Instance.SwapTiles(new Vector2Int(Position.x, (int)(Position.y - GridManager.Instance.Distance)), Position);

                        GridManager.Instance.swipeAnimation(this, GridManager.Instance.Grid[column, row - 1]);
                    }
                    if (touchDragPos.y > touchStartPos.y)
                    {
                        //GridManager.Instance.SwapTiles(new Vector2Int(Position.x, (int)(Position.y + GridManager.Instance.Distance)), Position);


                        GridManager.Instance.swipeAnimation(this, GridManager.Instance.Grid[column, row + 1]);
                    }
                    swipeSessionStarted = false;
                }
            }
        }
    }

    public void moveTo()
    {
        isMoving = true;
    }

    void Update()
    {
        if (isMoving)
        {
            _transform.position = Vector2.Lerp(_transform.position, MoveToPosition, moveSpeed);
            if (((Vector2)_transform.position - MoveToPosition).magnitude < 0.15f)
            {
                _transform.position = MoveToPosition;
                Position = MoveToPosition;
                isMoving = false;
                if (isControlTile)
                {
                    isControlTile = false;
                    GridManager.Instance.checkMatchesAfterTilesMoveStopped();
                }
            }
        }
    }

}
