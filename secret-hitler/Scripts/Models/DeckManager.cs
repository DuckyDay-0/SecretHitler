using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DeckManager
{
	private List<Enums.PolicyType> drawDeck = new();
	private List<Enums.PolicyType> discardPile = new();
	private readonly Random rnd = new();

	public int RemainingCards => drawDeck.Count;
	public int DiscardedCards => discardPile.Count;
    public void InitializeDeck()
	{
		drawDeck.AddRange(Enumerable.Repeat(Enums.PolicyType.Liberal, 6));
		drawDeck.AddRange(Enumerable.Repeat(Enums.PolicyType.Fascist, 11));

		Shuffle();
	}

	public void Shuffle()
    {
       drawDeck = drawDeck.OrderBy(x => rnd.Next()).ToList();
    }

	public List<Enums.PolicyType> DrawPolicies(int count)
    {
		List<Enums.PolicyType> drawnCards = new();

		for (int i = 0; i < count; i++)
		{

			if (drawDeck.Count == 0)
			{
				ReshuffleDiscardPile();
			}

			if (drawDeck.Count > 0)
			{
				Enums.PolicyType card = drawDeck[0];
				drawDeck.RemoveAt(0);
				drawnCards.Add(card);
            }
		}

		return drawnCards;
    }

	public void Discard(Enums.PolicyType policy)
	{ 
		discardPile.Add(policy);
	}

	public List<Enums.PolicyType> Peek(int count = 3)
	{
		if (drawDeck.Count < count)
		{
            ReshuffleDiscardPile();
        }
		return drawDeck.Take(count).ToList();
	}

	public void ReshuffleDiscardPile()
    {
        drawDeck.AddRange(discardPile);
        discardPile.Clear();
        Shuffle();
    }
}
