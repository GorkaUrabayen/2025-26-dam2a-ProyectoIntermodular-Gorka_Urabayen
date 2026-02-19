using UnityEngine;
using UnityEngine.Tilemaps;

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
    private GameObject prefabSeleccionado; // Guarda qué torre estamos intentando poner
    private bool modoColocacion = false;

    void Update()
    {
        // Si no estamos en modo colocación o no hay torre que mover, salimos
        if (!modoColocacion || torreTemporal == null) return;

        // 1. Obtener posición del ratón en el mundo
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // 2. Calcular celda del Tilemap
        Vector3Int celdaApuntada = generadorMapa.tilemap.WorldToCell(mouseWorldPos);

        // 3. Posicionamiento Visual
        Vector3 posObjetivo;
        if (forzarCentroRejilla)
        {
            posObjetivo = generadorMapa.tilemap.GetCellCenterWorld(celdaApuntada);
        }
        else
        {
            float sens = PlayerPrefs.GetFloat("SensibilidadRaton", 1.0f);
            posObjetivo = Vector3.Lerp(torreTemporal.transform.position, mouseWorldPos, Time.deltaTime * 15f * sens);
        }

        // Aplicar correcciones de profundidad y offset
        Vector3 posFinal = posObjetivo + (Vector3)correccionVisual;
        posFinal.z = -1f; 

        torreTemporal.transform.position = posFinal;

        // 4. Feedback visual (Verde si se puede, Rojo si no)
        ActualizarColor(celdaApuntada);

        // 5. Click Izquierdo: Intentar colocarla
        if (Input.GetMouseButtonDown(0))
        {
            IntentarColocarTorre(celdaApuntada, posFinal);
        }

        // 6. Click Derecho: Cancelar compra
        if (Input.GetMouseButtonDown(1))
        {
            CancelarColocacion();
        }
    }

    bool EsPosicionConstruible(Vector3Int celda)
    {
        if (generadorMapa.mapa == null) return false;

        // Verificar límites del mapa
        if (celda.x < 0 || celda.x >= generadorMapa.anchoMapa ||
            celda.y < 0 || celda.y >= generadorMapa.altoMapa) return false;

        // Solo se puede construir si el valor en el mapa es 0 (suelo libre)
        return generadorMapa.mapa[celda.x, celda.y] == 0; 
    }

    void ActualizarColor(Vector3Int celda)
    {
        SpriteRenderer sr = torreTemporal.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        if (EsPosicionConstruible(celda))
            sr.color = new Color(1, 1, 1, 0.6f); // Blanco semitransparente
        else
            sr.color = new Color(1, 0.2f, 0.2f, 0.6f); // Rojo semitransparente
    }

    void IntentarColocarTorre(Vector3Int celda, Vector3 posVisual)
    {
        if (!EsPosicionConstruible(celda)) return;

        Torre torreScript = torreTemporal.GetComponent<Torre>();
        if (torreScript == null) return;

        // Verificamos si tenemos dinero suficiente usando el coste del script de la torre
        if (GameManager.instancia.GastarDinero(torreScript.coste))
        {
            // Marcamos el mapa para que no se pueda poner otra torre encima
            generadorMapa.mapa[celda.x, celda.y] = 3; 

            torreTemporal.transform.position = posVisual;

            // Devolvemos el color normal y activamos la torre
            SpriteRenderer sr = torreTemporal.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.white;
                sr.sortingOrder = 10;
            }

            torreScript.estaColocada = true;
            
            torreTemporal = null;
            modoColocacion = false;
        }
        else
        {
            Debug.Log("Dinero insuficiente para esta torre.");
        }
    }

    // Esta función es la que debes llamar desde tus botones
    // tipo 0 = Arquero Normal, tipo 1 = Arquero Negro
    public void EmpezarColocacion(int tipo)
    {
        if (modoColocacion) return;

        // Selección de prefab
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