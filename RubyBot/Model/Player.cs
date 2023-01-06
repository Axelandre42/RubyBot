namespace RubyBot.Model;

public class Player
{
	public ulong Id { get; set; }
	public int Seed { get; set; }
	
	public virtual Persona? LastUsed { get; set; }

	public virtual List<Persona> Personas { get; set; }
}