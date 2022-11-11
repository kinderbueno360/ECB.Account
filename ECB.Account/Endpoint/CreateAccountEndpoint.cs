using Boc.Domain.Events;
using ECB.ClientAccount.Domain;
using ECB.ClientAccount.Transitions;
using LaYumba.Functional;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;
using static LaYumba.Functional.F;

namespace ECB.ClientAccount.Api.Endpoint
{
    
    public static class CreateAccountEndpoint
    {
        public static void ConfigureCreateAccountEndpoint
        (
            this WebApplication app,
            Func<CreateAccountWithOptions, Validation<CreateAccountWithOptions>> validate,
            Action<Event> saveAndPublish
        ) 
        => app.MapPost("/account", (CreateAccountWithOptions cmd)
        => validate(cmd)
            .Bind(x=>Initialize(x, saveAndPublish))
            .Match(
                Invalid: errs => BadRequest(new { Errors = errs }),
                Valid: id => Ok(id)));

        static Validation<Guid> Initialize(CreateAccountWithOptions cmd, Action<Event> saveAndPublish)
        {
            Guid id = Guid.NewGuid();
            DateTime now = DateTime.UtcNow;

            var create = new CreateAccount
            (
               Timestamp: now,
               AccountId: id,
               Currency: cmd.Currency
            );

            var depositCash = new AcknowledgeCashDeposit
            (
               Timestamp: now,
               AccountId: id,
               Amount: cmd.InitialDepositAccount,
               BranchId: cmd.BranchId
            );

            var setOverdraft = new SetOverdraft
            (
               Timestamp: now,
               AccountId: id,
               Amount: cmd.AllowedOverdraft
            );

            var transitions =
               from e1 in AccountTransitctions.Create(create)
               from e2 in AccountTransitctions.Deposit(depositCash)
               from e3 in AccountTransitctions.SetOverdraft(setOverdraft)
               select List<Event>(e1, e2, e3);

            return transitions(default(AccountState))
               .Do(t => t.Item1.ForEach(saveAndPublish))
               .Map(_ => id);
        }
    }

    public record CreateAccountWithOptions
    (
       DateTime Timestamp,

       CurrencyCode Currency,
       decimal InitialDepositAccount,
       decimal AllowedOverdraft,
       Guid BranchId
    )
       : Command(Timestamp);
}
