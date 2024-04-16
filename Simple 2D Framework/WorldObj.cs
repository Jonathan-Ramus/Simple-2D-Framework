namespace Simple_2D_Framework
{
    /// <summary>
    /// Represents an object which can reside in a Game World with integer X and Y coordinates
    /// </summary>
    public abstract class WorldObj
    {
        
        public World? World {  get; set; }
        public string? Name { get; set; }
        public int X {  get; set; }
        public int Y { get; set; }
        public int Type {  get; set; }
        public bool Solid { get; set; }

        /// <summary>
        /// Called when the World Object is added to a World. By default does nothing.
        /// </summary>
        public virtual void OnCreate() { }

        /// <summary>
        /// Called when the World Object is removed from a World. By default does nothing.
        /// </summary>
        public virtual void OnDestroy() { }

        public override string ToString()
        {
            return String.Format("Name: {0}, Type: {1}, X: {2}, Y: {3}", Name != null ? Name : null, Type, X, Y);
        }
    }
}
