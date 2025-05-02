using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SEB.Server
{
    public class TournamentManager
    {
        private void HandleTournament(NetworkStream stream)
        {
            /*var tournament = Tournament.TournamentManager.CurrentTournament;

            if (tournament == null || tournament.Participants.Count == 0)
            {
                string noData = "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"No tournament data yet. This is a stub.\"}";
                byte[] buffer = Encoding.UTF8.GetBytes(noData);
                stream.Write(buffer, 0, buffer.Length);
                return;
            }

            var db = new Database.DatabaseManager();

            var participantInfo = tournament.Participants
                .GroupBy(p => p.UserId)
                .Select(g => new
                {
                    username = db.GetUserById(g.Key).Username,
                    count = g.Sum(x => x.Count)
                }).ToList();

            var responseObj = new
            {
                status = tournament.Status,
                start_time = tournament.StartTime.ToString("o"),
                participants = participantInfo,
                winner = tournament.Status == "ended"
                    ? tournament.GetWinnerIds().Select(id => db.GetUserById(id).Username).ToList()
                    : new List<string>()
            };

            string json = JsonConvert.SerializeObject(responseObj);
            string response = "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n" + json;
            byte[] respBuf = Encoding.UTF8.GetBytes(response);
            stream.Write(respBuf, 0, respBuf.Length);*/
        }


    }
}
