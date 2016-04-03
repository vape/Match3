using Match3.Core;
using Match3.Scenes;
using Match3.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.InputListeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match3
{
    public class MenuManager : GameObject
    {
        private Rect startButtonRect;
        private Texture2D startButton;
        private Texture2D startButtonHighlighted;

        private BitmapFont riffic48;
        private BitmapFont riffic96;

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

            riffic48 = App.LoadContent<BitmapFont>("Fonts/Riffic_48");
            riffic96 = App.LoadContent<BitmapFont>("Fonts/Riffic_96");

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
