using Boc.Domain;

namespace ECB.ClientAccount

{
   public enum AccountStatus
   { Requested, Active, Frozen, Dormant, Closed }
}

namespace ECB.ClientAccount.Domain
{
   public sealed record AccountState
   (
      CurrencyCode Currency,
      AccountStatus Status = AccountStatus.Requested,
      decimal Balance = 0,
      decimal AllowedOverdraft = 0
   );
}
