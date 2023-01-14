namespace RubyBot.Model;

public class Room
{
	public ulong Id { get; set; }
	public byte Number { get; set; }
	public bool Busy { get; set; }

	public virtual Hotel Hotel { get; set; } = null!;
}