using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
// Clase base para el comportamiento de los enemigos.
// Controla el movimiento por waypoints, salud, recompensas y escalado de dificultad.
[RequireComponent(typeof(Rigidbody2D))]
public class Enemigo : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 2f;
    protected List<Vector3> waypoints;
    private int indiceWaypoint = 0;
    private bool listoParaMover = false;

    [Header("Vida")]
    public int vida = 10;
    private bool estaVivo = true;
    public int recompensa = 5; 

    [Header("Visual")]
    public SpriteRenderer spriteRenderer; 
    // Evento para notificar al GameManager cuando un enemigo cruza la meta
    public delegate void LlegadaFinal();
    public event LlegadaFinal OnLlegadaFinal;

    [Header("Animaciones")]
    public Animator animator;

    protected Rigidbody2D rb;
    private Collider2D col; 

    protected virtual void Awake()
    {
        // Inicialización de componentes y configuración física
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        // Usamos Kinematic para tener control total sobre la posición mediante código
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
    }

    protected virtual void Start()
    {
        ConfigurarDificultad();
    }

    // Ajusta las estadísticas del enemigo según el número de nivel extraído del nombre de la escena.
    // Implementa un escalado lineal de dificultad.
    private void ConfigurarDificultad()
    {
        string nombreEscena = SceneManager.GetActiveScene().name;
        // Verificamos si estamos en una escena de nivel para aplicar el escalado
        if (nombreEscena.StartsWith("Nivel"))
        {
            string numStr = nombreEscena.Replace("Nivel", "");
            if (int.TryParse(numStr, out int nivelActual))
            {
                // Escalado: +5 de vida por cada nivel superado
                int vidaExtra = nivelActual * 5;
                vida += vidaExtra;
            }
        }
    }

    void Update()
    {
        // Optimizamos: No procesar movimiento si el juego está pausado o el enemigo ha muerto
        if (Mathf.Approximately(Time.timeScale, 0f)) return;
        if (listoParaMover && estaVivo) Mover();
    }
    // Gestiona la resta de vida, feedback visual de daño y muerte.
    public void RecibirDanio(int cantidad)
    {
        if (!estaVivo) return;
        vida -= cantidad;
        // Feedback visual: Reiniciamos la corrutina si ya estaba parpadeando
        StopAllCoroutines(); 
        StartCoroutine(FeedbackDanio());

        if (animator != null) animator.SetTrigger("RecibirDanio");

        if (vida <= 0)
        {
            // Entregar recompensa económica al jugador a través del Singleton del GameManager
            if (GameManager.instancia != null)
                GameManager.instancia.GanarDinero(recompensa);
            Morir(true);
        }
    }
    // Corrutina para dar feedback visual (parpadeo rojo) al recibir impactos.
    IEnumerator FeedbackDanio()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = Color.white;
        }
    }
    // Inicializa la ruta de waypoints generada procedimentalmente
    public void SetWaypoints(List<Vector3> puntos)
    {
        if (puntos == null || puntos.Count < 2) return;
        waypoints = new List<Vector3>(puntos);
        indiceWaypoint = 0;
        Vector3 inicio = waypoints[0];
        inicio.z = 0;
        transform.position = inicio;
        rb.position = inicio;
        listoParaMover = true;
        if (animator != null) animator.SetBool("Caminando", true);
    }
    // Lógica de desplazamiento hacia el siguiente waypoint.
    // Incluye rotación del sprite (Flip) según la dirección.
    protected virtual void Mover()
    {
        // Si ya no devbe moverse o el indice es inválido. salimos
        if (!estaVivo || !listoParaMover || waypoints == null || indiceWaypoint >= waypoints.Count) 
            return;

        Vector3 puntoObjetivo = waypoints[indiceWaypoint];
        Vector2 objetivo2D = new Vector2(puntoObjetivo.x, puntoObjetivo.y);
        Vector2 posicionActual2D = rb.position;
        
        Vector2 direccion = objetivo2D - posicionActual2D;
        float distancia = direccion.magnitude;
        // Comprobamos si el enemigo ha alcanzado el waypoint actual
        if (distancia < 0.25f)
        {
            indiceWaypoint++;
            // Si ya no hay más puntos, muere
            if (indiceWaypoint >= waypoints.Count) 
            {
                LlegarAlFinal();
                return;
            }
        }
        else
        {
            // Movimiento físico mediante Rigidbody2D para evitar traspasar colisiones si las hubiera
            float paso = velocidad * Time.deltaTime;
            rb.MovePosition(posicionActual2D + direccion.normalized * Mathf.Min(paso, distancia));
            // Gestión del encaramiento (Flip) del sprite según la dirección en X
            if (direccion.x > 0.1f) transform.localScale = new Vector3(1, 1, 1);
            else if (direccion.x < -0.1f) transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    // Acción ejecutada cuando el enemigo logra atravesar las defensas.
    private void LlegarAlFinal()
    {
        if (!estaVivo) return;
        estaVivo = false;
        listoParaMover = false;
        rb.velocity = Vector2.zero;
        // Desactivamos física y colisiones
        rb.isKinematic = true; 
        rb.simulated = false;
        if (col != null) col.enabled = false;
        // Disparamos el evento de llegada para restar vidas al jugador
        if (OnLlegadaFinal != null) OnLlegadaFinal.Invoke();
        Morir(false);
    }
    // Finaliza la existencia del objeto y notifica al sistema.
    public void Morir(bool muertoPorJugador)
    {
        estaVivo = false;
        listoParaMover = false;
        rb.simulated = false;
        // Notificamos al GameManager para llevar el conteo de oleadas/enemigos restantes
        if (col != null) col.enabled = false;
        if (GameManager.instancia != null) GameManager.instancia.NotificarMuerteEnemigo();
        if (animator != null) { animator.SetBool("Caminando", false); animator.SetTrigger("Morir"); }
        // En un proyecto mayor, aquí usaríamos un Object Pool en lugar de Destroy
        Destroy(gameObject);
    }
}