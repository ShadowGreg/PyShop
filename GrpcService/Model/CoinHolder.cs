namespace GrpcService.Services;

public class CoinHolder //: IUser // класс для банка с расширением полей пользователя
{
    private            int            _rating;
    protected readonly UserProfile    _userProfile = new();
    protected readonly List<CoinItem> _coinsList   = new();

    public CoinHolder()
    {
    }

    public CoinHolder(string inName, int inRating)
    {
        _userProfile.Name = inName;
        _userProfile.Amount = 0;
        _rating = inRating;
    }

    public CoinHolder(CoinHolder inCoinHolder)
    {
        _userProfile.Name = inCoinHolder.GetName();
        _userProfile.Amount = inCoinHolder.GetAmount();
        _rating = inCoinHolder.GetRating();
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
        _coinsList.Add(inCoin);
        _userProfile.Amount += 1;
    }

    public CoinItem GiveAwayCoin()
    {
        _coinsList[0].AddHistory(_userProfile.Name + "/AWAY/");
        _userProfile.Amount -= 1;
        var tempCoin = _coinsList[0];
        _coinsList.RemoveAt(0);
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