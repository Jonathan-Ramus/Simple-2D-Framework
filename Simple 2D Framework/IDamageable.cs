namespace Simple_2D_Framework
{
    public interface IDamageable
    {
        [Flags]
        public enum DamageType
        {
            Stab = 0,
            Slice = 1,
            Blunt = 2,
            Fire = 4,
            Ice = 8,
            Magic = 16,
            Lightning = 32
        }
        
        public void ReceiveDamage(Actor? origin, int damage, DamageType damageType);
    }
}
