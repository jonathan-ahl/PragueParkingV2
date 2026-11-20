namespace PragueParking
{
    // Car ärver allt från Vehicle (RegNr, Storlek, Ankomst ...)
    // och behöver bara tala om vad den heter.
    public class Car : Vehicle
    {
        public Car()
        {
        }

        public Car(string regNr, int storlek) : base(regNr, storlek)
        {
        }

        public override string Typnamn
        {
            get { return "Bil"; }
        }
    }
}
