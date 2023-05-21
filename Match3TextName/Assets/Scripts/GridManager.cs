using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;
public class GridManager : MonoBehaviour
{
    [HideInInspector]
    public GameObject ObjectPulled;
    [HideInInspector]
    public List<GameObject> ObjectPulledList;


    //public List<Sprite> Sprites = new List<Sprite>();
    [SerializeField]
    private SpriteAtlas spriteAtlass;
    //public GameObject TilePrefab;
    //public int GridDimension = 8;

    [NonSerialized]
    public int GridWidth;
    [NonSerialized]
    public int GridHeight;
    [NonSerialized]
    public float Distance;
    private float topInitPosition;
    private Transform _transform;
    private Vector2 gridBasePosition;

    //public GameObject[,] Grid;
    [NonSerialized]
    public Tile[,] Grid;
    public Vector2[,] GridPositions;

    private Transform selectedTileTransform;
    private Transform moveToTileTransform;
    private Vector2 selectedTilePos;
    private Vector2 moveToTilePos;
    private Tile selectedTile;
    private Tile moveToTile;


    [NonSerialized]
    public List<Tile> columnMatches;
    [NonSerialized]
    public List<Tile> rowMatches;

    [NonSerialized]
    public bool isSwiping;
    [NonSerialized]
    public bool isSwipingBack;

    [NonSerialized]
    public bool tilesAreMoving;

    private bool controlTileIsAssigned;

    //private Tile controlTile;

    public static GridManager Instance { get; private set; }


    void Awake() { Instance = this; 
    }

    // Start is called before the first frame update
    void Start()
    {
        GridWidth = 7;
        GridHeight = 7;
        Distance = 1.4f;
        controlTileIsAssigned = false;
        tilesAreMoving = false;
        _transform = transform;
        gridBasePosition = (Vector2)_transform.position;
        columnMatches = new List<Tile>();
        rowMatches = new List<Tile>();
        Grid = new Tile[GridWidth, GridHeight];
        GridPositions = new Vector2[GridWidth, GridHeight];
        InitGrid();
        isSwiping = false;
        isSwipingBack = false;
    }

    //populating initial grids on scene
    private void InitGrid()
    {
        Vector3 positionOffset = gridBasePosition - new Vector2(GridWidth / 2.0f * Distance - Distance / 2, GridHeight / 2.0f * Distance - Distance / 2); ; // 1


        for (int row = 0; row < GridHeight; row++)
        {
            for (int column = 0; column < GridWidth; column++) // 2
            {
                List<int> possibleSprites = new List<int>() { 1, 2, 3, 4, 5 };

                int left1 = GetSpriteAt(column - 1, row); //2
                int left2 = GetSpriteAt(column - 2, row);
                 
                if (left2 !=0 && left1 == left2) // 3
                {
                    possibleSprites.Remove(left1); // 4
                }
                int down1 = GetSpriteAt(column, row - 1); // 5
                int down2 = GetSpriteAt(column, row - 2);

                if (down2 != 0 && down1 == down2)
                {
                    possibleSprites.Remove(down1);
                }


                ObjectPulledList = ObjectPuller.current.GetTilePullList();
                ObjectPulled = ObjectPuller.current.GetGameObjectFromPull(ObjectPulledList);
                SpriteRenderer renderer = ObjectPulled.GetComponent<SpriteRenderer>(); // 4
                int spriteNo = possibleSprites[UnityEngine.Random.Range(0, possibleSprites.Count)];
                renderer.sprite = spriteAtlass.GetSprite(spriteNo.ToString()); // 5

                ObjectPulled.transform.position = new Vector3(column * Distance, row * Distance, 0) + positionOffset; // 7
                Tile tile = ObjectPulled.GetComponent<Tile>();
                tile.Position = ObjectPulled.transform.position;
                tile.MoveToPosition = tile.Position;
                tile.row = row;
                tile.column = column;
                tile._spriteRenderer = renderer;
                tile.spriteNumber = spriteNo;
                tile.indexOfResource = spriteNo - 1;

                Grid[column, row] = tile;
                GridPositions[column, row] = new Vector2(tile.Position.x, tile.Position.y);

                ObjectPulled.SetActive(true);

            }
        }
        //0 is default value, here can be used any column because we need the tile that higest in row
        topInitPosition = GridPositions[0, GridHeight-1].y+Distance;
    }

    public void SwapTiles() // 1
    {
        getAllMatchedTiles();

    }

    private int GetSpriteAt(int column, int row)
    {
        if (column < 0 || column >= GridWidth
            || row < 0 || row >= GridHeight)
            return 0;
        Tile tile = Grid[column, row];
        return tile.spriteNumber;
    }

