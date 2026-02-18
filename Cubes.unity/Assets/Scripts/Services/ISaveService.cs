namespace CubeGame
{
    public interface ISaveService
    {
        bool HasSave { get; }
        void Save();
        void Load();
        void ClearSave();
    }
}
