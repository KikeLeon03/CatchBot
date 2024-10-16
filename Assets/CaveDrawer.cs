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

    public int caveWidth;

    public int xBorderDistance=20;
    public int yBorderDistance=10;

    public int pointsToClearInEndWhenClose = 5;
    public int pointsToClearInMidWhenClose = 6;

    private int halfXBorderDistance;
    private int halfYBorderDistance;

    private TileBase tile;

    void Start()
    {
        halfXBorderDistance = Mathf.CeilToInt(xBorderDistance/2);
        halfYBorderDistance = Mathf.CeilToInt(yBorderDistance/2);
        tile = tileDrawer.tileCave;
    }

    public bool GenerateBordersIfNeeded(int[] playerLastPosInt, int[] playerNewPosInt)
    {
        bool res = false;

        if (playerLastPosInt[0] > playerNewPosInt[0])
        {
            res = true;
            while (playerLastPosInt[0] > playerNewPosInt[0])
            {
                tileDrawer.DrawTileColumn(new Vector2Int(playerLastPosInt[0] - halfXBorderDistance, playerLastPosInt[1] - halfYBorderDistance), yBorderDistance, tile);
                playerLastPosInt[0]--;
            }
        }
        else
        {
            if (playerLastPosInt[0] < playerNewPosInt[0])
            {
                res = true;
                while (playerLastPosInt[0] < playerNewPosInt[0])
                {
                    tileDrawer.DrawTileColumn(new Vector2Int(playerLastPosInt[0] + halfXBorderDistance, playerLastPosInt[1] - halfYBorderDistance), yBorderDistance, tile);
                    playerLastPosInt[0]++;
                }
            }
        }
        if (playerLastPosInt[1] > playerNewPosInt[1])
        {
            res = true;
            while (playerLastPosInt[1] > playerNewPosInt[1])
            {
                tileDrawer.DrawTileRow(new Vector2Int(playerLastPosInt[0] - halfXBorderDistance, playerLastPosInt[1] - halfYBorderDistance), xBorderDistance, tile);
                playerLastPosInt[1]--;
            }
        }
        else
        {
            if (playerLastPosInt[1] < playerNewPosInt[1])
            {
                res = true;
                while (playerLastPosInt[1] < playerNewPosInt[1])
                {
                    tileDrawer.DrawTileRow(new Vector2Int(playerLastPosInt[0] - halfXBorderDistance, playerLastPosInt[1] + halfYBorderDistance), xBorderDistance, tile);
                    playerLastPosInt[1]++;
                }
            }
        }
        return res;
    }

    public void ClearEndOfCave(int[] playerNewPosInt)
    {
        int pointsToClearWhenCloseNow =
            Vector2.Distance(followLineDrawer.points.Last.Value, new Vector2(playerNewPosInt[0], playerNewPosInt[1])) >
            Vector2.Distance(followLineDrawer.points.First.Value, new Vector2(playerNewPosInt[0], playerNewPosInt[1])) ?
            pointsToClearInEndWhenClose : -pointsToClearInEndWhenClose;

        followLineDrawer.DeleteXTilesCollidingWithFollowLine(pointsToClearWhenCloseNow, caveWidth);
    }

    public void ClearMiddleOfCave()
    {
        followLineDrawer.ClearFromMiddle(pointsToClearInMidWhenClose, caveWidth);
    }
}
