using System.ComponentModel.DataAnnotations;

namespace RubyBot.Model;

public class Persona
{
	public int Id { get; set; }
	public string Name { get; set; } = null!;
	public string Alias { get; set; } = null!;
	public bool Locked { get; set; }
	
	public byte Precision { get; set; }
	public byte Strength { get; set; }
	public byte Agility { get; set; }
	public byte Parade { get; set; }
	public byte Resistance { get; set; }
	
	public virtual Player Player { get; set; } = null!;
}