namespace GmailGameNarrator.Game
{
    class Player
    {
        public string name { get; }
        public string address { get; }

        public Player(string name, string address)
        {
            this.name = name;
            this.address = address.Trim().ToLowerInvariant();
        }
    }
}
