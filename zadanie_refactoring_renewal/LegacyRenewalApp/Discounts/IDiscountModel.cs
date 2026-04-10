namespace LegacyRenewalApp;

public interface IDiscountModel
{
    bool IsApplicable(Customer customer, SubscriptionPlan plan, int seatCount);
    decimal Calculate(decimal baseAmount, Customer customer, int seatCount);
    string GetNote();
}