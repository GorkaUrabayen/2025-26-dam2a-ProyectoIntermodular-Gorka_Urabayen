using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI; // Necesario para detectar los botones

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Header("Stats Actuales")]
    public int vidas;
    public int dinero;
    public int enemigosRestantes = 0;

    [Header("UI References (Se buscan solas)")]
    public TMP_Text txtVidas;
    public TMP_Text txtDinero;

    [Header("Cursor Settings")]
    public Texture2D cursorSprite;
    public Vector2 hotSpot = Vector2.zero;

    void Awake()
    {
        // Sistema Singleton: mantiene el primer GameManager y destruye los nuevos al recargar escena
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

    void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        // IMPORTANTE: Siempre reanudar el tiempo al cargar
        Time.timeScale = 1f; 

        // 1. Resetear Stats según el nivel
        if (escena.name == "Nivel1")
        {
            vidas = 10;
            dinero = 30;
        }
        else if (escena.name == "Nivel2")
        {
            vidas = 10;
            dinero = 60;
        }

        enemigosRestantes = 0;

        // 2. Limpiar referencias viejas para que no den error
        txtVidas = null;
        txtDinero = null;

        // 3. Buscar la nueva UI de la escena cargada
        StartCoroutine(BuscarUIPorPasos());
        ConfigurarCursor();
    }

    IEnumerator BuscarUIPorPasos()
    {
        // Esperamos a que Unity termine de renderizar el primer frame
        yield return new WaitForEndOfFrame();
        ReconectarUI();

        // Reintento de seguridad por si la escena tarda en cargar
        yield return new WaitForSeconds(0.1f);
        if (txtVidas == null || txtDinero == null) ReconectarUI();
    }

    private void ReconectarUI()
    {
        // A. Conectar Textos de Vidas y Dinero
        TMP_Text[] todosLosTextos = FindObjectsOfType<TMP_Text>(true);
        foreach (TMP_Text t in todosLosTextos)
        {
            if (t.gameObject.name == "TextoVidas") txtVidas = t;
            if (t.gameObject.name == "TextoDinero") txtDinero = t;
        }

        // B. Conectar Botones automáticamente por Nombre
        Button[] todosLosBotones = FindObjectsOfType<Button>(true);
        foreach (Button btn in todosLosBotones)
        {
            // Limpiamos referencias muertas del Inspector
            btn.onClick.RemoveAllListeners();

            // Asignamos la función según el nombre que tenga el botón en la Hierarchy
            if (btn.gameObject.name == "Reiniciar") 
                btn.onClick.AddListener(() => ReiniciarNivel());
            
            if (btn.gameObject.name == "Volver al menu") 
                btn.onClick.AddListener(() => VolverAlMenu());

            if (btn.gameObject.name == "Salir") 
                btn.onClick.AddListener(() => SalirDelJuego());
        }

        ActualizarUI();
    }

    public void ActualizarUI()
    {
        if (txtVidas != null) txtVidas.text = "Vidas: " + vidas;
        if (txtDinero != null) txtDinero.text = "Dinero: " + dinero;
    }

    // --- LÓGICA DE JUEGO ---

    public void NotificarMuerteEnemigo()
    {
        enemigosRestantes--;
        if (enemigosRestantes <= 0)
        {
            string escenaActual = SceneManager.GetActiveScene().name;
            if (escenaActual == "Nivel1")
                Invoke("PasarAlSiguienteNivel", 2f);
            else if (escenaActual == "Nivel2")
                Invoke("CargarVictoria", 2f);
        }
    }

    public void PasarAlSiguienteNivel() => SceneManager.LoadScene("Nivel2");
    public void CargarVictoria() => SceneManager.LoadScene("Victoria");

    public void PerderVida(int cantidad)
    {
        vidas -= cantidad;
        if (vidas < 0) vidas = 0;
        ActualizarUI();
        if (vidas <= 0) SceneManager.LoadScene("Derrota");
    }

    public void GanarDinero(int cantidad)
    {
        dinero += cantidad;
        ActualizarUI();
    }

    public bool GastarDinero(int cantidad)
    {
        if (dinero >= cantidad)
        {
            dinero -= cantidad;
            ActualizarUI();
            return true;
        }
        return false;
    }

    public void ConfigurarCursor()
    {
        if (cursorSprite != null) Cursor.SetCursor(cursorSprite, hotSpot, CursorMode.Auto);
    }

    // --- MÉTODOS DE NAVEGACIÓN ---

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f; 
        string nombreEscenaActual = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(nombreEscenaActual);
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f; 
        // Cambiado a "EscenaInicial" para coincidir con tu carpeta de Assets
        SceneManager.LoadScene("EscenaInicio");
    }

    public void SalirDelJuego()
    {
        Application.Quit();
        Debug.Log("Saliendo del juego...");
    }
}