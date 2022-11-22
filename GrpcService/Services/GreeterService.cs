using System.Collections;
using System.Reflection;
using System.Threading.Tasks.Dataflow;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR;

namespace GrpcService.Services;

public interface IUser
{
    public void SetName(string inName);
    public string GetName();
    public long GetAmount();
    public void AddCoin(CoinItem inCoin);
    public int GetRating();
    public UserProfile GetUserProfile();
}

public class CoinHolder: IUser // класс для банка с расширением полей пользователя
{
    private          int            _rating;
    private readonly UserProfile    _userProfile = new UserProfile();
    private          List<CoinItem> coinsList    = new List<CoinItem>();
    public CoinHolder()
    {
    }

    public CoinHolder(string inName, int inRating)
    {
        _userProfile.Name   = inName;
        _userProfile.Amount = 0;
        _rating             = inRating;
    }
    public CoinHolder(CoinHolder inCoinHolder)
    {
        _userProfile.Name   = inCoinHolder.GetName();
        _userProfile.Amount = inCoinHolder.GetAmount();
        _rating             = inCoinHolder.GetRating();
    }
    public void SetName(string inName)
    {
        _userProfile.Name = inName;
    }
    public string GetName()
    {
        return _userProfile.Name;
    }
    public long GetAmount()
    {
        return _userProfile.Amount;
    }
    public void AddCoin(CoinItem inCoin)
    {
        coinsList.Add(inCoin);
        //inCoin.AddHistory(_userProfile.Name);
        _userProfile.Amount += 1;
    }

    public CoinItem GiveAwayCoin()
    {
        coinsList[0].AddHistory(_userProfile.Name + "/AWAY/");
        _userProfile.Amount += 1;
        CoinItem tempCoin = coinsList[0];
        coinsList.RemoveAt(0);
        return tempCoin;
    }
    public int GetRating()
    {
        return _rating;
    }
    public void SetRating(int inRating)
    {
        _rating = inRating;
    }
    public UserProfile GetUserProfile()
    {
        return _userProfile;
    }
    public float DistributionCoefficient { get; set; }
}

public interface ICoin
{
    public void AddHistory(string inMSG);
    public Coin GetCoin();
    public long HistoryCount();
}

public class CoinItem: ICoin // расширяем возможности класса монет 
{
    private const    string EMISSION_MSG = "Emission?";
    private readonly Coin   _coin        = new Coin();
    public CoinItem()
    {
        CreateCoin();
    }
    private static long CreateID()
    {
        DateTime centuryBegin =
            new DateTime(2022, 11, 15); //событие от которого рассчитывается количество тактов

        DateTime currentDate = DateTime.Now;
        return currentDate.Ticks - centuryBegin.Ticks;
    }
    private void CreateCoin()
    {
        _coin.Id      =  CreateID();
        _coin.History += EMISSION_MSG;
    }
    public void AddHistory(string inMsg)
    {
        _coin.History += inMsg + "?";
    }
    public Coin GetCoin()
    {
        return _coin;
    }
    public long HistoryCount()
    {
        return _coin.History.Split(new[] { '?' }).Length - 1;
    }
}

public class CoinBank
{
    private readonly List<CoinHolder> _coinHolders = new List<CoinHolder>();
    private readonly List<CoinItem>   _coinObjList  = new List<CoinItem>();
    public CoinBank(List<CoinHolder> inCoinHolders)
    {
        //подcчитали индивидуальный коэффициент распределения по модели
        float IndividualCoefficient(List<CoinHolder> inUsersList, CoinHolder userItem)
        {
            return userItem.GetRating() / (float)inUsersList.Sum(user => user.GetRating());
        }

        //создали держателей монет в банке
        foreach (CoinHolder holder in inCoinHolders)
        {
            CoinHolder tempCoinHolder = new CoinHolder();
            tempCoinHolder.SetName(holder.GetName());
            tempCoinHolder.SetRating(holder.GetRating());
            tempCoinHolder.DistributionCoefficient = IndividualCoefficient(inCoinHolders, holder);
            _coinHolders.Add(tempCoinHolder);
        }
    }
    public CoinHolder GetCoinHolder(int inNum)
    {
        return _coinHolders[inNum];
    }
    private int AddCoinToHolder(CoinHolder inHolder, int coinNum, int amount)
    {
        inHolder.AddCoin(_coinObjList[coinNum]);
        _coinObjList[coinNum].AddHistory(inHolder.GetName());

        return coinNum - amount;
    }
    public Response BankCoinEmission(EmissionAmount inEmissionAmount)
    {
        var tempAmount = inEmissionAmount.Amount;

        if (inEmissionAmount.Amount >= _coinHolders.Count)
        {
            //просто создали экземпляры класса монет для дальнейшего испольщования
            for (long i = 0; i < tempAmount; i++)
            {
                _coinObjList.Add(new CoinItem());
            }

            //начинаем распределение монет по одной на каждого
            int coinCount = (int)tempAmount - 1; //подсчёт остатка монет после распределения
            foreach (CoinHolder holder in _coinHolders)
            {
                coinCount = AddCoinToHolder(holder, coinCount, 1);
            }

            // распределяем монеты в соответствии с коэффициентом
            foreach (CoinHolder holder in _coinHolders)
            {
                int coinNum = (int)Math.Round(
                    holder.DistributionCoefficient * (tempAmount - _coinHolders.Count), MidpointRounding.AwayFromZero);
                if (coinCount >= 0)
                {
                    for (int i = 0; i < coinNum; i++)
                    {
                        coinCount = AddCoinToHolder(holder, coinCount, 1);
                    }
                }
                else if (coinCount < 0)
                {
                    return new Response { Status = Response.Types.Status.Ok };
                }
            }

            //если после распределения если монеты остались докидываем их товарищу с максимальным рейтингом
            if (coinCount > 0)
            {
                float max = 0;
                int index = 0;
                // ищем индекс держателя с максимальным коэффициентом
                for (int i = 0; i < _coinHolders.Count; i++)
                {
                    if (_coinHolders[i].DistributionCoefficient > max)
                    {
                        max   = _coinHolders[i].DistributionCoefficient;
                        index = i;
                    }
                }

                for (int i = 0; i > coinCount; i++)
                {
                    coinCount = AddCoinToHolder(_coinHolders[index], coinCount, 1);
                }
            }

            return new Response { Status = Response.Types.Status.Ok };
        }

        return new Response { Status = Response.Types.Status.Failed };
    }

