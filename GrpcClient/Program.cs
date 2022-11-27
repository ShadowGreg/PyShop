using Grpc.Net.Client;
using GrpcClient;

using var channel = GrpcChannel.ForAddress("http://localhost:5238");

var client = new Billing.BillingClient(channel);

async Task UserList()
{
    Console.WriteLine();
    Console.WriteLine("UserList:");

    var reply = client.ListUsers(new None());
    var hasNext = true;
    while (hasNext)
    {
        hasNext = await reply.ResponseStream.MoveNext(new CancellationToken());

        if (!hasNext) continue;

        Console.WriteLine("User: " + reply.ResponseStream.Current.Name);
        Console.WriteLine("Amount: " + reply.ResponseStream.Current.Amount);
        Console.WriteLine();
    }
}

await UserList();
var secondReply = client.CoinsEmission(new EmissionAmount { Amount = 10 });

Console.WriteLine("EmissionAmount 10 Coin: " + secondReply.Status);

await UserList();

var thirdReply = client.MoveCoins(
    new MoveCoinsTransaction { SrcUser = "boris", DstUser = "maria", Amount = 5 });
Console.WriteLine("Move 5 Coins from boris to maria : " + thirdReply.Status);

await UserList();

Console.WriteLine("Press any key to exit...");

Console.ReadKey();