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
    public GameObject panelPausa; 

    [Header("Cursor Settings")]
    public Texture2D cursorSprite;
    public Vector2 hotSpot = Vector2.zero;

    private bool juegoPausado = false;

    void Awake()
    {
        // Sistema Singleton: mantiene este objeto entre escenas
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
        // Detectar pausa manual con Espacio
        string escena = SceneManager.GetActiveScene().name;
        if (Input.GetKeyDown(KeyCode.Space) && escena != "EscenaInicio" && escena != "Derrota" && escena != "Victoria")
        {
            AlternarPausa();
        }
    }

    public void AlternarPausa()
    {
        juegoPausado = !juegoPausado;

        // Si está pausado, tiempo a 0. Si no, tiempo a 1.
        Time.timeScale = juegoPausado ? 0f : 1f;

        if (panelPausa != null) 
            panelPausa.SetActive(juegoPausado);
    }

    void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        Time.timeScale = 1f; 
        juegoPausado = false;

        // Gestión de Audio
        if (AudioManager.instancia != null)
        {
            if (escena.name == "Derrota") AudioManager.instancia.PlayMusicaDerrota();
            else if (escena.name == "Victoria") AudioManager.instancia.PlayMusicaVictoria();
            else if (escena.name == "EscenaInicio") AudioManager.instancia.PlayMusicaMenu();
            else AudioManager.instancia.PlayMusicaNivel();
        }

        // Resetear Stats según el nivel
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
        panelPausa = null; 

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
        // 1. Conectar Textos
        TMP_Text[] todosLosTextos = FindObjectsOfType<TMP_Text>(true);
        foreach (TMP_Text t in todosLosTextos)
        {
            if (t.gameObject.name == "TextoVidas") txtVidas = t;
            if (t.gameObject.name == "TextoDinero") txtDinero = t;
        }

        // 2. Buscar el Panel de Pausa
        GameObject buscado = GameObject.Find("MenuPausa");
        if (buscado != null) 
        {
            panelPausa = buscado;
            panelPausa.SetActive(false); 
        }

        // 3. Conectar Botones automáticamente
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
            if (escenaActual == "Nivel1") Invoke("PasarAlSiguienteNivel", 2f);
            else if (escenaActual == "Nivel2") Invoke("CargarVictoria", 2f);
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
        if (cursorSprite != null) 
            Cursor.SetCursor(cursorSprite, hotSpot, CursorMode.Auto);
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

    public void SalirDelJuego()
    {
        Application.Quit();
    }
}