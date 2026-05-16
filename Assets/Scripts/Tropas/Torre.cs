using UnityEngine;
// Clase base para el sistema de torres.
// Gestiona la detección automática de objetivos, el enfriamiento (cooldown) de ataque y la lógica de encaramiento hacia el enemigo.
public class Torre : MonoBehaviour
{
    [Header("Disparo")]
    public GameObject flechaPrefab;
    public Transform puntoDisparo;
    public float alcance = 5f;
    public float cadencia = 1f;
    public int danio = 2;

    [Header("Coste")]
    public int coste = 20; // Cuánto cuesta la torre

    [HideInInspector]
    public bool estaColocada = false; // Solo dispara si está colocada

    protected float tiempoUltimoDisparo = 0f;
    protected Enemigo objetivoActual;

    [Header("Animaciones")]
    public Animator animator;

    void Update()
    {
        // Solo ejecutamos la lógica si la torre ya ha sido construida definitivamente
        if (!estaColocada) return; 
        // Localizamos al objetivo más óptimo dentro del rango
        objetivoActual = BuscarEnemigoCercano();

        if (objetivoActual != null)
        {
            // Girar el sprite hacia el enemigo
            ApuntarAOEnemigo();
            // Control de tiempo para respetar la cadencia de fuego
            if (Time.time - tiempoUltimoDisparo >= 1f / cadencia)
            {
                Atacar();
                tiempoUltimoDisparo = Time.time;
            }
        }
        else
        {
            // Si no hay enemigos, volvemos al estado de espera
            if (animator != null)
                animator.Play("Idle");
        }
    }
    // Utiliza el algoritmo de búsqueda por proximidad para encontrar el enemigo más cercano.
    Enemigo BuscarEnemigoCercano()
    {
        // En proyectos más grandes se usaría un sistema de detección por Trigger para optimizar
        Enemigo[] enemigos = FindObjectsOfType<Enemigo>();
        Enemigo masCercano = null;
        float distanciaMin = Mathf.Infinity;

        foreach (Enemigo e in enemigos)
        {
            float dist = Vector2.Distance(transform.position, e.transform.position);
            // Verificamos si es el más cercano de los encontrados y si está dentro del radio de alcance
            if (dist < distanciaMin && dist <= alcance)
            {
                distanciaMin = dist;
                masCercano = e;
            }
        }

        return masCercano;
    }
    // Gestiona el encaramiento visual (Flip) de la torre.
    // Cambia el escala en X según la posición relativa del enemigo.
    void ApuntarAOEnemigo()
    {
        if (objetivoActual == null) return;

        Vector3 dir = objetivoActual.transform.position - transform.position;
        // Si el enemigo está a la izquierda (x negativo), invertimos el localScale
        transform.localScale = dir.x < 0 ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
    }

    // Método virtual para realizar el ataque. 
    // Al ser virtual, permite que las clases hijas (como Arquero) personalicen su propio ataque.
    protected virtual void Atacar()
    {
        if (flechaPrefab != null && puntoDisparo != null && objetivoActual != null)
        {
            // Instanciamos el proyectil y le asignamos el objetivo y daño
            GameObject flecha = Instantiate(flechaPrefab, puntoDisparo.position, Quaternion.identity);
            Flecha f = flecha.GetComponent<Flecha>();
            if (f != null)
                f.SetObjetivo(objetivoActual.transform, danio);
        }
    }
}
