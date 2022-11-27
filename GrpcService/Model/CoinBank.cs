namespace GrpcService.Services;

public class CoinBank
{
    private static readonly List<CoinHolder> UserProfiles = new()
    {
        new CoinHolder("boris", 5000),
        new CoinHolder("maria", 1000),
        new CoinHolder("oleg", 800)
    };
    
    private readonly List<CoinHolder> _coinHolders = new();
    private readonly List<CoinItem> _coinObjList = new();

    public CoinBank()
    {
        //подcчитали индивидуальный коэффициент распределения по модели
        float IndividualCoefficient(IEnumerable<CoinHolder> inUsersList, CoinHolder userItem)
        {
            return userItem.GetRating() / (float)inUsersList.Sum(user => user.GetRating());
        }

        //создали держателей монет в банке
        foreach (var holder in UserProfiles)
        {
            var tempCoinHolder = new CoinHolder();
            tempCoinHolder.SetName(holder.GetName());
            tempCoinHolder.SetRating(holder.GetRating());
            tempCoinHolder.DistributionCoefficient = IndividualCoefficient(UserProfiles, holder);
            _coinHolders.Add(tempCoinHolder);
        }
    }

    public IList<CoinHolder> CoinHolders => _coinHolders;

    public CoinHolder GetCoinHolder(int holderNumber)
    {
        return _coinHolders[holderNumber];
    }

    private int AddCoinToHolder(CoinHolder holder, int coinNum, int amount)
    {
        holder.AddCoin(_coinObjList[coinNum]);
        _coinObjList[coinNum].AddHistory(holder.GetName());

        return coinNum - amount;
    }

    public Response CoinEmission(EmissionAmount inEmissionAmount)
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
            var coinCount = (int)tempAmount - 1; //подсчёт остатка монет после распределения
            foreach (var holder in _coinHolders)
            {
                coinCount = AddCoinToHolder(holder, coinCount, 1);
            }

            // распределяем монеты в соответствии с коэффициентом
            foreach (var holder in _coinHolders)
            {
                var coinNum = (int)Math.Round(
                    holder.DistributionCoefficient * (tempAmount - _coinHolders.Count), MidpointRounding.AwayFromZero);
                if (coinCount >= 0)
                {
                    for (var i = 0; i < coinNum; i++)
                    {
                        coinCount = AddCoinToHolder(holder, coinCount, 1);
                    }
                }
                else
                {
                    return new Response { Status = Response.Types.Status.Ok };
                }
            }

            //если после распределения если монеты остались докидываем их товарищу с максимальным рейтингом
            if (coinCount > 0)
            {
                float max = 0;
                var index = 0;
                // ищем индекс держателя с максимальным коэффициентом
                for (var i = 0; i < _coinHolders.Count; i++)
                {
                    if (_coinHolders[i].DistributionCoefficient > max)
                    {
                        max = _coinHolders[i].DistributionCoefficient;
                        index = i;
                    }
                }

                for (var i = 0; i > coinCount; i++)
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
        var srcUsers = _coinHolders.Where(x => x.GetName() == inMoveCoinsTransaction.SrcUser).ToArray();

        switch (srcUsers.Length)
        {
            case 1 when srcUsers[0].GetAmount() >= inMoveCoinsTransaction.Amount:
            {
                for (var i = 0; i < (int)inMoveCoinsTransaction.Amount; i++)
                {
                    var tempCoin = srcUsers[0].GiveAwayCoin();
                    var dstUsers = _coinHolders.Where(x => x.GetName() == inMoveCoinsTransaction.DstUser).ToArray();
                    dstUsers[0].AddCoin(tempCoin);
                    tempCoin.AddHistory("/ADD/" + dstUsers[0].GetName());
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
        var maxHistory = _coinObjList.Select(t => t.HistoryCount()).Prepend(0).Max();

        return (from coinItem in _coinObjList where coinItem.HistoryCount() == maxHistory select coinItem.GetCoin())
            .ToList();
    }
}