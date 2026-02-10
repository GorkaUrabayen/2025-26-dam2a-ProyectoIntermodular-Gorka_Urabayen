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

        // 1. Obtener posición del ratón en el mundo
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
        mousePos.z = 0f;

        // 2. Convertir a coordenadas de celda del Tilemap
        // Esto es lo que soluciona que el clic "elija la de arriba"
        Vector3Int celdaActual = generadorMapa.tilemap.WorldToCell(mousePos);
        
        // 3. Posicionar el fantasma en el centro real de la celda
        Vector3 posCentrada = generadorMapa.tilemap.GetCellCenterWorld(celdaActual);
        posCentrada.z = -1f; 
        
        torreTemporal.transform.position = posCentrada;

        // 4. Feedback visual (Rojo si no se puede colocar)
        ActualizarColor(celdaActual);

        if (Input.GetMouseButtonDown(0))
            IntentarColocarTorre(celdaActual);

        if (Input.GetMouseButtonDown(1))
            CancelarColocacion();
    }

    bool EsPosicionValida(Vector3Int celda)
    {
        // Validar que no nos salgamos del array 'mapa'
        if (celda.x < 0 || celda.x >= generadorMapa.anchoMapa || 
            celda.y < 0 || celda.y >= generadorMapa.altoMapa) return false;

        // Comprobar en tu matriz GenerarMapa.mapa
        // 0 = Suelo libre, 1 = Camino, 2 = Borde, 3 = Torre ya ocupada
        return generadorMapa.mapa[celda.x, celda.y] == 0;
    }

    void ActualizarColor(Vector3Int celda)
    {
        SpriteRenderer sr = torreTemporal.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // Si la posición es válida (valor 0), blanco transparente. Si no, rojo.
        sr.color = EsPosicionValida(celda) ? new Color(1, 1, 1, 0.5f) : new Color(1, 0, 0, 0.5f);
    }

    void IntentarColocarTorre(Vector3Int celda)
    {
        if (!EsPosicionValida(celda))
        {
            Debug.Log("No puedes colocar aquí: camino u ocupado");
            return;
        }

        Torre torreScript = torreTemporal.GetComponent<Torre>();
        if (GameManager.instancia.GastarDinero(torreScript.coste))
        {
            // MARCAR CELDA COMO OCUPADA (Valor 3)
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