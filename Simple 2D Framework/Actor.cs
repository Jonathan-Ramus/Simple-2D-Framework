using System.Diagnostics;

namespace Simple_2D_Framework
{
    /// <summary>
    /// Represents a creature residing in a World. A creature can act on its own and it can be killed. Creatures can carry weapons, armor, and items.
    /// </summary>
    public abstract class Actor : WorldObj, IDamageable, IThinker
    {
        /// <summary>
        /// Whether or not this Actor is a Player
        /// </summary>
        public bool IsPlayer { get; set; }

        /// <summary>
        /// Maximum HitPoints. When calling Heal(), HitPoints will not exceed this value, unless IgnoreMaxHitpoints is set to "true".
        /// </summary>
        public int MaxHitPoints { get; set; }

        /// <summary>
        /// This Actor's current HP value. If it drops below 1, the Actor will enter the "Dead" State.
        /// </summary>
        public int HitPoints { get; set; }
        public enum ActorState
        {
            Idle,
            Active,
            Dead
        }
        
        [Flags]
        public enum ActorModifiers
        {
            Burned = 1,
            Paralyzed = 2,
            Frozen = 4,
            Confused = 8,
            Poisoned = 16,
            Sleeping = 32,
            Flying = 64,
            Phasing = 128,
            Shielded = 256,
            Bleeding = 512,
            Irradiated = 1024,
            Sick = 2048,
            Steady = 4096,
            Heavy = 8192,
            Hungry = 16384
        }

        /// <summary>
        /// The Actor's current State. Actor behavior may change depending on its State.
        /// </summary>
        private ActorState State {  get; set; }

        /// <summary>
        /// Bitmasked flags containing some Actor Modifiers. World Objects and Items may check for these flags.
        /// </summary>
        private ActorModifiers Modifiers { get; set; }

        /// <summary>
        /// This Actor's current target. May be used for navigation or attacking.
        /// </summary>
        public Actor? Target { get; set; }

        /// <summary>
        /// This Actor's currently held weapon. A Weapon defines the Actor's attacking behavior. An Actor whose Weapon is null may not attack.
        /// </summary>
        private Weapon? Weapon = null;

        /// <summary>
        /// All Armor currently equipped by this Actor. Each armor piece can reduce (or increase) the amount of damage recieved when this Actor is attacked.
        /// Two Armors of the same BodyType cannot be worn simultaneously.
        /// </summary>
        private readonly Dictionary<int, Armor> Armors = [];

        /// <summary>
        /// A list of all Items this Actor is carrying. Actors may equip or use these Items.
        /// </summary>
        private readonly List<Item> Items = [];
        
        /// <summary>
        /// Set this Actor's current State
        /// </summary>
        /// <param name="state">The State to change into</param>
        public void SetState(ActorState state)
        {
            State = state;
        }

        /// <summary>
        /// Get this Actor's current State
        /// </summary>
        /// <returns></returns>
        public ActorState GetState()
        {
            return State;
        }
        
        /// <summary>
        /// Add "modifiers" in the form of flags. World Objects and Items can check for these flags
        /// </summary>
        /// <param name="modifiers">The flags to be set</param>
        public void AddModifiers(ActorModifiers modifiers)
        {
            Modifiers |= modifiers;
        }

        /// <summary>
        /// Remove "modifiers" in the form of flags. World Objects and Items can check for these flags
        /// </summary>
        /// <param name="modifiers">The flags to be reset</param>
        public void RemoveModifiers(ActorModifiers modifiers)
        {
            Modifiers &= ~modifiers;
        }

        /// <summary>
        /// Get the current list of "modifier"-flags set for this Actor
        /// </summary>
        /// <returns>Bitmasked Flags Enum</returns>
        public ActorModifiers GetModifiers()
        {
            return Modifiers;
        }
        
        /// <summary>
        /// Unequip a weapon
        /// </summary>
        public virtual void UnequipWeapon()
        {
            if (Weapon != null)
            {
                Items.Add(Weapon);
                Weapon.OnUnequip();
                Weapon = null;
            }
        }

        /// <summary>
        /// Equip a weapon. The weapon must be in the Actor's inventory
        /// </summary>
        /// <param name="weapon">The weapon to equip</param>
        public virtual void EquipWeapon(Weapon? weapon)
        {
            if (!Items.Contains(weapon)) {  return; }
            if(Weapon != null)
            {
                UnequipWeapon();
            }
            Items.Remove(weapon);
            Weapon = weapon;
            weapon.Owner = this;
            Weapon.OnEquip();
        }

        /// <summary>
        /// Unequip an armor piece.
        /// </summary>
        /// <param name="bodyPart">The body part from where to remove the armor</param>
        public virtual void UnequipArmor(int bodyPart)
        {
            if (Armors.ContainsKey(bodyPart))
            {
                Items.Add(Armors[bodyPart]);
                Armors[bodyPart].OnUnequip();
                Armors.Remove(bodyPart);
            }
        }

