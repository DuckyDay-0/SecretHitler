using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Enums;

public class ExecutiveManager
{
    private readonly DeckManager _deck;
    private readonly List<Player> _players;
    private readonly HashSet<int> _investigatedPlayerIds = new();

    public Player? SpecialElectionTarget { get; private set; }

    public event Action<Enums.ExecutiveActionType>? OnActionTriggered;
    public event Action<List<PolicyType>>? OnPolicyPeekReceived;
    public event Action<Player, PartyMembership>? OnInvestigationCompleted;
    public event Action<Player, bool>? OnPlayerExecuted; // Player, wasHitler

    public ExecutiveManager(DeckManager deck, List<Player> players)
    {
        _deck = deck;
        _players = players;
    }

    public ExecutiveActionType DetermineAction(int fascistPolicyCount, int totalPlayers)
    {
        return fascistPolicyCount switch
        {
            1 => (totalPlayers >= 9) ? ExecutiveActionType.InvestigateLoyalty : ExecutiveActionType.None,
            2 => (totalPlayers >= 7) ? ExecutiveActionType.InvestigateLoyalty : ExecutiveActionType.None,
            3 => (totalPlayers <= 6) ? ExecutiveActionType.PolicyPeek : ExecutiveActionType.SpecialElection,
            4 => ExecutiveActionType.Execution,
            5 => ExecutiveActionType.Execution, // Отключва се и Veto power
            _ => ExecutiveActionType.None
        };
    }

    public void ExecutePolicyPeek()
    {
        var top3 = _deck.Peek(3);
        OnPolicyPeekReceived?.Invoke(top3);
    }

    public List<Player> GetInvestigablePlayers(Player currentPresident)
    {
        // Не може да разследваш себе си, мъртви или вече разследвани играчи
        return _players
            .Where(p => p.IsAlive && p.ID != currentPresident.ID && !_investigatedPlayerIds.Contains(p.ID))
            .ToList();
    }

    public void ExecuteInvestigation(Player target)
    {
        _investigatedPlayerIds.Add(target.ID);
        OnInvestigationCompleted?.Invoke(target, target.PartyMembership);
    }

    public List<Player> GetSpecialElectionCandidates(Player currentPresident)
    {
        return _players.Where(p => p.IsAlive && p.ID != currentPresident.ID).ToList();
    }

    public void ExecuteSpecialElection(Player chosenNextPresident)
    {
        SpecialElectionTarget = chosenNextPresident;
        GD.Print($"[ExecutiveManager] Извънредни избори! Следващ временен президент: {chosenNextPresident.Name}");
    }

    public List<Player> GetExecutablePlayers(Player currentPresident)
    {
        return _players.Where(p => p.IsAlive && p.ID != currentPresident.ID).ToList();
    }

    public void ExecutePlayer(Player target)
    {
        target.IsAlive = false;
        bool wasHitler = (target.SecretRole == Role.Hitler);

        GD.Print($"[ExecutiveManager] Играчът {target.Name} беше ЕКЗЕКУТИРАН! Беше ли Хитлер: {wasHitler}");
        OnPlayerExecuted?.Invoke(target, wasHitler);
    }
}