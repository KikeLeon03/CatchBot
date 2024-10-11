using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileDrawer : MonoBehaviour
{
    [SerializeField]
    public Tilemap tilemap;

    [SerializeField]
    public TileBase tileCave;

    public const int maxFill = 1000;

    private int filledTiles;

    private Vector2Int[] directions = new Vector2Int[] {
        new Vector2Int(1, 0),   // Right
        new Vector2Int(-1, 0),  // Left
        new Vector2Int(0, 1),   // Up
        new Vector2Int(0, -1)   // Down
    };

    public void DrawTile(Vector2Int startPos, TileBase tile)
    {
        tilemap.SetTile((Vector3Int)startPos, tile);
    }

    // Example of removing a tile (optional)
    public void RemoveTile(Vector2Int position)
    {
        tilemap.SetTile((Vector3Int)position, null);  // Set to null to remove the tile
    }

    // Function to draw a row of tiles
    public void DrawTileRow(Vector2Int startPos, int length, TileBase tile)
    {
        for (int i = 0; i < length; i++)
        {
            Vector2Int tilePosition = new Vector2Int(startPos.x + i, startPos.y);
            DrawTile(tilePosition, tile);
        }
    }
    public void DeleteTileRow(Vector2Int startPos, int length, TileBase tile)
    {
        for (int i = 0; i < length; i++)
        {
            Vector2Int tilePosition = new Vector2Int(startPos.x + i, startPos.y);
            DrawTile(tilePosition, null);
        }
    }

    public void DrawTileColumn(Vector2Int startPos, int height, TileBase tile)
    {
        for (int i = 0; i < height; i++)
        {
            Vector2Int tilePosition = new Vector2Int(startPos.x, startPos.y + i);
            DrawTile(tilePosition, tile);
        }
    }
    public void DeleteTileColumn(Vector2Int startPos, int height, TileBase tile)
    {
        for (int i = 0; i < height; i++)
        {
            Vector2Int tilePosition = new Vector2Int(startPos.x, startPos.y + i);
            DrawTile(tilePosition, null);
        }
    }

    public void DrawLine(Vector2Int pointA, Vector2Int pointB, TileBase tile)
    {
        int x0 = pointA.x;
        int y0 = pointA.y;
        int x1 = pointB.x;
        int y1 = pointB.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            // Set the tile at the current position
            tilemap.SetTile(new Vector3Int(x0, y0, 0), tile);

            // If we have reached the end point, break
            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    public void DrawCircunfernce(Vector2Int center, int radius, TileBase tile)
    {
        int x = radius;
        int y = 0;
        int decisionOver2 = 1 - x; // Decision parameter for midpoint algorithm

        while (x >= y)
        {
            // Draw tiles at each of the symmetrical positions around the center
            SetSymmetricTiles(center, x, y, tile);

            y++;

            if (decisionOver2 <= 0)
            {
                decisionOver2 += 2 * y + 1;
            }
            else
            {
                x--;
                decisionOver2 += 2 * (y - x) + 1;
            }
        }
    }

    public void DrawFullSquare(Vector2Int lowBotPoint, int width, int height, TileBase tile)
    {
        Vector2Int newPoint;
        int centerX = lowBotPoint.x;
        int centerY = lowBotPoint.y;


        if (width > 0)
        {
            for (int x = 0; x <= width; x++)
            {
                for (int y = 0; y <= height; y++)
                {
                    newPoint = new Vector2Int(centerX + x, centerY + y);
                    DrawTile(newPoint, tile);
                }
            }
        }
        else
        {
            for (int x = 0; x <= width; x++)
            {
                for (int y = 0; y <= height; y++)
                {
                    newPoint = new Vector2Int(centerX - x, centerY + y);
                    DrawTile(newPoint, tile);
                }
            }
        }


        
    }
    public void DrawCircle(Vector2Int center, int radius, TileBase tile)
    {
        int x = radius;
        int y = 0;
        int decisionOver2 = 1 - x; // Decision parameter for midpoint algorithm

        while (x >= y)
        {
            // Draw the horizontal lines between symmetric points
            DrawHorizontalLine(center, -x, x, y, tile);
            DrawHorizontalLine(center, -x, x, -y, tile);

            y++;

            if (decisionOver2 <= 0)
            {
                decisionOver2 += 2 * y + 1;
            }
            else
            {
                x--;
                decisionOver2 += 2 * (y - x) + 1;
            }
        }
    }

    private void DrawHorizontalLine(Vector2Int center, int startX, int endX, int yOffset, TileBase tile)
    {
        for (int xOffset = startX; xOffset <= endX; xOffset++)
        {
            DrawTile(new Vector2Int(center.x + xOffset, center.y + yOffset), tile);
        }
    }
    private void SetSymmetricTiles(Vector2Int center, int x, int y, TileBase tile)
    {
        // This function should draw a tile at each of the 8 symmetric points on the circumference
        DrawTile(new Vector2Int(center.x + x, center.y + y), tile);
        DrawTile(new Vector2Int(center.x + x, center.y - y), tile);
        DrawTile(new Vector2Int(center.x - x, center.y + y), tile);
        DrawTile(new Vector2Int(center.x - x, center.y - y), tile);
        DrawTile(new Vector2Int(center.x + y, center.y + x), tile);
        DrawTile(new Vector2Int(center.x + y, center.y - x), tile);
        DrawTile(new Vector2Int(center.x - y, center.y + x), tile);
        DrawTile(new Vector2Int(center.x - y, center.y - x), tile);
    }

    public void DrawShape(Vector2Int[] points, TileBase tile)
    {
        for (int i = 0; i < points.Length; i++)
        {
            DrawTile(points[i], tile);
            DrawLine(points[i], points[(i + 1) % points.Length], tile);
        }
    }

    public void FloodFill(Vector2Int point, TileBase tile)
    {
        FloodFill(point, tile, 0);
        filledTiles = 0;
    }

    public void FloodFill(Vector2Int seedPosition, TileBase tile, int gen)
    {
        TileBase tileInPlace = tilemap.GetTile((Vector3Int)seedPosition);
        if (tileInPlace == null)
        {
            DrawTile(seedPosition, tile);
            filledTiles++;

            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int neighborPos = seedPosition + directions[i];
                if (tilemap.GetTile((Vector3Int)neighborPos) == null)
                {
                    if (filledTiles > maxFill) { return; }

                    FloodFill(neighborPos, tile, gen + 1);
                }
            }
        }
    }

    public void DeleteTilesInSquare(Vector2Int center, int deleteRadius)
    {
        int squareDeleteRadius = deleteRadius * deleteRadius + 1;

        for (int x = -deleteRadius; x <= deleteRadius; x++)
        {
            for (int y = -deleteRadius; y <= deleteRadius; y++)
            {
                Vector2Int tilePosition = new Vector2Int(center.x + x, center.y + y);
                RemoveTile(tilePosition);
            }
        }
    }
}
