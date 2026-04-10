namespace LegacyRenewalApp;

public class TaxRateCalculator : ITaxRateCalculator
{
    //zamiast tego ogromnego if-else
    public decimal GetRate(string country)
    {
        return country switch
        {
            "Poland" => 0.23m,
            "Germany" => 0.19m,
            "Czech Republic" => 0.21m,
            "Norway" => 0.25m,
            _ => 0.20m
        };
    }
}