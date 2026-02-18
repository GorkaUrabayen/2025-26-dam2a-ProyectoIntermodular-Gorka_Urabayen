using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

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
        // Sistema Singleton para que el GameManager no se destruya
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
        // 1. Resetear Stats según el nivel
        if (escena.name == "Nivel1" || escena.name == "Nivel2")
        {
            vidas = 10;
            dinero = 30;
        }

        enemigosRestantes = 0;

        // 2. Limpiar referencias viejas (OBLIGATORIO para cambiar de escena)
        txtVidas = null;
        txtDinero = null;

        // 3. Buscar los nuevos textos de esta escena
        // Usamos una Corrutina para esperar un instante a que Unity despierte los objetos de la UI
        StartCoroutine(BuscarUIPorPasos());
        
        ConfigurarCursor();
    }

    IEnumerator BuscarUIPorPasos()
    {
        // Esperamos un frame para que los objetos de la jerarquía existan
        yield return new WaitForEndOfFrame();
        ReconectarUI();

        // Por si acaso la escena es pesada, reintentamos a la décima de segundo
        yield return new WaitForSeconds(0.1f);
        if (txtVidas == null || txtDinero == null) ReconectarUI();
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

    public void ActualizarUI()
    {
        if (txtVidas != null) txtVidas.text = "Vidas: " + vidas;
        if (txtDinero != null) txtDinero.text = "Dinero: " + dinero;
    }

    private void ReconectarUI()
    {
        // Buscamos todos los TMP_Text de la escena actual (incluyendo desactivados)
        TMP_Text[] todosLosTextos = FindObjectsOfType<TMP_Text>(true);
        
        foreach (TMP_Text t in todosLosTextos)
        {
            if (t.gameObject.name == "TextoVidas") txtVidas = t;
            if (t.gameObject.name == "TextoDinero") txtDinero = t;
        }

        ActualizarUI();
    }

    public void ConfigurarCursor()
    {
        if (cursorSprite != null) Cursor.SetCursor(cursorSprite, hotSpot, CursorMode.Auto);
    }
}