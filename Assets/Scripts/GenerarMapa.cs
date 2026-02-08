using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenerarMapa : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase caminoTile;
    public TileBase bordeTile;
    public TileBase sueloTile;

    public int anchoMapa = 15;
    public int altoMapa = 15;

    public int minZigZag = 2;
    public int maxZigZag = 3;

    // Matriz del mapa
    public int[,] mapa;

    // Lista de waypoints para los enemigos
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

        // 1️⃣ Pintar bordes y suelo
        for (int x = 0; x < anchoMapa; x++)
        {
            for (int y = 0; y < altoMapa; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (x == 0 || y == 0 || x == anchoMapa - 1 || y == altoMapa - 1)
                {
                    tilemap.SetTile(pos, bordeTile);
                    mapa[x, y] = 2; // borde
                }
                else
                {
                    tilemap.SetTile(pos, sueloTile);
                    mapa[x, y] = 0; // suelo
                }
            }
        }

        // 2️⃣ Elegir inicio y fin del camino
        Vector2Int inicio = new Vector2Int(1, Random.Range(1, altoMapa - 1));
        Vector2Int fin = new Vector2Int(anchoMapa - 2, Random.Range(1, altoMapa - 1));

        // 3️⃣ Generar camino
        List<Vector2Int> camino = GenerarCamino(inicio, fin, Random.Range(minZigZag, maxZigZag + 1));

        // 4️⃣ Pintar camino y rellenar matriz / waypoints
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
