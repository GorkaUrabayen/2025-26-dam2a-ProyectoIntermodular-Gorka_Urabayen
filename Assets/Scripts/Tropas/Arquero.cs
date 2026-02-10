using UnityEngine;

public class Arquero : Torre
{
    protected void Awake()
    {
        alcance = 2f; // Ejemplo: menos alcance que torre base
        coste = 30;   // Ejemplo: el arquero cuesta 30 monedas
    }

    protected override void Atacar()
    {
        if (objetivoActual == null) return;

        Vector3 direccion = objetivoActual.transform.position - transform.position;
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

        if (animator != null)
        {
            if (angulo >= 67.5f && angulo < 112.5f)
                animator.Play("Shoot Up");
            else if (angulo >= 22.5f && angulo < 67.5f)
                animator.Play("Shoot Diagonal Up");
            else if (angulo >= -22.5f && angulo < 22.5f)
                animator.Play("Shoot Front");
            else if (angulo >= -67.5f && angulo < -22.5f)
                animator.Play("Shoot Diagonal Down");
            else
                animator.Play("Shoot Down");
        }

        // Instanciar flecha
        if (flechaPrefab != null && puntoDisparo != null)
        {
            GameObject flecha = Instantiate(flechaPrefab, puntoDisparo.position, Quaternion.identity);
            Flecha f = flecha.GetComponent<Flecha>();
            if (f != null)
                f.SetObjetivo(objetivoActual.transform, danio);
        }
    }
}
