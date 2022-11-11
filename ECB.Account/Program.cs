using Boc.Data;
using ECB.ClientAccount;
using ECB.ClientAccount.Api.Endpoint;
using ECB.ClientAccount.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
IEventStore eventStore = new InMemoryEventStore();
app.ConfigureMakeTransferEndpoint(x => x, x => new AccountState(new CurrencyCode(), AccountStatus.Active, 10000, 1000), eventStore.Persist);
app.ConfigureCreateAccountEndpoint(x => x, eventStore.Persist);

app.UseHttpsRedirection();

app.Run();
