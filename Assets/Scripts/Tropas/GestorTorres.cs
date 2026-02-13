using UnityEngine;
using UnityEngine.Tilemaps;

public class GestorTorres : MonoBehaviour
{
    [Header("Prefabs y Referencias")]
    public GameObject prefabArquero;
    public GenerarMapa generadorMapa;

    [Header("Ajustes Visuales")]
    public Vector2 correccionVisual = Vector2.zero; 
    public bool forzarCentroRejilla = true; 

    private GameObject torreTemporal;
    private bool modoColocacion = false;

    void Update()
    {
        if (!modoColocacion || torreTemporal == null) return;

        // 1. Obtener posición del ratón y Sensibilidad
        float sens = PlayerPrefs.GetFloat("SensibilidadRaton", 1.0f);
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // 2. Calcular celda
        Vector3Int celdaApuntada = generadorMapa.tilemap.WorldToCell(mouseWorldPos);

        // 3. Posicionamiento Visual
        Vector3 posObjetivo;
        if (forzarCentroRejilla)
        {
            posObjetivo = generadorMapa.tilemap.GetCellCenterWorld(celdaApuntada);
        }
        else
        {
            // Si no forzamos al centro, usamos la sensibilidad para suavizar el seguimiento
            // A más sensibilidad, el 'Lerp' es más rápido.
            posObjetivo = Vector3.Lerp(torreTemporal.transform.position, mouseWorldPos, Time.deltaTime * 15f * sens);
        }

        // Aplicar correcciones
        Vector3 posFinal = posObjetivo + (Vector3)correccionVisual;
        posFinal.z = -1f; 

        torreTemporal.transform.position = posFinal;

        // 4. Feedback visual
        ActualizarColor(celdaApuntada);

        // 5. Click Izquierdo: Confirmar
        if (Input.GetMouseButtonDown(0))
        {
            IntentarColocarTorre(celdaApuntada, posFinal);
        }

        // 6. Click Derecho: Cancelar
        if (Input.GetMouseButtonDown(1))
        {
            CancelarColocacion();
        }
    }

    bool EsPosicionConstruible(Vector3Int celda)
    {
        if (generadorMapa.mapa == null) return false;

        if (celda.x < 0 || celda.x >= generadorMapa.anchoMapa ||
            celda.y < 0 || celda.y >= generadorMapa.altoMapa) return false;

        return generadorMapa.mapa[celda.x, celda.y] == 0; 
    }

    void ActualizarColor(Vector3Int celda)
    {
        SpriteRenderer sr = torreTemporal.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        if (EsPosicionConstruible(celda))
            sr.color = new Color(1, 1, 1, 0.6f);
        else
            sr.color = new Color(1, 0.2f, 0.2f, 0.6f);
    }

    void IntentarColocarTorre(Vector3Int celda, Vector3 posVisual)
    {
        if (!EsPosicionConstruible(celda)) return;

        Torre torreScript = torreTemporal.GetComponent<Torre>();
        int precio = (torreScript != null) ? torreScript.coste : 20; 

        if (GameManager.instancia.GastarDinero(precio))
        {
            generadorMapa.mapa[celda.x, celda.y] = 3; 

            torreTemporal.transform.position = posVisual;

            SpriteRenderer sr = torreTemporal.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.white;
                sr.sortingOrder = 10;
            }

            if (torreScript != null) torreScript.estaColocada = true;
            
            torreTemporal = null;
            modoColocacion = false;
        }
    }

    public void EmpezarColocacion()
    {
        if (modoColocacion) return;

        modoColocacion = true;
        torreTemporal = Instantiate(prefabArquero);
        
        Torre torreScript = torreTemporal.GetComponent<Torre>();
        if (torreScript != null) torreScript.estaColocada = false;
    }

    void CancelarColocacion()
    {
        if (torreTemporal != null) Destroy(torreTemporal);
        modoColocacion = false;
    }
}