using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicio : MonoBehaviour
{
    // Función para el botón JUGAR
    public void Jugar() 
    {
        SceneManager.LoadScene("Nivel1"); 
    }
    public void AbrirOpciones() 
    {
        SceneManager.LoadScene("Opciones"); 
    }

    // Función para el botón CONTROLES (Nueva escena)
    public void IrAControles()
    {
        // Guardamos la escena actual (EscenaInicio) para saber a dónde volver
        PlayerPrefs.SetString("UltimaEscena", SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("Controles"); 
    }

    // Función para el botón VOLVER (dentro de la escena Controles)
    public void VolverAEscenaAnterior()
    {
        string escenaAnterior = PlayerPrefs.GetString("UltimaEscena", "EscenaInicio");
        SceneManager.LoadScene(escenaAnterior);
    }

    // Función para el botón SALIR
    public void Salir()
    {
        Application.Quit();
    }
}