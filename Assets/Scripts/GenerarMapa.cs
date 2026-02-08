using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenerarMapa : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase caminoTile;
    public TileBase sueloTile;

    // Borde
    public TileBase bordeArriba;
    public TileBase bordeAbajo;
    public TileBase bordeIzquierda;
    public TileBase bordeDerecha;

    // Opcional: esquinas
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

    void Start()
    {
        GenerarMapaCompleto();
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
                Vector3Int pos = new Vector3Int(x, y, 0);

                // Esquinas
                if (x == 0 && y == 0) tilemap.SetTile(pos, esquinaAbajoIzquierda);
                else if (x == 0 && y == altoMapa - 1) tilemap.SetTile(pos, esquinaArribaIzquierda);
                else if (x == anchoMapa - 1 && y == 0) tilemap.SetTile(pos, esquinaAbajoDerecha);
                else if (x == anchoMapa - 1 && y == altoMapa - 1) tilemap.SetTile(pos, esquinaArribaDerecha);

                // Bordes
                else if (y == altoMapa - 1) tilemap.SetTile(pos, bordeArriba);
                else if (y == 0) tilemap.SetTile(pos, bordeAbajo);
                else if (x == 0) tilemap.SetTile(pos, bordeIzquierda);
                else if (x == anchoMapa - 1) tilemap.SetTile(pos, bordeDerecha);

                // Suelo
                else
                {
                    tilemap.SetTile(pos, sueloTile);
                    mapa[x, y] = 0; // suelo
                }

                // Marcar los bordes en la matriz
                if (x == 0 || y == 0 || x == anchoMapa - 1 || y == altoMapa - 1)
                    mapa[x, y] = 2; // borde
            }
        }

        // Generar camino como antes
        Vector2Int inicio = new Vector2Int(1, Random.Range(1, altoMapa - 1));
        Vector2Int fin = new Vector2Int(anchoMapa - 2, Random.Range(1, altoMapa - 1));
        List<Vector2Int> camino = GenerarCamino(inicio, fin, Random.Range(minZigZag, maxZigZag + 1));

        foreach (var celda in camino)
        {
            tilemap.SetTile(new Vector3Int(celda.x, celda.y, 0), caminoTile);
            mapa[celda.x, celda.y] = 1; // camino
            waypoints.Add(tilemap.CellToWorld(new Vector3Int(celda.x, celda.y, 0)) + new Vector3(0.5f, 0.5f, 0));
        }
    }

    List<Vector2Int> GenerarCamino(Vector2Int inicio, Vector2Int fin, int zigzags)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        path.Add(inicio);
        Vector2Int actual = inicio;

        int totalZig = zigzags;
        int ancho = fin.x - inicio.x;
        int segmento = ancho / (totalZig + 1);

        for (int z = 0; z <= totalZig; z++)
        {
            int targetX = inicio.x + segmento * (z + 1);
            int targetY = Random.Range(1, altoMapa - 1);

            // Horizontal
            int stepX = targetX > actual.x ? 1 : -1;
            while (actual.x != targetX)
            {
                actual.x += stepX;
                path.Add(new Vector2Int(actual.x, actual.y));
            }

            // Vertical
            int stepY = targetY > actual.y ? 1 : -1;
            while (actual.y != targetY)
            {
                actual.y += stepY;
                path.Add(new Vector2Int(actual.x, actual.y));
            }
        }

        // Último tramo hasta fin
        int stepXFin = fin.x > actual.x ? 1 : -1;
        while (actual.x != fin.x)
        {
            actual.x += stepXFin;
            path.Add(new Vector2Int(actual.x, actual.y));
        }

        int stepYFin = fin.y > actual.y ? 1 : -1;
        while (actual.y != fin.y)
        {
            actual.y += stepYFin;
            path.Add(new Vector2Int(actual.x, actual.y));
        }

        return path;
    }
}
