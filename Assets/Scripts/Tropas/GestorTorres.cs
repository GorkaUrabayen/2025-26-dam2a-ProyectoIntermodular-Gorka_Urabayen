using UnityEngine;
using UnityEngine.Tilemaps;

public class GestorTorres : MonoBehaviour
{
    [Header("Prefabs y Referencias")]
    public GameObject prefabArquero;       
    public GenerarMapa generadorMapa;      

    private GameObject torreTemporal;      
    private bool modoColocacion = false;

    void Update()
    {
        // Si no estamos en modo colocación, no hacemos nada
        if (!modoColocacion || torreTemporal == null) return;

        // 1. Obtener posición del ratón en el mundo
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // 2. Convertir posición del mundo a coordenadas de la rejilla (Tilemap)
        Vector3Int celdaApuntada = generadorMapa.tilemap.WorldToCell(mousePos);
        
        // 3. Centrar la "torre fantasma" en la celda
        Vector3 posFinalMundo = generadorMapa.tilemap.GetCellCenterWorld(celdaApuntada);
        posFinalMundo.z = -1f; // Asegurar que esté por delante del fondo
        
        torreTemporal.transform.position = posFinalMundo;

        // 4. Feedback visual (Verde/Blanco si se puede, Rojo si no)
        ActualizarColor(celdaApuntada);

        // 5. Click Izquierdo: Confirmar colocación
        if (Input.GetMouseButtonDown(0))
        {
            IntentarColocarTorre(celdaApuntada);
        }

        // 6. Click Derecho: Cancelar
        if (Input.GetMouseButtonDown(1))
        {
            CancelarColocacion();
        }
    }

    // --- LÓGICA DE VALIDACIÓN ---
    bool EsPosicionConstruible(Vector3Int celda)
    {
        // Seguridad: Si el mapa no se ha generado aún
        if (generadorMapa.mapa == null) return false;

        // Comprobación de límites del array para evitar errores de "Index out of range"
        if (celda.x < 0 || celda.x >= generadorMapa.anchoMapa || 
            celda.y < 0 || celda.y >= generadorMapa.altoMapa) return false;

        // Consultamos tu matriz: 0 = Suelo, 1 = Camino, 2 = Borde, 3 = Torre ya puesta
        int valorEnMatriz = generadorMapa.mapa[celda.x, celda.y];

        // SOLO permitimos si es Suelo (0)
        return valorEnMatriz == 0;
    }

    void ActualizarColor(Vector3Int celda)
    {
        SpriteRenderer sr = torreTemporal.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        if (EsPosicionConstruible(celda))
        {
            // Se puede colocar: Color normal pero con transparencia
            sr.color = new Color(1, 1, 1, 0.6f); 
        }
        else
        {
            // No se puede: Tinte rojo transparente
            sr.color = new Color(1, 0.2f, 0.2f, 0.6f); 
        }
    }

    void IntentarColocarTorre(Vector3Int celda)
    {
        if (!EsPosicionConstruible(celda)) return;

        Torre torreScript = torreTemporal.GetComponent<Torre>();
        
        // Verificamos si tenemos dinero suficiente en el GameManager
        if (GameManager.instancia.GastarDinero(torreScript.coste))
        {
            // Marcamos la celda en la matriz como "Ocupada por Torre" (3)
            generadorMapa.mapa[celda.x, celda.y] = 3; 

            // Devolvemos el sprite a su estado sólido y visible
            SpriteRenderer sr = torreTemporal.GetComponent<SpriteRenderer>();
            sr.color = Color.white;
            sr.sortingOrder = 10; // Aseguramos que se vea sobre enemigos y mapa
            
            // Activamos la lógica de la torre
            torreScript.estaColocada = true;
            
            // Liberamos las variables para poder colocar otra después
            torreTemporal = null;
            modoColocacion = false;
        }
        else
        {
            Debug.Log("No tienes suficiente oro");
        }
    }

    // Método que debes llamar desde tu botón de la UI
    public void EmpezarColocacion()
    {
        if (modoColocacion) return; // Evitar crear múltiples torres a la vez

        modoColocacion = true;
        torreTemporal = Instantiate(prefabArquero);
        
        // Nos aseguramos de que la torre no empiece a disparar mientras la movemos
        Torre torreScript = torreTemporal.GetComponent<Torre>();
        if(torreScript != null) torreScript.estaColocada = false;
    }

    void CancelarColocacion()
    {
        if (torreTemporal != null) 
        {
            Destroy(torreTemporal);
        }
        modoColocacion = false;
    }
}