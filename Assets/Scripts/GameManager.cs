using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Header("Stats")]
    public int vidas = 10;
    public int dinero = 100;

    [Header("UI")]
    public TMP_Text txtVidas;
    public TMP_Text txtDinero;

    void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ActualizarUI();
    }

    public void PerderVida(int cantidad)
    {
        vidas -= cantidad;
        ActualizarUI();

        if (vidas <= 0)
        {
            Debug.Log("Has perdido");
        }
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
