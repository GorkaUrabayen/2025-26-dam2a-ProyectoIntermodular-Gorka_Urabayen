using UnityEngine;
using UnityEngine.UI;
// Gestiona el menú de ajustes del juego.
// Controla la persistencia de datos mediante PlayerPrefs y la sincronización
// de la interfaz de usuario con los sistemas de audio y control.

//¡El slider de sensibilidad se ara mas adelante!
public class ControladorOpciones : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider sliderVolumen;
    public Slider sliderSensibilidad;

    [Header("Configuración Dinámica")]
    // Arrastra aquí el objeto PADRE que contiene el Slider y el Icono
    public GameObject contenedorSlider; 

    void Start()
    {
        // 1. CARGA DE DATOS PERSISTENTES
        // Recuperamos los valores guardados en el registro del sistema. 
        // Si es la primera vez que se juega, se usan valores por defecto (0.5f y 1.0f).
        float vol = PlayerPrefs.GetFloat("VolumenMaster", 0.5f);
        float sens = PlayerPrefs.GetFloat("SensibilidadRaton", 1.0f);

        // 2. CONFIGURACIÓN DEL SLIDER DE VOLUMEN
        if (sliderVolumen != null)
        {
            sliderVolumen.value = vol;
            
            // Sincronización inicial con el AudioManager al arrancar la escena
            if (AudioManager.instancia != null) 
                AudioManager.instancia.ActualizarVolumen(vol);
            
            // Suscripción mediante delegados al evento onValueChanged.
            // Esto permite que el volumen cambie en tiempo real mientras el usuario arrastra el slider.
            sliderVolumen.onValueChanged.AddListener(delegate { SetVolumen(); });
        }

        // 3. CONFIGURACIÓN DEL SLIDER DE SENSIBILIDAD
        if (sliderSensibilidad != null)
        {
            sliderSensibilidad.value = sens;
            sliderSensibilidad.onValueChanged.AddListener(delegate { SetSensibilidad(); });
        }

        // 4. ESTADO INICIAL DE LA UI
        // Garantizamos que el panel de opciones esté oculto al iniciar la partida.
        if (contenedorSlider != null) 
            contenedorSlider.SetActive(false);
    }

    // Alterna la visibilidad del panel de opciones.
    // Método diseñado para ser llamado desde un botón de la interfaz (OnClick).
    public void AlternarSliderVolumen()
    {
        if (contenedorSlider != null)
        {
            // Lógica de conmutación (Toggle): invierte el estado booleano actual.
            bool nuevoEstado = !contenedorSlider.activeSelf;
            contenedorSlider.SetActive(nuevoEstado);
        }
    }
        // Actualiza y guarda el volumen maestro.
    public void SetVolumen()
    {
        if (sliderVolumen == null) return;

        float valor = sliderVolumen.value;
        PlayerPrefs.SetFloat("VolumenMaster", valor);

        // Comunicar el cambio al AudioManager en tiempo real
        if (AudioManager.instancia != null) 
            AudioManager.instancia.ActualizarVolumen(valor);
    }
    // Actualiza y guarda la sensibilidad. (por ahora no funciona)
    public void SetSensibilidad()
    {
        if (sliderSensibilidad == null) return;

        float valor = sliderSensibilidad.value;
        PlayerPrefs.SetFloat("SensibilidadRaton", valor);
    }
}