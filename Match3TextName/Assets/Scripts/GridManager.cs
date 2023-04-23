using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;

public class GridManager : MonoBehaviour
{
    public List<Sprite> Sprites = new List<Sprite>();
    public GameObject TilePrefab;
    public int GridDimension = 8;
    public float Distance = 1.1f;
    [NonSerialized]
    //public GameObject[,] Grid;
    public Tile[,] Grid;

    private Transform selectedTileTransform;
    private Transform moveToTileTransform;
    private Vector2 selectedTilePos;
    private Vector2 moveToTilePos;
    private Tile selectedTile;
    private Tile moveToTile;

    [NonSerialized]
    public bool isSwiping;
    private bool isSwipingBack;

    public static GridManager Instance { get; private set; }


    void Awake() { Instance = this; }

    // Start is called before the first frame update
    void Start()
    {
        Grid = new Tile[GridDimension, GridDimension];
        InitGrid();
        isSwiping = false;
        isSwipingBack = false;
    }

    //populating initial grids on scene
    private void InitGrid()
    {
        Vector3 positionOffset = transform.position - new Vector3(GridDimension /** Distance*/ / 2.0f, GridDimension /** Distance*/ / 2.0f, 0); // 1


        for (int row = 0; row < GridDimension; row++)
        {
            for (int column = 0; column < GridDimension; column++) // 2
            {
                List<Sprite> possibleSprites = new List<Sprite>(Sprites); // 1

                //Choose what sprite to use for this cell
                Sprite left1 = GetSpriteAt(column - 1, row); //2
                Sprite left2 = GetSpriteAt(column - 2, row);
                if (left2 != null && left1 == left2) // 3
                {
                    possibleSprites.Remove(left1); // 4
                }

                Sprite down1 = GetSpriteAt(column, row - 1); // 5
                Sprite down2 = GetSpriteAt(column, row - 2);
                if (down2 != null && down1 == down2)
                {
                    possibleSprites.Remove(down1);
                }

                GameObject newTile = Instantiate(TilePrefab); // 3
                SpriteRenderer renderer = newTile.GetComponent<SpriteRenderer>(); // 4
                renderer.sprite = possibleSprites[UnityEngine.Random.Range(0, possibleSprites.Count)]; // 5
                newTile.transform.parent = transform; // 6
                newTile.transform.position = new Vector3(column * Distance, row * Distance, 0) + positionOffset; // 7
                newTile.transform.parent = transform;
                Tile tile = newTile.GetComponent<Tile>();
                tile.Position = new Vector2Int(column, row);
                tile.row = row;
                tile.column = column;
                tile._spriteRenderer = renderer;

                Grid[column, row] = tile; // 8
            }
        }
    }


    public void SwapTiles(Vector2 tile1Position, Vector2 tile2Position) // 1
    {

        // 2
        Tile tile1 = Grid[(int)tile1Position.x, (int)tile1Position.y];
        SpriteRenderer renderer1 = tile1._spriteRenderer;

        Tile tile2 = Grid[(int)tile2Position.x, (int)tile2Position.y];
        SpriteRenderer renderer2 = tile2._spriteRenderer;

        //3
        Sprite temp = renderer1.sprite;
        renderer1.sprite = renderer2.sprite;
        renderer2.sprite = temp;

        isSwiping = false;


        bool changesOccurs = CheckMatches();
        if (!changesOccurs)
        {
            temp = renderer1.sprite;
            renderer1.sprite = renderer2.sprite;
            renderer2.sprite = temp;
            isSwipingBack = true;
        }
        else
        {
            do
            {
                FillHoles();
            } while (CheckMatches());
        }
    }

    private Sprite GetSpriteAt(int column, int row)
    {
        if (column < 0 || column >= GridDimension
            || row < 0 || row >= GridDimension)
            return null;
        Tile tile = Grid[column, row];
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        return renderer.sprite;
    }

    SpriteRenderer GetSpriteRendererAt(int column, int row)
    {
        if (column < 0 || column >= GridDimension
             || row < 0 || row >= GridDimension)
            return null;
        Tile tile = Grid[column, row];
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        return renderer;
    }

