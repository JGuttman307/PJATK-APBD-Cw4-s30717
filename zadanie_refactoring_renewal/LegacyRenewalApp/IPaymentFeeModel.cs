namespace LegacyRenewalApp;

public interface IPaymentFeeModel
{
    bool Supports(string method);
    decimal Calculate(decimal amount);
    string Note { get; }
}