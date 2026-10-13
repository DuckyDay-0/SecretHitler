using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

public partial class LegislativeManager : Node
{

	private readonly DeckManager Deck;
	public List<Enums.PolicyType> PresidentHand { get; private set; } = new();
	public List<Enums.PolicyType> ChancellorHand { get; private set; } = new();

	public bool isVetoUnlocked { get; private set; } = false;
	public bool isVetoRequested { get; private set; } = false;

	public event Action<List<Enums.PolicyType>>? OnPresidentReceivedCards;
	public event Action<List<Enums.PolicyType>>? OnChancellorReceivedCards;
	public event Action? OnVetoRequested;
	public event Action<Enums.PolicyType>? OnPolicyEnacted;
	public event Action OnVetoApproved;

	public LegislativeManager(DeckManager deck)
	{ 
		Deck = deck;
	}

	public void StartSession(bool vetoAvailable)
	{ 
		isVetoUnlocked = vetoAvailable;
		isVetoRequested = false;
		PresidentHand.Clear();
		ChancellorHand.Clear();

		PresidentHand = Deck.DrawPolicies(3);
		OnPresidentReceivedCards?.Invoke(new List<Enums.PolicyType>(PresidentHand));
    }

	public void PresidentDiscard(int cardIndex)
	{
		if (cardIndex < 0 || cardIndex >= PresidentHand.Count)
		{ 
			GD.Print("Invalid card index");
			return;
		}

		Enums.PolicyType discardedCard = PresidentHand[cardIndex];
		PresidentHand.RemoveAt(cardIndex);
		Deck.Discard(discardedCard);

        //Remaining card is sent to the Chancellor
		ChancellorHand = new List<Enums.PolicyType>(PresidentHand);
		PresidentHand.Clear();

		OnChancellorReceivedCards?.Invoke(new List<Enums.PolicyType>(ChancellorHand));
    }

	public void ChancellorDiscard(int cardIndex)
	{
		if (cardIndex < 0 || cardIndex >= ChancellorHand.Count)
		{ 
			GD.Print("Invalid card index");
			return;
		}

		Enums.PolicyType enactedPolicy = ChancellorHand[cardIndex];
		ChancellorHand.RemoveAt(cardIndex);

		Enums.PolicyType discarded = ChancellorHand[0];
		ChancellorHand.Clear();
		Deck.Discard(discarded);

		OnPolicyEnacted?.Invoke(enactedPolicy);
    }

	public void RequestVeto()
	{
		if (!isVetoUnlocked)
		{ 
			GD.Print("Veto is not unlocked yet.");
			return;
        }

		isVetoRequested = true;
		OnVetoRequested?.Invoke();
    }

	public void ResovleVeto(bool presidentAgrees)
	{
		if (!isVetoRequested)
		{
			return;
		}

		if (presidentAgrees)
		{
			foreach (var card in ChancellorHand)
			{
				Deck.Discard(card);
			}
			ChancellorHand.Clear();
			isVetoRequested = false;

			OnVetoApproved?.Invoke();
		}
		else
		{
            isVetoRequested = false;
			OnChancellorReceivedCards?.Invoke(new List<Enums.PolicyType>(ChancellorHand));
        }
	}

}
