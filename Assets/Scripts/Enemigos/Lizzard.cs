public class Lizzard : Enemigo
{
    protected override void Awake()
    {
        base.Awake(); 
        velocidad = 2f;
        vida = 6;
        recompensa = 15; // Dinero Lizzard
    }
}