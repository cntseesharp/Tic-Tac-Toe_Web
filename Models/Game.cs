/// <summary>
/// Класс, в котором описана игра
/// </summary>

namespace WebApplication1.Models
{
    public class Game
    {
        byte[] gameboard = new byte[9];
        bool playersMove = true;
        byte[] moves = new byte[9];
        byte counter = 0; //Вместо List


        //Мне кажется, что это херня. Массивы - ссылочные типы. Нужно придумать обёртку
        public byte[] Gameboard { get { return gameboard; } }
        public byte[] Moves { get { return moves; } }
        public byte MovesCounter { get { return counter; } }

        //public bool IsFinished { get; set; }
        
        public Game() { }

        public bool MakeMove(bool player, int cell) //true - крестик, false - нолик
        {
            //Проверка на игрока в теории позволит избежать коллизий "я нажал на несколько сразу"
            if (player != playersMove) return false;
            if (gameboard[cell] != 0) return false; //Ячейка занята

            gameboard[cell] = player ? (byte)1 : (byte)2;
            moves[counter++] = (byte)cell; //Зачем хранить лишние данные? Первый всегда ходит крестик

            playersMove = !playersMove;
            return true;
        }

        //Unused
        public bool CanMove(bool player)
        {
            return player == playersMove;
        }
    }

    public class ApiGame : Game
    {
        public string Hash { get; set; }
        public ApiGame(string hash)
        {
            Hash = hash;
        }
    }
}