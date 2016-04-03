using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.InputListeners;

using Match3.Core;
using Match3.Utilities;
using Match3.World;
using Match3.World.Animation;


namespace Match3
{
    public class GridManager : GameObject
    {
        private struct BonusInfo
        {
            public BlockBonusType BonusType;
            public BlockType BlockType;
            public Point GridPosition;
        }

        private const int matchLength = 3;
        private const int blockSideSize = 48;
        private const int blockGap = 8;

        public bool FieldAnimating
        {
            get
            {
                return field.Animating;
            }
        }

        public event EventHandler<ChainClearedEventArgs> ChainCleared;
        public event EventHandler<ChainClearedEventArgs> BombCleared;
        public event EventHandler<ChainClearedEventArgs> LineCleared;

        private Point blockSize;
        private Point fieldSize;
        private Point fieldPosition;

        private BlockField field;
        private Block selectedBlock;
        private List<BonusInfo> bonuses;

        private MouseListener mouseListener;
        private Point mousePosition;
        private bool mouseClicked;

        private int currentMultiplier;

        public GridManager(int width, int height)
        {
            blockSize = new Point(blockSideSize);
            fieldSize = new Point(width, height);
            fieldPosition = new Point((App.Viewport.Width - (width * (blockSideSize + blockGap)) + blockGap) / 2,
                                      (App.Viewport.Height - (height * (blockSideSize + blockGap)) + blockGap) / 2);

            bonuses = new List<BonusInfo>();
        }

        protected override void OnLoad()
        {
            field = new BlockField(GenerateInitialField());

            Action<Block> animationEnded = (block) =>
            {
                if (field.Animating)
                    return;

                OnLoaded();
            };

            foreach (var block in field)
                block.AttachAnimation(new ScaleUpAnimation(animationEnded));

            mouseListener = App.InputListener.AddListener<MouseListener>();
            mouseListener.MouseDown += MouseDownHandler;
        }

        protected override void OnUpdate()
        {
            foreach (var block in field)
                block?.Update();

            if (mouseClicked)
            {
                CheckInteraction();

                mouseClicked = false;
                mousePosition = Point.Zero;
            }
        }

        protected override void OnDraw(SpriteBatch sBatch)
        {
            foreach (var block in field)
                block?.Draw(sBatch);
        }


        private void CheckInteraction()
        {
            foreach (var block in field)
            {
                if (block.Usable() && block.ViewRect.Contains(mousePosition))
                {
                    if (selectedBlock == null)
                    {
                        Select(block);
                        break;
                    }
                    else
                    {
                        if (selectedBlock != block)
                        {
                            var swap = new Swap(selectedBlock, block);

                            if (!swap.CanSwap)
                            {
                                Deselect(); // or Select(block);
                                break;
                            }

                            PerformSwap(swap);
                            Deselect();
                        }
                    }
                }
            }
        }

        private Block[,] GenerateInitialField()
        {
            Block[,] field = new Block[fieldSize.Y, fieldSize.X];

            for (int y = 0; y < fieldSize.Y; ++y)
            {
                for (int x = 0; x < fieldSize.X; ++x)
                {
                    var viewPos = GridToView(x, y);
                    var gridPos = new Point(x, y);

                    // Assumes matchLength >= 3
                    BlockType blockType;
                    do
                        blockType = Block.GetRandomBlockType();
                    while ((x >= 2 &&
                           field[y, x - 1].Type == blockType &&
                           field[y, x - 2].Type == blockType) ||
                           (y >= 2 &&
                           field[y - 1, x].Type == blockType &&
                           field[y - 2, x].Type == blockType));

                    var block = new Block(gridPos, viewPos, blockSize, blockType);
                    field[y, x] = block;
                }
            }

            return field;
        }

        #region Field Control

        private void PerformSwap(Swap swap)
        {
            Action<Swap> onInvalidSwap = (s) =>
            {
                s.Make(); // swap back
            };

            Action<Swap> onValidSwap = (s) =>
            {
                field.SwapValues(s.From.GridPosition, s.To.GridPosition);

                OnSwapped(s);
            };

            if (IsValidSwap(swap))
                swap.Make(onValidSwap);
            else
                swap.Make(onInvalidSwap);
        }

