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

        // 1. Obtener posición del ratón
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // 2. Calcular celda
        Vector3Int celdaApuntada = generadorMapa.tilemap.WorldToCell(mouseWorldPos);

        // 3. Posicionamiento Visual
        Vector3 posFinal;
        if (forzarCentroRejilla)
        {
            posFinal = generadorMapa.tilemap.GetCellCenterWorld(celdaApuntada);
        }
        else
        {
            posFinal = mouseWorldPos;
        }

        // Aplicar correcciones
        posFinal += (Vector3)correccionVisual;
        posFinal.z = -1f; 

        torreTemporal.transform.position = posFinal;

        // 4. Feedback visual
        ActualizarColor(celdaApuntada);

        // 5. Click Izquierdo: Confirmar (Aquí estaba el error, ahora pasamos posFinal)
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

    // Corregido: Ahora esta función recibe la posición visual final para fijar la torre
    void IntentarColocarTorre(Vector3Int celda, Vector3 posVisual)
    {
        if (!EsPosicionConstruible(celda)) return;

        // Asegúrate de que el script "Torre" tenga una variable pública "coste"
        Torre torreScript = torreTemporal.GetComponent<Torre>();
        int precio = (torreScript != null) ? torreScript.coste : 20; 

        if (GameManager.instancia.GastarDinero(precio))
        {
            generadorMapa.mapa[celda.x, celda.y] = 3; // Marcamos como ocupado

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
        else
        {
            Debug.Log("No tienes suficiente dinero");
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