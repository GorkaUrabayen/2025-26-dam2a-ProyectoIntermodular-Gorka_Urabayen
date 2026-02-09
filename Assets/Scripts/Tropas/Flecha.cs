using UnityEngine;

public class Flecha : MonoBehaviour
{
    private Transform objetivo;
    private int danio;
    public float velocidad = 8f;

    public void SetObjetivo(Transform obj, int dmg)
    {
        objetivo = obj;
        danio = dmg;
    }

    void Update()
    {
        if (objetivo == null)
        {
            Destroy(gameObject);
            return;
        }

        // Mover flecha hacia el objetivo
        Vector3 dir = (objetivo.position - transform.position).normalized;
        transform.position += dir * velocidad * Time.deltaTime;

        // Rotar flecha para apuntar
        float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo);

        // Colisión aproximada
        if (Vector3.Distance(transform.position, objetivo.position) < 0.1f)
        {
            Enemigo e = objetivo.GetComponent<Enemigo>();
            if (e != null)
                e.RecibirDanio(danio);

            Destroy(gameObject);
        }
    }
}
