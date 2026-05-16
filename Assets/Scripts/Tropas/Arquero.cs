using UnityEngine;
// Subclase de Torre que define el comportamiento específico de la unidad Arquero.
// Implementa lógica de animaciones direccionales y disparo de proyectiles (flechas).
public class Arquero : Torre
{
    protected virtual void Awake()
    {
        // CONFIGURACIÓN DE ESTADÍSTICAS BÁSICAS
        // Sobrescribimos los valores de la clase base Torre
        alcance = 3f;
        coste = 30;
        // Ajuste de balanceo: 3 de daño permite eliminar enemigos estándar de 6 de vida en 2 disparos
        danio = 3; 
    }
    // Implementación específica del ataque para el arquero.
    // Incluye lógica de selección de animaciones según el ángulo del objetivo y spawn de flechas.
    protected override void Atacar()
    {
        // Validación de seguridad: No atacar si el objetivo ha desaparecido o muerto
        if (objetivoActual == null) return;
        // Calculamos el vector de dirección normalizado hacia el objetivo
        Vector2 dir = (objetivoActual.transform.position - transform.position).normalized;
        // FASE 1: GESTIÓN DE ANIMACIONES DIRECCIONALES
        // Utilizamos el vector de dirección para determinar qué clip de animación reproducir
        if (animator != null)
        {
           
            if (dir.y > 0.6f)
            {
                if (Mathf.Abs(dir.x) < 0.3f) animator.Play("Shoot Up");
                else animator.Play("Shoot Diagonal Up");
            }
            else if (dir.y < -0.6f)
            {
                if (Mathf.Abs(dir.x) < 0.3f) animator.Play("Shoot Down");
                else animator.Play("Shoot Diagona Down");
            }
            else
            {
                animator.Play("Shoot Front");
            }
        }

        // FASE 2: INSTANCIACIÓN DE PROYECTIL
        // El daño se transfiere de la torre a la flecha para que esta lo aplique al impactar
        if (flechaPrefab != null && puntoDisparo != null)
        {
            // Creamos la flecha en la posición del punto de disparo (boquilla del arco)
            GameObject flecha = Instantiate(flechaPrefab, puntoDisparo.position, Quaternion.identity);
            Flecha f = flecha.GetComponent<Flecha>();
            if (f != null)
                // Inyectamos la referencia del objetivo y el valor de daño actual
                f.SetObjetivo(objetivoActual.transform, danio); 
        }
    }
}