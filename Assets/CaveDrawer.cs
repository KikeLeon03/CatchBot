using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CaveDrawer : MonoBehaviour
{
    [SerializeField]
    FollowLineDrawer followLineDrawer;

    [SerializeField]
    TileDrawer tileDrawer;

    [SerializeField]
    GeneralMovement player;

    int[] playerLastPosInt = new int[2];
    int[] playerNewPosInt = new int[2];

    public int caveWidth;

    public int xBorderDistance=20;
    public int yBorderDistance=10;

    private int halfXBorderDistance;
    private int halfYBorderDistance;

    private TileBase tile;

    void Start()
    {
        halfXBorderDistance = Mathf.CeilToInt(xBorderDistance/2);
        halfYBorderDistance = Mathf.CeilToInt(yBorderDistance/2);
        tile = tileDrawer.tileCave;

        playerLastPosInt = new int[] { Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.y)};
    }
    void Update()
    {
        playerNewPosInt = new int[] { Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.y) };
    }

    public void GenerateBordersIfNeeded()
    {
        if (playerLastPosInt[0] > playerNewPosInt[0]) {
            tileDrawer.DrawTileColumn(new Vector2Int(playerNewPosInt[0] - halfXBorderDistance, playerLastPosInt[1] - halfYBorderDistance), yBorderDistance, tile);
        }
        else
        {
            if (playerLastPosInt[0] < playerNewPosInt[0])
            {
                tileDrawer.DrawTileColumn(new Vector2Int(playerNewPosInt[0] + halfXBorderDistance, playerLastPosInt[1] - halfYBorderDistance), yBorderDistance, tile);
            }
        }
        if (playerLastPosInt[1] > playerNewPosInt[1])
        {
            tileDrawer.DrawTileRow(new Vector2Int(playerNewPosInt[0] - halfXBorderDistance, playerLastPosInt[1] - halfYBorderDistance), xBorderDistance, tile);
        }
        else
        {
            if (playerLastPosInt[1] < playerNewPosInt[1])
            {
                tileDrawer.DrawTileRow(new Vector2Int(playerNewPosInt[0] - halfXBorderDistance, playerLastPosInt[1] + halfYBorderDistance), xBorderDistance, tile);
            }
        }
        playerLastPosInt = playerNewPosInt;

        tileDrawer.DeleteTilesInSquare(new Vector2Int(Mathf.RoundToInt(followLineDrawer.lastGenPoint.x), Mathf.RoundToInt(followLineDrawer.lastGenPoint.y)), caveWidth);

        followLineDrawer.DeleteTilesCollidingWithFollowLine(caveWidth);
    }
}
