namespace LegacyRenewalApp;

public class SupportFeeCalculator : ISupportFeeCalculator
{
    public decimal Calculate(string planCode, bool includePremiumSupport, ref string notes)
    {
        if (!includePremiumSupport)
            return 0m;

        return planCode switch
        {
            "START" => AddNote(250m, ref notes),
            "PRO" => AddNote(400m, ref notes),
            "ENTERPRISE" => AddNote(700m, ref notes),
            _ => 0m
        };
    }
    private decimal AddNote(decimal value, ref string notes)
    {
        notes += "premium support included; ";
        return value;
    }

}