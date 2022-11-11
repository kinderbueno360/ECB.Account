using Boc.Domain.Events;
using ECB.ClientAccount.Domain;
using LaYumba.Functional;
using static LaYumba.Functional.F;

namespace ECB.ClientAccount.Transitions
{
    public static class AccountTransitctions
    {
        // handle commands

        public static Transition<AccountState, CreatedAccount>
        Create(CreateAccount cmd)
           => _ =>
        {
            var evt = cmd.ToEvent();
            var newState = evt.ToAccount();
            return (evt, newState);
        };


        public static Transition<AccountState, DepositedCash>
        Deposit(AcknowledgeCashDeposit cmd)
           => account =>
        {
            if (account.Status != AccountStatus.Active)
                return Errors.AccountNotActive;

            var evt = cmd.ToEvent();
            var newState = account.Apply(evt);

            return (evt, newState);
        };

        public static Transition<AccountState, AlteredOverdraft>
        SetOverdraft(SetOverdraft cmd)
           => account =>
        {
            var evt = cmd.ToEvent(cmd.Amount - account.AllowedOverdraft);
            var newState = account.Apply(evt);

            return (evt, newState);
        };

        public static Validation<FrozeAccount> Freeze
           (this AccountState @this, FreezeAccount cmd)
        {
            if (@this.Status == AccountStatus.Frozen)
                return Errors.AccountNotActive;

            return cmd.ToEvent();
        }

        // apply events

        public static AccountState ToAccount(this CreatedAccount evt)
           => new AccountState
              (
                 Currency: evt.Currency,
                 Status: AccountStatus.Active
              );

        public static AccountState Apply(this AccountState acc, Event evt)
           => evt switch
           {
               DepositedCash e => acc with { Balance = acc.Balance + e.Amount },
               DebitedTransfer e => acc with { Balance = acc.Balance - e.DebitedAmount },
               FrozeAccount => acc with { Status = AccountStatus.Frozen },
               AlteredOverdraft e => acc with { AllowedOverdraft  = e.By },
               _ => throw new InvalidOperationException()
           };

        public static Option<AccountState> From(IEnumerable<Event> history)
               => history.Match(
                  Empty: () => None,
                  Otherwise: (createdEvent, otherEvents) => Some(
                     otherEvents.Aggregate(
                        seed: Account.Create((CreatedAccount)createdEvent),
                        func: (state, evt) => state.Apply(evt))));
    }
}
