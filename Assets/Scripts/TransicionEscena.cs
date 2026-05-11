using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransicionEscena : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private AnimationClip animacionFinal; 
    
    // Variable para saber a dónde ir al terminar la animación
    private string escenaDestino;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Método que llamará el GameManager
    public void IniciarTransicion(string nombreEscena)
    {
        escenaDestino = nombreEscena;
        StartCoroutine(CambiarEscena());
    }

    IEnumerator CambiarEscena()
    {
        // Usamos un trigger genérico llamado "Iniciar" (configúralo así en el Animator)
        animator.SetTrigger("Iniciar");
        
        // Esperamos la duración del clip
        yield return new WaitForSeconds(animacionFinal.length);
        
        SceneManager.LoadScene(escenaDestino);
    }
}