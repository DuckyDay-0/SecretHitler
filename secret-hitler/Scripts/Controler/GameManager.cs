using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Enums;
using static Godot.OpenXRCompositionLayer;
using static Godot.Projection;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	public List<Player> players { get; private set; } = new();
	public DeckManager Deck { get; private set; } = new();
	public int LiberalPolicies { get; private set; } = 0;
	public int FascistPolicies { get; private set; } = 0;
	public int ElectionTracker { get; private set; } = 0;
	public int CurrentPresidentIndex { get; private set; } = 0;
	public Player? CurrentPresident => players.Count > 0 ? players[CurrentPresidentIndex] : null;
	public Player? NominatedChancellor { get; set; }
	public Player? PrevPresident { get; private set; }
	public Player? PrevChancellor { get; private set; }
	public VotingManager Voting { get; private set; } = new();
	public LegislativeManager Legislative { get; private set; } = null;
	public ExecutiveManager Executive { get; private set; }
	private int? regularPresidentBackupId = null;
    public int GetAlivePlayer => players.Count(p => p.IsAlive);

    private Random rnd = new Random();


	
    public override void _Ready()
    {
        Instance = this;
		Voting.OnVotingCompleted += HandleVotingResult;
		StartGame(new List<string> { "Играч 1", "Играч 2", "Играч 3", "Играч 4", "Играч 5" });
        Legislative = new LegislativeManager(Deck);
		Legislative.OnPolicyEnacted += HandlePolicyEnacted;
		Legislative.OnVetoApproved += HandleVetoApproved;

		Executive = new ExecutiveManager(Deck, players);
		Executive.OnPlayerExecuted += HandlePlayerExecuted;
    }

    private void CheckExecutiveAction()
    {
        var action = Executive.DetermineAction(FascistPolicies, players.Count);

        if (action == ExecutiveActionType.None)
        {
            AdvancePresident();
            return;
        }

        GD.Print($"[GameManager] Активирана президентска сила: {action}");
    }

    public void FinishExecutiveAction()
    {
        if (Executive.SpecialElectionTarget != null)
        {
            regularPresidentBackupId = CurrentPresidentIndex;
            CurrentPresidentIndex = players.IndexOf(Executive.SpecialElectionTarget);
            Executive.ExecuteSpecialElection(null!); // Нулираме избора
            NominatedChancellor = null;
            GD.Print($"[GameManager] Започва извънреден ред на: {CurrentPresident?.Name}");
        }
        else
        {
            AdvancePresident();
        }
    }

    public void AdvancePresident()
    {
        if (GetAlivePlayer == 0) return;

        if (regularPresidentBackupId.HasValue)
        {
            CurrentPresidentIndex = regularPresidentBackupId.Value;
            regularPresidentBackupId = null;
        }

        do
        {
            CurrentPresidentIndex = (CurrentPresidentIndex + 1) % players.Count;
        }
        while (!players[CurrentPresidentIndex].IsAlive);

        NominatedChancellor = null;
        GD.Print($"[GameManager] Нов редовен президент: {CurrentPresident?.Name}");
    }

    private void HandlePlayerExecuted(Player executedPlayer, bool wasHitler)
    {
        if (wasHitler)
        {
            GD.Print("[GameManager] КРАЙ НА ИГРАТА! Хитлер беше убит! ПОБЕДА ЗА ЛИБЕРАЛИТЕ!");
            return;
        }

        GD.Print($"[GameManager] Играта продължава без {executedPlayer.Name}.");
        FinishExecutiveAction();
    }

    public void StartLegislativePhase()
	{ 
		GD.Print("[GameManager] Legislative Session Started");
        bool vetoUnlocked = FascistPolicies >= 5;
		Legislative.StartSession(vetoUnlocked);
    }

	public void HandlePolicyEnacted(Enums.PolicyType enactedPolicy)
	{
		if (enactedPolicy == Enums.PolicyType.Fascist)
		{
			FascistPolicies++;
			GD.Print($"{FascistPolicies} Fascist Policie accepted");
		}

		if (enactedPolicy == Enums.PolicyType.Liberal)
		{
			LiberalPolicies++;
			GD.Print($"{LiberalPolicies} Liberal Policies accepted");
		}

		CheckEndGameCondition(enactedPolicy);
	}

	public void HandleVetoApproved()
	{
		GD.Print("[GameManager] Вето е одобрено! Изборите се провалиха.");
		OnElectionFail();
	}

	private void CheckEndGameCondition(Enums.PolicyType enactedPolicy)
	{
		if (LiberalPolicies >= 5)
		{ 
			GD.Print("[GameManager] КРАЙ НА ИГРАТА! 5 Liberal Policies accepted! ПОБЕДА ЗА ЛИБЕРАЛТЕ!");
			return;
		}

		if (FascistPolicies >= 6)
		{
			GD.Print("[GameManager] КРАЙ НА ИГРАТА! 6 Fascist Policies accepted! ПОБЕДА ЗА ФАШИСТИТЕ!");
			return;
		}

		if (enactedPolicy == Enums.PolicyType.Fascist)
		{
			CheckExcecutiveAction();
		}
		else 
		{
			AdvancePresident();
		}
	}

	private void CheckExcecutiveAction()
	{
        GD.Print($"[GameManager] Фашистки закон активиран на позиция {FascistPolicies}. Проверка за Executive Action...");
		AdvancePresident();
    }

    public void StartVotingPhase()
	{
		if (NominatedChancellor == null)
		{ 
			GD.PrintErr("No nominated chancellor to vote on.");
			return;
		}

		GD.Print("Voting Session Started");
		Voting.StartNewVotingSession(players);
    }

	public void HandleVotingResult(VotingResult result)
	{
		GD.Print($"--- РЕЗУЛТАТИ ОТ ГЛАСУВАНЕТО ---");
		GD.Print($"Ja: {result.JaCount} | Nein: {result.NeinCount} -> {(result.Passed ? "УСПЕШНО" : "ПРОВАЛЕНО")}");


		foreach (var (playerId, vote) in result.IndividualVotes)
		{
			var player = players.First(p => p.ID == playerId);
			GD.Print($"{player.Name} : {vote}");
		}

		if (result.Passed)
		{
			if (FascistPolicies >= 3 && NominatedChancellor.SecretRole == Enums.Role.Hitler)
			{
				GD.Print("[GameManager] КРАЙ НА ИГРАТА! Хитлер беше избран за канцлер след 3-тия фашистки закон! ПОБЕДА ЗА ФАШИСТИТЕ!");
				return;
			}
			OnElectionSuccess(NominatedChancellor);
		}
		else
		{
			OnElectionFail();
		}
	}

    public void NextPresident()
    {
        if (players.Count == 0)
        {
            return;
        }
        do
        {
            CurrentPresidentIndex = (CurrentPresidentIndex + 1) % players.Count;
        } while (!CurrentPresident.IsAlive);
        NominatedChancellor = null;
        GD.Print($"[GameManager] Нов ред. Настоящ президент: {CurrentPresident?.Name}");
    }
    public bool CanBeNominatedAsChancellor(Player candidate)
    {
        // 1. Не може да номинираш мъртъв играч
        if (!candidate.IsAlive)
            return false;

        // 2. Президентът не може да номинира себе си
        if (candidate.ID == CurrentPresident?.ID)
            return false;

        // 3. Последният избран канцлер НИКОГА не може да бъде избран веднага отново
        if (PrevChancellor != null && candidate.ID == PrevChancellor.ID)
            return false;

        // 4. При 6 или повече живи играчи, последният президент също има Term Limit
        if (GetAlivePlayer > 5)
        {
            if (PrevPresident != null && candidate.ID == PrevPresident.ID)
                return false;
        }

        return true;
    }

	public List<Player> GetEligableChancellerPlayers(Player candidate)
	{
		return players.Where(CanBeNominatedAsChancellor).ToList();
	}

	public void OnElectionSuccess(Player electedChancellor)
	{
		ElectionTracker = 0;
		PrevPresident = CurrentPresident;
		PrevChancellor = electedChancellor;

		foreach (var p in players)
		{ 
			p.WasPresident = (p.ID == CurrentPresident?.ID);
			p.WasChancellor = (p.ID == electedChancellor.ID);

            GD.Print($"[GameManager] Избрано правителство: Президент {CurrentPresident?.Name}, Канцлер {electedChancellor.Name}");
        }

    }

	public void OnElectionFail()
	{
		ElectionTracker++;
        GD.Print($"[GameManager] Изборите се провалиха! Election Tracker: {ElectionTracker}/3");
        if (ElectionTracker >= 3)
		{
			HandleChaosPolicy();
		}
		else
		{ 
			NextPresident();
        }
	}

	public void HandleChaosPolicy()
	{
        GD.Print("[GameManager] ВРИВА НА ХАОС! 3 провалени избора. Активира се първата карта от тестето.");
		var chaosPolicy = Deck.DrawPolicies(1);

		if (chaosPolicy.Any())
		{
			if (chaosPolicy[0] == Enums.PolicyType.Fascist)
			{
				FascistPolicies++;
			}
			else 
			{
				LiberalPolicies++;
			}
            GD.Print($"[GameManager] Автоматично приет закон: {chaosPolicy[0].ToString()}");
        }

        ElectionTracker = 0;
        PrevPresident = null;
		PrevChancellor = null;

		foreach (var p in players)
		{ 
			p.WasChancellor = false;
			p.WasPresident = false;
        }

		NextPresident();
    }


    public void StartGame(List<string> playersName)
	{
		if (players.Count < 5 || players.Count > 10)
		{
			GD.PrintErr("Invalid number of players. Must be between 5 and 10.");
			return;
        }
		LiberalPolicies = 0;
		FascistPolicies = 0;
		ElectionTracker = 0;

        Deck.InitializeDeck();
		SetupPlayers(playersName);
		AssignRoles();

		CurrentPresidentIndex = rnd.Next(0, players.Count);
        GD.Print($"Играта стартира успешно с {players.Count} играчи! Първи президент: {CurrentPresident?.Name}");

    }

	private void SetupPlayers(List<string> playersName)
    {
        players.Clear();
        for (int i = 0; i < playersName.Count; i++)
        {
            players.Add(new Player(i, playersName[i]));
			GD.Print($"[GameManager] Играч {i + 1}: {playersName[i]} е добавен към играта.");
        }
    }

	private void AssignRoles()
	{
		int totalPlayers = players.Count;
		var (liberalCount,fascistCount) = GetRoleCounts(totalPlayers);

		List<Enums.Role> roleDeck = new();
		roleDeck.AddRange(Enumerable.Repeat(Enums.Role.Liberal, liberalCount));
		roleDeck.AddRange(Enumerable.Repeat(Enums.Role.Fascist, fascistCount));
		roleDeck.Add(Enums.Role.Hitler);

		roleDeck.OrderBy(x => rnd.Next()).ToList();
		for (int i = 0; i < totalPlayers; i++)
		{
			players[i].SecretRole = roleDeck[i];
		}
    }

	private (int liberalCount, int fascistCount) GetRoleCounts(int totalPlayers) => totalPlayers switch
	{
		5 => (3, 1),
		6 => (4, 1),
		7 => (4, 2),
		8 => (5, 2),
		9 => (5, 3),
		10 => (6, 3),
		_ => throw new ArgumentException("Invalid number of players. Must be between 5 and 10.")

	};

	public string GetRoleRevealInfo(Player targetPlayer)
	{
		if (targetPlayer.SecretRole == Enums.Role.Liberal)
		{
            return "Ти си ЛИБЕРАЛ.\nТвоята цел е да прокарате 5 либерални закона или да екзекутирате Хитлер.";
        }

		if (targetPlayer.SecretRole == Enums.Role.Fascist)
		{ 
			var otherFascists = players
				.Where(p => p.SecretRole == Enums.Role.Fascist && p.ID != targetPlayer.ID)
				.Select(p => p.Name);

			var secretHitler = players.First(p => p.SecretRole == Enums.Role.Hitler);
			string otherFascistsStr = "";

            if (otherFascists.Any())
			{
                otherFascistsStr = string.Join(", ", otherFascists);
			}
            return $"Ти си ФАШИСТ.\nХитлер е: {secretHitler.Name}\nДруги фашисти: {otherFascistsStr}";
        }


		if (players.Count < 6)
		{
			var soleFascist = players.First(p => p.SecretRole == Enums.Role.Fascist);
            return $"Ти си ХИТЛЕР.\nТъй като играете с {players.Count} играчи, знаеш своя фашист: {soleFascist.Name}";
        }
        return "Ти си ХИТЛЕР.\nПри игра със 7+ играчи НЕ знаеш кои са фашистите. Опитай се да ги разпознаеш!";
    }
}

    
	
