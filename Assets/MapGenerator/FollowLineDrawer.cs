using Google.Protobuf.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using Unity.MLAgents.Integrations.Match3;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;

public class FollowLineDrawer : MonoBehaviour
{
    public GeneralMovement player;
    public float minPlayerToHeadTileDistance = 20f;
    public int pointCount = 20;           // N�mero de puntos de la l�nea
    public float segmentLength = 2.0f;    // Distancia entre puntos
    public float maxAngleChange = 30f;    // �ngulo m�ximo para evitar cambios bruscos
    public LineRenderer lineRenderer;     // El LineRenderer que usaremos para dibujar la l�nea
    public TileDrawer tileDrawer;
    public int caveWidth = 5;
    public int caveThinckness = 40;

    public LinkedList<Vector2> points;   // Los puntos que forman la l�nea

    void Start()
    {
        // Inicializar el LineRenderer y los puntos
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = pointCount;
        lineRenderer.startWidth = 0.1f;  // Grosor inicial de la l�nea
        lineRenderer.endWidth = 0.1f;    // Grosor final de la l�nea
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));  // Usar un material b�sico
        lineRenderer.startColor = UnityEngine.Color.red;
        lineRenderer.endColor = UnityEngine.Color.red;

        GenerateLine();
        DrawLine();
        GenerateCaveDeletengTilesInLine();
    }

    // Genera los puntos de la l�nea
    void GenerateLine()
    {
        points = new LinkedList<Vector2>();

        // Primer punto en el origen o en cualquier punto inicial que desees
        points.AddLast(Vector2.zero);

        // Vector para la direcci�n inicial (puede ser cualquier direcci�n)
        Vector2 direction = Vector2.right;  // Direcci�n inicial hacia la derecha

        // Generar el resto de los puntos
        while (points.Count < pointCount)
        {
            bool validPoint = false;
            int attempts = 0;

            while (!validPoint && attempts < 100) // Limitar el n�mero de intentos para evitar ciclos infinitos
            {
                // Cambiar la direcci�n suavemente
                direction = ChangeDirection(direction);

                // Crear el nuevo punto a lo largo de la direcci�n calculada
                Vector2 newPoint = points.Last.Value + direction * segmentLength;

                // Verificar que el nuevo punto no est� demasiado cerca de los puntos anteriores
                if (!IsTooCloseToOtherPoints(newPoint))
                {
                    points.AddLast(newPoint);
                    validPoint = true;
                }

                attempts++;
            }

            if (!validPoint)
            {
                // Si no se encontr� un punto v�lido despu�s de muchos intentos, termina el proceso
                Debug.LogWarning("No se pudo encontrar un punto v�lido para la posici�n " + points.Count);
                break;
            }
        }
    }

    // Controla los cambios de direcci�n suavemente
    Vector2 ChangeDirection(Vector2 currentDirection)
    {
        // Cambiar el �ngulo dentro del l�mite definido por maxAngleChange
        float angleChange = UnityEngine.Random.Range(-maxAngleChange, maxAngleChange);

        // Crear una nueva direcci�n rotada
        Vector2 newDirection = Quaternion.Euler(0, 0, angleChange) * currentDirection;

        // Normalizar la direcci�n para mantener el tama�o constante
        return newDirection.normalized;
    }

    // TODO: Verifica si el nuevo punto est� demasiado cerca de los puntos anteriores
    bool IsTooCloseToOtherPoints(Vector2 newPoint)
    {
        /*
        float minDistance = 0f; // Puedes ajustar este valor seg�n sea necesario
        int point1Pos=0;
        int point2Pos=0;

        foreach (var point in points)
        {   
            if (Vector2.Distance(newPoint, point) < minDistance)
            {
                return true; // El nuevo punto est� demasiado cerca a alg�n punto anterior
            }
        }

        return false; // El nuevo punto est� a una distancia segura de todos los puntos previos
        */
        return false;
    }

    // Dibuja la l�nea usando LineRenderer
    void DrawLine()
    {
        int index = 0;
        foreach (var point in points)
        {
            lineRenderer.SetPosition(index, point);
            index++;
        }
    }

    void GenerateCaveDeletengTilesInLine()
    {
        AddTilesSurroundingFollowLine(caveThinckness, tileDrawer.tileCave);
        DeleteTilesCollidingWithFollowLine(caveWidth);
    }

    void AddTilesSurroundingFollowLine(int height, TileBase tile)
    {
        foreach (var point in points)
        {
            int width = player.transform.position.x < point.x ? Mathf.CeilToInt(segmentLength) : -Mathf.CeilToInt(segmentLength);
            tileDrawer.DrawFullSquare(roundVector2ToInt(point), width, caveThinckness, tileDrawer.tileCave);
        }
    }

    void DeleteTilesCollidingWithFollowLine(int deleteRadius)
    {
        foreach (var point in points)
        {
            DeleteTilesInSquare(roundVector2ToInt(point), deleteRadius);
        }
    }

    void DeleteTilesInSquare(Vector2Int center, int deleteRadius)
    {
        int squareDeleteRadius = deleteRadius * deleteRadius + 1;

        for (int x = -deleteRadius; x <= deleteRadius; x++)
        {
            for (int y = -deleteRadius; y <= deleteRadius; y++)
            {
                Vector2Int tilePosition = new Vector2Int(center.x + x, center.y + y);
                tileDrawer.RemoveTile(tilePosition);
            }
        }
    }

    public void GenerateCaveTilesIfNeeded(Vector3 playerPos)
    {
        Vector2 newPoint = Vector2.zero;

        if (DistanceBetweenVector2AndVector3(points.First.Value, playerPos) < minPlayerToHeadTileDistance)
        {
            newPoint = GeneratePointCave(points.First.Value, points.First.Next.Value);
            points.AddFirst(newPoint);
            points.RemoveLast();
        }
        if (DistanceBetweenVector2AndVector3(points.Last.Value, playerPos) < minPlayerToHeadTileDistance)
        {
            newPoint = GeneratePointCave(points.Last.Value, points.Last.Previous.Value);
            points.AddLast(newPoint);
            points.RemoveFirst();
        }

        DrawLine();

        Vector2Int newPointInt = roundVector2ToInt(newPoint);

        int width = player.transform.position.x < newPoint.x ? Mathf.CeilToInt(segmentLength) : -Mathf.CeilToInt(segmentLength);
        tileDrawer.DrawFullSquare(roundVector2ToInt(newPoint), width, caveThinckness, tileDrawer.tileCave);

        //DeleteTilesCollidingWithFollowLine(caveWidth);
    }


    Vector2 GeneratePointCave(Vector2 prevPoint, Vector2 prevPrevPoint)
    {
        int attempts = 0;

        // Calculate and normalize the direction
        Vector2 direction = (prevPoint - prevPrevPoint).normalized;

        while (attempts < 100) // Limitar los intentos para evitar ciclos infinitos
        {
            // Suavemente cambiar la direcci�n
            direction = ChangeDirection(direction);

            // Generar el nuevo punto usando el punto anterior y la direcci�n normalizada
            Vector2 newPoint = prevPoint + direction * segmentLength;

            // Verificar si el nuevo punto est� adecuadamente distanciado de otros puntos
            if (!IsTooCloseToOtherPoints(newPoint))
            {
                return newPoint;
            }

            attempts++;
        }

        // Devuelve un valor por defecto si fallan los intentos
        return Vector2.one; // Or any appropriate error vector
    }

    public Vector2 GetMiddlePoint()
    {
        float midPointPos = pointCount / 2;
        int i = 0;
        foreach(Vector2 point in points)
        {
            if(i > midPointPos)
            {
                return point;
            }
            i++;
        }
        return Vector2.zero;
    }

    public Vector2Int roundVector2ToInt(Vector2 Vector2)
    {
        return new Vector2Int(Mathf.RoundToInt(Vector2.x), Mathf.RoundToInt(Vector2.y));
    }

    public Vector2Int roundVector2ToIntOffset(Vector2 Vector2, float x, float y, float z)
    {
        return new Vector2Int(Mathf.RoundToInt(Vector2.x + x), Mathf.RoundToInt(Vector2.y + y));
    }

    float DistanceBetweenVector2AndVector3(Vector2 vec2, Vector3 vec3)
    {
        // Ignoramos el componente z de vec3
        return Vector2.Distance(vec2, new Vector2(vec3.x, vec3.y));
    }
}









