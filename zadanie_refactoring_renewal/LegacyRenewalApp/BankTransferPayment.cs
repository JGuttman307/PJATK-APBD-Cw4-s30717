namespace LegacyRenewalApp;
public class BankTransferPayment : IPaymentFeeModel
{
    public bool Supports(string method) => method == "BANK_TRANSFER";

    public decimal Calculate(decimal amount) => amount * 0.01m;

    public string Note => "bank transfer fee;";
}