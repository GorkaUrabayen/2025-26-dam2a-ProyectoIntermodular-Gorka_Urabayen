using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

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
    public GameObject panelPausa; // NUEVO: Para guardar el menú de pausa

    [Header("Cursor Settings")]
    public Texture2D cursorSprite;
    public Vector2 hotSpot = Vector2.zero;

    private bool juegoPausado = false; // NUEVO: Control de estado

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

    // NUEVO: Detectar la tecla Espacio en cada frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Solo pausar si no estamos en el menú principal
            if (SceneManager.GetActiveScene().name != "EscenaInicio")
            {
                AlternarPausa();
            }
        }
    }

    // NUEVO: Función para pausar/reanudar
    public void AlternarPausa()
    {
        juegoPausado = !juegoPausado;

        if (juegoPausado)
        {
            Time.timeScale = 0f; // Congela el juego
            if (panelPausa != null) panelPausa.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f; // Descongela el juego
            if (panelPausa != null) panelPausa.SetActive(false);
        }
    }

    void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        Time.timeScale = 1f; 
        juegoPausado = false; // Resetear estado de pausa al cambiar nivel

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
        txtVidas = null;
        txtDinero = null;
        panelPausa = null; // Limpiar referencia vieja

        StartCoroutine(BuscarUIPorPasos());
        ConfigurarCursor();
    }

    IEnumerator BuscarUIPorPasos()
    {
        yield return new WaitForEndOfFrame();
        ReconectarUI();

        yield return new WaitForSeconds(0.1f);
        if (txtVidas == null || txtDinero == null) ReconectarUI();
    }

    private void ReconectarUI()
    {
        // Conectar Textos
        TMP_Text[] todosLosTextos = FindObjectsOfType<TMP_Text>(true);
        foreach (TMP_Text t in todosLosTextos)
        {
            if (t.gameObject.name == "TextoVidas") txtVidas = t;
            if (t.gameObject.name == "TextoDinero") txtDinero = t;
        }

        // NUEVO: Buscar el Panel de Pausa automáticamente
        // Asegúrate de que tu objeto de UI de pausa se llame exactamente "MenuPausa"
        GameObject buscado = GameObject.Find("MenuPausa");
        if (buscado != null) 
        {
            panelPausa = buscado;
            panelPausa.SetActive(false); // Empezar oculto
        }

        // Conectar Botones
        Button[] todosLosBotones = FindObjectsOfType<Button>(true);
        foreach (Button btn in todosLosBotones)
        {
            btn.onClick.RemoveAllListeners();

            if (btn.gameObject.name == "Reiniciar") 
                btn.onClick.AddListener(() => ReiniciarNivel());
            
            if (btn.gameObject.name == "Volver al menu") 
                btn.onClick.AddListener(() => VolverAlMenu());

            if (btn.gameObject.name == "Salir") 
                btn.onClick.AddListener(() => SalirDelJuego());

            // NUEVO: Botón para volver al juego desde el menú de pausa
            if (btn.gameObject.name == "Reanudar")
                btn.onClick.AddListener(() => AlternarPausa());
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

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f; 
        string nombreEscenaActual = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(nombreEscenaActual);
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("EscenaInicio");
    }

    public void SalirDelJuego()
    {
        Application.Quit();
        Debug.Log("Saliendo del juego...");
    }
}