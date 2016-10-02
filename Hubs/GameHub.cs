using System.Collections.Generic;
using WebApplication1.Models;
using System.Linq;
using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNet.SignalR.Hubs;

namespace WebApplication1.Hubs
{
    [HubName("gameHub")]
    public class GameHub : Hub
    {
        static Dictionary<string, HubUser> users = new Dictionary<string, HubUser>();
        static Queue<string> queue = new Queue<string>();

        public GameHub()
        {
            new Task(Pulse).Start();
        }

        /// <summary>
        /// Endless cycle in order to matchmaking
        /// Бесконечный цикл, который создаёт матчмейкинговые игры
        /// </summary>
        void Pulse()
        {
            while (true)
            {
                while (queue.Count > 1)
                {
                    var player1 = queue.Dequeue();
                    var player2 = queue.Dequeue();
                    if (!IsRegistered(player1))
                        if (IsRegistered(player2))
                        {
                            queue.Enqueue(player2);
                            return;
                        }

                if (!IsRegistered(player2))
                        if (IsRegistered(player1))
                        {
                            queue.Enqueue(player1);
                            return;
                        }

                    Game game = new Game();

                    users[player1].ActiveGame = game;
                    users[player1].Player = true;
                    users[player2].ActiveGame = game;
                    users[player2].Player = false;

                    Clients.Client(player1).gameInform(true);
                    Clients.Client(player2).gameInform(false);
                }
                Thread.Sleep(250);
            }
        }

        /// <summary>
        /// Calls when client clicks on cell of gamefield 
        /// Вызывается при нажатии игроком на ячейку игрового поля
        /// </summary>
        /// <param name="cell"></param>
        public void RequestMove(byte cell)
        {
            if (!IsRegistered(Context.ConnectionId) ||
                users[Context.ConnectionId].ActiveGame == null) return;

            HubUser current = users[Context.ConnectionId];

            current.ActiveGame.MakeMove(current.Player, cell);

            GameStatus status = new ProcessingUnit().CalculateNextMove(!current.Player, current.ActiveGame, current.PVP);

            UpdateClientsGameboards(current.ActiveGame);

            if (status != GameStatus.InProgress)
            {
                CreateReplay(current.ActiveGame, (byte)status);
                ResetPlayers(current.ActiveGame);
            }
        }

        /// <summary>
        /// Assigns new Game instance to player and register new client if isn't registered yet
        /// Присваивает игрокам новый экземпляр Game и регистрирует в списке, если игрок еще не зарегистрирован
        /// </summary>
        /// <param name="pvp">True if player requests game against another player, false if requests game against AI</param>
        /// <param name="player">True - X, false - O. Aka - wanna make first move</param>
        public void RequestNewGame(bool pvp, bool player)
        {
            if (!IsRegistered(Context.ConnectionId))
                users.Add(Context.ConnectionId, new HubUser() { LastConnection = DateTime.Now, Player = player, ActiveGame = null, PVP = pvp });

            HubUser user = users[Context.ConnectionId];

            if (user.ActiveGame == null)
            {
                user.PVP = pvp;
                user.Player = player;
                if (!pvp)
                {
                    user.ActiveGame = new Game();
                    Clients.Client(Context.ConnectionId).gameInform(player);
                    if (!player)
                        RequestMove(0);
                }
                else
                    if (!queue.Contains(Context.ConnectionId))
                    queue.Enqueue(Context.ConnectionId);
            }
        }

        /// <summary>
        /// Returns JSON that includes game replay as array of cells to put marks on and count of moves
        /// Возвращает JSON в котором содержится реплей игры в виде массива клеток, на которые ходили игроки и количества ходов
        /// </summary>
        /// <param name="hash">Replay hash</param>
        public void RequestReplay(string hash)
        {
            var game = Helper.Replay.Games.Where(x => x.hash == hash).FirstOrDefault();
            if (game == null) return;
            Clients.Caller.replayParse(Helper.jsSerializer.Serialize(game.moves), game.movesCount);
        }

        /// <summary>
        /// Send gameboard information to all clients assigned to this game
        /// Отправляет информацию о игровом поле всем назначенным на него игрокам
        /// </summary>
        /// <param name="game">Game instance</param>
        void UpdateClientsGameboards(Game game)
        {
            foreach (var user in users.Where(x => ReferenceEquals(x.Value.ActiveGame, game)))
                Clients.Client(user.Key).updateGameboard(Helper.jsSerializer.Serialize(game.Gameboard));
        }

        /// <summary>
        /// Unassign all players
        /// Отвязывает всех игроков от игры
        /// </summary>
        /// <param name="game">Game instance</param>
        void ResetPlayers(Game game)
        {
            foreach (var user in users.Where(x => ReferenceEquals(x.Value.ActiveGame, game)))
                user.Value.ActiveGame = null;
        }

        /// <summary>
        /// Create replay of game and send hash of it to assigned players
        /// Создаёт реплей игры и отправляет игрокам его хэш
        /// </summary>
        /// <param name="game">Game instance</param>
        void CreateReplay(Game game, byte status)
        {
            string hash = game.SaveReplay();

            foreach (var user in users.Where(x => ReferenceEquals(x.Value.ActiveGame, game)))
                Clients.Client(user.Key).gameFinished(hash, status);
        }
        bool IsRegistered(string id)
        {
            return users.ContainsKey(id);
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            if (!IsRegistered(Context.ConnectionId)) return base.OnDisconnected(stopCalled);
            var user = users[Context.ConnectionId];
            if (user.ActiveGame != null)
            {
                if (user.PVP)
                {
                    CreateReplay(user.ActiveGame, user.Player ? (byte)0x02 : (byte)0x01);
                }
                ResetPlayers(user.ActiveGame);
            }
            users.Remove(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }
    }
}