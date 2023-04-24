using System;
using UnityEngine;

public class Tile : MonoBehaviour
{

    //private static Tile selected; // 1 
    private SpriteRenderer Renderer; // 2
    [NonSerialized]
    public Vector2Int Position;
    [NonSerialized]
    public Transform _transform;
    private Vector2 touchStartPos;
    private Vector2 touchDragPos;

    private float maxSwipeDistance = 0.9f;
    private float maxAxeDeviation = 0.4f;

    [NonSerialized]
    public GameObject _gameObject;

    [NonSerialized]
    public int row;
    [NonSerialized]
    public int column;

    private bool swipeSessionStarted;

    private Vector2 moveToPos;

    [NonSerialized]
    public SpriteRenderer _spriteRenderer;

    private bool isMoving;

    // Start is called before the first frame update
    void Start()
    {
        Renderer = GetComponent<SpriteRenderer>();
        _transform = transform;
        _gameObject = _transform.gameObject;
        swipeSessionStarted = false;
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
        _gameObject.SetActive(false);
    }

    private void OnMouseDown() //6
    {
        touchStartPos = CommonData.Instance._camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        swipeSessionStarted = true;
        //if (selected != null)
        //{
        //    if (selected == this)
        //        return;
        //    selected.Unselect();
        //    if (Vector2Int.Distance(selected.Position, Position) == 1)
        //    {
        //        GridManager.Instance.SwapTiles(Position, selected.Position);
        //        selected = null;
        //    }
        //    else
        //    {
        //        selected = this;
        //        Select();
        //    }
        //}
        //else
        //{
        //    selected = this;
        //    Select();
        //}
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

    public void moveTo(Vector2 moveToPos, int newColumn, int newRow)
    {
        this.moveToPos = moveToPos;
        isMoving = true;
        row = newRow;
        column = newColumn;
    }

    void Update()
    {
        if (isMoving)
        {
            _transform.position = Vector2.Lerp(_transform.position, moveToPos, 0.2f);
            if (((Vector2)_transform.position - moveToPos).magnitude < 0.15f)
            {
                isMoving = false;
                _transform.position = moveToPos;

            }
        }
    }

}
