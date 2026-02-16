using UnityEngine;
using System.Collections.Generic;

public class Oso : Enemigo 
{
    // Usamos Start para cambiar las estadísticas sin romper el movimiento
    void Start()
    {
        // El Oso es un tanque lento
        velocidad = 3f; 
        vida = 40;
    }
}