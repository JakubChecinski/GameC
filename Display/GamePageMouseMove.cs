using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Game.Engine;

namespace Game.Display
{
    public partial class GamePage : Page
    {
        // overriden methods to move items around the item grid
        protected void ItemsMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // mouse button was clicked
            var image = e.Source as Image;
            if ((Movable || ItemSellFlag) && image != null && ItemGrid.CaptureMouse())
            {
                soundEngine.PlaySound(Sound.SoundNames.MOUSE_CLICK_GAME_1);
                mousePosition = e.GetPosition(ItemCanvas);
                PrevX = MyGridPositionX(mousePosition);
                PrevY = MyGridPositionY(mousePosition);
                draggedImage = image;
                ItemGrid.Children.Remove(draggedImage);
                draggedImage.Height = ItemGrid.ActualHeight / 6.0; // convenience rescaling (6 rows in the grid)
                draggedImage.Width = ItemGrid.ActualWidth / 5.0;
                ItemCanvas.Children.Add(draggedImage);
                Canvas.SetLeft(draggedImage, MyCanvasPositionX(mousePosition));
                Canvas.SetTop(draggedImage, MyCanvasPositionY(mousePosition));
            }
        }
        protected void ItemsMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // mouse button was released
            if (draggedImage != null)
            {
                soundEngine.PlaySound(Sound.SoundNames.MOUSE_CLICK_GAME_2);
                ItemGrid.ReleaseMouseCapture();
                ItemCanvas.Children.Remove(draggedImage);
                if (RemovableItems)
                {
                    int gridPosX = (int)(5.0 * mousePosition.X / ItemCanvas.ActualWidth);
                    int gridPosY = (int)(6.0 * mousePosition.Y / ItemCanvas.ActualHeight);
                    if (gridPosX < 0 || gridPosX > 4 || gridPosY < 0 || gridPosY > 5)
                    {
                        currentSession.RemoveItemPosition(PrevX, PrevY);
                        // sell an item (it's just those two lines of game logic so I put them here)
                        currentSession.CurrentPlayer.Gold += Index.ProduceSpecificItem(draggedImage.Name).GoldValue;
                        currentSession.RefreshStats();
                        draggedImage = null;
                    }
                    else
                    {
                        ItemGrid.Children.Add(draggedImage);
                        Grid.SetRow(draggedImage, PrevY);
                        Grid.SetColumn(draggedImage, PrevX);
                        draggedImage = null;
                    }
                }
                else
                {
                    ItemGrid.Children.Add(draggedImage);
                    Grid.SetRow(draggedImage, MyGridPositionY(mousePosition));
                    Grid.SetColumn(draggedImage, MyGridPositionX(mousePosition));
                    // if the target already had an image, swap the selected two
                    Image oldImage = GetImageFromGrid(MyGridPositionX(mousePosition), MyGridPositionY(mousePosition), true);
                    if (oldImage != null)
                    {
                        Grid.SetRow(oldImage, PrevY);
                        Grid.SetColumn(oldImage, PrevX);
                    }
                    //
                    draggedImage = null;
                    currentSession.RemoveItemPosition(PrevX, PrevY);
                    currentSession.AddItemPosition(MyGridPositionX(mousePosition), MyGridPositionY(mousePosition));
                }
            }
        }
        protected void ItemsMouseMove(object sender, MouseEventArgs e)
        {
            // mouse button is held down and the cursor is being moved
            if (draggedImage != null)
            {
                var position = e.GetPosition(ItemCanvas);
                var offset = position - mousePosition;
                mousePosition = position;
                Canvas.SetLeft(draggedImage, Canvas.GetLeft(draggedImage) + offset.X);
                Canvas.SetTop(draggedImage, Canvas.GetTop(draggedImage) + offset.Y);
            }
        }

        // methods to convert between item grid and item canvas positions
        // this will help with smooth rendering
        protected int MyGridPositionX(Point position)
        {
            int gridPos = (int)(5.0 * position.X / ItemCanvas.ActualWidth);
            if (gridPos < 0) gridPos = 0;
            if (gridPos > 4) gridPos = 4;
            return gridPos;
        }
        protected int MyGridPositionY(Point position)
        {
            int gridPos = (int)(6.0 * position.Y / ItemCanvas.ActualHeight);
            if (gridPos < 0) gridPos = 0;
            if (gridPos > 5) gridPos = 5;
            return gridPos;
        }
        protected double MyCanvasPositionX(Point position)
        {
            double gridColumnWidth = ItemCanvas.ActualWidth / 5.0;
            double newPos = 0.0;
            while (newPos < position.X)
                newPos += gridColumnWidth;
            return newPos - gridColumnWidth;
        }
        protected double MyCanvasPositionY(Point position)
        {
            double rowColumnWidth = ItemCanvas.ActualHeight / 6.0;
            double newPos = 0.0;
            while (newPos < position.Y)
                newPos += rowColumnWidth;
            return newPos - rowColumnWidth;
        }
    }
}
