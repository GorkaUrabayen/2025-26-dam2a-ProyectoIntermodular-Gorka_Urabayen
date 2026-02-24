using UnityEngine;
using UnityEngine.UI;

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
        // 1. Cargar valores guardados (usamos 0.5f como valor por defecto si no hay nada guardado)
        float vol = PlayerPrefs.GetFloat("VolumenMaster", 0.5f);
        float sens = PlayerPrefs.GetFloat("SensibilidadRaton", 1.0f);

        // 2. Configurar Slider de Volumen
        if (sliderVolumen != null)
        {
            sliderVolumen.value = vol;
            
            // Aplicar el volumen guardado al AudioManager nada más empezar
            if (AudioManager.instancia != null) 
                AudioManager.instancia.ActualizarVolumen(vol);
            
            // Escuchar cambios en el slider
            sliderVolumen.onValueChanged.AddListener(delegate { SetVolumen(); });
        }

        // 3. Configurar Slider de Sensibilidad
        if (sliderSensibilidad != null)
        {
            sliderSensibilidad.value = sens;
            sliderSensibilidad.onValueChanged.AddListener(delegate { SetSensibilidad(); });
        }

        // 4. Asegurarse de que el panel empiece oculto
        if (contenedorSlider != null) 
            contenedorSlider.SetActive(false);
    }

    /// <summary>
    /// Función principal para el botón de Opciones. 
    /// Muestra u oculta el contenedor según su estado actual.
    /// </summary>
    public void AlternarSliderVolumen()
    {
        if (contenedorSlider != null)
        {
            // El símbolo '!' invierte el estado: si es true pasa a false y viceversa
            bool nuevoEstado = !contenedorSlider.activeSelf;
            contenedorSlider.SetActive(nuevoEstado);
        }
    }

    public void SetVolumen()
    {
        if (sliderVolumen == null) return;

        float valor = sliderVolumen.value;
        PlayerPrefs.SetFloat("VolumenMaster", valor);

        // Comunicar el cambio al AudioManager en tiempo real
        if (AudioManager.instancia != null) 
            AudioManager.instancia.ActualizarVolumen(valor);
    }

    public void SetSensibilidad()
    {
        if (sliderSensibilidad == null) return;

        float valor = sliderSensibilidad.value;
        PlayerPrefs.SetFloat("SensibilidadRaton", valor);
    }
}