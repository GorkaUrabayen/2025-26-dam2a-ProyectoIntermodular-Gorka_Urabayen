//Clase Oso que ereda de enemigo (el tanque)
public class Oso : Enemigo 
{
    protected override void Awake()
    {
        base.Awake(); 
        velocidad = 1f; 
        vida = 40;
        recompensa = 10; // Dinero Oso
    }
}