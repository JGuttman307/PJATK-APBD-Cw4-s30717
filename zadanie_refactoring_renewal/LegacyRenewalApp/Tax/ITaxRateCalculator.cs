namespace LegacyRenewalApp;

public interface ITaxRateCalculator
{
        decimal GetRate(string country);
}