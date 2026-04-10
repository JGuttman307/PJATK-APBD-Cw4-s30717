namespace LegacyRenewalApp;

public interface ISupportFeeCalculator
{
    decimal Calculate(string planCode, bool includePremiumSupport, ref string notes);

}