using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

using Match3.Core;
using Match3.Utilities;


namespace Match3
{
    public class UIManager : GameObject
    {
        private struct ScoreLabel
        {
            public float CreationTime;
            public string Text;
        }

        public event EventHandler MenuPressed;
        public event EventHandler RestartPressed;

        public GameScreen CurrentScreen
        { get; set; }

        private int score;
        private float stopTime;
        private List<ScoreLabel> labels;
        private float labelAliveTime;

        private BitmapFont riffic24;
        private BitmapFont riffic32;
        private BitmapFont riffic48;
        private BitmapFont riffic96;

        private Texture2D leftLabelBackground;
        private Texture2D rightLabelBackground;

        private float resultScreenAlpha;

        public UIManager(float levelTime)
        {
            score = 0;
            stopTime = App.Time + levelTime;
            labels = new List<ScoreLabel>();
            labelAliveTime = 4;

            riffic24 = App.LoadContent<BitmapFont>("Fonts/Riffic_24");
            riffic32 = App.LoadContent<BitmapFont>("Fonts/Riffic_32");
            riffic48 = App.LoadContent<BitmapFont>("Fonts/Riffic_48");
            riffic96 = App.LoadContent<BitmapFont>("Fonts/Riffic_96");

            leftLabelBackground = App.LoadContent<Texture2D>("LeftLabelBackground");
            rightLabelBackground = App.LoadContent<Texture2D>("RightLabelBackground");
        }

        public void AddScore(int scoreValue, int multiplier)
        {
            score += scoreValue * multiplier;

            labels.Add(new ScoreLabel()
            {
                CreationTime = App.Time,
                Text = " + " + scoreValue + (multiplier > 1 ? " x " + multiplier : "")
            });
        }

        protected override void OnUpdate()
        {
            switch (CurrentScreen)
            {
                case GameScreen.Game:
                    labels.RemoveAll((x) => App.Time - x.CreationTime > labelAliveTime);
                    break;
                case GameScreen.Result:
                    UpdateResultScreen();
                    break;
            }
        }

        private void UpdateResultScreen()
        {
            if (resultScreenAlpha < 0.85f)
                resultScreenAlpha += App.DeltaTime * 2;

            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Space))
                RestartPressed?.Invoke(this, EventArgs.Empty);
            else if (keyboardState.IsKeyDown(Keys.Escape))
                MenuPressed?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnDraw(SpriteBatch sBatch)
        {
            switch (CurrentScreen)
            {
                case GameScreen.Game:
                    DrawGameScreen(sBatch);
                    break;
                case GameScreen.Result:
                    DrawResultScreen(sBatch);
                    break;
            }
        }

        private void DrawGameScreen(SpriteBatch sBatch)
        {
            var scoreLabelPosition = new Vector2(20, 25);
            var timeLabelPosition = new Vector2(App.Viewport.Width - 130, 25);

            var scoreLabelBackgroundRect = new Rectangle(0, 0, 252, 84);
            var timeLabelBackgroundRect = new Rectangle(App.Viewport.Width - 252, 0, 252, 84);

            sBatch.Draw(leftLabelBackground, scoreLabelBackgroundRect, Color.White);
            sBatch.Draw(rightLabelBackground, timeLabelBackgroundRect, Color.White);

            var timeLeft = stopTime - App.Time > 0 ? stopTime - App.Time : 0;

            sBatch.DrawStringWithShadow(riffic32, $"Score: {score:000000}", scoreLabelPosition, Color.White);
            sBatch.DrawStringWithShadow(riffic32, $"{timeLeft:00.0}", timeLabelPosition, Color.White);

            for (int i = 0; i < labels.Count; ++i)
            {
                var position = new Vector2(20, 100 + (riffic32.LineHeight * i));
                var alpha = (1 - (App.Time - labels[i].CreationTime) / labelAliveTime) / 3;

                sBatch.DrawString(riffic32, labels[i].Text, position, new Color(Color.Black, alpha));
            }
        }

        private void DrawResultScreen(SpriteBatch sBatch)
        {
            sBatch.DrawRect(App.Viewport.Bounds, new Color(Color.Black, resultScreenAlpha));

            sBatch.DrawStringFromCenter(riffic96, "SCORE: " + score, App.Viewport.Bounds.Center.ToVector2(), Color.White);
            sBatch.DrawStringFromCenter(riffic24, "press Space to restart, or Escape to exit to menu",
                                        (App.Viewport.Bounds.Center + new Point(0, 50)).ToVector2(), Color.White);
        }
    }
}
