
using System.ComponentModel.DataAnnotations;

namespace PebbleJar.Domain;

public enum DataScope
{
    Account = 1,
    Transaction = 2,
}

public class DataVersion
{
    [Key]
    public DataScope Scope { get; init; }
    public int Revision { get; init; }
}
