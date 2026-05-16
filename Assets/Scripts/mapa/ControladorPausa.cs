using UnityEngine;
using UnityEngine.SceneManagement;
// Gestiona el estado de pausa del juego, controlando la escala de tiempo,
// la visibilidad de los paneles de la interfaz y la navegación hacia el menú.
public class ControladorPausa : MonoBehaviour
{
    public GameObject panelPausa; 
    public GameObject botonPausaUI;    // El botón de "Pausa"
    public GameObject botonReanudarUI; // El botón de "Play" que acabas de crear
    private bool estaPausado = false;


    void Update()
    {
        // Atajo de teclado estándar para alternar la pausa
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (estaPausado) Reanudar();
            else Pausar();
        }
    }
    // Detiene el tiempo de juego y muestra los elementos de la interfaz de pausa.
    public void Pausar()
    {
        estaPausado = true;
        panelPausa.SetActive(true);
        
        // INTERCAMBIO DE BOTONES
        botonPausaUI.SetActive(false);    // Apagamos pausa
        botonReanudarUI.SetActive(true);  // Encendemos reanudar
        
        Time.timeScale = 0f; 
    }

    // Restablece el tiempo de juego y oculta el menú de pausa.
    public void Reanudar()
    {
        estaPausado = false;
        panelPausa.SetActive(false);
        
        // INTERCAMBIO DE BOTONES
        botonPausaUI.SetActive(true);     // Encendemos pausa
        botonReanudarUI.SetActive(false); // Apagamos reanudar
        
        Time.timeScale = 1f; 
    }
    // Carga la escena del menú principal. 
    // Es fundamental restablecer el Time.timeScale antes de cambiar de escena.
    public void IrAlMenu()
    {
        // Importante: Si cargamos una escena mientras el tiempo está en 0, 
        // la nueva escena podría aparecer congelada.
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }
    
}