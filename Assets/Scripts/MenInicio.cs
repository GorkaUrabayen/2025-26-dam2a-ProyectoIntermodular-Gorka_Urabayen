using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicio : MonoBehaviour
{
    // TIENE QUE SER PUBLIC
    public void Jugar() 
    {
        SceneManager.LoadScene("Nivel1"); 
    }

    // TIENE QUE SER PUBLIC
    public void Salir()
    {
        Application.Quit();
    }
  public void AbrirOpciones()
    {
      {
        // Guardamos la escena actual antes de irnos
        PlayerPrefs.SetString("UltimaEscena", SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("Opciones"); 
    }
}
public void VolverAEscenaAnterior()
    {
        // Buscamos la clave "UltimaEscena". Si no existe, por defecto va al MenuPrincipal
        string escenaAnterior = PlayerPrefs.GetString("UltimaEscena", "MenuPrincipal");
        SceneManager.LoadScene(escenaAnterior);
    }
}