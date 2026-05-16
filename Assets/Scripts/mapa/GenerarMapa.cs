using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
// Sistema de generación procedimental de escenarios.
// Crea de forma dinámica la topología del nivel, los límites visuales (bordes) 
// y una ruta aleatoria (camino) para los enemigos utilizando una matriz lógica.
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

    public int minZigZag = 2; // Cantidad mínima de cambios de dirección
    public int maxZigZag = 3; // Cantidad máxima de cambios de dirección

    // Matriz lógica que representa el estado de cada celda:
    // 0 = Suelo (Construible), 1 = Camino (No construible), 2 = Obstáculo/Borde.    
    public int[,] mapa;
    // Lista de coordenadas en el mundo que los enemigos seguirán.
    public List<Vector3> waypoints;
    // Evento para notificar a otros sistemas (como el Spawner) que el mapa está listo
    public event System.Action OnMapaGenerado;
    [Header("Ajustes de Posición")]
    public int offsetY = 5; // Cuántas casillas quieres que suba el mapa
    void Start()
    {
        GenerarMapaCompleto();
        OnMapaGenerado?.Invoke();
    }

    // Ejecuta el pipeline de generación: limpia el tilemap, dibuja los bordes, rellena el suelo y traza el camino aleatorio.
    void GenerarMapaCompleto()
    {
        tilemap.ClearAllTiles();
        mapa = new int[anchoMapa, altoMapa];
        waypoints = new List<Vector3>();
        // FASE 1: Construcción de la rejilla base y marcos decorativos
        for (int x = 0; x < anchoMapa; x++)
        {
            for (int y = 0; y < altoMapa; y++)
            {
                // Aplicamos el Offset para que las coordenadas lógicas (0,0) 
                // se dibujen en la posición visual correcta (x, y + offsetY)
                Vector3Int pos = new Vector3Int(x, y + offsetY, 0);
                // Lógica de asignación de Tiles de borde y esquinas
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
                    // Relleno de área construible
                    tilemap.SetTile(pos, sueloTile);
                    mapa[x, y] = 0;
                }
                // Definimos los bordes como zonas no construibles (Estado 2)
                if (x == 0 || y == 0 || x == anchoMapa - 1 || y == altoMapa - 1)
                    mapa[x, y] = 2;
            }
        }

        // FASE 2: Generación del camino aleatorio (Algoritmo de Zig-Zag)
        // Definimos puntos de inicio y fin dentro de los márgenes seguros
        Vector2Int inicio = new Vector2Int(1, Random.Range(1, altoMapa - 1));
        Vector2Int fin = new Vector2Int(anchoMapa - 2, Random.Range(1, altoMapa - 1));
        List<Vector2Int> camino = GenerarCamino(inicio, fin, Random.Range(minZigZag, maxZigZag + 1));
        // FASE 3: Volcado de la ruta al Tilemap y generación de Waypoints
        foreach (var celda in camino)
        {
            // APLICAMOS EL OFFSET TAMBIÉN AL CAMINO
            Vector3Int posCelda = new Vector3Int(celda.x, celda.y + offsetY, 0);
            
            tilemap.SetTile(posCelda, caminoTile);
            mapa[celda.x, celda.y] = 1;
            // Convertimos la posición de la rejilla en coordenadas de mundo para la IA
            Vector3 centroMundo = tilemap.GetCellCenterWorld(posCelda);
            centroMundo.z = -1f; 
            
            waypoints.Add(centroMundo);
        }
    }
    // Algoritmo de trazado de rutas basado en segmentos horizontales y saltos verticales.
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
    // Generación de puntos intermedios aleatorios (nodos del zigzag)
    for (int z = 0; z <= totalZig; z++)
    {
        // El targetX ahora respeta el límite de no acercarse al final antes de tiempo
        int targetX = inicio.x + segmento * (z + 1);
        
        // Evitamos por completo que targetX sea igual a fin.x
        if (targetX >= fin.x) targetX = fin.x - 1;

        int targetY = Random.Range(1, altoMapa - 1);

        // Avance en el eje X (Horizontal)
        int stepX = targetX > actual.x ? 1 : -1;
        while (actual.x != targetX)
        {
            actual.x += stepX;
            path.Add(new Vector2Int(actual.x, actual.y));
        }

       // Avance en el eje Y (Vertical)
        int stepY = targetY > actual.y ? 1 : -1;
        while (actual.y != targetY)
        {
            actual.y += stepY;
            path.Add(new Vector2Int(actual.x, actual.y));
        }
    }

    // TRAMO FINAL: Conexión forzada con el punto de destino
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
