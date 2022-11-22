using GrpcService.Services;

namespace TestGRPC;

using GrpcService;

public class UnitTest1
{
    private List<CoinHolder> testProfiles = new List<CoinHolder>()
                                            {
                                                new CoinHolder("boris", 5000),
                                                new CoinHolder("maria", 1000),
                                                new CoinHolder("oleg", 800)
                                            };

    private List<CoinHolder> secondTestProfiles = new List<CoinHolder>()
                                                  {
                                                      new CoinHolder("boris", 5000),
                                                      new CoinHolder("maria", 1000),
                                                      new CoinHolder("boris", 800)
                                                  };

    private CoinItem testCoin = new CoinItem();

    private readonly EmissionAmount _emissionAmount = new EmissionAmount() { Amount = 10 };
    private readonly Response _negativeResponse = new Response() { Status = Response.Types.Status.Failed };
    private readonly Response _unspecifiedResponse = new Response() { Status = Response.Types.Status.Unspecified };
    private readonly Response _positiveResponse = new Response() { Status = Response.Types.Status.Ok };

    private readonly MoveCoinsTransaction _positiveMove = new MoveCoinsTransaction()
                                                          {
                                                              SrcUser = "boris",
                                                              DstUser = "maria",
                                                              Amount  = 5
                                                          };

    private readonly MoveCoinsTransaction _failMove = new MoveCoinsTransaction()
                                                      {
                                                          SrcUser = "boris",
                                                          DstUser = "maria",
                                                          Amount  = 15
                                                      };


    [Fact]
    public void userItem_Test()
    {
        Assert.NotNull(testProfiles[0].GetUserProfile());
    }

    [Fact]
    public void userItem_with_proto_Test()
    {
        string expectedName = "boris";
        const int expectedAmount = 0;
        const int expectedRating = 5000;

        Assert.Equal(expectedName, testProfiles[0].GetName());
        Assert.Equal(expectedAmount, testProfiles[0].GetAmount());
        Assert.Equal(expectedRating, testProfiles[0].GetRating());

        expectedName = "denis";
        testProfiles[0].SetName(expectedName);
        Assert.Equal(expectedName, testProfiles[0].GetName());
    }

    [Fact]
    public void CoinItem_Emission_ID_with_proto_Test()
    {
        Assert.NotNull(testCoin.GetCoin());
    }

    [Fact]
    public void Add_CoinItem_to_USER_Test()
    {
        const int expectedAmount = 1;
        testProfiles[0].AddCoin(testCoin);
        Assert.Equal(expectedAmount, testProfiles[0].GetAmount());
    }

    [Fact]
    public void CoinItem_Emission_HISTORY_with_proto_Test()
    {
        const string expectedMsg = "Emission?";
        Assert.Equal(expectedMsg, testCoin.GetCoin().History);
    }

    [Fact]
    public void CoinItem_HistoryCount_Test()
    {
        const int expectedMsg = 2;
        testCoin.AddHistory("mario");
        Assert.Equal(expectedMsg, testCoin.HistoryCount());
    }
    [Fact]
    public void CoinHolder_Coeff_Test()
    {
        var expectedCoeff = 0;
        Assert.Equal(expectedCoeff, testProfiles[0].DistributionCoefficient);
    }
    [Fact]
    public void CoinHolder_Rating_Test()
    {
        var expectedRating = 5000;
        Assert.Equal(expectedRating, testProfiles[0].GetRating());
    }
    [Fact]
    public void CoinHolder_Amount_with_proto_Test()
    {
        var expectedAmount = 0;
        Assert.Equal(expectedAmount, testProfiles[0].GetAmount());
        expectedAmount = 1;
        testProfiles[0].AddCoin(testCoin);
        Assert.Equal(expectedAmount, testProfiles[0].GetAmount());
    }
    [Fact]
    public void CoinBank_Emission_Fail_Test()
    {
        EmissionAmount _emissionFailAmount = new EmissionAmount() { Amount = 2 };
        var testBank = new CoinBank(testProfiles);
        var actualMsg = testBank.BankCoinEmission(_emissionFailAmount);
        Assert.Equal(_negativeResponse, actualMsg);
    }
    [Fact]
    public void CoinBank_Emission_OK_Test()
    {
        var testBank = new CoinBank(testProfiles);
        var actualMsg = testBank.BankCoinEmission(_emissionAmount);
        Assert.Equal(_positiveResponse, actualMsg);
    }
    [Fact]
    public void CoinBank_Emission_OK_User_Amount_Test()
    {
        var testBank = new CoinBank(testProfiles);
        var actualMsg = testBank.BankCoinEmission(_emissionAmount);
        Assert.Equal(6, testBank.GetCoinHolder(0).GetAmount());
        //внутри основного кода надо будет прбрасывать это! 
        testProfiles[0].GetUserProfile().Amount = testBank.GetCoinHolder(0).GetAmount();
        Assert.Equal(6, testProfiles[0].GetAmount());
        Assert.Equal(6, testProfiles[0].GetUserProfile().Amount);
    }
    [Fact]
    public void CoinBank_BankMoveCoins_Fail_Test()
    {
        var testBank = new CoinBank(testProfiles);
        var tempMsg = testBank.BankCoinEmission(_emissionAmount);
        var actualMsg = testBank.BankMoveCoins(_failMove);
        Assert.Equal(_negativeResponse, actualMsg);
    }
    [Fact]
    public void CoinBank_BankMoveCoins_Unspecified_Test()
    {
        var testBank = new CoinBank(secondTestProfiles);
        var tempMsg = testBank.BankCoinEmission(_emissionAmount);
        var actualMsg = testBank.BankMoveCoins(_positiveMove);
        Assert.Equal(_unspecifiedResponse, actualMsg);
    }
    [Fact]
    public void CoinBank_BankMoveCoins_OK_Test()
    {
        var testBank = new CoinBank(testProfiles);
        var tempMsg = testBank.BankCoinEmission(_emissionAmount);
        var actualMsg = testBank.BankMoveCoins(_positiveMove);
        Assert.Equal(_positiveResponse, actualMsg);
    }
    [Fact]
    public void Find_MAX_History_Test()
    {
        Random rnd = new Random();

        var tempMove = new MoveCoinsTransaction()
                       {
                           SrcUser = "maria",
                           DstUser = "boris",
                           Amount  = 7
                       };
        var secondMove = new MoveCoinsTransaction()
                         {
                             SrcUser = "maria",
                             DstUser = "boris",
                             Amount  = 7
                         };
        var thirdMove = new MoveCoinsTransaction()
                        {
                            SrcUser = "boris",
                            DstUser = "maria",
                            Amount  = 5
                        };
        var fifthMove = new MoveCoinsTransaction()
                        {
                            SrcUser = "boris",
                            DstUser = "oleg",
                            Amount  = 3
                        };


        var testBank = new CoinBank(testProfiles);
        testBank.BankCoinEmission(_emissionAmount);
        testBank.BankMoveCoins(_positiveMove);
        testBank.BankMoveCoins(secondMove);
        testBank.BankMoveCoins(thirdMove);
        testBank.BankMoveCoins(fifthMove);
        var actualCoin = testBank.GetLongestHistoryCoin();
        Assert.NotNull(actualCoin);
    }
}