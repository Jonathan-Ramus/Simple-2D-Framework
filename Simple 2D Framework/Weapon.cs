namespace Simple_2D_Framework
{
    /// <summary>
    /// Represents a weapon with which an Actor can deal damage
    /// </summary>
    public abstract class Weapon : Item
    {  
        public Actor? Owner {  get; set; }
        
        /// <summary>
        /// A function to get a list of targets in range of a weapon.
        /// </summary>
        /// <returns>List of targets in range</returns>
        public abstract List<Actor> GetTargets();
        
        /// <summary>
        /// Attack a specified target with this weapon
        /// </summary>
        /// <param name="target">The target to attack</param>
        public abstract void Attack(Actor target);

        public virtual void OnEquip() { }

        public virtual void OnUnequip() { }
    }
}
