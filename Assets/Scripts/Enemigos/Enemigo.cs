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

    // --- Awake protegido para poder sobrescribir en subclases ---
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        if (listoParaMover && estaVivo)
            Mover();
    }

    public void SetWaypoints(List<Vector3> puntos)
    {
        if (puntos == null || puntos.Count < 2)
            return;

        waypoints = new List<Vector3>(puntos);
        indiceWaypoint = 0;

        Vector3 inicio = waypoints[0];
        inicio.z = 0;
        rb.position = inicio;

        listoParaMover = true;

        if (animator != null)
            animator.SetBool("Caminando", true);
    }

    // --- Mover está fuera de SetWaypoints ---
    protected virtual void Mover()
    {
        if (indiceWaypoint >= waypoints.Count) return;

        Vector3 objetivo = waypoints[indiceWaypoint];
        objetivo.z = 0;

        Vector2 nuevaPos = Vector2.MoveTowards(rb.position, (Vector2)objetivo, velocidad * Time.deltaTime);
        rb.MovePosition(nuevaPos);

        if (Vector2.Distance(rb.position, (Vector2)objetivo) < 0.1f)
        {
            indiceWaypoint++;
            if (indiceWaypoint >= waypoints.Count)
            {
                listoParaMover = false;
                OnLlegadaFinal?.Invoke();
                Morir();
            }
        }
    }

    public void RecibirDanio(int cantidad)
    {
        if (!estaVivo) return;

        vida -= cantidad;
        if (animator != null) animator.SetTrigger("RecibirDanio");

        if (vida <= 0)
            Morir();
            GameManager.instancia.GanarDinero(5);

    }

    public void Morir()
    {
        if (!estaVivo) return;

        estaVivo = false;
        listoParaMover = false;

        if (animator != null)
            animator.SetTrigger("Morir");

        GameManager.instancia.PerderVida(1);
        Destroy(gameObject);

    }
}