        private void ClearBonus(Block bonusBlock, Action<Block> animationEndedCallback = null)
        {
            if (bonusBlock.Bonus == BlockBonusType.Bomb)
            {
                foreach (var block in GetBombBonusBlocks(bonusBlock))
                {
                    block.AttachAnimation(new ScaleDownAnimation(animationEndedCallback,
                                                                 delay: 0.35f));

                    if (block.Bonus != BlockBonusType.None &&
                        block != bonusBlock)
                        ClearBonus(block, animationEndedCallback);
                }

                RaiseBombCleared();
            }
            else if (bonusBlock.Bonus == BlockBonusType.HorizontalLine ||
                     bonusBlock.Bonus == BlockBonusType.VerticalLine)
            {
                foreach (var block in GetLineBonusBlocks(bonusBlock))
                {
                    var speed = 1f;

                    if (bonusBlock.Bonus == BlockBonusType.HorizontalLine)
                        speed = Math.Abs(block.X - bonusBlock.X);
                    else
                        speed = Math.Abs(block.Y - bonusBlock.Y);

                    block.AttachAnimation(new ScaleDownAnimation(animationEndedCallback,
                                                                 speed: speed,
                                                                 delay: speed / 5f));

                    if (block.Bonus != BlockBonusType.None &&
                        block != bonusBlock)
                        ClearBonus(block, animationEndedCallback);
                }

                RaiseLineCleared();
            }
        }

        private void Clear()
        {
            var chains = Chain.FindChains(field, matchLength);

            Action<Block> animationEnded = (block) =>
            {
                field[block] = null;

                if (field.Animating)
                    return;

                OnCleared(chains.Count);
            };

            foreach (var chain in chains)
            {
                StoreChainBonuses(chain);

                foreach (var block in chain.Blocks)
                {
                    block.AttachAnimation(new ScaleDownAnimation(animationEnded));

                    if (block.Bonus != BlockBonusType.None)
                        ClearBonus(block, animationEnded);
                }

                RaiseChainCleared(chain.Length);
            }

            if (chains.Count == 0)
                OnCleared(0);
        }

        private void Refill()
        {
            int blocksCreated = 0;

            Action<Block> animationEnded = (block) =>
            {
                if (field.Animating)
                    return;

                OnRefilled(blocksCreated);
            };

            for (int y = 0; y < fieldSize.Y; ++y)
            {
                for (int x = 0; x < fieldSize.X; ++x)
                {
                    if (field[y, x] == null)
                    {
                        var gridPosition = new Point(x, y);
                        var viewPosition = GridToView(gridPosition);
                        var speed = 2 * (y + 3);

                        var block = new Block(gridPosition, viewPosition - new Vector2(0, 200), blockSize);
                        block.AttachAnimation(new MovingAnimation(viewPosition, gridPosition, 
                                                                  animationEnded, speed));


                        field[y, x] = block;
                        blocksCreated++;
                    }
                }
            }

            if (blocksCreated == 0)
                OnRefilled(0);
        }

        private void FillGaps()
        {
            int blocksMoved = 0;
            int bonusesAdded = 0;

            Action<Block> animationEnded = (block) =>
            {
                if (field.Animating)
                    return;

                OnGapsFilled(blocksMoved, bonusesAdded);
            };

            foreach (var bonus in bonuses)
            {
                var bonusBlock = new Block(bonus.GridPosition, GridToView(bonus.GridPosition),
                                           blockSize, bonus.BlockType, bonus.BonusType);

                bonusBlock.AttachAnimation(new ScaleUpAnimation(animationEnded, speed: 3f));
                field[bonusBlock] = bonusBlock;

                bonusesAdded++;
            }

            bonuses.Clear();

            for (int y = fieldSize.Y - 1; y >= 0; --y)
            {
                for (int x = 0; x < fieldSize.X; ++x)
                {
                    if (field[y, x] == null)
                    {
                        int offset = 0;

                        for (int i = y; i >= 0; --i)
                        {
                            if (field[i, x].Usable() && field[y - offset, x] == null)
                            {
                                var gridPosition = new Point(x, y - offset);
                                var viewPosition = GridToView(x, y - offset);
                                var speed = ((y - offset) + 3);

                                field[i, x].AttachAnimation(new MovingAnimation(viewPosition, 
                                                                                gridPosition, 
                                                                                animationEnded,
                                                                                speed));
                                field[i, x].GridPosition = gridPosition;
                                field[y - offset, x] = field[i, x];
                                field[i, x] = null;

                                blocksMoved++;
                                offset++;
                            }
                        }
                    }
                }
            }

            if (blocksMoved == 0 && bonusesAdded == 0)
                OnGapsFilled(0, 0);
        }

        #endregion

        #region Main

        private void OnSwapped(Swap swap)
        {
            currentMultiplier = 1;

            Clear();
        }

