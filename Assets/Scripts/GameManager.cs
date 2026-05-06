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
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
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

    public void AlternarPausa()
    {
        // Si no hay panel de pausa, intentamos buscarlo una vez más
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

        // Música
        if (AudioManager.instancia != null)
        {
            if (escena.name == "Derrota") AudioManager.instancia.PlayMusicaDerrota();
            else if (escena.name == "Victoria") AudioManager.instancia.PlayMusicaVictoria();
            else if (escena.name == "EscenaInicio") AudioManager.instancia.PlayMusicaMenu();
            else AudioManager.instancia.PlayMusicaNivel();
        }

        // Configuración de stats por nivel
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
        // Un segundo intento tras un breve delay por si la UI tarda en instanciarse
        yield return new WaitForSeconds(0.1f);
        if (panelPausa == null) ReconectarUI();
    }

    private void ReconectarUI()
    {
        // 1. Buscar Textos (incluyendo desactivados)
        TMP_Text[] todosLosTextos = Resources.FindObjectsOfTypeAll<TMP_Text>();
        foreach (TMP_Text t in todosLosTextos)
        {
            if (t.gameObject.scene.name == null) continue; // Ignorar prefabs de la carpeta Assets
            if (t.gameObject.name == "TextoVidas") txtVidas = t;
            if (t.gameObject.name == "TextoDinero") txtDinero = t;
        }

        // 2. Buscar Panel Pausa y Botones
        GameObject[] todosLosGos = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in todosLosGos)
        {
            if (go.gameObject.scene.name == null) continue;

            // Encontrar el panel
            if (go.name == "MenuPausa") 
            {
                panelPausa = go;
                panelPausa.SetActive(false);
            }

            // Configurar botones si el objeto tiene componente Button
            Button btn = go.GetComponent<Button>();
            if (btn != null)
            {
                if (go.name == "Reiniciar") { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(ReiniciarNivel); }
                if (go.name == "Volver al menu") { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(VolverAlMenu); }
                if (go.name == "Reanudar") { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(AlternarPausa); }
                if (go.name == "Salir") { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(Salir); }
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
                if (nivelActual >= nivelMaximo) CargarVictoria();
                else SceneManager.LoadScene("Nivel" + (nivelActual + 1));
            }
        }
    }

    public void CargarVictoria() => SceneManager.LoadScene("Victoria");

    public void PerderVida(int cantidad)
    {
        vidas -= cantidad;
        if (vidas < 0) vidas = 0;
        ActualizarUI();
        if (vidas <= 0) SceneManager.LoadScene("Derrota");
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("EscenaInicio");
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Time.timeScale = 1f; 
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}