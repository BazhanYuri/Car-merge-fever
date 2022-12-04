namespace HomaGames.HomaBelly
{
    public interface ISettingsProvider
    {
        int Order { get; }
        string Name { get; }
        string Version { get; }
        void Draw();
    }
}