        private void OnCleared(int chainsCleared)
        {
            if (chainsCleared != 0)
            {
                currentMultiplier++;
                FillGaps();
            }
        }

        private void OnGapsFilled(int blocksMoved, int bonusesAdded)
        {
            Refill();
        }

        private void OnRefilled(int blocksCreated)
        {
            if (blocksCreated != 0)
                Clear();
        }

        private void OnLoaded()
        {
            // ...
        }

        #endregion

        #region Utils
        
        private void StoreChainBonuses(Chain chain)
        {
            if (chain.ChainType == ChainType.Intersection)
            {
                var bonus = new BonusInfo()
                {
                    BonusType = BlockBonusType.Bomb,
                    BlockType = chain.IntersectionBlock.Type,
                    GridPosition = chain.IntersectionBlock.GridPosition
                };

                bonuses.Add(bonus);
            }
            else
            {
                if (chain.Length == 4)
                {
                    var bonus = new BonusInfo()
                    {
                        BonusType = chain.ChainType == ChainType.Horizontal ?
                                                       BlockBonusType.HorizontalLine :
                                                       BlockBonusType.VerticalLine,
                        BlockType = chain.BlockType,
                        GridPosition = chain.Blocks[Utils.GetRand(0, 4)].GridPosition
                    };

                    bonuses.Add(bonus);
                }
                else if (chain.Length >= 5)
                {
                    var bonus = new BonusInfo()
                    {
                        BonusType = BlockBonusType.Bomb,
                        BlockType = chain.BlockType,
                        GridPosition = chain.Blocks[Utils.GetRand(0, chain.Length)].GridPosition
                    };

                    bonuses.Add(bonus);
                }
            }
        }

        private bool IsValidSwap(Swap swap)
        {
            foreach (var other in Swap.FindSwaps(field, matchLength))
            {
                if ((swap.From.GridPosition == other.From.GridPosition &&
                     swap.To.GridPosition == other.To.GridPosition) ||
                    (swap.From.GridPosition == other.To.GridPosition &&
                     swap.To.GridPosition == other.From.GridPosition))
                    return true;
            }

            return false;
        }

        private void Select(Block block)
        {
            if (selectedBlock != null)
                Deselect();

            selectedBlock = block;
            selectedBlock.Selected = true;
        }

        private void Deselect()
        {
            selectedBlock.Selected = false;
            selectedBlock = null;
        }

        private Vector2 GridToView(Point position)
        {
            return GridToView(position.X, position.Y);
        }

        private Vector2 GridToView(int x, int y)
        {
            return new Vector2(fieldPosition.X + (blockSideSize + blockGap) * x,
                               fieldPosition.Y + (blockSideSize + blockGap) * y);
        }

        public List<Block> GetLineBonusBlocks(Block block)
        {
            var blocks = new List<Block>();

            if (block.Bonus == BlockBonusType.HorizontalLine)
            {
                for (int x = 0; x < field.Width; ++x)
                    if (field[block.Y, x].Usable())
                        blocks.Add(field[block.Y, x]);
            }
            else if (block.Bonus == BlockBonusType.VerticalLine)
            {
                for (int y = 0; y < field.Height; ++y)
                    if (field[y, block.X].Usable())
                        blocks.Add(field[y, block.X]);
            }

            return blocks;
        }

        public List<Block> GetBombBonusBlocks(Block block)
        {
            var blocks = new List<Block>();

            for (int y = -1; y <= 1; ++y)
            {
                for (int x = -1; x <= 1; ++x)
                {
                    bool validIndex = block.X + x >= 0 && block.Y + y >= 0 &&
                                      block.X + x < field.Width && block.Y + y < field.Height;

                    if (validIndex && field[block.Y + y, block.X + x].Usable())
                        blocks.Add(field[block.Y + y, block.X + x]);
                }
            }

            return blocks;
        }

        #endregion

        #region Events

        private void RaiseLineCleared()
        {
            LineCleared?.Invoke(this, new ChainClearedEventArgs(8, currentMultiplier));
        }

        private void RaiseBombCleared()
        {
            BombCleared?.Invoke(this, new ChainClearedEventArgs(9, currentMultiplier));
        }

        private void RaiseChainCleared(int chainLength)
        {
            ChainCleared?.Invoke(this, new ChainClearedEventArgs(chainLength, currentMultiplier));
        }

        private void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (e.PreviousState.LeftButton != ButtonState.Released)
                return;

            mouseClicked = true;
            mousePosition = e.Position;
        }

        #endregion
    }
}
