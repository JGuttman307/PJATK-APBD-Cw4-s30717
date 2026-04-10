namespace LegacyRenewalApp;
public class InvoicePayment : IPaymentFeeModel
{
    public bool Supports(string method) => method == "INVOICE";

    public decimal Calculate(decimal amount) => 0m;

    public string Note => "invoice payment;";
}