/*
Задача 
В примере кода ниже генерируется список фиксаций состояния счета игры в течение матча.
Разработайте функцию Game.getScore(offset), которая вернет счет на момент offset.
Нужно суметь понять суть написанного кода, заметить нюансы, разработать функцию вписывающуюся стилем в существующий код, желательно адекватной алгоритмической сложности.
 */

using System.Runtime.CompilerServices;

namespace Task1
{
    class App
    {
        static void Main(string[] args)
        {
            int offset = 4;
            GameStamp[] gameStampsArray = new GameStamp[2];
            var firstGameStamp = new GameStamp(1, 2, 3);
            var secondGameStamp = new GameStamp(4, 2, 3);
            gameStampsArray[0] = firstGameStamp;
            gameStampsArray[1] = secondGameStamp;
            var item = new Game(gameStampsArray).getScore(offset);
        }
    }

    /// <summary>
    /// Счёт - структура,  конструктор принимает два параметра инт.
    /// мне его надо вернуть 
    /// </summary>
    public struct Score
    {
        public int home;
        public int away;

        public Score(int home, int away)
        {
            this.home = home;
            this.away = away;
        }
    }

    /// <summary>
    /// Публичная структура котая имеет поля котрые можно задвать и вызвать напрямую - по идее потому что get set не описан
    /// и можно задавать через констркутор GameStamp
    /// </summary>
    public struct GameStamp
    {
        public int   offset;
        public Score score;
        public GameStamp(int offset, int home, int away)
        {
            this.offset = offset;
            this.score  = new Score(home, away);
        }
    }

    /// <summary>
    /// Публичный класс игра 
    /// </summary>
    public class Game
    {
        const int TIMESTAMPS_COUNT = 50000;

        const double PROBABILITY_SCORE_CHANGED = 0.0001;

        const double PROBABILITY_HOME_SCORE = 0.45;

        const int OFFSET_MAX_STEP = 3;

        GameStamp[] gameStamps;

        public Game() // конструктор класс котрый определяется через gameStamps
        {
            this.gameStamps = new GameStamp[] { };
        }

        public Game(GameStamp[] gameStamps) // Второй конструктор класс котрый определяется через gameStamps
        {
            this.gameStamps = gameStamps;
        }

        GameStamp generateGameStamp(GameStamp previousValue)
        {
            Random rand = new Random();

            bool scoreChanged = rand.NextDouble() > 1 - PROBABILITY_SCORE_CHANGED;
            int homeScoreChange = scoreChanged && rand.NextDouble() > 1 - PROBABILITY_HOME_SCORE ? 1 : 0;
            int awayScoreChange = scoreChanged && homeScoreChange == 0 ? 1 : 0;
            int offsetChange = (int)(Math.Floor(rand.NextDouble() * OFFSET_MAX_STEP)) + 1;

            return new GameStamp(
                previousValue.offset + offsetChange,
                previousValue.score.home + homeScoreChange,
                previousValue.score.away + awayScoreChange
            );
        }

        static Game generateGame() // генерирует generateGame
        {
            Game game = new Game();
            game.gameStamps = new GameStamp[TIMESTAMPS_COUNT];

            GameStamp currentStamp = new GameStamp(0, 0, 0);
            for (int i = 0; i < TIMESTAMPS_COUNT; i++)
            {
                game.gameStamps[i] = currentStamp;
                currentStamp       = game.generateGameStamp(currentStamp);
            }

            return game;
        }

        public static void task1() /// сгенерили класс и напечатали
        {
            Game game = generateGame();
            game.printGameStamps();
        }

        void printGameStamps()
        {
            foreach (GameStamp stamp in this.gameStamps)
            {
                Console.WriteLine($"{stamp.offset}: {stamp.score.home}-{stamp.score.away}");
            }
        }
        /*
         вид вызова функции Game.getScore(offset)
         объект калсса Game имеет функцию getScore, 
         которая принимает offset и вернет счет на момент offset
         */
        public Score getScore(int offset)
        {
            foreach (GameStamp stamp in gameStamps)
            {
                if (stamp.offset == offset)
                {
                    return stamp.score;
                }
            }

            throw new ArgumentException();
        }
    }
}