using Grpc.Net.Client;
using GrpcClient;

using var channel = GrpcChannel.ForAddress("http://localhost:5238");

var client = new Billing.BillingClient(channel);

async Task UserList()
{
    var reply = client.ListUsers(new None());
    var hasNext = true;
    while (hasNext)
    {
        hasNext = await reply.ResponseStream.MoveNext(new CancellationToken());

        if (hasNext)
        {
            Console.WriteLine("User: " + reply.ResponseStream.Current.Name);
            Console.WriteLine("User: " + reply.ResponseStream.Current.Amount);
        }
    }
}

UserList();
var secondReply = client.CoinsEmission(new EmissionAmount() { Amount = 10 });

Console.WriteLine("EmissionAmount 10 Coin: " + secondReply.Status);

UserList();
var therdReply = client.MoveCoins(
    new MoveCoinsTransaction() { SrcUser = "boris", DstUser = "maria", Amount = 5 });
UserList();
var forthReply = client.LongestHistoryCoin(new None());
Console.WriteLine("Coin history: " + forthReply.History);


Console.WriteLine("Press any key to exit...");

Console.ReadKey();