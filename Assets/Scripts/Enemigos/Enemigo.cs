using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public delegate void LlegadaFinal();
    public event LlegadaFinal OnLlegadaFinal;

    [Header("Animaciones")]
    public Animator animator;

    protected Rigidbody2D rb;
    private Collider2D col; 

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
    }

    protected virtual void Start()
    {
        ConfigurarDificultad();
    }

    // --- NUEVA LÓGICA DE DIFICULTAD ---
    private void ConfigurarDificultad()
    {
        string nombreEscena = SceneManager.GetActiveScene().name;
        if (nombreEscena.StartsWith("Nivel"))
        {
            string numStr = nombreEscena.Replace("Nivel", "");
            if (int.TryParse(numStr, out int nivelActual))
            {
                // Por cada nivel (empezando en el 1), sumamos 5 de vida.
                // Nivel 1: +5, Nivel 2: +10, etc.
                int vidaExtra = nivelActual * 5;
                vida += vidaExtra;
                Debug.Log(gameObject.name + " aparece con " + vida + " de vida (Nivel " + nivelActual + ")");
            }
        }
    }

    void Update()
    {
        if (Mathf.Approximately(Time.timeScale, 0f)) return;
        if (listoParaMover && estaVivo) Mover();
    }

    public void RecibirDanio(int cantidad)
    {
        if (!estaVivo) return;
        vida -= cantidad;
        
        StopAllCoroutines(); 
        StartCoroutine(FeedbackDanio());

        if (animator != null) animator.SetTrigger("RecibirDanio");

        if (vida <= 0)
        {
            if (GameManager.instancia != null)
                GameManager.instancia.GanarDinero(recompensa);
            Morir(true);
        }
    }

    IEnumerator FeedbackDanio()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = Color.white;
        }
    }

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

    protected virtual void Mover()
    {
        if (!estaVivo || !listoParaMover || waypoints == null || indiceWaypoint >= waypoints.Count) 
            return;

        Vector3 puntoObjetivo = waypoints[indiceWaypoint];
        Vector2 objetivo2D = new Vector2(puntoObjetivo.x, puntoObjetivo.y);
        Vector2 posicionActual2D = rb.position;
        
        Vector2 direccion = objetivo2D - posicionActual2D;
        float distancia = direccion.magnitude;

        if (distancia < 0.25f)
        {
            indiceWaypoint++;
            if (indiceWaypoint >= waypoints.Count) 
            {
                LlegarAlFinal();
                return;
            }
        }
        else
        {
            float paso = velocidad * Time.deltaTime;
            rb.MovePosition(posicionActual2D + direccion.normalized * Mathf.Min(paso, distancia));
            
            if (direccion.x > 0.1f) transform.localScale = new Vector3(1, 1, 1);
            else if (direccion.x < -0.1f) transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void LlegarAlFinal()
    {
        if (!estaVivo) return;
        estaVivo = false;
        listoParaMover = false;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true; 
        rb.simulated = false;
        if (col != null) col.enabled = false;
        if (OnLlegadaFinal != null) OnLlegadaFinal.Invoke();
        Morir(false);
    }

    public void Morir(bool muertoPorJugador)
    {
        estaVivo = false;
        listoParaMover = false;
        rb.simulated = false;
        if (col != null) col.enabled = false;
        if (GameManager.instancia != null) GameManager.instancia.NotificarMuerteEnemigo();
        if (animator != null) { animator.SetBool("Caminando", false); animator.SetTrigger("Morir"); }
        Destroy(gameObject);
    }
}