    SpriteRenderer GetSpriteRendererAt(int column, int row)
    {
        if (column < 0 || column >= GridWidth
             || row < 0 || row >= GridHeight)
            return null;
        Tile tile = Grid[column, row];
        SpriteRenderer renderer = tile._spriteRenderer;
        return renderer;
    }

    bool CheckMatches()
    {
        HashSet<SpriteRenderer> matchedTiles = new HashSet<SpriteRenderer>(); // 1
        HashSet<Tile> matchedTileTiles = new HashSet<Tile>();

        for (int column = 0; column < GridWidth; column++) // 2
        {
            for (int row = 0; row < GridHeight; row++)
            {
                SpriteRenderer current = GetSpriteRendererAt(column, row); // 3
                Tile currentTile = Grid[column, row];

                List<SpriteRenderer> horizontalMatches = FindColumnMatchForTile(column, row, /*currentTile._spriteRenderer.sprite /*current.sprite*/currentTile.spriteNumber); // 4

                if (horizontalMatches.Count >= 2)
                {
                    matchedTiles.UnionWith(horizontalMatches);
                    matchedTiles.Add(current); // 5

                    matchedTileTiles.UnionWith(columnMatches);
                    matchedTileTiles.Add(currentTile);
                }

                List<SpriteRenderer> verticalMatches = FindRowMatchForTile(column, row, /*currentTile._spriteRenderer.sprite*/ currentTile.spriteNumber); // 6
                if (verticalMatches.Count >= 2)
                {
                    matchedTiles.UnionWith(verticalMatches);
                    matchedTiles.Add(current);

                    matchedTileTiles.UnionWith(rowMatches);
                    matchedTileTiles.Add(currentTile);
                }
            }
        }
        foreach (Tile tile in matchedTileTiles) // 7
        {
            tile.isMatched = true;
        }

        return matchedTileTiles.Count > 0; // 8
    }






    List<SpriteRenderer> FindColumnMatchForTile(int col, int row, int spriteNumber /*Sprite sprite*/)
    {
        List<SpriteRenderer> result = new List<SpriteRenderer>();
        columnMatches.Clear();

        for (int i = col + 1; i < GridWidth; i++)
        {
            SpriteRenderer nextColumn = GetSpriteRendererAt(i, row);

            Tile tile = Grid[i, row];
            int nexColumnSpriteNumber = tile.spriteNumber;
            if (/*nextColumn.sprite != sprite*/nexColumnSpriteNumber!= spriteNumber)
            {
                break;
            }
            result.Add(nextColumn);
            columnMatches.Add(tile);
        }
        //Debug.Log(result.Count);
        return result;
    }

    List<SpriteRenderer> FindRowMatchForTile(int col, int row, int spriteNumber /*Sprite sprite*/)
    {
        List<SpriteRenderer> result = new List<SpriteRenderer>();
        rowMatches.Clear();
        for (int i = row + 1; i < GridHeight; i++)
        {
            SpriteRenderer nextRow = GetSpriteRendererAt(col, i);
            Tile tile = Grid[col, i];
            int nexColumnSpriteNumber = tile.spriteNumber;
            if (/*nextColumn.sprite != sprite*/nexColumnSpriteNumber != spriteNumber)
            {
                break;
            }
            result.Add(nextRow);
            rowMatches.Add(tile);
        }
        return result;
    }


    private void getAllMatchedTiles()
    {
        List<Tile> toDisactivateTiles = new List<Tile>();
        controlTileIsAssigned = false;
        for (int column = 0; column < GridWidth; column++)
        {
            int x = 0;
            for (int row = 0; row < GridHeight; row++) // 1
            {
                while (Grid[column, row].isMatched) // 2
                {
                    if (row < GridHeight - 1)
                    {
                        //going up on column and assign new coordinates to upper tile, wit other words, make coordinates of upper tile equal to current tile
                        for (int filler = row; filler < GridHeight - 1; filler++) // 3
                        {
                            Vector2 moveToPosition = new Vector2(Grid[column, filler].Position.x, Grid[column, filler].Position.y - Distance * x);

                            Tile current = Grid[column, filler];
                            Tile next = Grid[column, filler + 1];

                            next.row = filler;
                            next.MoveToPosition = moveToPosition;


                            Grid[column, filler] = next;

                            if (current.isMatched && !toDisactivateTiles.Contains(current)) toDisactivateTiles.Add(current);


                        }
                        pullNewTile(column, GridHeight - 1, x);

                    }
                    if (row == GridHeight - 1)
                    {
                        Tile current = Grid[column, row];
                        pullNewTile(column, GridHeight - 1, x);
                        if (current.isMatched && !toDisactivateTiles.Contains(current)) toDisactivateTiles.Add(current);
                        //current.DisactivateTile();
                    }

                    x++;
                }
            }
        }


        //StartCoroutine(ResourcesManager.Instance.resourcesCounter(toDisactivateTiles.Count, toDisactivateTiles[0].indexOfResource)); 

        foreach (Tile tile in toDisactivateTiles)
        {
            ResourcesManager.Instance.setResoures(tile.indexOfResource, 1);
            ResourcesManager.Instance.updateResourcesTxt(tile.indexOfResource);


            tile.DisactivateTile();

        }



        foreach (Tile tile in Grid)
        {
            if (tile.Position != tile.MoveToPosition) tile.moveTo();
        }
        tilesAreMoving = true;
    }


