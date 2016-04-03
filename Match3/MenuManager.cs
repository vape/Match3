using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Match3.Core;
using Match3.Scenes;
using Match3.Utilities;


namespace Match3
{
    public class MenuManager : GameObject
    {
        private Rect startButtonRect;
        private Texture2D startButton;
        private Texture2D startButtonHighlighted;

        private ButtonState previousState;
        private bool highlighted;
        private float scale;

        public MenuManager()
        {
            startButton = App.LoadContent<Texture2D>("Menu/StartButton");
            startButtonHighlighted = App.LoadContent<Texture2D>("Menu/StartButtonHighlighted");

            startButtonRect = new Rect(new Vector2((App.Viewport.Width - startButton.Width) / 2,
                                                   (App.Viewport.Height - startButton.Height) / 2),
                                       new Point(startButton.Width, startButton.Height));

            scale = 0;
        }

        protected override void OnUpdate()
        {
            if (scale < 1)
                scale += App.DeltaTime * 4;
            else
                scale = 1;

            var mouseState = Mouse.GetState();

            if (startButtonRect.Contains(mouseState.Position))
            {
                highlighted = true;

                if (mouseState.LeftButton == ButtonState.Pressed &&
                    previousState == ButtonState.Released)
                    App.LoadScene(new GameScene());
            }
            else
            {
                highlighted = false;
            }

            previousState = mouseState.LeftButton;
        }

        protected override void OnDraw(SpriteBatch sBatch)
        {
            if (highlighted)
                sBatch.Draw(startButtonHighlighted, startButtonRect.ScaleFromCenter(scale));
            else
                sBatch.Draw(startButton, startButtonRect.ScaleFromCenter(scale));
        }
    }
}
