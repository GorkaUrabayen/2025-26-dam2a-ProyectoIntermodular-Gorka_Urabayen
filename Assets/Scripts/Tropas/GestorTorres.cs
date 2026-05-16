using UnityEngine;
using UnityEngine.Tilemaps;
// Gestiona el sistema de construcción de torres.
// Se encarga del posicionamiento en rejilla , validación de celdas
// construibles y comunicación con la matriz lógica del mapa.
public class GestorTorres : MonoBehaviour
{
    [Header("Prefabs de Torres")]
    public GameObject prefabArqueroNormal;
    public GameObject prefabArqueroRojo;

    [Header("Referencias")]
    public GenerarMapa generadorMapa;

    [Header("Ajustes Visuales")]
    public Vector2 correccionVisual = Vector2.zero; 
    public bool forzarCentroRejilla = true; 

    private GameObject torreTemporal;
    private GameObject prefabSeleccionado; 
    private bool modoColocacion = false;

    void Update()
    {
        // Solo procesamos la lógica si el jugador ha seleccionado una torre para colocar
        if (!modoColocacion || torreTemporal == null) return;
        // FASE 1: Conversión de coordenadas de Ratón (Pantalla) a Mundo
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        // Obtenemos la celda del Tilemap correspondiente a la posición del ratón
        Vector3Int celdaApuntada = generadorMapa.tilemap.WorldToCell(mouseWorldPos);
        // FASE 2: Cálculo de la posición objetivo (Snapping)
        Vector3 posObjetivo;
        if (forzarCentroRejilla)
        {
            // Ajusta la torre exactamente al centro de la celda de la rejilla
            posObjetivo = generadorMapa.tilemap.GetCellCenterWorld(celdaApuntada);
        }
        else
        {
            // Movimiento suavizado aplicando la sensibilidad guardada en PlayerPrefs
            float sens = PlayerPrefs.GetFloat("SensibilidadRaton", 1.0f);
            posObjetivo = Vector3.Lerp(torreTemporal.transform.position, mouseWorldPos, Time.deltaTime * 15f * sens);
        }
        // Aplicamos correcciones visuales y profundidad (Z=-1 para estar sobre el suelo)
        Vector3 posFinal = posObjetivo + (Vector3)correccionVisual;
        posFinal.z = -1f; 

        torreTemporal.transform.position = posFinal;
        // FASE 3: Feedback visual (Color de validación)
        ActualizarColor(celdaApuntada);
        // FASE 4: Confirmación o Cancelación
        if (Input.GetMouseButtonDown(0))
        {
            IntentarColocarTorre(celdaApuntada, posFinal);
        }

        if (Input.GetMouseButtonDown(1))
        {
            CancelarColocacion();
        }
    }
    // Valida si una celda es apta para la construcción basándose en la matriz lógica.
    // Traduce las coordenadas del mundo de Unity a las coordenadas de nuestra matriz mapa.
    bool EsPosicionConstruible(Vector3Int celda)
    {
        if (generadorMapa.mapa == null) return false;
        // IMPORTANTE: Ajustamos la coordenada Y restando el offsetY para que coincida con la matriz [x,y]
        int xLogica = celda.x;
        int yLogica = celda.y - generadorMapa.offsetY; 
        // Comprobación de límites de la matriz
        if (xLogica < 0 || xLogica >= generadorMapa.anchoMapa ||
            yLogica < 0 || yLogica >= generadorMapa.altoMapa) return false;
        // Consultamos el estado en la matriz: 0 significa Suelo Libre
        return generadorMapa.mapa[xLogica, yLogica] == 0; 
    }
    // Cambia el color del "fantasma" de la torre para indicar si la posición es válida.
    // Blanco/Transparente = OK, Rojo = Bloqueado.
    void ActualizarColor(Vector3Int celda)
    {
        if (torreTemporal == null) return;
        SpriteRenderer sr = torreTemporal.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        if (EsPosicionConstruible(celda))
            sr.color = new Color(1, 1, 1, 0.6f); 
        else
            sr.color = new Color(1, 0.2f, 0.2f, 0.6f); 
    }
    // Finaliza el proceso de construcción, resta el dinero y actualiza la matriz.
    void IntentarColocarTorre(Vector3Int celda, Vector3 posVisual)
    {
        
        int xLogica = celda.x;
        int yLogica = celda.y - generadorMapa.offsetY;

       
        if (!EsPosicionConstruible(celda)) return;
        Torre torreScript = torreTemporal.GetComponent<Torre>();
        if (torreScript == null) return;
        // Comprobación de economía mediante el GameManager (Singleton)
        if (GameManager.instancia.GastarDinero(torreScript.coste))
        {   
            // Marcamos la celda como Ocupada (Estado 3) para impedir futuras construcciones ahí
            generadorMapa.mapa[xLogica, yLogica] = 3; 

            torreTemporal.transform.position = posVisual;
            // Restauramos la opacidad del sprite
            SpriteRenderer sr = torreTemporal.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.white;
                sr.sortingOrder = 10;
            }
            // Activamos la funcionalidad de la torre
            torreScript.estaColocada = true;
            torreTemporal = null;
            modoColocacion = false;
        }
    }
    // Inicia el modo de construcción según el tipo de torre seleccionado en la UI.
    public void EmpezarColocacion(int tipo)
    {
        if (modoColocacion) return;

        if (tipo == 0) prefabSeleccionado = prefabArqueroNormal;
        else if (tipo == 1) prefabSeleccionado = prefabArqueroRojo;

        if (prefabSeleccionado == null) return;

        modoColocacion = true;
        torreTemporal = Instantiate(prefabSeleccionado);
        
        Torre torreScript = torreTemporal.GetComponent<Torre>();
        if (torreScript != null) torreScript.estaColocada = false;
    }

    void CancelarColocacion()
    {
        if (torreTemporal != null) Destroy(torreTemporal);
        modoColocacion = false;
        prefabSeleccionado = null;
    }
}