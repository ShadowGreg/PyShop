namespace GrpcService.Services;

public class CoinItem 
{
    private const string EmissionMsg = "Emission?";
    private readonly Coin _coin = new();

    public CoinItem()
    {
        CreateCoin();
    }

    private static long CreateId()
    {
        var centuryBegin =
            new DateTime(2022, 11, 15); //событие от которого рассчитывается количество тактов

        var currentDate = DateTime.Now;
        return currentDate.Ticks - centuryBegin.Ticks;
    }

    private void CreateCoin()
    {
        _coin.Id = CreateId();
        _coin.History += EmissionMsg;
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