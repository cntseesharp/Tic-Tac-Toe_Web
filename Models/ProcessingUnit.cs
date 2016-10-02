using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Реализация ИИ
/// </summary>
namespace WebApplication1.Models
{
    public class ProcessingUnit
    {
        static Random randomizer = new Random();
        short[] pass;
        static short[] passL = new short[3] { 0, 1, -1 };
        delegate byte GetCell(short i, short j);
        Game game;
        bool player;
        List<Move> moves = new List<Move>();

        /// <summary>
        /// Просчёт следующего хода для ИИ
        /// </summary>
        /// <param name="playerInput">Игрок, для которого нужно посчитать ход</param>
        /// <param name="gameInput">Ссылка на экземпляр Game, в котором проходит игра</param>
        /// <param name="onlyChecks">Только проверить состояние игры</param>
        /// <returns>Возвращает enum со статусом игры в данный момент</returns>
        public GameStatus CalculateNextMove(bool playerInput, Game gameInput, bool onlyChecks = false)
        {
            ///0 1 2
            ///3 4 5
            ///6 7 8
            ///
            ///i = 0-2, j = 0-2
            ///cell[i + j * 3]  ||| 
            ///cell[i*3 + j]    ---
            ///cell[i*4]        \\\
            ///cell[2 * (i+1)]  ///  <-- Экономим на int i = 1; i < 3. Нет, не экономим, новый цикл
            ///empty, enemyCell, myCells
            ///if (myCells == 2 && empty != -1) makeMove(empty) | if (enemyCell == 2 && empty != -1) makeMove(empty)
            ///
            ///wasMoveMade = makeMove();
            ///if(!wasMoveMade) makeRandomMove();

            ///V1, V2, V3, H1, H2, H2, Z1, Z2
            ///myCell V1++; enemyCell V1--; if V1 == 2...
            ///for(...) if(current == 0) makeMove(current);
            game = gameInput;
            player = playerInput;

            //out победителя
            bool res = false;
            
            //Счётная основа для ячеек
            pass = new short[3] { 0, -1, 1 };
            if (player)
            {
                pass[1] = 1;
                pass[2] = -1;
            }

            if (IsGameFinished(out res))
            {
                if (res)
                    return GameStatus.XWon;
                return GameStatus.OWon;
            }
            if (CheckDraw())
                return GameStatus.Draw;

            if (onlyChecks)
                return GameStatus.InProgress;

            //Просчёт ходов
            GetMoves((i, j) => { return (byte)(i + j * 3); });   //Вертикальные
            GetMoves((i, j) => { return (byte)(i * 3 + j); });   //Горизонтальные
            GetMoves((i, j) => { return (byte)(j * 4); });       //Наклон влево
            GetMoves((i, j) => { return (byte)(2 * (j + 1)); }); //Наклон вправо

            if (!TryToMove(moves.FirstOrDefault(x => x.result == CellResult.Win)))
                if (!TryToMove(moves.FirstOrDefault(x => x.result == CellResult.Preventing)))
                    TryToMove(moves[randomizer.Next(moves.Count - 1)]); //Мы ведь не должны ходить рандомно, если есть другие ходы, верно?

            if (IsGameFinished(out res))
            {
                if (res)
                    return GameStatus.XWon;
                return GameStatus.OWon;
            }

            if (CheckDraw())
                return GameStatus.Draw;

            return GameStatus.InProgress;
        }

        /// <summary>
        /// Попытка совершить ход
        /// </summary>
        /// <param name="current">Ячейка, в которую нужно совершить ход</param>
        /// <returns>True - если ход совершен</returns>
        bool TryToMove(Move current)
        {
            if (current != null)
                return game.MakeMove(player, current.cell);
            return false;
        }

        /// <summary>
        /// Проверка на ничью
        /// </summary>
        /// <returns>Закончилась ли игра без победы одного из игроков</returns>
        public bool CheckDraw()
        {
            bool draw = true;
            for (int i = 0; i < 9; i++)
                if (game.Gameboard[i] == 0)
                {
                    draw = false;
                    break;
                }
            if (draw)
                return true;
            return false;
        }

        /// <summary>
        /// Просчёт возможных ходов
        /// </summary>
        /// <param name="cellCalc">Делегат. Принимает 2 атрибута: i, j текущего цикла, должен вернуть ячейку для проверки</param>
        void GetMoves(GetCell cellCalc)
        {
            for (short i = 0; i < 3; i++)
            {
                short thisOne = 0;//Если набирается 2, то на линии минимум 2 клетки игрока
                short empty = -1;
                for (short j = 0; j < 3; j++)
                {
                    byte cell = cellCalc(i, j);
                    thisOne += pass[game.Gameboard[cell]];
                    if (game.Gameboard[cell] == 0)
                    {
                        empty = cell;
                        moves.Add(new Move { cell = empty, result = CellResult.Random });
                    }
                }
                if (thisOne == 2 && empty != -1)
                    moves.Add(new Move { cell = empty, result = CellResult.Win });
                if (thisOne == -2 && empty != -1)
                    moves.Add(new Move { cell = empty, result = CellResult.Preventing });
            }
        }

        /// <summary>
        /// Проверка игроков на победу
        /// </summary>
        /// <param name="result">Возвращает bool победившего игрока</param>
        /// <returns>true, в случае, если игра окончилась победой одного из игроков</returns>
        bool IsGameFinished(out bool result)
        {
            bool res = false;
            result = res;
            if (Check3((i, j) => { return (byte)(i + j * 3); }, out res) ||
                Check3((i, j) => { return (byte)(i * 3 + j); }, out res) ||
                Check3((i, j) => { return (byte)(j * 4); }, out res) ||
                Check3((i, j) => { return (byte)(2 * (j + 1)); }, out res))
            {
                result = res;
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Проверяет выигрышные комбинации
        /// </summary>
        /// <param name="cellCalc">Делагат, в который предаются i, j циклов. Возвращает ячейку для проверки</param>
        /// <param name="res">Победивший игрок</param>
        /// <returns>Окончена ли игра</returns>
        bool Check3(GetCell cellCalc, out bool res)
        {
            res = false;
            for (short i = 0; i < 3; i++)
            {
                short thisOne = 0;
                for (short j = 0; j < 3; j++)
                {
                    byte cell = cellCalc(i, j);
                    thisOne += passL[game.Gameboard[cell]];
                }

                if (thisOne == 3)
                {
                    res = true;
                    return true;
                }
                if (thisOne == -3)
                {
                    res = false;
                    return true;
                }
            }
            return false;
        }
    }

    //Структуры нельзя приводить к null
    class Move
    {
        public short cell;
        public CellResult result;
    }

    enum CellResult
    {
        Random,
        Win,
        Preventing,
    }

    public enum GameStatus
    {
        InProgress,
        XWon,
        OWon,
        Draw,
    }
}