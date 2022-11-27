using GrpcService.Services;

namespace TestGRPC;

public class CoinItemTests
{
    private readonly CoinItem _testCoin = new CoinItem();
    
    [Fact]
     public void CoinItem_Emission_ID_with_proto_Test()
     {
         Assert.NotNull(_testCoin.GetCoin());
     }
     
     [Fact]
     public void CoinItem_Add_History_Test()
     {
         _testCoin.AddHistory("ADD_HISTORY");
         
         Assert.NotNull(_testCoin.GetCoin().History);
     }
     
     [Fact]
     public void CoinItem_History_Count_Test()
     {
         Assert.Equal(1,_testCoin.HistoryCount());
     }
}