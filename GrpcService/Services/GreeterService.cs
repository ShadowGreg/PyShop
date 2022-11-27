using Grpc.Core;

namespace GrpcService.Services;

public class GreeterService : Billing.BillingBase
{
    private readonly CoinBank _coinBank;
    
    public GreeterService(CoinBank coinBank)
    {
        _coinBank = coinBank;
    }
    

    //Billing.ListUsers() – перечисляет пользователей в сервисе. 
    public override async Task ListUsers(None request, IServerStreamWriter<UserProfile> responseStream,
        ServerCallContext context)
    {
        foreach (var userItem in _coinBank.CoinHolders)
        {
            await responseStream.WriteAsync(userItem.GetUserProfile());
        }
    }


    //Billing.CoinsEmission() – распределяет по пользователям amount монет, учитывая рейтинг. Пользователи получают
    //количество монет пропорциональное рейтингу, при этом каждый пользователь должен получить не менее 1-й монеты.
    //Каждая монета имеет свой id и историю перемещения между пользователями, начиная с эмиссии
    public override Task<Response> CoinsEmission(EmissionAmount request, ServerCallContext context)
    {
        var msg = _coinBank.CoinEmission(request).Status;

        return Task.FromResult(new Response { Status = msg });
    }

    //.Billing.MoveCoins() – перемещает монеты от пользователя к пользователю если у пользователя-источника достаточно
    //монет на балансе, в противном случае возвращает ошибку.
    public override Task<Response> MoveCoins(MoveCoinsTransaction request, ServerCallContext context)
    {
        return Task.FromResult(new Response { Status = _coinBank.BankMoveCoins(request).Status });
    }

    //.Billing.LongestHistoryCoin() – возвращает монету с самой длинной историей перемещения между пользователями.
    public override Task<Coin> LongestHistoryCoin(None request, ServerCallContext context)
    {
        var coins = _coinBank.GetLongestHistoryCoin();
        return coins.Select(Task.FromResult).FirstOrDefault()!;
    }
}