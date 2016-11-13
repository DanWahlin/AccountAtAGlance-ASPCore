namespace AccountAtAGlance.Model
{
    public class Position
    {
        // Primitive properties
        public int Id { get; set; }
        public int SecurityId { get; set; }
        public decimal Shares { get; set; }
        public decimal Total { get; set; }
        public int BrokerageAccountId { get; set; }

        //Navigation Properties
        public virtual Security Security { get; set; }
    }
}
