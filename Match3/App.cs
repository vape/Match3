using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.InputListeners;

using Match3.Core;
using Match3.Scenes;


namespace Match3
{
    public class App : Game
    {
        public const int TargetFPS = 60;

        public const int WindowWitdh = 1024;
        public const int WindowHeight = 640;

        public static Scene Scene
        {
            get
            {
                return instance.currentScene;
            }
        }

        public static InputListenerManager InputListener
        { get; private set; }

        public static float DeltaTime
        { get; private set; }
        public static float Time
        { get; private set; }

        public static GraphicsDevice Graphics
        {
            get
            {
                return instance.GraphicsDevice;
            }
        }
        public static Viewport Viewport
        {
            get
            {
                return instance.GraphicsDevice.Viewport;
            }
        }

        private static App instance;

        public static T LoadContent<T>(string name)
        {
            return instance.Content.Load<T>(name);
        }

        public static void LoadScene(Scene scene)
        {
            instance.SetNextScene(scene);
        }

        public static void Quit()
        {
            instance.exitRequested = true;
        }

        Texture2D background;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Scene currentScene;
        private Scene nextScene;

        private bool exitRequested;

        public App()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = WindowWitdh;
            graphics.PreferredBackBufferHeight = WindowHeight;

            instance = this;
        }

        protected override void Initialize()
        {
            IsMouseVisible = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1f / TargetFPS);
            InputListener = new InputListenerManager();

            SetNextScene(new GameScene());

            background = Content.Load<Texture2D>("Background");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (exitRequested)
                Environment.Exit(0);

            if (nextScene != null)
            {
                if (currentScene != null)
                    currentScene.Destroy(true);

                currentScene = nextScene;
                currentScene.Load();

                nextScene = null;
            }

            Time = (float)gameTime.TotalGameTime.TotalSeconds;
            DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            InputListener.Update(gameTime);

            if (currentScene != null)
                currentScene.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin();
            spriteBatch.Draw(background, Viewport.Bounds, Color.White);

            if (currentScene != null)
                currentScene.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void SetNextScene(Scene scene)
        {
            nextScene = scene;
        }
    }
}