    bool CheckMatches()
    {
        HashSet <SpriteRenderer> matchedTiles = new HashSet <SpriteRenderer>(); // 1
        for (int row = 0; row < GridDimension; row++)
        {
            for (int column = 0; column < GridDimension; column++) // 2
            {
                SpriteRenderer current = GetSpriteRendererAt(column, row); // 3

                List<SpriteRenderer> horizontalMatches = FindColumnMatchForTile(column, row, current.sprite); // 4
                if (horizontalMatches.Count >= 2)
                {
                    matchedTiles.UnionWith(horizontalMatches);
                    matchedTiles.Add(current); // 5
                }

                List<SpriteRenderer> verticalMatches = FindRowMatchForTile(column, row, current.sprite); // 6
                if (verticalMatches.Count >= 2)
                {
                    matchedTiles.UnionWith(verticalMatches);
                    matchedTiles.Add(current);
                }
            }
        }

        foreach (SpriteRenderer renderer in matchedTiles) // 7
        {
            renderer.sprite = null;
        }
        return matchedTiles.Count > 0; // 8
    }

    List <SpriteRenderer> FindColumnMatchForTile(int col, int row, Sprite sprite)
    {
        List<SpriteRenderer> result = new List<SpriteRenderer>();
        for (int i = col + 1; i < GridDimension; i++)
        {
            SpriteRenderer nextColumn = GetSpriteRendererAt(i, row);
            if (nextColumn.sprite != sprite)
            {
                break;
            }
            result.Add(nextColumn);
        }
        return result;
    }

    List <SpriteRenderer> FindRowMatchForTile(int col, int row, Sprite sprite)
    {
        List<SpriteRenderer> result = new List<SpriteRenderer> ();
        for (int i = row + 1; i < GridDimension; i++)
        {
            SpriteRenderer nextRow = GetSpriteRendererAt(col, i);
            if (nextRow.sprite != sprite)
            {
                break;
            }
            result.Add(nextRow);
        }
        return result;
    }

    void FillHoles()
    {
        for (int column = 0; column < GridDimension; column++)
        {
            for (int row = 0; row < GridDimension; row++) // 1
            {
                while (GetSpriteRendererAt(column, row).sprite == null) // 2
                {
                    for (int filler = row; filler < GridDimension - 1; filler++) // 3
                    {
                        SpriteRenderer current = GetSpriteRendererAt(column, filler); // 4
                        SpriteRenderer next = GetSpriteRendererAt(column, filler + 1);
                        current.sprite = next.sprite;
                    }
                    SpriteRenderer last = GetSpriteRendererAt(column, GridDimension - 1);
                    last.sprite = Sprites[UnityEngine.Random.Range(0, Sprites.Count)]; // 5
                }
            }
        }
    }

    public void swipeAnimation(Tile selectedTile, Tile moveToTile) {
        selectedTileTransform = selectedTile._transform;
        moveToTileTransform = moveToTile._transform;
        selectedTilePos = selectedTileTransform.position;
        moveToTilePos = moveToTileTransform.position;
        this.selectedTile = selectedTile;
        this.moveToTile = moveToTile;
        isSwiping = true;
    }

    private void changeTilesOnGrid(Tile tile1, Tile tile2)
    {
        Grid[tile1.column, tile1.row] = tile2;
        Grid[tile2.column, tile2.row] = tile1;
        int tempColumn = tile1.column;
        int tempRow = tile1.row;
        tile1.column = tile2.column;
        tile1.row = tile2.row;
        tile2.column = tempColumn;
        tile2.row = tempRow;
    }

    // Update is called once per frame
    void Update()
    {
        if (isSwiping) {
            selectedTileTransform.position = Vector2.Lerp(selectedTileTransform.position, moveToTilePos, 0.2f);
            moveToTileTransform.position = Vector2.Lerp(moveToTileTransform.position, selectedTilePos, 0.2f);
            if (((Vector2)selectedTileTransform.position - moveToTilePos).magnitude < 0.15f) {

                changeTilesOnGrid(selectedTile, moveToTile);
                if (CheckMatches()) SwapTiles(moveToTilePos, selectedTilePos);
                else {
                    isSwiping = false;
                    isSwipingBack = true;
                    changeTilesOnGrid(selectedTile, moveToTile);
                }
            }
        }

        if (isSwipingBack)
        {
            selectedTileTransform.position = Vector2.Lerp(selectedTileTransform.position, selectedTilePos, 0.2f);
            moveToTileTransform.position = Vector2.Lerp(moveToTileTransform.position, moveToTilePos, 0.2f);

            if (((Vector2)selectedTileTransform.position - selectedTilePos).magnitude < 0.15f) {
                isSwipingBack = false;
                moveToTileTransform.position = moveToTilePos;
                selectedTileTransform.position = selectedTilePos;
            }
        }
    }
}
