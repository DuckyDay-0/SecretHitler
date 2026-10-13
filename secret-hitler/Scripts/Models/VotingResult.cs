using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class VotingResult : Node
{
	public bool Passed { get; set; }
    public int JaCount { get; set; }
    public int NeinCount { get; set; }
    public Dictionary<int, Enums.VoteType> IndividualVotes { get; set; }
}
