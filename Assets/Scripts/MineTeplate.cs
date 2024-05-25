using UnityEngine;
using UnityEngine.Tilemaps;

public class MineTeplate : MonoBehaviour
{
    private GameManager gameManager;

    private bool tileUnlocked = false;
    private bool tileFlagged = false;

    private bool isBomb = false;
    private int neighbouringBombsCount = 0;

    Tilemap minesTileMap;
    MineTeplate[] neighbouringMineTemplates = new MineTeplate[8];
    int[] xAdjustment = { -1, 0, 1, 1, 1, 0, -1, -1 };
    int[] yAdjustment = { 1, 1, 1, 0, -1, -1, -1, 0 };
    bool[] alloted;

    SpriteRenderer spriteRenderer;
    public Sprite bombSprite;
    public Sprite[] numberSprites;
    public Color[] numberColors = { Color.white, Color.black, Color.green, Color.blue, Color.yellow, Color.red };

    public Sprite lockedMineTileSprite;
    public Color lockedMineTileColor = Color.grey;

    public Sprite FlaggedTileSprite;

    void Awake()
    {
        alloted = new bool[8];
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        tileUnlocked = false;
        isBomb = false;
        neighbouringBombsCount = 0;

        spriteRenderer.sprite = lockedMineTileSprite;
        spriteRenderer.color = lockedMineTileColor;

        for (int i = 0; i < alloted.Length; i++)
            alloted[i] = false;
    }

    public void IdentifyNeighbouringMines(Tilemap minesTileMap, int x, int y, GameManager gameManager)
    {
        this.minesTileMap = minesTileMap;
        this.gameManager = gameManager;

        for (int i = 0; i < xAdjustment.Length; i++)
        {
            if (alloted[i] == true)
                continue;

            alloted[i] = true;
            Vector3 vectorAdjustment = new Vector3(xAdjustment[i], yAdjustment[i], 0);
            Vector3Int newAdjustedVector = minesTileMap.WorldToCell(transform.position + vectorAdjustment);
            TileBase neighbouringTileBase = minesTileMap.GetTile(newAdjustedVector);
            if (neighbouringTileBase)
            {
                GameObject gameObject = minesTileMap.GetInstantiatedObject(newAdjustedVector);
                MineTeplate neighbouringMineTemplate = gameObject.GetComponent<MineTeplate>();
                neighbouringMineTemplates[i] = neighbouringMineTemplate;
                neighbouringMineTemplate.LinkTiles(this, i);
            }
        }
    }

    void LinkTiles(MineTeplate neighbouringMineTemplate, int index)
    {
        neighbouringMineTemplates[(index + 4) % 8] = neighbouringMineTemplate;
        alloted[(index + 4) % 8] = true;
    }

    public bool GetIsBomb()
    {
        return isBomb;
    }

    public bool GetIsFlagged()
    {
        return tileFlagged;
    }

    public void SelectAsMine()
    {
        isBomb = true;
        for (int i = 0; i < neighbouringMineTemplates.Length; i++)
        {
            if (neighbouringMineTemplates[i])
                neighbouringMineTemplates[i].IncreaseNeighbouringBombsCount();
        }
    }

    public void IncreaseNeighbouringBombsCount()
    {
        neighbouringBombsCount++;
    }

    private void ZeroNeighbouringBombsCase()
    {
        for (int i = 0; i < neighbouringMineTemplates.Length; i++)
        {
            MineTeplate mineTeplate = neighbouringMineTemplates[i];
            if (mineTeplate && !mineTeplate.GetIsFlagged() && !mineTeplate.GetIsBomb() && !gameManager.CheckIfUnlocked(mineTeplate))
            {
                gameManager.MarkTileAsUnlocked(mineTeplate);
                gameManager.DecreaseNonBombTileCount();
                neighbouringMineTemplates[i].UnlockTile(i);
            }
        }
    }

    // private void ZeroNeighbouringBombsCase(int prevIndex)
    // {
    //     int indexToAvoid = (prevIndex + 4) % 8;

    //     for (int i = 0; i < neighbouringMineTemplates.Length; i++)
    //     {
    //         if (i == indexToAvoid)
    //             continue;

    //         MineTeplate mineTeplate = neighbouringMineTemplates[i];
    //         if (mineTeplate && !mineTeplate.GetIsFlagged() && !mineTeplate.GetIsBomb() && !gameManager.CheckIfUnlocked(mineTeplate))
    //         {
    //             gameManager.MarkTileAsUnlocked(mineTeplate);
    //             gameManager.DecreaseNonBombTileCount();
    //             mineTeplate.UnlockTile(i);
    //         }
    //     }
    // }

    public void UnlockTile()
    {
        if (tileFlagged || tileUnlocked)
            return;

        tileUnlocked = true;
        if (isBomb)
        {
            spriteRenderer.sprite = bombSprite;
            gameManager.BombCaught();
        }
        else
        {
            gameManager.DecreaseNonBombTileCount();
            gameManager.MarkTileAsUnlocked(this);

            spriteRenderer.sprite = numberSprites[neighbouringBombsCount];
            spriteRenderer.color = Color.white;
            
            if (neighbouringBombsCount == 0)
                ZeroNeighbouringBombsCase();
        }
    }

    public void UnlockTile(int prevIndex)
    {
        if (tileFlagged || isBomb)
            return;
     
        tileUnlocked = true;
        spriteRenderer.sprite = numberSprites[neighbouringBombsCount];
        spriteRenderer.color = Color.white;
   
        if (neighbouringBombsCount == 0)
            ZeroNeighbouringBombsCase();
    }

    public void FlagTile()
    {
        if (tileUnlocked)
            return;

        tileFlagged = !tileFlagged;
        if (tileFlagged)
            spriteRenderer.sprite = FlaggedTileSprite;
        else
            spriteRenderer.sprite = lockedMineTileSprite;
    }
}
