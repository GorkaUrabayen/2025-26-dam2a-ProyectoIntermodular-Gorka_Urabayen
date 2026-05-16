using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
// Núcleo central del juego (GameBrain). 
// Gestiona el estado global, la economía, el flujo de niveles, las transiciones
// y la reconexión automática de la interfaz de usuario.
public class GameManager : MonoBehaviour
{
    // Instancia estática para acceso global desde cualquier otro script
    public static GameManager instancia;

    [Header("Configuración de Progresión")]
    public int nivelMaximo = 10;
    public int dineroBase = 50;
    public int dineroPorNivel = 10;
    public int vidasIniciales = 10;

    [Header("Stats Actuales")]
    public int vidas;
    public int dinero;
    public int enemigosRestantes = 0;

    [Header("UI References")]
    public TMP_Text txtVidas;
    public TMP_Text txtDinero;
    public GameObject panelPausa; 

    [Header("Cursor Settings")]
    public Texture2D cursorSprite;
    public Vector2 hotSpot = Vector2.zero;

    private bool juegoPausado = false;

    void Awake()
    {
        // PATRÓN SINGLETON: Garantiza que solo exista un GameManager en todo el juego.
        if (instancia == null)
        {
            instancia = this;
            // Persistencia: El objeto no se destruye al cargar nuevas escenas
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Si aparece un segundo GameManager (por ejemplo, al volver al menú), se elimina automáticamente
            Destroy(gameObject);
            return; // Importante para que no ejecute nada más si es un duplicado
        }
    }
    // Suscripción a eventos del SceneManager para detectar cambios de escena
    void OnEnable() { SceneManager.sceneLoaded += AlCargarEscena; }
    void OnDisable() { SceneManager.sceneLoaded -= AlCargarEscena; }

    void Update()
    {
        // Control de pausa mediante teclado (Barra Espaciadora)
        string escena = SceneManager.GetActiveScene().name;
        bool esEscenaDeJuego = escena != "EscenaInicio" && escena != "Derrota" && escena != "Victoria";

        if (Input.GetKeyDown(KeyCode.Space) && esEscenaDeJuego)
        {
            AlternarPausa();
        }
    }

    // Wrapper para cargar escenas utilizando un sistema de transiciones (Fade u otros).    
    private void CargarEscenaConTransicion(string nombreEscena)
    {
        // Busca el script de transición en la escena actual
        TransicionEscena trans = FindObjectOfType<TransicionEscena>();

        if (trans != null)
        {
            trans.IniciarTransicion(nombreEscena);
        }
        else
        {
            // Si por algún motivo no hay panel de transición, carga directo
            SceneManager.LoadScene(nombreEscena);
        }
    }

    public void AlternarPausa()
    {
        if (panelPausa == null) ReconectarUI();

        juegoPausado = !juegoPausado;
        // Manipulación de la escala de tiempo: 0 = Congelado, 1 = Tiempo real
        Time.timeScale = juegoPausado ? 0f : 1f;

        if (panelPausa != null) 
            panelPausa.SetActive(juegoPausado);
    }
    // Se ejecuta automáticamente cada vez que se carga una escena nueva.
    // Resetea estados, cambia la música y recalcula la economía del nivel.
    void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        // Reset de estado básico
        Time.timeScale = 1f; 
        juegoPausado = false;

        // Gestión centralizada de música según la escena actual
        if (AudioManager.instancia != null)
        {
            if (escena.name == "Derrota") AudioManager.instancia.PlayMusicaDerrota();
            else if (escena.name == "Victoria") AudioManager.instancia.PlayMusicaVictoria();
            else if (escena.name == "EscenaInicio") AudioManager.instancia.PlayMusicaMenu();
            else AudioManager.instancia.PlayMusicaNivel();
        }

        // Lógica de progresión: Ajustamos el dinero inicial según el número de nivel
        if (escena.name.StartsWith("Nivel"))
        {
            string numeroString = escena.name.Replace("Nivel", "");
            if (int.TryParse(numeroString, out int numNivel))
            {
                vidas = vidasIniciales;
                dinero = dineroBase + (numNivel * dineroPorNivel);
            }
        }

        enemigosRestantes = 0;
        