    private void pullNewTile(int column, int row, int multiplier)
    {
        ObjectPulledList = ObjectPuller.current.GetTilePullList();
        ObjectPulled = ObjectPuller.current.GetGameObjectFromPull(ObjectPulledList);
        SpriteRenderer renderer = ObjectPulled.GetComponent<SpriteRenderer>();
        int spriteNo = UnityEngine.Random.Range(1, 6);
        renderer.sprite = spriteAtlass.GetSprite(spriteNo.ToString());

        Vector2 position = new Vector2(GridPositions[column, row].x, topInitPosition+ Distance* multiplier);
        ObjectPulled.transform.position = position;
        Tile tile = ObjectPulled.GetComponent<Tile>();
        tile.Position = position;
        tile.MoveToPosition = GridPositions[column,row];
        tile.row = row;
        tile.column = column;
        tile._spriteRenderer = renderer;
        tile.spriteNumber = spriteNo;
        tile.indexOfResource = spriteNo-1;

        Grid[column, row] = tile; // 8
        ObjectPulled.SetActive(true);

        if (!controlTileIsAssigned)
        {
            controlTileIsAssigned = true;
            Grid[column, row].isControlTile = true;
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
        int tile1Column = tile1.column;
        int tile1Row = tile1.row;
        int tile2Column = tile2.column;
        int tile2Row = tile2.row;

        Vector2 tile1Position = tile1.Position;
        Vector2 tile2Position = tile2.Position;

        Tile tile1Temp = Grid[tile1Column, tile1Row];
        Tile tile2Temp = Grid[tile2Column, tile2Row];

        tile1Temp.column = tile2Column;
        tile1Temp.row = tile2Row;
        tile2Temp.column = tile1Column;
        tile2Temp.row = tile1Row;

        tile1Temp.Position = tile2Position;
        tile1Temp.MoveToPosition = tile1Temp.Position;

        tile2Temp.Position = tile1Position;
        tile2Temp.MoveToPosition = tile2Temp.Position;

        Grid[tile1Column, tile1Row] = tile2Temp;
        Grid[tile2Column, tile2Row] = tile1Temp;
    }


    public IEnumerator checkMatchesCoroutine () {
        yield return new WaitForSeconds(0.2f);
        tilesAreMoving = false;
        if (CheckMatches())
        {
            controlTileIsAssigned = false;
            SwapTiles();
        }
    }

    public void checkMatchesAfterTilesMoveStopped() {
        StartCoroutine(checkMatchesCoroutine());
    }



    private void FixedUpdate()
    {
        if (isSwiping)
        {
            selectedTileTransform.position = Vector2.Lerp(selectedTileTransform.position, moveToTilePos, 0.2f);
            moveToTileTransform.position = Vector2.Lerp(moveToTileTransform.position, selectedTilePos, 0.2f);
            if (((Vector2)selectedTileTransform.position - moveToTilePos).magnitude < 0.15f)
            {

                moveToTileTransform.position = selectedTilePos;
                selectedTileTransform.position = moveToTilePos;

                changeTilesOnGrid(selectedTile, moveToTile);
                if (CheckMatches())
                {
                    isSwiping = false;
                    SwapTiles();
                }
                else
                {
                    isSwiping = false;
                    changeTilesOnGrid(selectedTile, moveToTile);
                    isSwipingBack = true;
                }
            }
        }

        if (isSwipingBack)
        {
            selectedTileTransform.position = Vector2.Lerp(selectedTileTransform.position, selectedTilePos, 0.2f);
            moveToTileTransform.position = Vector2.Lerp(moveToTileTransform.position, moveToTilePos, 0.2f);

            if (((Vector2)selectedTileTransform.position - selectedTilePos).magnitude < 0.15f)
            {
                isSwipingBack = false;
                moveToTileTransform.position = moveToTilePos;
                selectedTileTransform.position = selectedTilePos;
            }
        }
    }

}
