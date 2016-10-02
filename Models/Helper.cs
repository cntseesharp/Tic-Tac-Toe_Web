using WebApplication1.DAL;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Script.Serialization;

namespace WebApplication1.Models
{
    public static class Helper
    {
        public static GameReplayContext Replay = new GameReplayContext();
        public static SHA256 Hasher = SHA256.Create();
        public static JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

        /// <summary>
        /// Создаён новый экземпляр GameReplay и сохраняет его в базу
        /// </summary>
        /// <param name="game">Экземпляр класса Game, для которого нужно совершить сохранение</param>
        /// <returns>Хэш игры в бвзе</returns>
        public static string SaveReplay(this Game game)
        {
            string hash = BitConverter.ToString(Hasher.ComputeHash(BitConverter.GetBytes(Replay.Games.Count()))).Replace("-", "");
            Replay.Games.Add(new GameReplay() { hash = hash, moves = game.Moves, movesCount = game.MovesCounter });
            Replay.SaveChangesAsync();
            return hash;
        }

        static Helper()
        {
            try
            {
                int trhower = Replay.Games.Count();
            }
            catch
            {
                Replay.Games.Add(new GameReplay() { hash = "00000000000000", moves = new byte[1] { 0xFF }, movesCount = 0 });
                Replay.SaveChanges();
            }
        }
    }
}