    public Response BankMoveCoins(MoveCoinsTransaction inMoveCoinsTransaction)
    {
        CoinHolder[] src = _coinHolders.Where(x => x.GetName() == inMoveCoinsTransaction.SrcUser).ToArray();
        switch (src.Length)
        {
            case 1 when src[0].GetAmount() >= inMoveCoinsTransaction.Amount:
            {
                for (int i = 0; i < (int)inMoveCoinsTransaction.Amount; i++)
                {
                    CoinItem tempCoin = src[0].GiveAwayCoin();
                    CoinHolder[] dst = _coinHolders.Where(x => x.GetName() == inMoveCoinsTransaction.DstUser).ToArray();
                    dst[0].AddCoin(tempCoin);
                    tempCoin.AddHistory("/ADD/" + dst[0].GetName());
                }

                return new Response { Status = Response.Types.Status.Ok };
            }
            case 1:
                return new Response { Status = Response.Types.Status.Failed };
            case > 1:
                return new Response { Status = Response.Types.Status.Unspecified };
            default:
                return new Response { Status = Response.Types.Status.Failed };
        }
    }

    public List<Coin> GetLongestHistoryCoin()
    {
        long maxHistory = _coinObjList.Select(t => t.HistoryCount()).Prepend(0).Max();

        return (from coinItem in _coinObjList where coinItem.HistoryCount() == maxHistory select coinItem.GetCoin())
            .ToList();
    }
}

public class GreeterService: Billing.BillingBase
{
    private static readonly List<CoinHolder> UserProfiles = new List<CoinHolder>()
                                                             {
                                                                 new CoinHolder("boris", 5000),
                                                                 new CoinHolder("maria", 1000),
                                                                 new CoinHolder("oleg", 800)
                                                             };

    private static readonly CoinBank CoinBank = new CoinBank(UserProfiles);

    //Billing.ListUsers() – перечисляет пользователей в сервисе. 
    public override async Task ListUsers(None request, IServerStreamWriter<UserProfile> responseStream,
                                         ServerCallContext context)
    {
        foreach (CoinHolder userItem in UserProfiles)
        {
            await responseStream.WriteAsync(userItem.GetUserProfile());
        }
    }


    //Billing.CoinsEmission() – распределяет по пользователям amount монет, учитывая рейтинг. Пользователи получают
    //количество монет пропорциональное рейтингу, при этом каждый пользователь должен получить не менее 1-й монеты.
    //Каждая монета имеет свой id и историю перемещения между пользователями, начиная с эмиссии
    public override Task<Response> CoinsEmission(EmissionAmount request, ServerCallContext context)
    {
        var msg = CoinBank.BankCoinEmission(request).Status;
        for (int i = 0; i < UserProfiles.Count; i++)
        {
            UserProfiles[i].GetUserProfile().Amount = CoinBank.GetCoinHolder(i).GetAmount();
        }

        return Task.FromResult(new Response { Status = msg });
    }

    //.Billing.MoveCoins() – перемещает монеты от пользователя к пользователю если у пользователя-источника достаточно
    //монет на балансе, в противном случае возвращает ошибку.
    public override Task<Response> MoveCoins(MoveCoinsTransaction request, ServerCallContext context)
    {
        return Task.FromResult(new Response { Status = CoinBank.BankMoveCoins(request).Status });
    }

    //.Billing.LongestHistoryCoin() – возвращает монету с самой длинной историей перемещения между пользователями.
    public override Task<Coin> LongestHistoryCoin(None request, ServerCallContext context)
    {
        List<Coin> coins = CoinBank.GetLongestHistoryCoin();
        return coins.Select(item => Task.FromResult(item)).FirstOrDefault()!;
    }
}