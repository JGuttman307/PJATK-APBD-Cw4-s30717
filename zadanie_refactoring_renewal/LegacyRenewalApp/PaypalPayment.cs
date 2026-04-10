namespace LegacyRenewalApp;

public class PaypalPayment : IPaymentFeeModel
{
    public bool Supports(string method) => method == "PAYPAL";

    public decimal Calculate(decimal amount) => amount * 0.035m;

    public string Note => "paypal fee;";
}