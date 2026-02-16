using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Header("Stats Iniciales")]
    public int vidasIniciales = 10;
    public int dineroInicial = 100;

    [Header("Stats Actuales")]
    public int vidas;
    public int dinero;
    public int enemigosRestantes = 0;

    [Header("UI")]
    public TMP_Text txtVidas;
    public TMP_Text txtDinero;

    [Header("Cursor del Sistema")]
    public Texture2D cursorSprite;
    public Vector2 hotSpot = Vector2.zero;

    void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
            vidas = vidasIniciales;
            dinero = dineroInicial;
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
        if (escena.name == "Nivel1")
        {
            vidas = vidasIniciales;
            dinero = dineroInicial;
        }

        enemigosRestantes = 0;

        // Buscamos la UI con un pequeño retraso para asegurar que los objetos existan
        CancelInvoke("ReconectarUI"); 
        Invoke("ReconectarUI", 0.1f);
        
        ConfigurarCursor();
    }

    public void NotificarMuerteEnemigo()
    {
        enemigosRestantes--;
        if (enemigosRestantes <= 0)
        {
            Invoke("PasarAlSiguienteNivel", 2f);
        }
    }

    public void PasarAlSiguienteNivel()
    {
        if (SceneManager.GetActiveScene().name == "Nivel1")
        {
            SceneManager.LoadScene("Nivel2");
        }
    }

    public void PerderVida(int cantidad)
    {
        vidas -= cantidad;
        if (vidas < 0) vidas = 0;
        ActualizarUI();
        if (vidas <= 0) CargarDerrota();
    }

    void CargarDerrota()
    {
        if (AudioManager.instancia != null) AudioManager.instancia.DetenerMusicaYDestruir();
        SceneManager.LoadScene("Derrota");
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

    public void ActualizarUI()
    {
        if (txtVidas != null) txtVidas.text = "Vidas: " + vidas;
        if (txtDinero != null) txtDinero.text = "Dinero: " + dinero;
    }

    private void ReconectarUI()
    {
        GameObject objVidas = GameObject.Find("TextoVidas");
        if (objVidas != null) txtVidas = objVidas.GetComponent<TMP_Text>();

        GameObject objDinero = GameObject.Find("TextoDinero");
        if (objDinero != null) txtDinero = objDinero.GetComponent<TMP_Text>();

        ActualizarUI();
    }

    public void ConfigurarCursor()
    {
        if (cursorSprite != null) Cursor.SetCursor(cursorSprite, hotSpot, CursorMode.Auto);
    }
}