using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework.Graphics;


namespace Match3.Core
{
    public abstract class Scene : AppObject
    {
        private readonly List<GameObject> sceneObjects = new List<GameObject>();

        protected override void OnUpdate()
        {
            for (int i = sceneObjects.Count - 1; i >= 0; --i)
            {
                if (sceneObjects[i].IsDestroyed)
                {
                    sceneObjects.RemoveAt(i);
                    continue;
                }

                if (sceneObjects[i].IsEnabled)
                    sceneObjects[i].Update();
            }
        }

        protected override void OnDraw(SpriteBatch sBatch)
        {
            for (int i = sceneObjects.Count - 1; i >= 0; --i)
            {
                sceneObjects[i].Draw(sBatch);
            }
        }

        protected override void OnDestroy()
        {
            foreach (var gameObject in sceneObjects)
                RemoveFromScene(gameObject, true);
        }

        public IEnumerable<T> GetSceneObjects<T>() where T: GameObject
        {
            return sceneObjects.OfType<T>();
        }

        public IEnumerable<GameObject> GetSceneObjects()
        {
            return sceneObjects;
        }

        public void AddToScene(GameObject gameObject)
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            gameObject.Load();
            sceneObjects.Add(gameObject);
        }

        public void RemoveFromScene(GameObject gameObject, bool immediate = false)
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            gameObject.Destroy(immediate);
        }
    }
}
