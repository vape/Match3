using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

using Match3.Core;
using Match3.Utilities;
using Microsoft.Xna.Framework.Input;

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

        private float stopTime;
        private List<ScoreLabel> labels;
        private float labelAliveTime;
        private int score;

        private BitmapFont riffic16;
        private BitmapFont riffic24;
        private BitmapFont riffic32;
        private BitmapFont riffic48;
        private BitmapFont riffic96;

        private float resultScreenAlpha = 0;

        public UIManager(float levelTime)
        {
            stopTime = App.Time + levelTime;
            labelAliveTime = 5;
            labels = new List<ScoreLabel>();

            riffic16 = App.LoadContent<BitmapFont>("Fonts/Riffic_16");
            riffic24 = App.LoadContent<BitmapFont>("Fonts/Riffic_24");
            riffic32 = App.LoadContent<BitmapFont>("Fonts/Riffic_32");
            riffic48 = App.LoadContent<BitmapFont>("Fonts/Riffic_48");
            riffic96 = App.LoadContent<BitmapFont>("Fonts/Riffic_96");
        }

        public void AddScore(int score, int multiplier)
        {
            this.score += score * multiplier;

            labels.Add(new ScoreLabel()
            {
                CreationTime = App.Time,
                Text = " + " + score + (multiplier > 1 ? " x " + multiplier : "")
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
                resultScreenAlpha += App.DeltaTime * 3;

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
            var scoreLabelPosition = new Vector2(25, 20);
            var timeLabelPosition = new Vector2(App.Viewport.Width - 105, 20);

            var timeLeft = stopTime - App.Time > 0 ? stopTime - App.Time : 0;

            sBatch.DrawString(riffic32, String.Format("Score: {0}", score), scoreLabelPosition, Color.Black);
            sBatch.DrawString(riffic32, String.Format("{0:00.0}", timeLeft), timeLabelPosition, Color.Black);

            for (int i = 0; i < labels.Count; ++i)
            {
                var position = new Vector2(25, 60 + (riffic32.LineHeight * i));
                var alpha = 1 - (App.Time - labels[i].CreationTime) / labelAliveTime;

                sBatch.DrawString(riffic24, labels[i].Text, position, new Color(Color.Black, alpha));
            }
        }

        private void DrawResultScreen(SpriteBatch sBatch)
        {
            sBatch.DrawRect(App.Viewport.Bounds, new Color(Color.Black, resultScreenAlpha));
            sBatch.DrawStringFromCenter(riffic96, "SCORE: " + score, App.Viewport.Bounds.Center, Color.White);
            sBatch.DrawStringFromCenter(riffic24, "press Space to restart, or Escape to exit to menu",
                                        App.Viewport.Bounds.Center + new Point(0, 50), Color.White);
        }
    }
}
