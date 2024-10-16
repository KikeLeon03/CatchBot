using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlatformDrawer : MonoBehaviour
{
    [SerializeField]
    FollowLineDrawer followLineDrawer;

    [SerializeField]
    TileDrawer tileDrawer;

    public int caveWidth;

    public int maxJumpDistance = 5;
    public int minJumpDistance = 1;

    private int platformCicleCounter;

    private TileBase tile;

    void Start()
    {
        tile = tileDrawer.tilePlatform;
        platformCicleCounter = 0;
    }

    public void MayGenerateRandomPlatform(Vector2Int pos)
    {
        int randomNumber = UnityEngine.Random.Range(minJumpDistance, maxJumpDistance);

        if(platformCicleCounter >= randomNumber)
        {
            platformCicleCounter = 0;
            GenerateRandomPlatform(pos);
        }
        else
        {
            platformCicleCounter++;
        }
    }

    void GenerateRandomPlatform(Vector2Int pos)
    {
        int randomNumber = UnityEngine.Random.Range(0, 3);

        switch (randomNumber)
        {
            case 0:
                tileDrawer.DrawFullSquare(pos, UnityEngine.Random.Range(3, 5), UnityEngine.Random.Range(2, 11), tile);
                break;
            case 1:
                tileDrawer.DrawTileColumn(pos, UnityEngine.Random.Range(4, 15), tile);
                break;
            default:
                tileDrawer.DrawCircle(pos, UnityEngine.Random.Range(1,5), tile);
                break;
        }
    }


}