        // Al cargar escena, las referencias de UI se pierden (son de la escena anterior).
        // Las limpiamos y lanzamos la corrutina de reconexión.
        txtVidas = null;
        txtDinero = null;
        panelPausa = null; 

        StopAllCoroutines(); 
        StartCoroutine(BuscarUIPorPasos());
        ConfigurarCursor();
    }
    // Corrutina para asegurar que la UI se encuentre incluso si tarda unos frames en instanciarse.
    IEnumerator BuscarUIPorPasos()
    {
        yield return new WaitForEndOfFrame();
        ReconectarUI();
        yield return new WaitForSeconds(0.1f);
        if (panelPausa == null) ReconectarUI();
    }
    // Sistema de "Auto-Binding". Busca en la jerarquía los elementos de UI por nombre
    // y les asigna los listeners a los botones dinámicamente.
    private void ReconectarUI()
    {
        if (instancia != this) return;

        // Búsqueda de objetos incluso si están desactivados (Resources.FindObjectsOfTypeAll)
        TMP_Text[] todosLosTextos = Resources.FindObjectsOfTypeAll<TMP_Text>();
        foreach (TMP_Text t in todosLosTextos)
        {
            if (t.gameObject.scene.name == null) continue;
            if (t.gameObject.name == "TextoVidas") txtVidas = t;
            if (t.gameObject.name == "TextoDinero") txtDinero = t;
        }

        
        GameObject[] todosLosGos = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in todosLosGos)
        {
            if (go.gameObject.scene.name == null) continue;

            if (go.name == "MenuPausa") 
            {
                panelPausa = go;
                panelPausa.SetActive(false);
            }
            // Asignación dinámica de funciones a botones de la interfaz
            Button btn = go.GetComponent<Button>();
            if (btn != null)
            {
                string nombreBoton = go.name.ToLower().Trim();
                if (nombreBoton == "reiniciar") { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(ReiniciarNivel); }
                if (nombreBoton == "volver al menu") { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(VolverAlMenu); }
                if (nombreBoton == "reanudar") { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(AlternarPausa); }
                if (nombreBoton == "salir") { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(Salir); }
            }
        }
        ActualizarUI();
    }

    public void ActualizarUI()
    {
        if (txtVidas != null) txtVidas.text = "Vidas: " + vidas;
        if (txtDinero != null) txtDinero.text = "Dinero: " + dinero;
    }

    public void NotificarMuerteEnemigo()
    {
        enemigosRestantes--;
        // Si no quedan enemigos, esperamos un breve momento y pasamos de nivel
        if (enemigosRestantes <= 0) Invoke("GestionarCambioDeNivel", 2f);
    }

    // Lógica de flujo de niveles. Detecta el nivel actual y calcula el siguiente.
    private void GestionarCambioDeNivel()
    {
        string nombreActual = SceneManager.GetActiveScene().name;
        if (nombreActual.StartsWith("Nivel"))
        {
            string numeroString = nombreActual.Replace("Nivel", "");
            if (int.TryParse(numeroString, out int nivelActual))
            {
                if (nivelActual >= nivelMaximo) CargarEscenaConTransicion("Victoria");
                else CargarEscenaConTransicion("Nivel" + (nivelActual + 1));
            }
        }
    }

    public void PerderVida(int cantidad)
    {
        vidas -= cantidad;
        if (vidas < 0) vidas = 0;
        ActualizarUI();
        if (vidas <= 0) CargarEscenaConTransicion("Derrota");
    }

    public void GanarDinero(int cantidad) { dinero += cantidad; ActualizarUI(); }
    // Valida si el jugador tiene suficiente dinero para realizar una compra.
    public bool GastarDinero(int cantidad)
    {
        if (dinero >= cantidad) { dinero -= cantidad; ActualizarUI(); return true; }
        return false;
    }

    public void ConfigurarCursor()
    {
        if (cursorSprite != null) Cursor.SetCursor(cursorSprite, hotSpot, CursorMode.Auto);
    }

    public void ReiniciarNivel()
    {
        CargarEscenaConTransicion(SceneManager.GetActiveScene().name);
    }

    public void VolverAlMenu()
    {
        CargarEscenaConTransicion("EscenaInicio");
    }

    public void Salir()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}