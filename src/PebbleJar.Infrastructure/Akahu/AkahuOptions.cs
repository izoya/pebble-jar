using PebbleJar.Extensions.Types;
using System.ComponentModel.DataAnnotations;

namespace PebbleJar.Infrastructure.Akahu
{
    public sealed class AkahuOptions
    {
        [Required]
        public required Uri BaseUrl { get; init; }
        [Required]
        public required SecretString AppIdToken { get; init; }
        [Required]
        public required SecretString UserAccessToken { get; init; }
    }
}
