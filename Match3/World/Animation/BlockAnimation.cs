﻿using Match3.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match3.World.Animation
{
    public abstract class BlockAnimation
    {
        public Block Block
        { get; private set; }
        public bool Animating
        { get; protected set; }

        protected float loadedTime;
        protected Action<Block> animationEndCallback;

        public BlockAnimation(Action<Block> animationEndCallback)
        {
            this.animationEndCallback = animationEndCallback;
        }

        public void Load(Block block)
        {
            Block = block;
            loadedTime = App.Time;
            Animating = true;

            OnAnimationLoad();
        }

        public Rect Update(Rect viewRect)
        {
            if (!Animating)
                return viewRect;

            return OnAnimationUpdate(viewRect);
        }

        public void Stop()
        {
            Animating = false;

            if (animationEndCallback != null)
            {
                animationEndCallback(Block);
                animationEndCallback = null;
            }
        }

        protected virtual void OnAnimationLoad() { }
        protected virtual Rect OnAnimationUpdate(Rect viewRect)
        {
            return viewRect;
        }
    }
}
