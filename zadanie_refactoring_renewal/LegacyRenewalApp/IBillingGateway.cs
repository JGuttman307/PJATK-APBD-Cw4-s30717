namespace LegacyRenewalApp;

public interface IBillingGateway
{
    //opakuje sobie LegacyBillingGateway
    void  SaveInvoice(RenewalInvoice invoice);
    void SendEmail(string to, string subject, string body);
}

public class BillingGateway : IBillingGateway
{
    public void SaveInvoice(RenewalInvoice invoice)
    {
        LegacyBillingGateway.SaveInvoice(invoice);
    }

    public void SendEmail(string to, string subject, string body)
    {
        LegacyBillingGateway.SendEmail(to, subject, body);
    }
}