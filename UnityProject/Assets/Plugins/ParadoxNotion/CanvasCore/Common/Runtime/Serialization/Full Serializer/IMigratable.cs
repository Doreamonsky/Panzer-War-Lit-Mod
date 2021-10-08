namespace ParadoxNotion.Serialization.FullSerializer
{
    /// Implement on type to migrate from another serialization-wise.
    /// This works in pair with the [fsMigrateToAttribute] and [fsMigrateVersionsAttribute] attributes.
    public interface IMigratable { }
    public interface IMigratable<T> : IMigratable
    {
        void Migrate(T model);
    }
}