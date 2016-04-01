using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Match3.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.InputListeners;
using System.Diagnostics;
using Match3.Core;
using Match3.Scenes;

namespace Match3.World
{
    public class Grid : GameObject
    {
        private const int blockSideSize = 64;
        private const int blockGap = 4;
        private const int matchLength = 3;

        private Point blockSize;
        private Point fieldSize;
        private Point fieldPosition;

        private Field field;
        private Block selectedBlock;

        private MouseListener mouseListener;

        private bool mouseClicked;
        private Point mousePosition;

        public Grid(int width, int height)
        {
            blockSize = new Point(blockSideSize);
            fieldSize = new Point(width, height);
            fieldPosition = new Point((App.Viewport.Width - (width * (blockSideSize + blockGap))) / 2,
                                      (App.Viewport.Height - (height * (blockSideSize + blockGap))) / 2);
        }


        protected override void OnLoad()
        {
            field = new Field(GenerateInitialField());

            Action<Block> onBlockAppeared = (block) =>
            {
                if (field.AnyBlocksAnimating())
                    return;

                OnLoaded();
            };

            foreach (var block in field)
                block.AnimateAppearing(onBlockAppeared);

            mouseListener = App.InputListener.AddListener<MouseListener>();
            mouseListener.MouseDown += MouseDownHandler;
        }

        protected override void OnUpdate()
        {
            foreach (var block in field)
            {
                if (block == null)
                    continue;

                if (block.IsMoving)
                    block.UpdateMoving();

                if (block.IsAnimating)
                    block.UpdateAnimation();
            }

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
            {
                if (block == null)
                    continue;

                block.Draw(sBatch);
            }
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
                    var viewPos = GridToScreen(x, y);
                    var gridPos = new Point(x, y);

                    // Assumes matchLength is more or equal to 3
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

        private void Clear()
        {
            var chains = Chain.FindChains(field, matchLength);

            Action<Block> onBlockDisappeared = (block) =>
            {
                field[block.Y, block.X] = null;

                if (field.AnyBlocksActive())
                    return;

                OnCleared(chains.Count);
            };

            foreach (var chain in chains)
            {
                foreach (var block in chain.Blocks)
                    block.AnimateDisappearing(onBlockDisappeared);
            }

            if(chains.Count == 0)
                OnCleared(0);
        }

        private void Refill()
        {
            int blocksCreated = 0;

            Action<Block> onBlockAppeared = (block) =>
            {
                if (field.AnyBlocksAnimating())
                    return;

                OnRefilled(blocksCreated);
            };

            for (int y = 0; y < fieldSize.Y; ++y)
            {
                for (int x = 0; x < fieldSize.X; ++x)
                {
                    if (field[y, x] == null)
                    {
                        var block = new Block(new Point(x, y), GridToScreen(x, y), blockSize);
                        block.AnimateAppearing(onBlockAppeared);
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

            Action<Block, Point, Point> onBlockMoved = (block, originPos, newPos) =>
            {
                if (field.AnyBlocksMoving())
                    return;

                OnGapsFilled(blocksMoved);
            };

            for (int y = fieldSize.Y - 1; y >= 0; --y)
            {
                for (int x = 0; x < fieldSize.X; ++x)
                {
                    if (field[y, x] == null)
                    {
                        int offset = 0;

                        for (int i = y; i >= 0; --i)
                        {
                            if (field[i, x].Usable())
                            {
                                field[i, x].MoveTo(GridToScreen(x, y - offset),
                                                   new Point(x, y - offset), 
                                                   setGridPositionImmediately:true,
                                                   movedCallback: onBlockMoved);
                                field.Move(x, i, x, y - offset);

                                blocksMoved++;
                                offset++;
                            }
                        }
                    }
                }
            }

            if (blocksMoved == 0)
                OnGapsFilled(0);
        }

        #region Main

        private void OnSwapped(Swap swap)
        {
            Clear();
        }

        private void OnCleared(int chainsCleared)
        {
            FillGaps();
        }

        private void OnGapsFilled(int blocksMoved)
        {
            if (blocksMoved != 0)
                Clear();
            else
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

        private Vector2 GridToScreen(int x, int y)
        {
            return new Vector2(fieldPosition.X + (blockSideSize + blockGap) * x,
                               fieldPosition.Y + (blockSideSize + blockGap) * y);
        }

        #endregion

        #region Events

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
