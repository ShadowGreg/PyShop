using GrpcService;
using GrpcService.Services;
namespace TestGRPC;

public class CoinHolderTest: CoinHolder
{
    private readonly List<CoinHolder> _profiles = new()
                                                     {
                                                         new CoinHolder("boris", 5000),
                                                         new CoinHolder("maria", 1000),
                                                         new CoinHolder("oleg", 800)
                                                     };

    [Fact]
    public void Create_new_Coin_holder_on_coin_holder_class_Test()
    {
        var holder = new CoinHolder(_profiles[0]);
        
        Assert.Equal(holder.GetName(), _profiles[0].GetName());
        Assert.Equal(holder.GetRating(), _profiles[0].GetRating());
        Assert.NotEqual(holder,_profiles[0]);
    }
    
    [Fact]
    public void SetName_Test()
    {
        _profiles[0].SetName("igor");
        
        Assert.Equal("igor",_profiles[0].GetName());
    }
    
    [Fact]
    public void AddCoin_Test()
    {
        var coin = new CoinItem();
        AddCoin(coin);

        Assert.NotEmpty(_coinsList);
    }
    
    [Fact]
    public void GiveAwayCoin_Test()
    {
        var coin = new CoinItem();
        AddCoin(coin);
        GiveAwayCoin();

        Assert.Empty(_coinsList);
    }
    
    [Fact]
    public void GetRating_Test()
    {
        Assert.Equal(5000,_profiles[0].GetRating());
    }
    
    [Fact]
    public void SetRating_Test()
    {
        _profiles[0].SetRating(4000);
        
        Assert.Equal(4000,_profiles[0].GetRating());
    }
    
    [Fact]
    public void GetUserProfile_Test()
    {
        UserProfile userProfile = new()
                                  {
                                      Name   = "oleg",
                                      Amount = 800
                                  };
        
        
        Assert.Equal(userProfile.Name,_profiles[2].GetUserProfile().Name);
    }
}