using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControladorOpciones : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider sliderVolumen;
    public Slider sliderSensibilidad;

    void Start()
    {
        // Cargamos valores guardados o ponemos por defecto (0.5 y 1.0)
        float volGuardado = PlayerPrefs.GetFloat("VolumenMaster", 0.5f);
        float sensGuardada = PlayerPrefs.GetFloat("SensibilidadRaton", 1.0f);

        if (sliderVolumen != null) sliderVolumen.value = volGuardado;
        if (sliderSensibilidad != null) sliderSensibilidad.value = sensGuardada;

        // Escuchamos cambios
        sliderVolumen.onValueChanged.AddListener(delegate { SetVolumen(); });
        sliderSensibilidad.onValueChanged.AddListener(delegate { SetSensibilidad(); });
    }

    public void SetVolumen()
    {
        PlayerPrefs.SetFloat("VolumenMaster", sliderVolumen.value);
        if (AudioManager.instancia != null)
        {
            AudioManager.instancia.ActualizarVolumen(sliderVolumen.value);
        }
    }

    public void SetSensibilidad()
    {
        PlayerPrefs.SetFloat("SensibilidadRaton", sliderSensibilidad.value);
    }

   public bool esEscenaIndependiente = false; // Marcar como TRUE solo en la escena de Opciones

public void Volver()
{
    if (esEscenaIndependiente)
    {
        string escenaAnterior = PlayerPrefs.GetString("UltimaEscena", "MenuInicio");
        SceneManager.LoadScene(escenaAnterior);
    }
    else
    {
        // Si es un panel dentro del nivel, simplemente se desactiva
        gameObject.SetActive(false);
        // Aquí podrías llamar al panel de pausa para que vuelva a aparecer
    }
}
}