using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorPausa : MonoBehaviour
{
    public GameObject panelPausa; 
    public GameObject botonPausaUI;    // El botón de "Pause"
    public GameObject botonReanudarUI; // El botón de "Play" que acabas de crear
    private bool estaPausado = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (estaPausado) Reanudar();
            else Pausar();
        }
    }

    public void Pausar()
    {
        estaPausado = true;
        panelPausa.SetActive(true);
        
        // INTERCAMBIO DE BOTONES
        botonPausaUI.SetActive(false);    // Apagamos pausa
        botonReanudarUI.SetActive(true);  // Encendemos reanudar
        
        Time.timeScale = 0f; 
    }

    public void Reanudar()
    {
        estaPausado = false;
        panelPausa.SetActive(false);
        
        // INTERCAMBIO DE BOTONES
        botonPausaUI.SetActive(true);     // Encendemos pausa
        botonReanudarUI.SetActive(false); // Apagamos reanudar
        
        Time.timeScale = 1f; 
    }

    public void IrAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }
    
}