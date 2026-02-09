using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Header("Vidas")]
    public int vidas = 20;

    [Header("Dinero")]
    public int dinero = 100;

    void Awake()
    {
        instancia = this;
    }

    // ❤️ PERDER VIDA
    public void PerderVida(int cantidad)
    {
        vidas -= cantidad;
        Debug.Log("Vidas restantes: " + vidas);

        if (vidas <= 0)
            GameOver();
    }

    void GameOver()
    {
        Debug.Log("GAME OVER");
        Time.timeScale = 0f;
    }

    // 💰 DINERO
    public bool GastarDinero(int cantidad)
    {
        if (dinero < cantidad)
            return false;

        dinero -= cantidad;
        Debug.Log("Dinero restante: " + dinero);
        return true;
    }

    public void GanarDinero(int cantidad)
    {
        dinero += cantidad;
        Debug.Log("Dinero: " + dinero);
    }
}
