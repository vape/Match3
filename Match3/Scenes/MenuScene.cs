using Match3.Core;

namespace Match3.Scenes
{
    public class MenuScene : Scene
    {
        protected override void OnLoad()
        {
            AddToScene(new MenuManager());
        }
    }
}
