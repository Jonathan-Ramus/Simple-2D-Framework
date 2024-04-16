namespace Simple_2D_Framework
{
    /// <summary>
    /// Represents an item that can reside in an Actor's inventory
    /// </summary>
    public class Item
    {
        public int Type { get; set; }
        public string? Name { get; set; }

        public string Info() { return ""; }
    }
}
