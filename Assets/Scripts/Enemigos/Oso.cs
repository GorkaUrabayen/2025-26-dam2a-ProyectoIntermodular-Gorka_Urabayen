public class Oso : Enemigo 
{
    protected override void Awake()
    {
        base.Awake(); 
        velocidad = 1f; 
        vida = 40;
        recompensa = 20; // Dinero Oso
    }
}