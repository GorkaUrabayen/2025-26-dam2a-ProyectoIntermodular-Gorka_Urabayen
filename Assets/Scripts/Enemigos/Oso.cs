public class Oso : Enemigo 
{
    protected override void Awake()
    {
        base.Awake(); 
        velocidad = 5f; 
        vida = 40;
        recompensa = 20; // Dinero Oso
    }
}