using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class VotingManager : Node
{
	private readonly List<Player> votingQueue = new();
	private readonly Dictionary<int, Enums.VoteType> voteResults = new();

	private int currentVoterId = 0;

	public event Action<Player>? OnVoterChanged;
	public event Action<VotingResult>? OnVotingCompleted;

	public bool IsVotingInProgress => votingQueue.Count > 0;
	public Player? CurrentVoter => votingQueue.Count > 0 ? votingQueue[currentVoterId] : null;

	public void StartNewVotingSession(List<Player> allPlayers)
	{ 
		votingQueue.Clear();
		voteResults.Clear();

		currentVoterId = 0;
		votingQueue.AddRange(allPlayers.FindAll(player => player.IsAlive));

		if (votingQueue.Count > 0)
		{
			OnVoterChanged?.Invoke(votingQueue[currentVoterId]);
		}
    }


	public void CastVode(Enums.VoteType vote)
	{
		voteResults[CurrentVoter.ID] = vote;
		currentVoterId++;

		if (IsVotingInProgress)
		{
			OnVoterChanged?.Invoke(CurrentVoter);
		}
		else 
		{
			FinalizeVoting();
		}
	}

	private void FinalizeVoting()
	{
        int jaCount = voteResults.Values.Count(v => v == Enums.VoteType.Ja);
		int neinCount = voteResults.Values.Count(v => v == Enums.VoteType.Nein);
		bool passed = jaCount > neinCount;

		var result = new VotingResult
		{
			Passed = passed,
			JaCount = jaCount,
			NeinCount = neinCount,
			IndividualVotes = new Dictionary<int, Enums.VoteType>(voteResults)
		};
		OnVotingCompleted?.Invoke(result);
	}
}
