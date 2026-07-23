namespace BehaviorTree
{
    public readonly struct BBKey<T>
    {
        public readonly string Name;

        public BBKey(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
        public override int GetHashCode() => Name.GetHashCode();
        public override bool Equals(object obj) => obj is BBKey<T> other && Name == other.Name;
    }
}