        /// <summary>
        /// Equip an armor piece. The armor must be in the Actor's inventory
        /// </summary>
        /// <param name="armor">The armor piece to equip</param>
        public virtual void EquipArmor(Armor? armor)
        {
            if (!Items.Contains(armor)) { return; }
            if (Armors.ContainsKey(armor.BodyPart))
            {
                UnequipArmor(armor.BodyPart);
            }
            Items.Remove(armor);
            Armors[armor.BodyPart] = armor;
            armor.Owner = this;
            armor.OnEquip();
        }

        /// <summary>
        /// Add an item to inventory
        /// </summary>
        /// <param name="item"></param>
        public void GiveItem(Item item)
        {
            Items.Add(item);
            Trace.TraceInformation($"{this.Name} got {item.Name}");
        }

        /// <summary>
        /// Remove an item from inventory
        /// </summary>
        /// <param name="item"></param>
        public void TakeItem(Item item)
        {
            Items.Remove(item);
            Trace.TraceInformation($"{this.Name} lost {item.Name}");
        }
        
        /// <summary>
        /// Use a usable item from inventory
        /// </summary>
        /// <param name="item">The item to be used</param>
        public void UseItem(Item item)
        {
            if(item is not IUsable || !Items.Contains(item)) { return; }
            (item as IUsable).Use(this);
            Trace.TraceInformation($"{this} used {item}");
        }

        /// <summary>
        /// Get a list a all items this Actor currently holds
        /// </summary>
        /// <returns>List of Items</returns>
        public List<Item> GetItems()
        {
            return new List<Item>(Items);
        }

        /// <summary>
        /// Damage the Actor
        /// </summary>
        /// <param name="origin">The source of the damage</param>
        /// <param name="damage">The amount of damage</param>
        /// <param name="damageType">The type of damage</param>
        public virtual void ReceiveDamage(Actor? origin, int damage, IDamageable.DamageType damageType)
        {
            //Reduce damage for each worn piece of armor
            foreach (var armor in Armors)
            {
                damage = armor.Value.ReduceDamage(damage, damageType);
            }

            damage = damage > 0 ? damage : 0;
            this.HitPoints -= damage;
            Trace.TraceInformation($"{this} recieved {damage} damage");

            if (this.HitPoints <= 0)
            {
                this.State = ActorState.Dead;
                this.Solid = false;
                Trace.TraceInformation($"{this} was killed by {origin.Name}");
                return;
            }

            //If the Actor survived the attack, get angry at the attacker
            if(origin != null)
            {
                this.Target = origin;
                this.State = ActorState.Active;
            }
        }

        /// <summary>
        /// Restore some amount of Hit Points
        /// </summary>
        /// <param name="hitpoints">The number of HP to restore</param>
        /// <param name="ignoreMaxHitPoints">Whether or not to ignore the Actor's maximum HP value. Set to true to allow overheal.</param>
        public virtual void Heal(int hitpoints, bool ignoreMaxHitPoints = false)
        {
            this.HitPoints += hitpoints;
            if (!ignoreMaxHitPoints && this.HitPoints > this.MaxHitPoints)
            {
                this.HitPoints = this.MaxHitPoints;
            }
        }

        /// <summary>
        /// Move to a new position if that spot is empty.
        /// </summary>
        /// <param name="x">X coordinate to move to</param>
        /// <param name="y">Y coordinate to move to</param>
        /// <returns></returns>
        public virtual bool TryMove(int x, int y)
        {
            if (World == null || (!World.CheckEmpty(x, y))) {  return false; }
            this.X = x;
            this.Y = y;
            return true;
        }

        /// <summary>
        /// Try attacking a target using the equipped weapon.
        /// </summary>
        /// <param name="target">The target to attack</param>
        /// <returns>"false" if no weapon is equipped, or if the target is out of the held weapon's range. Returns "true" otherwise.</returns>
        public virtual bool TryAttack(Actor? target)
        {
            if(target == null || Weapon == null || !Weapon.GetTargets().Contains(target)) { return false; }
            Weapon.Attack(target);
            return true;
        }

        /// <summary>
        /// Returns a list of targets this actor can hit with currently equipped weapon
        /// </summary>
        /// <returns>List of IDamageable</returns>
        public virtual List<Actor> GetTargets()
        {
            if (Weapon == null) return [];
            return Weapon.GetTargets();
        }

        /// <summary>
        /// Called every time step by an Idle Actor
        /// </summary>
        public abstract void Look();
        /// <summary>
        /// Called every time step by an Active Actor
        /// </summary>
        public abstract void Act();
        /// <summary>
        /// Called every time step by a Dead Actor
        /// </summary>
        public abstract void WhileDead();
        
        /// <summary>
        /// Called every this Actor's World advances one step. Calls a different function depending on the Actor's current State
        /// </summary>
        public virtual void Think()
        {
            switch (State)
            {
                case ActorState.Idle:
                    this.Look();
                    break;
                case ActorState.Active:
                    this.Act();
                    break;
                case ActorState.Dead:
                    this.WhileDead();
                    break;
            }
        }
    }
}
