using Microsoft.Xna.Framework.Graphics;


namespace Match3.Core
{
    public abstract class AppObject
    {
        public bool IsDestroyed
        { get; private set; }

        public void Load()
        {
            OnLoad();
        }

        public void Update()
        {
            OnUpdate();
        }

        public void Draw(SpriteBatch sBatch)
        {
            OnDraw(sBatch);
        }

        public void Destroy(bool immediate = false)
        {
            bool canceled = false;
            OnDestroy(ref canceled, immediate);

            IsDestroyed = !canceled || immediate;
        }

        protected virtual void OnLoad() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnDraw(SpriteBatch sBatch) { }

        protected virtual void OnDestroy() { }
        protected virtual void OnDestroy(ref bool cancel, bool immediate = false)
        {
            OnDestroy();
        }
    }
}
