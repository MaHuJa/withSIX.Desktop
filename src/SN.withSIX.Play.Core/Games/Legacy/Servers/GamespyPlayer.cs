// <copyright company="SIX Networks GmbH" file="GamespyPlayer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Games.Legacy.Servers
{
    public class GamespyPlayer : Player, IComparePK<GamespyPlayer>
    {
        int _deaths;
        int _score;
        string _team;

        public GamespyPlayer(Server server, string name, string team, int score, int deaths) : base(server, name) {
            _team = team;
            _score = score;
            _deaths = deaths;
        }

        public string Team
        {
            get { return _team; }
            set { SetProperty(ref _team, value); }
        }
        public int Score
        {
            get { return _score; }
            set { SetProperty(ref _score, value); }
        }
        public int Deaths
        {
            get { return _deaths; }
            set { SetProperty(ref _deaths, value); }
        }

        #region IComparePK<Player> Members

        public bool ComparePK(GamespyPlayer other) {
            return ComparePK((Player) other);
        }

        #endregion

        public override bool ComparePK(object obj) {
            var emp = obj as GamespyPlayer;
            if (emp != null)
                return ComparePK(emp);
            return false;
        }
    }
}