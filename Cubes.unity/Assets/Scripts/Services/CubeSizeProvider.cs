namespace CubeGame
{
    public class CubeSizeProvider
    {
        public float Size { get; private set; }

        public void Initialize(float panelHeight, float fillPercent)
        {
            Size = panelHeight * fillPercent;
        }
    }
}
