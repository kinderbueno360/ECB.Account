using Boc.Domain.Events;

namespace ECB.ClientAccount
{
    public abstract record Command(DateTime Timestamp);


    public record MakeTransfer
  (
     Guid DebitedAccountId,

     string Beneficiary,
     string Iban,
     string Bic,

     DateTime Date,
     decimal Amount,
     string Reference,

     DateTime Timestamp = default
  )
     : Command(Timestamp)
    {
        // useful for testing, when you don't need all the properties to be populated
        internal static MakeTransfer Dummy
           => new(default, default, default, default, default, default, default);

        public DebitedTransfer ToEvent() => new
        (
           Beneficiary: this.Beneficiary,
           Bic: this.Bic,
           DebitedAmount: this.Amount,
           EntityId: this.DebitedAccountId,
           Iban: this.Iban,
           Reference: this.Reference,
           Timestamp: this.Timestamp
        );
    }
}
