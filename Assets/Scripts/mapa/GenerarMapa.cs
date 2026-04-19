using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenerarMapa : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase caminoTile;
    public TileBase sueloTile;

    public TileBase bordeArriba;
    public TileBase bordeAbajo;
    public TileBase bordeIzquierda;
    public TileBase bordeDerecha;

    public TileBase esquinaArribaIzquierda;
    public TileBase esquinaArribaDerecha;
    public TileBase esquinaAbajoIzquierda;
    public TileBase esquinaAbajoDerecha;

    public int anchoMapa = 15;
    public int altoMapa = 15;

    public int minZigZag = 2;
    public int maxZigZag = 3;

    public int[,] mapa;
    public List<Vector3> waypoints;

    public event System.Action OnMapaGenerado;
    [Header("Ajustes de Posición")]
    public int offsetY = 5; // Cuántas casillas quieres que suba el mapa
    void Start()
    {
        GenerarMapaCompleto();
        OnMapaGenerado?.Invoke();
    }

    void GenerarMapaCompleto()
    {
        tilemap.ClearAllTiles();
        mapa = new int[anchoMapa, altoMapa];
        waypoints = new List<Vector3>();

        for (int x = 0; x < anchoMapa; x++)
        {
            for (int y = 0; y < altoMapa; y++)
            {
                // APLICAMOS EL OFFSET AQUÍ
                Vector3Int pos = new Vector3Int(x, y + offsetY, 0);

                if (x == 0 && y == 0) tilemap.SetTile(pos, esquinaAbajoIzquierda);
                else if (x == 0 && y == altoMapa - 1) tilemap.SetTile(pos, esquinaArribaIzquierda);
                else if (x == anchoMapa - 1 && y == 0) tilemap.SetTile(pos, esquinaAbajoDerecha);
                else if (x == anchoMapa - 1 && y == altoMapa - 1) tilemap.SetTile(pos, esquinaArribaDerecha);

                else if (y == altoMapa - 1) tilemap.SetTile(pos, bordeArriba);
                else if (y == 0) tilemap.SetTile(pos, bordeAbajo);
                else if (x == 0) tilemap.SetTile(pos, bordeIzquierda);
                else if (x == anchoMapa - 1) tilemap.SetTile(pos, bordeDerecha);

                else
                {
                    tilemap.SetTile(pos, sueloTile);
                    mapa[x, y] = 0;
                }

                if (x == 0 || y == 0 || x == anchoMapa - 1 || y == altoMapa - 1)
                    mapa[x, y] = 2;
            }
        }

        // Generar camino aleatorio
        Vector2Int inicio = new Vector2Int(1, Random.Range(1, altoMapa - 1));
        Vector2Int fin = new Vector2Int(anchoMapa - 2, Random.Range(1, altoMapa - 1));
        List<Vector2Int> camino = GenerarCamino(inicio, fin, Random.Range(minZigZag, maxZigZag + 1));

        foreach (var celda in camino)
        {
            // APLICAMOS EL OFFSET TAMBIÉN AL CAMINO
            Vector3Int posCelda = new Vector3Int(celda.x, celda.y + offsetY, 0);
            
            tilemap.SetTile(posCelda, caminoTile);
            mapa[celda.x, celda.y] = 1;

            Vector3 centroMundo = tilemap.GetCellCenterWorld(posCelda);
            centroMundo.z = -1f; 
            
            waypoints.Add(centroMundo);
        }
    }

    List<Vector2Int> GenerarCamino(Vector2Int inicio, Vector2Int fin, int zigzags)
{
    List<Vector2Int> path = new List<Vector2Int>();
    path.Add(inicio);
    Vector2Int actual = inicio;

    int totalZig = zigzags;
    int ancho = fin.x - inicio.x;
    
    // Calculamos el tamaño del segmento, pero restamos un margen de seguridad
    // para que los zigzags no toquen la columna del final (fin.x)
    int columnaLimite = fin.x - 1; 
    int segmento = (columnaLimite - inicio.x) / (totalZig + 1);

    for (int z = 0; z <= totalZig; z++)
    {
        // El targetX ahora respeta el límite de no acercarse al final antes de tiempo
        int targetX = inicio.x + segmento * (z + 1);
        
        // Evitamos por completo que targetX sea igual a fin.x
        if (targetX >= fin.x) targetX = fin.x - 1;

        int targetY = Random.Range(1, altoMapa - 1);

        // Movimiento en X
        int stepX = targetX > actual.x ? 1 : -1;
        while (actual.x != targetX)
        {
            actual.x += stepX;
            path.Add(new Vector2Int(actual.x, actual.y));
        }

        // Movimiento en Y
        int stepY = targetY > actual.y ? 1 : -1;
        while (actual.y != targetY)
        {
            actual.y += stepY;
            path.Add(new Vector2Int(actual.x, actual.y));
        }
    }

    // TRAMO FINAL: Una vez terminados los zigzags, vamos directos al final.
    // Primero terminamos de avanzar en X hasta la columna del final.
    int stepXFin = fin.x > actual.x ? 1 : -1;
    while (actual.x != fin.x)
    {
        actual.x += stepXFin;
        path.Add(new Vector2Int(actual.x, actual.y));
    }

    // Y finalmente ajustamos la Y si el punto final está a otra altura.
    int stepYFin = fin.y > actual.y ? 1 : -1;
    while (actual.y != fin.y)
    {
        actual.y += stepYFin;
        path.Add(new Vector2Int(actual.x, actual.y));
    }

    return path;
}
}
