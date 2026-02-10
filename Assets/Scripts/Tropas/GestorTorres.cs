using UnityEngine;

public class GestorTorres : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject prefabArquero;       
    public GenerarMapa generadorMapa;      

    private GameObject torreTemporal;      
    private bool modoColocacion = false;

    void Update()
    {
        if (!modoColocacion || torreTemporal == null) return;

        // 1. Posición del ratón corregida
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // 2. Calculamos la celda según el ratón
        Vector3Int celdaApuntada = generadorMapa.tilemap.WorldToCell(mousePos);
        
        // 3. Forzamos el centro de la celda para el dibujo
        Vector3 posFinalMundo = generadorMapa.tilemap.GetCellCenterWorld(celdaApuntada);
        posFinalMundo.z = -1f; 
        
        torreTemporal.transform.position = posFinalMundo;

        // 4. Feedback visual
        ActualizarColor(celdaApuntada);

        if (Input.GetMouseButtonDown(0))
            IntentarColocarTorre(celdaApuntada);

        if (Input.GetMouseButtonDown(1))
            CancelarColocacion();
    }

    // --- EL BLOQUEO REAL ESTÁ AQUÍ ---
    bool EsPosicionConstruible(Vector3Int celda)
    {
        // Si se sale del mapa, bloqueamos
        if (celda.x < 0 || celda.x >= generadorMapa.anchoMapa || 
            celda.y < 0 || celda.y >= generadorMapa.altoMapa) return false;

        // ACCEDEMOS A TU MATRIZ: 
        // 0 = Suelo, 1 = Camino, 2 = Borde, 3 = Torre
        int valorEnMatriz = generadorMapa.mapa[celda.x, celda.y];

        // SOLO permitimos si es exactamente 0
        return valorEnMatriz == 0;
    }

    void ActualizarColor(Vector3Int celda)
    {
        SpriteRenderer sr = torreTemporal.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        if (EsPosicionConstruible(celda))
            sr.color = new Color(1, 1, 1, 0.5f); // Blanco transparente
        else
            sr.color = new Color(1, 0, 0, 0.5f); // Rojo: PROHIBIDO
    }

    void IntentarColocarTorre(Vector3Int celda)
    {
        // Doble comprobación: si no es construible, salimos
        if (!EsPosicionConstruible(celda)) return;

        Torre torreScript = torreTemporal.GetComponent<Torre>();
        
        if (GameManager.instancia.GastarDinero(torreScript.coste))
        {
            // Marcamos la celda como ocupada por torre (3)
            generadorMapa.mapa[celda.x, celda.y] = 3; 

            SpriteRenderer sr = torreTemporal.GetComponent<SpriteRenderer>();
            sr.color = Color.white;
            sr.sortingOrder = 5; 
            
            torreScript.estaColocada = true;
            torreTemporal = null;
            modoColocacion = false;
        }
    }

    public void EmpezarColocacion()
    {
        if (modoColocacion) return;
        modoColocacion = true;
        torreTemporal = Instantiate(prefabArquero);
        torreTemporal.GetComponent<Torre>().estaColocada = false;
    }

    void CancelarColocacion()
    {
        if (torreTemporal != null) Destroy(torreTemporal);
        modoColocacion = false;
    }
}