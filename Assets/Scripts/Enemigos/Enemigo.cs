using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public delegate void LlegadaFinal();
    public event LlegadaFinal OnLlegadaFinal;

    [Header("Animaciones")]
    public Animator animator;

    protected Rigidbody2D rb;
    private Collider2D col; // Referencia para desactivar colisiones

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        // Aseguramos que no tenga fuerzas acumuladas
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    void Update()
    {
        if (Mathf.Approximately(Time.timeScale, 0f)) return;

        if (listoParaMover && estaVivo)
            Mover();
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

        if (animator != null)
            animator.SetBool("Caminando", true);
    }

    protected virtual void Mover()
    {
        // Si ya llegamos al final o estamos muertos, no hacemos nada
        if (indiceWaypoint >= waypoints.Count || !estaVivo) return;

        Vector3 puntoObjetivo = waypoints[indiceWaypoint];
        Vector2 objetivo2D = new Vector2(puntoObjetivo.x, puntoObjetivo.y);
        
        Vector2 posicionActual2D = rb.position;
        Vector2 direccion = objetivo2D - posicionActual2D;
        float distancia = direccion.magnitude;

        if (distancia < 0.1f)
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
            // Solo movemos si seguimos vivos después del cálculo
            if (estaVivo)
            {
                rb.MovePosition(posicionActual2D + direccion.normalized * Mathf.Min(paso, distancia));
            }
        }
    }

    private void LlegarAlFinal()
    {
        if (!estaVivo) return;

        estaVivo = false; 
        listoParaMover = false;
        
        // Paramos cualquier movimiento físico residual
        rb.velocity = Vector2.zero;

        // Desactivamos el collider para que otros enemigos no lo empujen mientras muere
        if (col != null) col.enabled = false;

        if (OnLlegadaFinal != null)
            OnLlegadaFinal.Invoke();

        Morir(false);
    }

    public void RecibirDanio(int cantidad)
    {
        if (!estaVivo) return;

        vida -= cantidad;

        if (animator != null)
            animator.SetTrigger("RecibirDanio");

        if (vida <= 0)
        {
            if (GameManager.instancia != null)
                GameManager.instancia.GanarDinero(5);
            
            Morir(true);
        }
    }

    public void Morir(bool muertoPorJugador)
    {
        // Aseguramos que todas las banderas de control estén apagadas
        estaVivo = false;
        listoParaMover = false;
        
        if (col != null) col.enabled = false;
        rb.simulated = false; // Desactiva el Rigidbody por completo

        if (animator != null)
            animator.SetTrigger("Morir");

        // Destruimos el objeto. 
        // Si tienes una animación de muerte larga, puedes usar Destroy(gameObject, 1f);
        Destroy(gameObject);
    }
}