namespace PragueParking
{
    public class MC : Vehicle
    {
        public MC()
        {
        }

        public MC(string regNr, int storlek) : base(regNr, storlek)
        {
        }

        public override string Typnamn
        {
            get { return "MC"; }
        }
    }
}
