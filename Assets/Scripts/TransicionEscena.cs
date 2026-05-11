using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Corregido: Management (faltaba una 'e')

public class TransicionEscena : MonoBehaviour
{
    private Animator animator;
    
    // Nota: AnimationClip es el tipo correcto para medir el tiempo. 
    // [SerializeField] corregido (estaba mal escrito como SerializeFiled)
    [SerializeField] private AnimationClip animacionFinal; 

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    IEnumerator CambiarEscena()
    {
        animator.SetTrigger("Nivel1");
        
        // Esperamos la duración exacta del clip de animación que pongas en el Inspector
        yield return new WaitForSeconds(animacionFinal.length);
        
        SceneManager.LoadScene("EscenaInicio");
    }
}