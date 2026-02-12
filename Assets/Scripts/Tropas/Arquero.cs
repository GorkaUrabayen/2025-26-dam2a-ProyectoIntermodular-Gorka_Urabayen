using UnityEngine;

public class Arquero : Torre
{
    protected void Awake()
    {
        alcance = 3f;
        coste = 30;
        danio = 3; // <--- Cambiamos el daño a 3 (6 vida / 2 disparos = 3 daño)
    }

    protected override void Atacar()
    {
        if (objetivoActual == null) return;

        Vector2 dir = (objetivoActual.transform.position - transform.position).normalized;

        if (animator != null)
        {
            // Lógica de animaciones (sin cambios)
            if (dir.y > 0.6f)
            {
                if (Mathf.Abs(dir.x) < 0.3f) animator.Play("Shoot Up");
                else animator.Play("Shoot Diagonal Up");
            }
            else if (dir.y < -0.6f)
            {
                if (Mathf.Abs(dir.x) < 0.3f) animator.Play("Shoot Down");
                else animator.Play("Shoot Diagonal Down");
            }
            else
            {
                animator.Play("Shoot Front");
            }
        }

        // DISPARO
        if (flechaPrefab != null && puntoDisparo != null)
        {
            GameObject flecha = Instantiate(flechaPrefab, puntoDisparo.position, Quaternion.identity);
            Flecha f = flecha.GetComponent<Flecha>();
            if (f != null)
                // Aquí pasamos el daño de 3 que definimos arriba
                f.SetObjetivo(objetivoActual.transform, danio); 
        }
    }
}