using UnityEngine;
using System.Collections.Generic;

public class Oso : Enemigo 
{
    protected override void Awake()
    {
        base.Awake(); 
        velocidad = 5f; 
        vida = 40;
    }
    
}