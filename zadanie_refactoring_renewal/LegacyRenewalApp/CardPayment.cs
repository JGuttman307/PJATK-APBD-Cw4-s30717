namespace LegacyRenewalApp;
    public class CardPayment : IPaymentFeeModel
    {
        public bool Supports(string method) => method == "CARD";

        public decimal Calculate(decimal amount) => amount * 0.02m;

        public string Note => "card payment fee;";
    }
