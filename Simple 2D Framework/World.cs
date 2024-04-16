using System.Diagnostics;

namespace Simple_2D_Framework
{
    /// <summary>
    /// Contains a collection of World Objects to model a 2D Video Game World
    /// </summary>
    public class World
    {
        private Actor Player { get; set; }
        public World()
        {
            if (!File.Exists("config.cfg"))
            {
                File.WriteAllText("config.cfg", "TraceOutput = TraceOutput.log");
            }

            string config = File.ReadAllLines("config.cfg")[0];
            string logPath = config.Split('=')[1].Trim();

            Trace.Listeners.Add(new TextWriterTraceListener(logPath, "worldListener"));
            Trace.AutoFlush = true;
            Trace.TraceInformation("World Created");
        }
        
        public enum DistanceMeasure
        {
            Manhattan,
            Chessboard
        }
        private readonly List<WorldObj> WorldObjects = [];
        private readonly List<IThinker> Thinkers = [];
        private readonly List<IThinker> RequestingAddition = [];
        private readonly List<IThinker> RequestingRemoval = [];

        /// <summary>
        /// Advances the Game World by one time step. Each thinker is allowed to perform their turn one by one.
        /// </summary>
        public void Advance()
        {
            foreach (var thinker in RequestingAddition)
            {
                Thinkers.Add(thinker);
            }
            foreach (var thinker in RequestingRemoval)
            {
                Thinkers.Remove(thinker);
            }
            RequestingRemoval.Clear();
            foreach (var thinker in Thinkers)
            {
                Trace.TraceInformation($"{thinker} is thinking");
                thinker.Think();
            }
        }

        /// <summary>
        /// Adds a World Object to the World
        /// </summary>
        /// <param name="worldObj">The World Object to be added</param>
        /// <param name="x">X coordinate where to add</param>
        /// <param name="y">Y coordinate where to add</param>
        public void AddObj(WorldObj worldObj, int x, int y)
        {
            if (worldObj == null)
            {
                Trace.TraceError("Added null as WorldObj");
                return;
            }
            worldObj.World = this;
            worldObj.X = x;
            worldObj.Y = y;
            WorldObjects.Add(worldObj);
            if(worldObj is IThinker)
            {
                var thinker = worldObj as IThinker;
                RequestingAddition.Add(thinker);
            }
            worldObj.OnCreate();
            Trace.TraceInformation($"Added WorldObj: {worldObj}");
        }

        /// <summary>
        /// Adds a Player World Object to the World. Other World Objects may ask the world about the Player World Object
        /// </summary>
        /// <param name="actor">The Player Object to be added</param>
        /// <param name="x">X coordinate where to add</param>
        /// <param name="y">Y coordinate where to add</param>
        public void AddPlayer(Actor actor, int x, int y)
        {
            if (actor == null)
            {
                Trace.TraceError("Added null as Player");
                return;
            }
            AddObj(actor, x, y);
            Player = actor;
            actor.IsPlayer = true;
        }

        public Actor GetPlayer()
        {
            return Player;
        }

        /// <summary>
        /// Removes a World Object from the World
        /// </summary>
        /// <param name="worldObj">The World Object to be removed</param>
        public void RemoveObj(WorldObj worldObj)
        {
            worldObj.OnDestroy();
            WorldObjects.Remove(worldObj);
            if (worldObj is IThinker)
            {
                RequestingRemoval.Add(worldObj as IThinker);
            }
            Trace.TraceInformation($"Removed WorldObj: {worldObj}");
        }

        /// <summary>
        /// Returns a list of World Objects within a given distance from a position in the world
        /// </summary>
        /// <param name="x">X coordinate to search from</param>
        /// <param name="y">Y coordinate to search from</param>
        /// <param name="maxDistance">Maximum distance to search for</param>
        /// <param name="measure">The type of distance measure to be used</param>
        /// <returns>List of WorldObj</returns>
        public List<WorldObj> GetWorldObjsInRange(int x, int y, int maxDistance, DistanceMeasure measure)
        {
            return measure switch
            {
                DistanceMeasure.Manhattan => WorldObjects.Where(o => ((o.X - x) >= 0 ? (o.X - x) : (x - o.X)) + ((o.Y - y) >= 0 ? (o.Y - y) : (y - o.Y)) <= maxDistance).ToList(),
                DistanceMeasure.Chessboard => WorldObjects.Where(o => ((o.X - x) >= 0 ? (o.X - x) : (x - o.X)) <= maxDistance && ((o.Y - y) >= 0 ? (o.Y - y) : (y - o.Y)) <= maxDistance).ToList(),
                _ => []
            };
        }

        /// <summary>
        /// Returns a list of all World Objects at a given coordinate set
        /// </summary>
        /// <param name="x">X coordinate to check</param>
        /// <param name="y">Y coordinate to check</param>
        /// <returns>List of WorldObj</returns>
        public List<WorldObj> CheckPosition(int x, int y)
        {
            return WorldObjects.Where(o => o.X == x && o.Y == y).ToList();
        }

        /// <summary>
        /// Check if a coordinate in the world is available to move to. A space is considered open if no World Object with the "Solid" flag set to "true" occupies the space.
        /// </summary>
        /// <param name="x">X coordinate to check</param>
        /// <param name="y">Y coordinate to check</param>
        /// <returns>"true" if space is available, "false" otherwise</returns>
        public bool CheckEmpty(int x, int y)
        {
            return !WorldObjects.Where(o => o.X == x && o.Y == y && o.Solid).Any();
        }
    }
}
