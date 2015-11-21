namespace GmailGameNarrator.Game
{
    class Player
    {
        public string name { get; }
        public string email { get; }

        public Player(string name, string email)
        {
            this.name = name;
            this.email = email;
        }
    }
}
