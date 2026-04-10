using System;
using System.Collections.Generic;

namespace LegacyRenewalApp;

public class DiscountCalculator
//jest DRY, i zamiast if dodaje klasę
{
    private readonly IEnumerable<IDiscountModel> _strategies;

    public DiscountCalculator(IEnumerable<IDiscountModel> model)
    {
        _strategies = model;
    }

    public (decimal amount, string notes) Calculate(
        decimal baseAmount,
        Customer customer,
        SubscriptionPlan plan,
        int seatCount,
        bool usePoints)
    {
        decimal total = 0;
        string notes = "";

        foreach (var strategy in _strategies)
        {
            if (strategy.IsApplicable(customer, plan, seatCount))
            {
                total += strategy.Calculate(baseAmount, customer, seatCount);
                notes += strategy.GetNote() + " ";
            }
        }

        // wyjątek dla loyalty points
        if (usePoints && customer.LoyaltyPoints > 0)
        {
            int points = Math.Min(customer.LoyaltyPoints, 200);
            total += points;
            notes += $"loyalty points used: {points}; ";
        }

        return (total, notes);
    }
}