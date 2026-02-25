using UniRx;

namespace CubeGame
{
    public class AvailableCubesModel
    {
        private readonly ReactiveCollection<string> _cubes = new();
        
        public IReadOnlyReactiveCollection<string> Cubes => _cubes;

        public void Populate(params string[] ids)
        {
            foreach (var id in ids)
            {
                AddCube(id);
            }
        }
        
        public void AddCube(string id) => _cubes.Add(id);
        public void RemoveCube(string id) => _cubes.Remove(id);
        public void Clear() => _cubes.Clear();
    }
}