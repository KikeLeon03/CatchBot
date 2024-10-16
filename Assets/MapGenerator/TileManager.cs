using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [SerializeField]
    FollowLineDrawer followLineDrawer;


    [SerializeField]
    CaveDrawer caveDrawer;

    [SerializeField]
    PlatformDrawer platformDrawer;

    [SerializeField]
    TileDrawer tileDrawer;

    [SerializeField]
    GeneralMovement player;

    public string state;

    int[] playerLastPosInt = new int[2];
    int[] playerNewPosInt = new int[2];

    private (Vector2, bool) pointGen;
    void Start()
    {
        /* Draw tests
        tileDrawer.DrawLine(new Vector3Int(-100, -100, 0), new Vector3Int(0, -50, 0), tileDrawer.tileCave);
        tileDrawer.DrawTileRow(new Vector3Int(0,0,0), 10, tileDrawer.tile1);
        tileDrawer.DrawTileColumn(new Vector3Int(0, 0, 0), 10, tileDrawer.tile1);
        tileDrawer.DrawTile(new Vector3Int(10,10,10), tileDrawer.tile1);
        tileDrawer.DrawLine(new Vector3Int(-100, -100, 0), new Vector3Int(0, -50, 0), tileDrawer.tile1);
        tileDrawer.DrawCircle(new Vector3Int(20, 20, 0), 10, tileDrawer.tile1);
        tileDrawer.FloodFill(new Vector3Int(20, 20, 0), tileDrawer.tile1);
        tileDrawer.DrawShape(new Vector3Int[] { new Vector3Int(-20, -20, 0), new Vector3Int(-30, -20, 0), new Vector3Int(-20, -10, 0) }, tileDrawer.tile1);
        tileDrawer.FloodFill(new Vector3Int(20, 20, 20), tileDrawer.tile1);
        */

        playerLastPosInt = new int[2] {Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.y) };

        state = "platform";


    }

    void Update()
    {


        playerNewPosInt = new int[2] { Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.y) };

        pointGen = followLineDrawer.GeneratePointIfNeeded(state);

        //CaveDrawerActions();

        PlatformDrawerActions();


        playerLastPosInt = playerNewPosInt;
    }

    void CaveDrawerActions()
    {
        if (caveDrawer.GenerateBordersIfNeeded(playerLastPosInt, playerNewPosInt)){
            caveDrawer.ClearMiddleOfCave();
        }
    }

    void PlatformDrawerActions()
    {
        if (pointGen.Item2)
        {
            platformDrawer.MayGenerateRandomPlatform(new Vector2Int(Mathf.RoundToInt(pointGen.Item1.x), Mathf.RoundToInt(pointGen.Item1.y)));
        }
    }

}
