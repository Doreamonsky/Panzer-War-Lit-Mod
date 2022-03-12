namespace ParadoxNotion.Serialization.FullSerializer
{
    ///<summary> Implement on type to migrate from another serialization-wise. This works in pair with the [fsMigrateToAttribute] and [fsMigrateVersionsAttribute] attributes.</summary>
    public interface IMigratable { }
    public interface IMigratable<T> : IMigratable
    {
        void Migrate(T model);
    }
}