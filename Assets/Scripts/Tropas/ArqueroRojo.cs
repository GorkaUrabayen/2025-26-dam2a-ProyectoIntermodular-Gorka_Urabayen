using UnityEngine;

public class ArqueroRojo: Arquero
{
    protected override void Awake()
    {
        // No llamamos a base.Awake para que no sobrescriba nuestros valores de élite
        alcance = 4.5f;   // Un poco más de alcance por ser élite
        coste = 50;      // Mucho más caro
        danio = 20;       // Daño masivo
        cadencia = 0.4f;  // Dispara lento (aprox. 1 flecha cada 2.5 segundos)
    }

    // Nota: Atacar() no hace falta escribirlo porque usa el de la clase Arquero
}