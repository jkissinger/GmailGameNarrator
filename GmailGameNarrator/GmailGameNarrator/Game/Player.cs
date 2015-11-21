namespace GmailGameNarrator.Game
{
    class Player
    {
        public string Name { get; }
        public string Address { get; }

        public Player(string name, string address)
        {
            Name = name;
            Address = address.Trim().ToLowerInvariant();
        }

        public override string ToString()
        {
            return Address + " playing as " + Name;
        }
    }
}
