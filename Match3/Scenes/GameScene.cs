using Match3.Core;

namespace Match3.Scenes
{
    public class GameScene : Scene
    {
        protected override void OnLoad()
        {
            AddToScene(new GameManager());
        }
    }
}
