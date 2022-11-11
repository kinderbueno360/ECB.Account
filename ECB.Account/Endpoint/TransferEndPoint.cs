using Boc.Domain.Events;
using ECB.ClientAccount.Domain;
using LaYumba.Functional;
using static Microsoft.AspNetCore.Http.Results;

namespace ECB.ClientAccount
{
    public static class TransferEndPoint
    {
        public static void ConfigureMakeTransferEndpoint
        (
           this WebApplication app,
           Func<MakeTransfer, Validation<MakeTransfer>> validate,
           Func<Guid, Option<AccountState>> getAccount,
           Action<Event> saveAndPublish
        )
        => app.MapPost("/account/transfer", (MakeTransfer transfer)
        => validate(transfer)
           .Bind(t => getAccount(t.DebitedAccountId)
              .ToValidation($"No account found for {t.DebitedAccountId}"))
           .Bind(acc => acc.Debit(transfer))
           .Do(result => saveAndPublish(result.Event))
           .Match(
              Invalid: errs => BadRequest(new { Errors = errs }),
              Valid: result => Ok(new { result.NewState.Balance })));
    }
}
