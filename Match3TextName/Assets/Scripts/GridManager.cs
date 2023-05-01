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
    [HideInInspector]
    public GameObject ObjectPulled;
    [HideInInspector]
    public List<GameObject> ObjectPulledList;


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


    public List<Tile> columnMatches;
    public List<Tile> rowMatches;

    [NonSerialized]
    public bool isSwiping;
    private bool isSwipingBack;


    public static GridManager Instance { get; private set; }


    void Awake() { Instance = this; }

    // Start is called before the first frame update
    void Start()
    {
        columnMatches = new List<Tile>();
        rowMatches = new List<Tile>();
        Grid = new Tile[GridDimension, GridDimension];
        InitGrid();
        isSwiping = false;
        isSwipingBack = false;
    }

    //populating initial grids on scene
    private void InitGrid()
    {
        Vector3 positionOffset = transform.position - new Vector3(GridDimension /** Distance*/ / 2.0f-0.5f+ Distance/2, GridDimension /** Distance*/ / 2.0f, 0); // 1


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


                ObjectPulledList = ObjectPuller.current.GetTilePullList();
                ObjectPulled = ObjectPuller.current.GetGameObjectFromPull(ObjectPulledList);


                SpriteRenderer renderer = ObjectPulled.GetComponent<SpriteRenderer>(); // 4
                renderer.sprite = possibleSprites[UnityEngine.Random.Range(0, possibleSprites.Count)]; // 5
                //ObjectPulled.transform.parent = transform; // 6
                ObjectPulled.transform.position = new Vector3(column * Distance, row * Distance, 0) + positionOffset; // 7
                Tile tile = ObjectPulled.GetComponent<Tile>();
                tile.Position = ObjectPulled.transform.position;
                tile.MoveToPosition = tile.Position;
                tile.row = row;
                tile.column = column;
                tile._spriteRenderer = renderer;

                ObjectPulled.SetActive(true);

                Grid[column, row] = tile; // 8
                //Debug.Log(column+" "+row);
            }
        }
    }

    private void pullNewTile(int column, int row, Vector3 position) {

        ObjectPulledList = ObjectPuller.current.GetTilePullList();
        ObjectPulled = ObjectPuller.current.GetGameObjectFromPull(ObjectPulledList); 
        SpriteRenderer renderer = ObjectPulled.GetComponent<SpriteRenderer>();
        renderer.sprite = Sprites[UnityEngine.Random.Range(0, Sprites.Count)];
        ObjectPulled.transform.position = position; 
        Tile tile = ObjectPulled.GetComponent<Tile>();
        tile.Position = position;
        tile.MoveToPosition = new Vector2 (position.x, position.y-Distance);
        tile.row = row;
        tile.column = column;
        tile._spriteRenderer = renderer;

        ObjectPulled.SetActive(true);

        Grid[column, row] = tile; // 8
    }


    public void SwapTiles(/*Tile tile1, Tile tile2*/) // 1
    {

        //Tile tileThis1 = tile1;
        //SpriteRenderer renderer1 = tileThis1._spriteRenderer;

        //Tile tileThis2 = tile2; 
        //SpriteRenderer renderer2 = tileThis2._spriteRenderer;

        //3
        //Sprite temp = renderer1.sprite;
        //renderer1.sprite = renderer2.sprite;
        //renderer2.sprite = temp;

        isSwiping = false;


        //bool changesOccurs = CheckMatches();
        //if (!changesOccurs)
        //{
        //    temp = renderer1.sprite;
        //    renderer1.sprite = renderer2.sprite;
        //    renderer2.sprite = temp;
        //    isSwipingBack = true;
        //}
        //else
        //{
            do
            {
                getAllMatchedTiles();
            } while (CheckMatches());
        //}
    }

    private Sprite GetSpriteAt(int column, int row)
    {
        if (column < 0 || column >= GridDimension
            || row < 0 || row >= GridDimension)
            return null;
        Tile tile = Grid[column, row];
        SpriteRenderer renderer = tile._spriteRenderer;
        return renderer.sprite;
    }

    SpriteRenderer GetSpriteRendererAt(int column, int row)
    {
        if (column < 0 || column >= GridDimension
             || row < 0 || row >= GridDimension)
            return null;
        Tile tile = Grid[column, row];
        SpriteRenderer renderer = tile._spriteRenderer;
        return renderer;
    }

    bool CheckMatches()
    {
        HashSet <SpriteRenderer> matchedTiles = new HashSet <SpriteRenderer>(); // 1
        HashSet<Tile> matchedTileTiles = new HashSet<Tile>(); 
        for (int row = 0; row < GridDimension; row++)
        {
            for (int column = 0; column < GridDimension; column++) // 2
            {
                SpriteRenderer current = GetSpriteRendererAt(column, row); // 3
                Tile currentTile = Grid[column, row];

                List<SpriteRenderer> horizontalMatches = FindColumnMatchForTile(column, row, currentTile._spriteRenderer.sprite /*current.sprite*/); // 4
                
                if (horizontalMatches.Count >= 2)
                {
                    matchedTiles.UnionWith(horizontalMatches);
                    matchedTiles.Add(current); // 5

                    matchedTileTiles.UnionWith(columnMatches);
                    matchedTileTiles.Add(currentTile);
                }

                List<SpriteRenderer> verticalMatches = FindRowMatchForTile(column, row, currentTile._spriteRenderer.sprite); // 6
                if (verticalMatches.Count >= 2)
                {
                    matchedTiles.UnionWith(verticalMatches);
                    matchedTiles.Add(current);

                    matchedTileTiles.UnionWith(rowMatches);
                    matchedTileTiles.Add(currentTile);
                }
            }
        }

        //foreach (SpriteRenderer renderer in matchedTiles) // 7
        //{
        //    renderer.sprite = null;
        //}
        foreach (Tile tile in matchedTileTiles) // 7
        {
            tile.isMatched = true;
            //tile.DisactivateTile();
        }


        return matchedTileTiles.Count > 0; // 8
    }

    List <SpriteRenderer> FindColumnMatchForTile(int col, int row, Sprite sprite)
    {
        List<SpriteRenderer> result = new List<SpriteRenderer>();
        columnMatches.Clear();

        for (int i = col + 1; i < GridDimension; i++)
        {
            SpriteRenderer nextColumn = GetSpriteRendererAt(i, row);
            Tile tile = Grid[i, row];
            if (nextColumn.sprite != sprite)
            {
                break;
            }
            result.Add(nextColumn);
            columnMatches.Add(tile);
        }
        return result;
    }

    List <SpriteRenderer> FindRowMatchForTile(int col, int row, Sprite sprite)
    {
        List<SpriteRenderer> result = new List<SpriteRenderer> ();
        rowMatches.Clear();
        for (int i = row + 1; i < GridDimension; i++)
        {
            SpriteRenderer nextRow = GetSpriteRendererAt(col, i);
            Tile tile = Grid[col, i];
            if (nextRow.sprite != sprite)
            {
                break;
            }
            result.Add(nextRow);
            rowMatches.Add(tile);
        }
        return result;
    }

    //void FillHoles()
    //{
    //    for (int column = 0; column < GridDimension; column++)
    //    {
    //        for (int row = 0; row < GridDimension; row++) // 1
    //        {
    //            while (GetSpriteRendererAt(column, row).sprite == null) // 2
    //            {
    //                for (int filler = row; filler < GridDimension - 1; filler++) // 3
    //                {
    //                    SpriteRenderer current = GetSpriteRendererAt(column, filler); // 4
    //                    SpriteRenderer next = GetSpriteRendererAt(column, filler + 1);
    //                    current.sprite = next.sprite;
    //                }
    //                SpriteRenderer last = GetSpriteRendererAt(column, GridDimension - 1);
    //                last.sprite = Sprites[UnityEngine.Random.Range(0, Sprites.Count)]; // 5
    //            }
    //        }
    //    }
    //}

    private void getAllMatchedTiles()
    {
        Vector2 moveToPosForPulledTile = Vector2.zero;
        for (int column = 0; column < GridDimension; column++)
        {
            for (int row = 0; row < GridDimension; row++) // 1
            {
                while (Grid[column,row].isMatched) // 2
                {
                    //going up on column and assign new coordinates to upper tile, wit other words, make coordinates of upper tile equal to current tile
                    for (int filler = row; filler < GridDimension - 1; filler++) // 3
                    {
                        Tile next = Grid[column, filler+1];
                        Tile current = Grid[column, filler];
                        next.row = filler;
                        //if (current.Position.y == current.MoveToPosition.y) next.MoveToPosition = current.Position;
                        //else next.MoveToPosition = current.MoveToPosition;

                        next.MoveToPosition = current.Position;
                        moveToPosForPulledTile = new Vector2(next.Position.x, next.Position.y + Distance);
                        Grid[column, filler] = next;
                        if (current.isMatched) current.DisactivateTile();
                        next.moveTo();
                    }
                    pullNewTile(column, GridDimension - 1, new Vector3(moveToPosForPulledTile.x, moveToPosForPulledTile.y, 0));
                    //foreach (Tile tile in Grid)
                    //{
                    //    if (tile.Position != tile.MoveToPosition) tile.moveTo();
                    //    //if (tile.isMatched) tile.DisactivateTile();
                    //    //Grid[tile.column, tile.row] = tile;
                    //}
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

        Vector2 tempPosition = tile1.Position;
        tile1.Position = tile2.Position;
        tile2.Position = tempPosition;
        tile1.MoveToPosition = tile1.Position;
        tile2.MoveToPosition = tile2.Position;
    }

    // Update is called once per frame
    void Update()
    {
        if (isSwiping) {
            selectedTileTransform.position = Vector2.Lerp(selectedTileTransform.position, moveToTilePos, 0.2f);
            moveToTileTransform.position = Vector2.Lerp(moveToTileTransform.position, selectedTilePos, 0.2f);
            if (((Vector2)selectedTileTransform.position - moveToTilePos).magnitude < 0.15f) {

                changeTilesOnGrid(selectedTile, moveToTile);
                if (CheckMatches())
                {
                    SwapTiles(/*moveToTile, selectedTile*/);
                    moveToTileTransform.position =  selectedTilePos;
                    selectedTileTransform.position = moveToTilePos;
                }
                else
                {
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
