using Godot;
using System;

public partial class Player
{
	public int ID { get; set; }
	public string Name { get; set; }
	public Enums.Role SecretRole { get; set; }
	public Enums.PartyMembership PartyMembership => SecretRole == Enums.Role.Liberal ? Enums.PartyMembership.Liberal : Enums.PartyMembership.Fascist;

	public bool IsAlive { get; set; } = true;
	public bool WasChancellor { get; set; } = false;
	public bool WasPresident { get; set; } = false;

	public Player(int id, string name)
	{
		ID = id;
		Name = name;
	}

}