/*void GenerateCaveAroundFollowLine()
    {
        Vector2Int botUpperPointOrigin = Vector2Int.zero;
        Vector2Int botUpperPointEnd = Vector2Int.zero;
        Vector2Int topUpperPointOrigin = Vector2Int.zero;
        Vector2Int topUpperPointEnd = Vector2Int.zero;

        Vector2Int topLowerPointOrigin = Vector2Int.zero;
        Vector2Int topLowerPointEnd = Vector2Int.zero;
        Vector2Int botLowerPointOrigin = Vector2Int.zero;
        Vector2Int botLowerPointEnd = Vector2Int.zero;

        for (int i= 0; i<points.Length-1; i++)
        {
            Debug.Log(points);
            botUpperPointOrigin = new Vector2Int(Mathf.RoundToInt(points[i].x), Mathf.RoundToInt(points[i].y + caveWidth), 0);
            botUpperPointEnd = new Vector2Int(Mathf.RoundToInt(points[i+1].x), Mathf.RoundToInt(points[i+1].y + caveWidth), 0);
            topUpperPointOrigin = new Vector2Int(Mathf.RoundToInt(points[i].x), Mathf.RoundToInt(points[i].y + caveWidth + caveThinckness), 0);
            topUpperPointEnd = new Vector2Int(Mathf.RoundToInt(points[i + 1].x), Mathf.RoundToInt(points[i + 1].y + caveWidth + caveThinckness), 0);

            tileDrawer.DrawLine(topUpperPointOrigin, topUpperPointEnd, tileDrawer.tileCave);
            tileDrawer.DrawLine(botUpperPointOrigin, botUpperPointEnd, tileDrawer.tileCave);
            

            

            topLowerPointOrigin = new Vector2Int(Mathf.RoundToInt(points[i].x), Mathf.RoundToInt(points[i].y - caveWidth), 0);
            topLowerPointEnd = new Vector2Int(Mathf.RoundToInt(points[i+1].x), Mathf.RoundToInt(points[i+1].y - caveWidth), 0);
            botLowerPointOrigin = new Vector2Int(Mathf.RoundToInt(points[i].x), Mathf.RoundToInt(points[i].y - caveWidth - caveThinckness), 0);
            botLowerPointEnd = new Vector2Int(Mathf.RoundToInt(points[i + 1].x), Mathf.RoundToInt(points[i + 1].y - caveWidth - caveThinckness), 0);

            tileDrawer.DrawLine(topLowerPointOrigin, topLowerPointEnd, tileDrawer.tileCave);
            tileDrawer.DrawLine(botLowerPointOrigin, botLowerPointEnd, tileDrawer.tileCave);
        }

        // Draw cave edges
        tileDrawer.DrawLine(new Vector2Int(Mathf.RoundToInt(points[0].x), Mathf.RoundToInt(points[0].y + caveWidth), 0), new Vector2Int(Mathf.RoundToInt(points[0].x), Mathf.RoundToInt(points[0].y + caveWidth + caveThinckness), 0), tileDrawer.tileCave);
        tileDrawer.DrawLine(new Vector2Int(Mathf.RoundToInt(points[0].x), Mathf.RoundToInt(points[0].y - caveWidth), 0), new Vector2Int(Mathf.RoundToInt(points[0].x), Mathf.RoundToInt(points[0].y - caveWidth - caveThinckness), 0), tileDrawer.tileCave);


        tileDrawer.DrawLine(topUpperPointEnd, botUpperPointEnd, tileDrawer.tileCave);
        tileDrawer.DrawLine(topLowerPointEnd, botLowerPointEnd, tileDrawer.tileCave);

        // Fill the walls
        tileDrawer.FloodFill(new Vector2Int(Mathf.RoundToInt(points[0].x) + 1, Mathf.RoundToInt(points[0].y) + Mathf.RoundToInt(caveWidth) + Mathf.RoundToInt(caveThinckness / 2), 0), tileDrawer.tileCave);
        tileDrawer.FloodFill(new Vector2Int(Mathf.RoundToInt(points[0].x) + 1, Mathf.RoundToInt(points[0].y) - Mathf.RoundToInt(caveWidth) - Mathf.RoundToInt(caveThinckness / 2), 0), tileDrawer.tileCave);


    }
    */