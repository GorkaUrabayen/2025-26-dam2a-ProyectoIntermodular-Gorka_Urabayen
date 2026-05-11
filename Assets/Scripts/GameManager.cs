using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
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
        // Implementación del patrón Singleton con persistencia
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return; // Importante para que no ejecute nada más si es un duplicado
        }
    }

    void OnEnable() { SceneManager.sceneLoaded += AlCargarEscena; }
    void OnDisable() { SceneManager.sceneLoaded -= AlCargarEscena; }

    void Update()
    {
        // Solo permitimos pausar si no estamos en menús o escenas finales
        string escena = SceneManager.GetActiveScene().name;
        bool esEscenaDeJuego = escena != "EscenaInicio" && escena != "Derrota" && escena != "Victoria";

        if (Input.GetKeyDown(KeyCode.Space) && esEscenaDeJuego)
        {
            AlternarPausa();
        }
    }

    // --- SISTEMA DE NAVEGACIÓN CON TRANSICIONES ---
    
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
        Time.timeScale = juegoPausado ? 0f : 1f;

        if (panelPausa != null) 
            panelPausa.SetActive(juegoPausado);
    }

    void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        // Reset de estado básico
        Time.timeScale = 1f; 
        juegoPausado = false;

        // Gestión de Audio (Si existe el AudioManager)
        if (AudioManager.instancia != null)
        {
            if (escena.name == "Derrota") AudioManager.instancia.PlayMusicaDerrota();
            else if (escena.name == "Victoria") AudioManager.instancia.PlayMusicaVictoria();
            else if (escena.name == "EscenaInicio") AudioManager.instancia.PlayMusicaMenu();
            else AudioManager.instancia.PlayMusicaNivel();
        }

        // Configuración de stats según el nivel
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
        
        // Limpiamos referencias para forzar la búsqueda en la nueva escena
        txtVidas = null;
        txtDinero = null;
        panelPausa = null; 

        StopAllCoroutines(); 
        StartCoroutine(BuscarUIPorPasos());
        ConfigurarCursor();
    }

    IEnumerator BuscarUIPorPasos()
    {
        yield return new WaitForEndOfFrame();
        ReconectarUI();
        yield return new WaitForSeconds(0.1f);
        if (panelPausa == null) ReconectarUI();
    }

    private void ReconectarUI()
    {
        if (instancia != this) return;

        // 1. Buscar Textos (incluyendo desactivados)
        TMP_Text[] todosLosTextos = Resources.FindObjectsOfTypeAll<TMP_Text>();
        foreach (TMP_Text t in todosLosTextos)
        {
            if (t.gameObject.scene.name == null) continue;
            if (t.gameObject.name == "TextoVidas") txtVidas = t;
            if (t.gameObject.name == "TextoDinero") txtDinero = t;
        }

        // 2. Buscar Panel Pausa y Botones
        GameObject[] todosLosGos = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in todosLosGos)
        {
            if (go.gameObject.scene.name == null) continue;

            if (go.name == "MenuPausa") 
            {
                panelPausa = go;
                panelPausa.SetActive(false);
            }

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
        if (enemigosRestantes <= 0) Invoke("GestionarCambioDeNivel", 2f);
    }

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