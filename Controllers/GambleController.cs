using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Linq;
using WebApplication1.Models;
using System.Security.Cryptography;
using System.Net.Http;

namespace WebApplication1.Controllers
{
    public class GambleController : ApiController
    {
        static List<ApiGame> activeGames = new List<ApiGame>(); // "кэш" игр
        static int id = 1;

        public GambleController() { }

        [HttpPost]
        public IHttpActionResult CreateNewGame(HttpRequestMessage req)
        {
            string hash = BitConverter.ToString(Helper.Hasher.ComputeHash(BitConverter.GetBytes(id++))).Replace("-", "");
            activeGames.Add(new ApiGame(hash));
            return Ok(hash);
        }

        [HttpPost]
        public IHttpActionResult RequestGameReplay(HttpRequestMessage req)
        {
            var compare = req.Content.ReadAsStringAsync().Result;
            var game = Helper.Replay.Games.Where(x => x.hash == compare).FirstOrDefault();
            if (game == null) return BadRequest();
            return Ok("{ \"moves\":" + Helper.jsSerializer.Serialize(game.moves) + ", \"count\":" + game.movesCount + " }");
        }

        [HttpPost]
        public IHttpActionResult RequestNextMove(HttpRequestMessage req)
        {
            var json = req.Content.ReadAsStringAsync().Result;
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Recieving>(json);
            var game = Select(obj.gameId);
            /*
            if (game.IsFinished)
                return Ok(game.Gameboard);
            */
            game.MakeMove(obj.player, obj.move);
            GameStatus status = new ProcessingUnit().CalculateNextMove(!obj.player, game);
            if (status != GameStatus.InProgress)
            {
                byte player = 0;
                if (status == GameStatus.XWon) player = 1;
                if (status == GameStatus.OWon) player = 2;
                return Ok("{ \"gameboard\":" + Helper.jsSerializer.Serialize(game.Gameboard) + ", \"isFinished\":true, \"player\":" + player + ", \"hash\":\"" + Helper.SaveReplay(game) + "\" }");
            }

            return Ok("{ \"gameboard\":" + Helper.jsSerializer.Serialize(game.Gameboard) + ", \"isFinished\":false }");
        }

        static ApiGame Select(string hash)
        {
            return activeGames.Where(x => x.Hash.Equals(hash)).FirstOrDefault();
        }
    }

    class Recieving
    {
        public string gameId;
        public int move;
        public bool player;
    }
}
