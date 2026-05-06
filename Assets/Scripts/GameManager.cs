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

    [Header("UI References (Se buscan solas)")]
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
        string escena = SceneManager.GetActiveScene().name;
        if (Input.GetKeyDown(KeyCode.Space) && escena != "EscenaInicio" && escena != "Derrota" && escena != "Victoria")
        {
            AlternarPausa();
        }
    }

    public void AlternarPausa()
    {
        juegoPausado = !juegoPausado;
        Time.timeScale = juegoPausado ? 0f : 1f;

        if (panelPausa != null) 
            panelPausa.SetActive(juegoPausado);
    }

    void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        Time.timeScale = 1f; 
        juegoPausado = false;

        if (AudioManager.instancia != null)
        {
            if (escena.name == "Derrota") AudioManager.instancia.PlayMusicaDerrota();
            else if (escena.name == "Victoria") AudioManager.instancia.PlayMusicaVictoria();
            else if (escena.name == "EscenaInicio") AudioManager.instancia.PlayMusicaMenu();
            else AudioManager.instancia.PlayMusicaNivel();
        }

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
        
        // --- IMPORTANTE: Limpiamos referencias viejas ---
        txtVidas = null;
        txtDinero = null;
        panelPausa = null; 

        // Esperamos un poquito más para asegurar que la escena cargó los objetos
        StopAllCoroutines(); 
        StartCoroutine(BuscarUIPorPasos());
        ConfigurarCursor();
    }

    IEnumerator BuscarUIPorPasos()
    {
        // Esperamos a que termine el frame de carga
        yield return new WaitForEndOfFrame();
        ReconectarUI();
        
        // Pequeño re-intento por seguridad
        yield return new WaitForSeconds(0.2f);
        if (panelPausa == null) ReconectarUI();
    }

    private void ReconectarUI()
    {
        // 1. Buscar Textos
        TMP_Text[] todosLosTextos = Resources.FindObjectsOfTypeAll<TMP_Text>();
        foreach (TMP_Text t in todosLosTextos)
        {
            if (t.gameObject.name == "TextoVidas") txtVidas = t;
            if (t.gameObject.name == "TextoDinero") txtDinero = t;
        }

        // 2. Buscar Panel Pausa por nombre (asegúrate que se llame así en la jerarquía)
        if (panelPausa == null)
        {
            GameObject[] todosLosGos = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject go in todosLosGos)
            {
                if (go.name == "MenuPausa") 
                {
                    panelPausa = go;
                    panelPausa.SetActive(false);
                    break;
                }
            }
        }

        // 3. Buscar y conectar Botones
        Button[] todosLosBotones = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button btn in todosLosBotones)
        {
            // Solo configuramos botones que pertenezcan a la escena actual (no assets del proyecto)
            if (btn.gameObject.scene.name == null) continue;

            btn.onClick.RemoveAllListeners();

            if (btn.gameObject.name == "Reiniciar") btn.onClick.AddListener(ReiniciarNivel);
            if (btn.gameObject.name == "Volver al menu") btn.onClick.AddListener(VolverAlMenu);
            if (btn.gameObject.name == "Reanudar") btn.onClick.AddListener(AlternarPausa);
            if (btn.gameObject.name == "Salir") btn.onClick.AddListener(Salir);
        }

        ActualizarUI();
    }

    // --- EL RESTO DEL SCRIPT SIGUE IGUAL ---

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
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}