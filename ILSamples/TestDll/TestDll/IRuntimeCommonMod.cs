namespace ShanghaiWindy.Core.Interface
{
    public interface IRuntimeCommonMod
    {
        void onStarted();

        void onUpdated();

        void onFixedUpdated();

        void onGUI();

        void onLateUpdated();

        void onDestroyed();

        void onSceneLoaded(string levelName);
    }
}