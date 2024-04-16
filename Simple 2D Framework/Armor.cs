using System.Diagnostics;

namespace Simple_2D_Framework
{
    /// <summary>
    /// Represents a piece of armor worn by an Actor
    /// </summary>
    public abstract class Armor : Item
    {
        public Actor? Owner { get; set; }
        public int BodyPart { get; set; }

        /// <summary>
        /// Implement here a function to reduce the damage by some amount
        /// </summary>
        /// <param name="damage">The amount of incoming damage</param>
        /// <param name="damageType">The type of incoming damage</param>
        /// <returns>The adjusted damage value</returns>
        public abstract int ReduceDamage(int damage, IDamageable.DamageType damageType);

        /// <summary>
        /// Called when an Actor equips this armor
        /// </summary>
        public virtual void OnEquip()
        {
            Trace.TraceInformation($"{Owner.Name} equipped {this.Name}");
        }

        /// <summary>
        /// Called when an Actor unequips this armor
        /// </summary>
        public virtual void OnUnequip()
        {
            Trace.TraceInformation($"{Owner.Name} unequipped {this.Name}");
        }
    }
}
