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

        private float endTime;
        private bool noTimeLeft;

        public GameManager()
        {
            uiManager = new UIManager(LevelTime);
            uiManager.RestartPressed += RestartPressedHandler;
            uiManager.MenuPressed += MenuPressedHandler;

            gridManager = new GridManager(FieldWidth, FieldHeight);
            gridManager.LineCleared += LineClearedHandler;
            gridManager.BombCleared += BombClearedHandler;
            gridManager.ChainCleared += ChainClearedHandler;

            App.Scene.AddToScene(uiManager);
            App.Scene.AddToScene(gridManager);

            endTime = App.Time + LevelTime;
        }

        protected override void OnUpdate()
        {
            noTimeLeft = endTime - App.Time < 0;

            if (noTimeLeft)
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
            uiManager.AddScore(score, multiplier);
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

        private void LineClearedHandler(object sender, ChainClearedEventArgs e)
        {
            AddScore(e.ChainLength * 100, e.Multiplier);
        }

        private void BombClearedHandler(object sender, ChainClearedEventArgs e)
        {
            AddScore(e.ChainLength * 200, e.Multiplier);
        }

        private void ChainClearedHandler(object sender, ChainClearedEventArgs e)
        {
            AddScore(e.ChainLength * 50, e.Multiplier);
        }

        #endregion
    }
}
