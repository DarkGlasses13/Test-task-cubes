using UniRx;

namespace CubeGame
{
    public class AvailableCubesModel
    {
        private readonly ReactiveCollection<string> _availableCubes = new();
        
        public IReadOnlyReactiveCollection<string> AvailableCubes => _availableCubes;

        public void Populate(params string[] ids)
        {
            foreach (var id in ids)
            {
                AddCube(id);
            }
        }
        
        public void AddCube(string id) => _availableCubes.Add(id);
        public void RemoveCube(string id) => _availableCubes.Remove(id);
        public void Clear() => _availableCubes.Clear();
    }
}