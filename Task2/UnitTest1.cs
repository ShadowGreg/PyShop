using Task1;
using Xunit.Sdk;

namespace Task2;

public class UnitTest1
{
    /// <summary>
    /// За основу тестирования я взял предположение, что я подсоединяюсь к модели и не меняю код и не переиспользую
    /// внутренние функции(наследуясь) генерации балов, а задаю снаружи и тестирую функцию getScore
    /// </summary>
   
    [Theory]
    [InlineData(1,0,1,2,1,1,3,3,5)]
    [InlineData(2,3,4,5,1,1,3,3,5)]
    [InlineData(8,0,1,6,7,1,3,3,5)]
    public void Values_in_game_base_getScore_Test(
        int offset1, int home1, int away1,
        int offset2, int home2, int away2,
        int offset3, int home3, int away3
    )
    {
        GameStamp firstGameStamp = new GameStamp(offset1, home1, away1);
        GameStamp secondGameStamp = new GameStamp(offset2, home2, away2);
        GameStamp thirdGameStamp = new GameStamp(offset3, home3, away3);
        GameStamp[] gameStamps = new GameStamp[3];
        gameStamps[0] = firstGameStamp;
        gameStamps[1] = secondGameStamp;
        gameStamps[2] = thirdGameStamp;
        Game game = new Game(gameStamps);
        Score expectedScore = new Score(3, 5);
        Assert.Equal(expectedScore,game.getScore(3));
    }
    
    [Fact]
    public void Values_not_game_base_getScore_Test()
    {
        GameStamp firstGameStamp = new GameStamp(1, 0, 1);
        GameStamp secondGameStamp = new GameStamp(2, 1, 1);
        GameStamp thirdGameStamp = new GameStamp(5, 3, 5);
        GameStamp[] gameStamps = new GameStamp[3];
        gameStamps[0] = firstGameStamp;
        gameStamps[1] = secondGameStamp;
        gameStamps[2] = thirdGameStamp;
        Game game = new Game(gameStamps);
        Assert.Throws<ArgumentException>(()=>game.getScore(3));
    }
    
    
}