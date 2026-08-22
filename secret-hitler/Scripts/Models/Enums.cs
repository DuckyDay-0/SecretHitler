using Godot;
using System;

public static class Enums
{
	public enum Role { Liberal, Fascist, Hitler }
	public enum PartyMembership { Liberal, Fascist }
	public enum PolicyType {Liberal, Fascist}
	public enum GameState { Setup, RoleReveal, Nomination, Voting, LegislativePresident, LegislativeChancellor, ExecutiveAction, GameOver }
	public enum VoteType { Ja, Nein }
}
