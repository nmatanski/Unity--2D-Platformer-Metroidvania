namespace Game.Characters.Abilities
{
    public abstract class BaseAbility : Character
    {
        protected Character Character { get; set; }

        private void Start()
        {
            Initialize();
        }

        protected override void Initialize()
        {
            base.Initialize();
            Character = GetComponent<Character>();
        }
    }
}
