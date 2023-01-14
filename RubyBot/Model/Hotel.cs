namespace RubyBot.Model;

public class Hotel
{
	public ulong Id { get; set; }
	public byte Size { get; set; }
	
	public virtual List<Room> Rooms { get; set; } = null!;
}