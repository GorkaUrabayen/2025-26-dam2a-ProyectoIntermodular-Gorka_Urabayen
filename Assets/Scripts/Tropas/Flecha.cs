using UnityEngine;
// Gestiona el comportamiento de los proyectiles disparados por las torres.
// Incluye lógica de persecución de objetivos, rotación automática y aplicación de daño.
public class Flecha : MonoBehaviour
{
    private Transform objetivo;
    private int danio;
    public float velocidad = 8f;
    // Inicializa los parámetros necesarios para que la flecha funcione.
    // Inyectado por la torre en el momento de la instanciación.
    public void SetObjetivo(Transform obj, int dmg)
    {
        objetivo = obj;
        danio = dmg;
    }

    void Update()
    {
        // GESTIÓN DE SEGURIDAD
        // Si el objetivo es destruido por otra torre mientras la flecha vuela,
        // destruimos el proyectil para evitar errores y saturación de memoria.
        if (objetivo == null)
        {
            Destroy(gameObject);
            return;
        }
        // FASE 1: MOVIMIENTO (Persecución)
        // Calculamos la dirección normalizada hacia la posición actual del objetivo
        Vector3 dir = (objetivo.position - transform.position).normalized;
        // Traslación lineal independiente del framerate usando Time.deltaTime
        transform.position += dir * velocidad * Time.deltaTime;
        // FASE 2: ROTACIÓN (Look At)
        // Calculamos el ángulo en radianes usando Atan2 y lo convertimos a grados.
        // Esto hace que la punta de la flecha siempre mire hacia el enemigo.
        float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo);
        // FASE 3: DETECCIÓN DE IMPACTO
        // Usamos una comprobación de distancia mínima para simular la colisión
        if (Vector3.Distance(transform.position, objetivo.position) < 0.1f)
        {   
            // Intentamos obtener el componente Enemigo del objetivo
            Enemigo e = objetivo.GetComponent<Enemigo>();

            if (e != null)
                // Aplicamos el daño inyectado al inicio
                e.RecibirDanio(danio);
            // Tras el impacto, eliminamos el proyectil de la escena
            Destroy(gameObject);
        }
    }
}
