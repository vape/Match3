using System;

using Match3.Core;
using Match3.Scenes;
using Match3.World;


namespace Match3
{
    public class GameManager : GameObject
    {
        private const int FieldWidth = 8;
        private const int FieldHeight = 8;
        private const float LevelTime = 60;

        public int Score
        { get; private set; }

        private GridManager gridManager;
        private UIManager uiManager;
        private SoundManager soundManager;

        private float startTime;
        private float levelTime;

        public GameManager()
        {
            uiManager = new UIManager();
            uiManager.RestartPressed += RestartPressedHandler;
            uiManager.MenuPressed += MenuPressedHandler;

            gridManager = new GridManager(FieldWidth, FieldHeight);
            gridManager.LineCollected += LineCollectedHandler;
            gridManager.BombCollected += BombCollectedHandler;
            gridManager.ChainCollected += ChainCollectedHandler;
            gridManager.BlockCollected += BlockCollectedHandler;

            soundManager = new SoundManager();

            App.Scene.AddToScene(uiManager);
            App.Scene.AddToScene(gridManager);
            App.Scene.AddToScene(soundManager);

            startTime = App.Time;
            levelTime = LevelTime;
        }

        protected override void OnUpdate()
        {
            var timeLeft = levelTime - (App.Time - startTime);

            uiManager.TimeLeft = timeLeft;
            uiManager.Score = Score;

            if (timeLeft <= 0)
            {
                if (gridManager.IsEnabled && !gridManager.FieldAnimating)
                {
                    gridManager.IsEnabled = false;
                    uiManager.CurrentScreen = GameScreen.Result;
                }
            }
        }

        private void AddScore(int score, int multiplier)
        {
            Score += score * multiplier;
            uiManager.ShowScore(score, multiplier);
        }

        #region Events

        private void RestartPressedHandler(object sender, EventArgs e)
        {
            App.LoadScene(new GameScene());
        }

        private void MenuPressedHandler(object sender, EventArgs e)
        {
            App.LoadScene(new MenuScene());
        }

        private void BlockCollectedHandler(object sender, BlockCollectedEventArgs e)
        {
            // soundManager.PlayBlockCollected();
        }

        private void LineCollectedHandler(object sender, ChainCollectedEventArgs e)
        {
            AddScore(e.ChainLength * 150, e.Multiplier);
            soundManager.PlayChainCollected(e.Multiplier);
        }

        private void BombCollectedHandler(object sender, ChainCollectedEventArgs e)
        {
            AddScore(e.ChainLength * 150, e.Multiplier);
            soundManager.PlayChainCollected(e.Multiplier);
        }

        private void ChainCollectedHandler(object sender, ChainCollectedEventArgs e)
        {
            AddScore(e.ChainLength * 100, e.Multiplier);
            soundManager.PlayChainCollected(e.Multiplier);
        }

        #endregion
    }
}
