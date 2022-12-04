namespace HomaGames.HomaBelly
{
    public interface IModelDeserializer<T>
    {
        T Deserialize(string json);
        T LoadFromCache();
    }
}
