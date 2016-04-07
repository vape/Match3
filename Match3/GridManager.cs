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

        public bool FieldAnimating => field.Animating;

        public event EventHandler<ChainCollectedEventArgs> ChainCollected;
        public event EventHandler<ChainCollectedEventArgs> BombCollected;
        public event EventHandler<ChainCollectedEventArgs> LineCollected;
        public event EventHandler<BlockCollectedEventArgs> BlockCollected;

        private Point blockSize;
        private Point fieldSize;
        private Point fieldPosition;

        private Field field;
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
            field = new Field(GenerateInitialField());

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

        protected override void OnDestroy()
        {
            mouseListener.MouseDown -= MouseDownHandler;
            App.InputListener.RemoveListener(mouseListener);
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
                                Select(block); // or Deselect();
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
            System.Diagnostics.Debug.Assert(matchLength >= 3, 
                "Initial field might be invalid for given matchLength.");

            Block[,] field = new Block[fieldSize.Y, fieldSize.X];

            for (int y = 0; y < fieldSize.Y; ++y)
            {
                for (int x = 0; x < fieldSize.X; ++x)
                {
                    var gridPos = new Point(x, y);
                    var viewPos = GridToView(gridPos);

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

        private void PerformSwap(Swap swap)
        {
            var swapValid = IsValidSwap(swap);

            Action<Swap> animationEnded = (s) =>
            {
                if (swapValid)
                {
                    field.Swap(s);
                    OnSwapped(s);
                    return;
                }

                s.Move(); // swap back
            };

            swap.Move(animationEnded);
        }

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
                        GridPosition = chain[Utils.GetRand(0, 4)].GridPosition
                    };

                    bonuses.Add(bonus);
                }
                else if (chain.Length >= 5)
                {
                    var bonus = new BonusInfo()
                    {
                        BonusType = BlockBonusType.Bomb,
                        BlockType = chain.BlockType,
                        GridPosition = chain[Utils.GetRand(0, chain.Length)].GridPosition
                    };

                    bonuses.Add(bonus);
                }
            }
        }

        private void CollectBonus(Block bonusBlock, Action<Block> animationEndedCallback = null)
        {
            if (bonusBlock.Bonus == BlockBonusType.Bomb)
            {
                var bombBlocks = GetBombBonusBlocks(bonusBlock);

                foreach (var block in bombBlocks)
                {
                    block.AttachAnimation(new ScaleDownAnimation(animationEndedCallback,
                                                                 delay: 0.35f));

                    if (block.Bonus != BlockBonusType.None &&
                        block != bonusBlock)
                        CollectBonus(block, animationEndedCallback);
                }

                RaiseBombCollected(bombBlocks.Count);
            }
            else if (bonusBlock.Bonus == BlockBonusType.HorizontalLine ||
                     bonusBlock.Bonus == BlockBonusType.VerticalLine)
            {
                var lineBlocks = GetLineBonusBlocks(bonusBlock);

                foreach (var block in lineBlocks)
                {
                    float speed;

                    if (bonusBlock.Bonus == BlockBonusType.HorizontalLine)
                        speed = Math.Abs(block.X - bonusBlock.X);
                    else
                        speed = Math.Abs(block.Y - bonusBlock.Y);

                    block.AttachAnimation(new ScaleDownAnimation(animationEndedCallback,
                                                                 speed: speed * 2,
                                                                 delay: speed / 10f));

                    if (block.Bonus != BlockBonusType.None &&
                        block != bonusBlock)
                        CollectBonus(block, animationEndedCallback);
                }

                RaiseLineCollected(lineBlocks.Count);
            }
        }

        private void Collect()
        {
            var chains = field.FindChains(matchLength);

            Action<Block> animationEnded = (block) =>
            {
                field[block] = null;
                RaiseBlockCollected(block);

                if (field.Animating)
                    return;

                OnCollected(chains.Count);
            };

            foreach (var chain in chains)
            {
                if (chain.Length > 3)
                    StoreChainBonuses(chain);

                foreach (var block in chain)
                {
                    block.AttachAnimation(new ScaleDownAnimation(animationEnded));

                    if (block.Bonus != BlockBonusType.None)
                        CollectBonus(block, animationEnded);
                }

                RaiseChainCollected(chain.Length);
            }

            if (chains.Count == 0)
                OnCollected(0);
        }

        private void Fill()
        {
            int blocksCreated = 0;

            Action<Block> animationEnded = (block) =>
            {
                if (field.Animating)
                    return;

                OnFilled(blocksCreated);
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
                        block.AttachAnimation(new MovingAnimation(viewPosition, true, animationEnded, speed));

                        field[gridPosition] = block;
                        blocksCreated++;
                    }
                }
            }

            if (blocksCreated == 0)
                OnFilled(0);
        }

        private void Collapse()
        {
            int blocksMoved = 0;
            int bonusesAdded = 0;

            Action<Block> animationEnded = (block) =>
            {
                if (field.Animating)
                    return;

                OnCollapsed(blocksMoved, bonusesAdded);
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
                                                                                true,
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
                OnCollapsed(0, 0);
        }

        #region Main

        private void OnSwapped(Swap swap)
        {
            currentMultiplier = 1;

            Collect();
        }

        private void OnCollected(int chainsCount)
        {
            if (chainsCount != 0)
            {
                currentMultiplier++;
                Collapse();
            }
        }

        private void OnCollapsed(int blocksMoved, int bonusesAdded)
        {
            Fill();
        }

        private void OnFilled(int blocksCreated)
        {
            if (blocksCreated != 0)
                Collect();
        }

        private void OnLoaded()
        {
            // ...
        }

        #endregion

        #region Utils

        public bool IsValidSwap(Swap swap)
        {
            foreach (var other in field.FindSwaps(matchLength))
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

        private void RaiseBlockCollected(Block block)
        {
            BlockCollected?.Invoke(this, new BlockCollectedEventArgs(block, currentMultiplier));
        }

        private void RaiseLineCollected(int blocksCollected)
        {
            LineCollected?.Invoke(this, new ChainCollectedEventArgs(blocksCollected, currentMultiplier));
        }

        private void RaiseBombCollected(int blocksCollected)
        {
            BombCollected?.Invoke(this, new ChainCollectedEventArgs(blocksCollected, currentMultiplier));
        }

        private void RaiseChainCollected(int blocksCollected)
        {
            ChainCollected?.Invoke(this, new ChainCollectedEventArgs(blocksCollected, currentMultiplier));
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
