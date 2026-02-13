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
        // Cargar valores guardados
        float vol = PlayerPrefs.GetFloat("VolumenMaster", 0.5f);
        float sens = PlayerPrefs.GetFloat("SensibilidadRaton", 1.0f);

        if (sliderVolumen != null) sliderVolumen.value = vol;
        if (sliderSensibilidad != null) sliderSensibilidad.value = sens;

        // Escuchar cambios en tiempo real
        sliderVolumen.onValueChanged.AddListener(delegate { SetVolumen(); });
        sliderSensibilidad.onValueChanged.AddListener(delegate { SetSensibilidad(); });
    }

    public void SetVolumen()
    {
        PlayerPrefs.SetFloat("VolumenMaster", sliderVolumen.value);
        if (AudioManager.instancia != null) 
            AudioManager.instancia.ActualizarVolumen(sliderVolumen.value);
    }

    public void SetSensibilidad()
    {
        PlayerPrefs.SetFloat("SensibilidadRaton", sliderSensibilidad.value);
    }

    public void Volver()
    {
        // Regresa a la escena guardada (Menu o Nivel) sin resetear
        string escenaAnterior = PlayerPrefs.GetString("UltimaEscena", "MenuPrincipal");
        SceneManager.LoadScene(escenaAnterior);
    }
}