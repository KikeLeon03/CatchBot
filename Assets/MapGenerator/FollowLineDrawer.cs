using Google.Protobuf.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using Unity.MLAgents.Integrations.Match3;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FollowLineDrawer : MonoBehaviour
{
    public GeneralMovement player;
    public float minPlayerToHeadTileDistance = 20f;
    public int pointCount = 20;           // Número de puntos de la línea
    public float segmentLength = 2.0f;    // Distancia entre puntos
    public float maxAngleChange = 30f;    // Ángulo máximo para evitar cambios bruscos
    public LineRenderer lineRenderer;     // El LineRenderer que usaremos para dibujar la línea
    public TileDrawer tileDrawer;
    public int caveWidth = 5;
    public int caveThinckness = 40;

    public Vector2 lastGenPoint;

    public LinkedList<Vector2> points;   // Los puntos que forman la línea

    private float halfCaveThinkness;
    private int halfCaveThinknessInt;

    public int pointsToClearWhenPlayerIsClose=8;

    void Start()
    {
        // Inicializar el LineRenderer y los puntos
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = pointCount;
        lineRenderer.startWidth = 0.1f;  // Grosor inicial de la línea
        lineRenderer.endWidth = 0.1f;    // Grosor final de la línea
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));  // Usar un material básico
        lineRenderer.startColor = UnityEngine.Color.red;
        lineRenderer.endColor = UnityEngine.Color.red;

        halfCaveThinkness = caveThinckness / 2;
        halfCaveThinknessInt = Mathf.RoundToInt(halfCaveThinkness);

        GenerateLine();
        DrawLine();
        //GenerateCaveDeletengTilesInLine();
    }

    // Genera los puntos de la línea
    void GenerateLine()
    {
        points = new LinkedList<Vector2>();

        // Primer punto en el origen o en cualquier punto inicial que desees
        points.AddLast(Vector2.zero);

        // Vector para la dirección inicial (puede ser cualquier dirección)
        Vector2 direction = Vector2.right;  // Dirección inicial hacia la derecha

        // Generar el resto de los puntos
        while (points.Count < pointCount)
        {
            bool validPoint = false;
            int attempts = 0;

            while (!validPoint && attempts < 100) // Limitar el número de intentos para evitar ciclos infinitos
            {
                // Cambiar la dirección suavemente
                direction = ChangeDirection(direction);

                // Crear el nuevo punto a lo largo de la dirección calculada
                Vector2 newPoint = points.Last.Value + direction * segmentLength;

                // Verificar que el nuevo punto no está demasiado cerca de los puntos anteriores
                if (!IsTooCloseToOtherPoints(newPoint))
                {
                    points.AddLast(newPoint);
                    validPoint = true;
                }

                attempts++;
            }

            if (!validPoint)
            {
                // Si no se encontró un punto válido después de muchos intentos, termina el proceso
                Debug.LogWarning("No se pudo encontrar un punto válido para la posición " + points.Count);
                break;
            }
        }
    }

    // Controla los cambios de dirección suavemente
    Vector2 ChangeDirection(Vector2 currentDirection)
    {
        // Cambiar el ángulo dentro del límite definido por maxAngleChange
        float angleChange = UnityEngine.Random.Range(-maxAngleChange, maxAngleChange);

        // Crear una nueva dirección rotada
        Vector2 newDirection = Quaternion.Euler(0, 0, angleChange) * currentDirection;

        // Normalizar la dirección para mantener el tamaño constante
        return newDirection.normalized;
    }

    // TODO: Verifica si el nuevo punto está demasiado cerca de los puntos anteriores
    bool IsTooCloseToOtherPoints(Vector2 newPoint)
    {
        /*
        float minDistance = 0f; // Puedes ajustar este valor según sea necesario
        int point1Pos=0;
        int point2Pos=0;

        foreach (var point in points)
        {   
            if (Vector2.Distance(newPoint, point) < minDistance)
            {
                return true; // El nuevo punto está demasiado cerca a algún punto anterior
            }
        }

        return false; // El nuevo punto está a una distancia segura de todos los puntos previos
        */
        return false;
    }

    // Dibuja la línea usando LineRenderer
    void DrawLine()
    {
        int index = 0;
        foreach (var point in points)
        {
            lineRenderer.SetPosition(index, point);
            index++;
        }
    }

    public void DeleteTilesCollidingWithFollowLine(int deleteRadius)
    {
        foreach (var point in points)
        {
            tileDrawer.DeleteTilesInSquare(roundVector2ToInt(point), deleteRadius);
        }
    }


    public Vector2 GeneratePointIfNeeded()
    {
        Vector3 playerPos = player.transform.position;
        Vector2 newPoint = Vector2.zero;
        bool flag = false;
        int pointsToClearWhenPlayerIsCloseDirection = 0;

        if (DistanceBetweenVector2AndVector3(points.First.Value, playerPos) < minPlayerToHeadTileDistance)
        {
            newPoint = GeneratePoint(points.First.Value, points.First.Next.Value);
            points.AddFirst(newPoint);
            points.RemoveLast();
        }   
        else
        {
            if (DistanceBetweenVector2AndVector3(points.Last.Value, playerPos) < minPlayerToHeadTileDistance)
            {
                newPoint = GeneratePoint(points.Last.Value, points.Last.Previous.Value);
                points.AddLast(newPoint);
                points.RemoveFirst();
            }
        }
        return newPoint;
    }

    Vector2 GeneratePoint(Vector2 prevPoint, Vector2 prevPrevPoint)
    {
        int attempts = 0;

        // Calculate and normalize the direction
        Vector2 direction = (prevPoint - prevPrevPoint).normalized;

        while (attempts < 100) // Limitar los intentos para evitar ciclos infinitos
        {
            // Suavemente cambiar la dirección
            direction = ChangeDirection(direction);

            // Generar el nuevo punto usando el punto anterior y la dirección normalizada
            Vector2 newPoint = prevPoint + direction * segmentLength;

            // Verificar si el nuevo punto está adecuadamente distanciado de otros puntos
            if (!IsTooCloseToOtherPoints(newPoint))
            {
                lastGenPoint = newPoint;
                DrawLine();
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