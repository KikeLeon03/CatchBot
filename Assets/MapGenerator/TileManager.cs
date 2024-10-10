using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [SerializeField]
    FollowLineDrawer followLineDrawer;

    [SerializeField]
    TileDrawer tileDrawer;

    [SerializeField]
    GeneralMovement player;
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
    }

    void Update()
    {
        followLineDrawer.GenerateCaveTilesIfNeeded(player.transform.position);
    }

    float DistanceBetweenVector2AndVector3(Vector2 vec2, Vector3 vec3)
    {
        // Ignoramos el componente z de vec3
        return Vector2.Distance(vec2, new Vector2(vec3.x, vec3.y));
    }
}
