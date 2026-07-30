namespace ACount.Models;

public enum Currency
{
    NZD = 1,
    USD = 2,
}

public class Account
{
    public Guid Id { get; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string AccountNumber { get; set; }
    public Currency Currency { get; set; } = Currency.NZD;

    private decimal CurrentBalance;

    public override string ToString()
    {
        return $"[Account] Name: {Name}, AccountNumber: {AccountNumber}, Currency: {Currency}";
    }
}

