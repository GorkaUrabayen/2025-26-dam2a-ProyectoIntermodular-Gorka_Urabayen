using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Header("Stats")]
    public int vidas = 10;
    public int dinero = 100;

    [Header("UI")]
    public TMP_Text txtVidas;
    public TMP_Text txtDinero;

    [Header("Cursor del Sistema")]
    public Texture2D cursorSprite;
    public Vector2 hotSpot = Vector2.zero;

    void Awake()
    {
        if (instancia == null)
            instancia = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        ActualizarUI();
        ConfigurarCursor();
    }

    public void ConfigurarCursor()
    {
        if (cursorSprite != null)
        {
            Cursor.SetCursor(cursorSprite, hotSpot, CursorMode.Auto);
        }
    }

    public void PerderVida(int cantidad)
    {
        vidas -= cantidad;

        if (vidas < 0)
            vidas = 0;

        ActualizarUI();

        if (vidas <= 0)
        {
            CargarDerrota();
        }
    }

    void CargarDerrota()
    {
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

    void ActualizarUI()
    {
        if (txtVidas != null)
            txtVidas.text = "Vidas: " + vidas;

        if (txtDinero != null)
            txtDinero.text = "Dinero: " + dinero;
    }
}
