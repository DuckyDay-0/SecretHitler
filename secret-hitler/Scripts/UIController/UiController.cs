using Godot;
using System;
using System.Collections.Generic;

public partial class UIManager : Node
{
    [ExportGroup("UI Panels")]
    [Export] public Control SetupPanel = null!;
    [Export] public Control PrivacyPanel = null!;
    [Export] public Control RoleRevealPanel = null!;
    [Export] public Control GameBoardPanel = null!;
    [Export] public Control NominationPanel = null!;
    [Export] public Control VotingPanel = null!;
    [Export] public Control VoteResultsPanel = null!;
    [Export] public Control LegislativePanel = null!;
    [Export] public Control ExecutivePanel = null!;
    [Export] public Control GameOverPanel = null!;

    [ExportGroup("Privacy Screen Elements")]
    [Export] public Label PrivacyTargetLabel = null!;
    [Export] public Button PrivacyReadyButton = null!;

    private Action? _onPrivacyConfirmedCallback;
    private readonly List<Control> _allPanels = new();

    public override void _Ready()
    {
        _allPanels.AddRange(new[]
        {
            SetupPanel, PrivacyPanel, RoleRevealPanel, GameBoardPanel,
            NominationPanel, VotingPanel, VoteResultsPanel,
            LegislativePanel, ExecutivePanel, GameOverPanel
        });

        PrivacyReadyButton.Pressed += OnPrivacyConfirmed;

        // Показваме само Setup панела в началото
        ShowPanel(SetupPanel);
    }

    /// <summary>
    /// Скрива всички панели и показва само подадения.
    /// </summary>
    public void ShowPanel(Control targetPanel)
    {
        foreach (var panel in _allPanels)
        {
            if (panel != null)
            {
                panel.Visible = (panel == targetPanel);
            }
        }
    }

    /// <summary>
    /// Универсален екран за предаване на телефона.
    /// Изпълнява подадения callback само след като играчът потвърди, че държи устройството.
    /// </summary>
    public void ShowPrivacyScreen(Player targetPlayer, string contextMessage, Action onConfirmed)
    {
        _onPrivacyConfirmedCallback = onConfirmed;
        PrivacyTargetLabel.Text = $"Предай телефона на:\n[ {targetPlayer.Name} ]\n\n({contextMessage})";
        ShowPanel(PrivacyPanel);
    }

    private void OnPrivacyConfirmed()
    {
        var callback = _onPrivacyConfirmedCallback;
        _onPrivacyConfirmedCallback = null;
        callback?.Invoke();
    }

    // --- Фазови UI методи ---

    public void OpenRoleRevealFlow(List<Player> players, int playerIndex = 0)
    {
        if (playerIndex >= players.Count)
        {
            // Всички са видели ролите си -> отиваме към първия борд
            ShowGameBoard();
            return;
        }

        var player = players[playerIndex];
        ShowPrivacyScreen(player, "Проверка на тайната роля", () =>
        {
            ShowPanel(RoleRevealPanel);
            // Тук подаваш данните от GameManager.Instance.GetRoleRevealInfo(player) към RoleReveal панела
        });
    }

    public void ShowGameBoard()
    {
        ShowPanel(GameBoardPanel);
        // Обновяване на броячите: GameManager.Instance.LiberalPolicies / FascistPolicies
    }

    public void StartNominationPhase(Player president)
    {
        ShowPanel(NominationPanel);
        // Показване на бутони с валидните кандидати от GameManager.Instance.GetEligibleChancellorCandidates()
    }

    public void StartVotingPhase()
    {
        var voting = GameManager.Instance.Voting;

        // Въртим Privacy screen за всеки играч от опашката за гласуване
        if (voting.CurrentVoter != null)
        {
            ShowPrivacyScreen(voting.CurrentVoter, "Тайно гласуване (Ja / Nein)", () =>
            {
                ShowPanel(VotingPanel);
            });
        }
    }

    public void ShowVoteSummary(VotingResult result)
    {
        ShowPanel(VoteResultsPanel);
        // Визуализира таблицата с резултатите за всички играчи
    }

    public void StartLegislativePresident(Player president)
    {
        ShowPrivacyScreen(president, "Законодателна сесия: Президент (тегли 3 карти)", () =>
        {
            ShowPanel(LegislativePanel);
            // Показва 3-те карти с опция за изхвърляне на една
        });
    }

    public void StartLegislativeChancellor(Player chancellor)
    {
        ShowPrivacyScreen(chancellor, "Законодателна сесия: Канцлер (избира 1 закон)", () =>
        {
            ShowPanel(LegislativePanel);
            // Показва 2-те останали карти + бутон за Вето (ако е отключено)
        });
    }
}