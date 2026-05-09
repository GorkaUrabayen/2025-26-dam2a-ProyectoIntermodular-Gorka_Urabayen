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
    private GameObject prefabSeleccionado; 
    private bool modoColocacion = false;

    void Update()
    {
        if (!modoColocacion || torreTemporal == null) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector3Int celdaApuntada = generadorMapa.tilemap.WorldToCell(mouseWorldPos);

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

        Vector3 posFinal = posObjetivo + (Vector3)correccionVisual;
        posFinal.z = -1f; 

        torreTemporal.transform.position = posFinal;

        ActualizarColor(celdaApuntada);

        if (Input.GetMouseButtonDown(0))
        {
            IntentarColocarTorre(celdaApuntada, posFinal);
        }

        if (Input.GetMouseButtonDown(1))
        {
            CancelarColocacion();
        }
    }

    bool EsPosicionConstruible(Vector3Int celda)
    {
        if (generadorMapa.mapa == null) return false;

        int xLogica = celda.x;
        int yLogica = celda.y - generadorMapa.offsetY; 

        if (xLogica < 0 || xLogica >= generadorMapa.anchoMapa ||
            yLogica < 0 || yLogica >= generadorMapa.altoMapa) return false;

        return generadorMapa.mapa[xLogica, yLogica] == 0; 
    }

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

    void IntentarColocarTorre(Vector3Int celda, Vector3 posVisual)
    {
        // 1. Calculamos las coordenadas lógicas
        int xLogica = celda.x;
        int yLogica = celda.y - generadorMapa.offsetY;

        // 2. Validamos si se puede construir
        if (!EsPosicionConstruible(celda)) return;

        Torre torreScript = torreTemporal.GetComponent<Torre>();
        if (torreScript == null) return;

        if (GameManager.instancia.GastarDinero(torreScript.coste))
        {
            // 3. Marcamos la matriz lógica como ocupada (3)
            generadorMapa.mapa[xLogica, yLogica] = 3; 

            torreTemporal.transform.position = posVisual;

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
            Debug.Log("Dinero insuficiente.");
        }
    }

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