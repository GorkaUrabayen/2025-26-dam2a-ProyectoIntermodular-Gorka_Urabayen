using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorDerrota : MonoBehaviour
{
    // Asegúrate de que estos nombres coincidan con tus escenas
    public string nombreEscenaMenu = "EscenaInicio";

    public void IrAlMenu()
    {
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    public void SalirDelJuego()
    {
        Debug.Log("Cerrando el juego...");
        Application.Quit();
    }
}