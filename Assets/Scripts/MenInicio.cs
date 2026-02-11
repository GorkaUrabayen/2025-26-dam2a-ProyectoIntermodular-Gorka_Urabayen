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
}