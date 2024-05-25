using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public Tilemap minesTileMap;

    private VisualText visualText;

    public int bombsCount = 10;
    public int nonBombTiles;
    public int totalTiles;

    Dictionary<MineTeplate, bool> mineUnlocked = new Dictionary<MineTeplate, bool>();

    private void Awake()
    {
        visualText = GetComponent<VisualText>();
    }

    private void Start()
    {
        StartCoroutine(EstablishConnectionAndSpawnBombs());
    }

    private void Update()
    { 
        if (Input.GetMouseButtonDown(0))
            UnlockTile();
        else if (Input.GetMouseButtonDown(1))
            FlagTile();
    }

    IEnumerator EstablishConnectionAndSpawnBombs()
    {
        yield return StartCoroutine(EstablishConnectionBetweenMines());
        yield return StartCoroutine(SpawnBombTiles());
    }

    IEnumerator EstablishConnectionBetweenMines()
    {
        for (int x = minesTileMap.cellBounds.xMin; x <= minesTileMap.cellBounds.xMax; x++)
        {
            for (int y = minesTileMap.cellBounds.yMin; y <= minesTileMap.cellBounds.yMax; y++)
            {
                TileBase tileBase = minesTileMap.GetTile(new Vector3Int(x, y, 0));
                if (tileBase != null)
                {
                    GameObject gameObject = minesTileMap.GetInstantiatedObject(new Vector3Int(x, y, 0));
                    MineTeplate mineTeplate = gameObject.GetComponent<MineTeplate>();
                    mineTeplate.IdentifyNeighbouringMines(minesTileMap, x, y, this);
                    mineUnlocked.Add(mineTeplate, false);
                }
            }
        }

        yield return null;
    }

    IEnumerator SpawnBombTiles()
    {
        Vector3Int tilemapSize = minesTileMap.size;
        bombsCount = tilemapSize.x * tilemapSize.y / 6;
        nonBombTiles = tilemapSize.x * tilemapSize.y - bombsCount;
        totalTiles = bombsCount + nonBombTiles;
        visualText.UpdatRemainingTilesCountText(nonBombTiles);

        int bombsToBeSpawned = bombsCount;
        while (bombsToBeSpawned > 0)
        {
            int randomX = Random.Range(-tilemapSize.x, tilemapSize.x);
            int randomY = Random.Range(-tilemapSize.y, tilemapSize.y);
            Vector3Int tilePosition = new Vector3Int(randomX, randomY, 0);
            TileBase tileBase = minesTileMap.GetTile(tilePosition);
            if (tileBase)
            {
                GameObject gameObject = minesTileMap.GetInstantiatedObject(tilePosition);
                MineTeplate mineTeplate = gameObject.GetComponent<MineTeplate>();
                if (mineTeplate.GetIsBomb() != true)
                {
                    mineTeplate.SelectAsMine();
                    bombsToBeSpawned--;
                }
            }
        }
        yield return null;
    }

    MineTeplate CheckMouseInput()
    {
        Vector3 mousePostion = Input.mousePosition;
        Vector3 mousePositionInWorld = Camera.main.ScreenToWorldPoint(mousePostion);
        Vector3Int positionOnTileMap = minesTileMap.WorldToCell(mousePositionInWorld);
        GameObject gameObject = minesTileMap.GetInstantiatedObject(positionOnTileMap);
        if (!gameObject)
            return null;
        else
            return gameObject.GetComponent<MineTeplate>();
    }

    public void UnlockTile()
    {
        MineTeplate mineTeplate = CheckMouseInput();
        if (mineTeplate)
            mineTeplate.UnlockTile();
    }

    public void FlagTile()
    {
        MineTeplate mineTeplate = CheckMouseInput();
        if (mineTeplate)
            mineTeplate.FlagTile();
    }

    public void DecreaseNonBombTileCount()
    {
        nonBombTiles--;
        visualText.UpdatRemainingTilesCountText(nonBombTiles);
        if (nonBombTiles <= 0)
        {
            visualText.EnableCongralutionsText();
            StartCoroutine(ReloadGame());
        }
    }

    public void BombCaught()
    {
        visualText.EnableGameOverText();
        StartCoroutine(ReloadGame());
    }

    IEnumerator ReloadGame()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool CheckIfUnlocked(MineTeplate mineTeplate)
    {
        return mineUnlocked[mineTeplate];
    }

    public void MarkTileAsUnlocked(MineTeplate mineTeplate)
    {
        mineUnlocked[mineTeplate] = true;
    }
}
