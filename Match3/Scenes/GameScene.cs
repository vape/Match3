using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Match3.Utilities;
using Match3.World;
using Match3.Core;

namespace Match3.Scenes
{
    public class GameScene : Scene
    {
        public static GameScene Instance
        { get; private set; }

        private GridManager grid;
        
        public GameScene()
        {
            Instance = this;
        }

        protected override void OnLoad()
        {
            grid = new GridManager(8, 8);
            AddToScene(grid);
        }
    }
}
