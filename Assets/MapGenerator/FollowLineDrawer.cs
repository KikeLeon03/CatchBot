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
    public int pointCount = 20;           // N�mero de puntos de la l�nea
    public float segmentLength = 2.0f;    // Distancia entre puntos
    public float maxAngleChange = 30f;    // �ngulo m�ximo para evitar cambios bruscos
    public LineRenderer lineRenderer;     // El LineRenderer que usaremos para dibujar la l�nea
    public TileDrawer tileDrawer;
    public int caveWidth = 5;
    public int caveThinckness = 40;
    public float maxUpAngleInPlatforms = 10f;
    public float minUpAngleInPlatforms = - 10f;
    private float maxUpAngleInPlatformsLeft;
    private float minUpAngleInPlatformsLeft;

    public Vector2 lastGenPoint;

    public LinkedList<Vector2> points;   // Los puntos que forman la l�nea

    private float halfCaveThinkness;
    private int halfCaveThinknessInt;

    public int pointsToClearWhenPlayerIsClose=8;

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

        maxUpAngleInPlatformsLeft = 180 - maxUpAngleInPlatforms;
        minUpAngleInPlatformsLeft = -180 - minUpAngleInPlatforms;

        halfCaveThinkness = caveThinckness / 2;
        halfCaveThinknessInt = Mathf.RoundToInt(halfCaveThinkness);

        GenerateLine();
        DrawLine();
        //GenerateCaveDeletengTilesInLine();
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
                direction = ChangeDirection(direction, "");

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
    Vector2 ChangeDirection(Vector2 currentDirection, string state)
    {
        // Cambiar el �ngulo dentro del l�mite definido por maxAngleChange
        float angleChange = UnityEngine.Random.Range(-maxAngleChange, maxAngleChange);

        // Crear una nueva direcci�n rotada
        Vector2 newDirection = Quaternion.Euler(0, 0, angleChange) * currentDirection;

        if (state == "platform")
        {
            float angleRight = Vector2.SignedAngle(Vector2.right, newDirection);

            // Verifica si apunta demasiado hacia arriba o demasiado hacia abajo
            if ((angleRight > maxUpAngleInPlatforms && angleRight < maxUpAngleInPlatformsLeft) || (angleRight < minUpAngleInPlatforms && angleRight > minUpAngleInPlatformsLeft))
            {
                return ChangeDirection(currentDirection, state).normalized;
            }
        }

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

    public void DeleteTilesCollidingWithFollowLine(int deleteRadius)
    {
        foreach (var point in points)
        {
            tileDrawer.DeleteTilesInSquare(roundVector2ToInt(point), deleteRadius);
        }
    }

    public void DeleteXTilesCollidingWithFollowLine(int X, int deleteRadius)
    {
        if(X > 0)
        {
            LinkedListNode<Vector2> t = points.First;
            while(X > 0)
            {
                tileDrawer.DeleteTilesInSquare(roundVector2ToInt(t.Value), deleteRadius);
                t = t.Next;
                X--;
            }

        }
        if(X < 0)
        {
            LinkedListNode<Vector2> t = points.Last;
            while (X < 0)
            {
                tileDrawer.DeleteTilesInSquare(roundVector2ToInt(t.Value), deleteRadius);
                t = t.Previous;
                X++;
            }
        }
    }


    public (Vector2,bool) GeneratePointIfNeeded(string state)
    {
        Vector3 playerPos = player.transform.position;
        Vector2 newPoint = Vector2.zero;
        bool flag = false;
        int pointsToClearWhenPlayerIsCloseDirection = 0;

        if (DistanceBetweenVector2AndVector3(points.First.Value, playerPos) < minPlayerToHeadTileDistance)
        {
            flag = true;
            newPoint = GeneratePoint(points.First.Value, points.First.Next.Value, state);
            points.AddFirst(newPoint);
            points.RemoveLast();
            DrawLine();

        }
        else
        {
            if (DistanceBetweenVector2AndVector3(points.Last.Value, playerPos) < minPlayerToHeadTileDistance)
            {
                flag = true;
                newPoint = GeneratePoint(points.Last.Value, points.Last.Previous.Value, state);
                points.AddLast(newPoint);
                points.RemoveFirst();
                DrawLine();

            }
        }
        return (newPoint, flag);
    }

    Vector2 GeneratePoint(Vector2 prevPoint, Vector2 prevPrevPoint, string state)
    {
        int attempts = 0;

        // Calculate and normalize the direction
        Vector2 direction = (prevPoint - prevPrevPoint).normalized;

        while (attempts < 100) // Limitar los intentos para evitar ciclos infinitos
        {
            // Suavemente cambiar la direcci�n
            direction = ChangeDirection(direction, state);

            // Generar el nuevo punto usando el punto anterior y la direcci�n normalizada
            Vector2 newPoint = prevPoint + direction * segmentLength;

            // Verificar si el nuevo punto est� adecuadamente distanciado de otros puntos
            if (!IsTooCloseToOtherPoints(newPoint))
            {
                lastGenPoint = newPoint;
                return newPoint;
            }

            attempts++;
        }

        // Devuelve un valor por defecto si fallan los intentos
        return Vector2.one; // Or any appropriate error vector
    }

    public LinkedListNode<Vector2>  GetMiddlePoint()
    {
        LinkedListNode<Vector2> middlePoint = points.First;

        float midPointPos = pointCount / 2;
        int i = 0;
        while (i < midPointPos){
            middlePoint = middlePoint.Next;
            i++;
        }
        return middlePoint;
    }

    public void ClearFromMiddle(int pointsToClear, int clearRadious) {
        LinkedListNode<Vector2> middle = GetMiddlePoint();
        LinkedListNode<Vector2> backMiddle = middle;
        LinkedListNode<Vector2> frontMiddle = middle;

        tileDrawer.DeleteTilesInSquare(new Vector2Int(Mathf.RoundToInt(middle.Value.x), Mathf.RoundToInt(middle.Value.y)), clearRadious);
        while (pointsToClear > 0)
        {
            pointsToClear--;

            backMiddle = backMiddle.Previous;
            frontMiddle = frontMiddle.Next;

            tileDrawer.DeleteTilesInSquare(new Vector2Int(Mathf.RoundToInt(backMiddle.Value.x), Mathf.RoundToInt(backMiddle.Value.y)), clearRadious);
            tileDrawer.DeleteTilesInSquare(new Vector2Int(Mathf.RoundToInt(frontMiddle.Value.x), Mathf.RoundToInt(frontMiddle.Value.y)), clearRadious);
        }
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