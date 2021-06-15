using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Game.Engine; 
using Game.Engine.Skills;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Game.Engine.Items;
using Game.Sound;
using Game.Sound.Players;
using System.ComponentModel;

namespace Game.Display
{
    /// <summary>
    /// Display the game world and animate the player character
    /// </summary>
    
    public partial class GamePage : Page
    {
        private MainWindow frameRef;
        private GameSession currentSession;
        public SoundEngine soundEngine;
        protected double stepSize = 24.0;
        private PageData pageData;
        protected FrameworkElement draggedImage;
        protected Point gridPosition;
        protected Point mousePosition;
        // track these so we can update them later
        protected Dictionary<int, Image> monsterImages;
        protected List<Image> obstacleImages;
        protected List<Image> portalImages;
        protected List<Image> interactionImages;
        public bool RemovableItems { get; set; }
        public bool Movable { get; set; }
        public bool ItemSellFlag { get; set; }
        public bool IgnoreNextKey { get; set; }
        private int prevX { get { return pageData.prevX; } set { pageData.prevX = value; } }
        private int prevY { get { return pageData.prevY; } set { pageData.prevY = value; } }
        protected string lastMove { get { return pageData.lastMove; } set { pageData.lastMove = value; } }
        public GamePage(MainWindow frame, string playerChoice)
        {
            InitializeComponent();
            // start background music player
            soundEngine = new SoundEngine(SoundContext.GamePage);
            soundEngine.PlayBackgroundMusic();

            frameRef = frame;
            frame.Background = new SolidColorBrush(Colors.Black);
            pageData = new PageData(this);
            // start game
            monsterImages = new Dictionary<int, Image>();
            obstacleImages = new List<Image>();
            portalImages = new List<Image>();
            interactionImages = new List<Image>();
            currentSession = new GameSession(this, playerChoice);
            Player.Source = currentSession.currentPlayer.GetImageSource();
            Grid.SetColumn(Player, currentSession.PlayerPosLeft);
            Grid.SetRow(Player, currentSession.PlayerPosTop);
            // prepare animations
            Movable = true;
            RemovableItems = false;
            ItemSellFlag = false;
            IgnoreNextKey = false;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.IsVisibleChanged += CleaningAfterPageClosedHandler;
            window.KeyDown += OnKeyDownHandler;
        }

        public void LoadGame(string filename)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                currentSession = (GameSession)formatter.Deserialize(stream);
                stream.Close();
                // remove all existing items
                for (int x = 0; x < 5; x++)
                {
                    for (int y = 0; y < 6; y++)
                    {
                        Image tmp = GetImageFromGrid(x, y);
                        if (tmp != null) ItemGrid.Children.Remove(tmp);
                    }
                }
                foreach (var kvp in monsterImages) WorldGrid.Children.Remove(kvp.Value);
                currentSession.ReInitialize(this);
                //
                stream = new FileStream(filename + ".pgd", FileMode.Open, FileAccess.Read, FileShare.Read);
                pageData = (PageData)formatter.Deserialize(stream);
                stream.Close();
                pageData.parent = this;
                pageData.RestoreItems();
            }
            catch (Exception exc)
            {
                AddConsoleText(exc.Message);
                AddConsoleText("Nie udalo sie wczytac gry. Zamiast tego wygenerowana zostala nowa.");
            }
        }
        
        public void MovePlayer(string direction)
        {
            // this function actually moves the player around
            Movable = false;
            switch(direction)
            {
                case "up":
                    lastMove = direction;
                    Grid.SetRow(Player, currentSession.PlayerPosTop - 1);
                    currentSession.PlayerPosTop -= 1;
                    break;
                case "down":
                    lastMove = direction;
                    Grid.SetRow(Player, currentSession.PlayerPosTop + 1);
                    currentSession.PlayerPosTop += 1;
                    break;
                case "left":
                    lastMove = direction;
                    Grid.SetColumn(Player, currentSession.PlayerPosLeft - 1);
                    currentSession.PlayerPosLeft -= 1;
                    break;
                case "right":
                    lastMove = direction;
                    Grid.SetColumn(Player, currentSession.PlayerPosLeft + 1);
                    currentSession.PlayerPosLeft += 1;
                    break;
                case "reverse":
                    switch (lastMove)
                    {
                        case "up":
                            Grid.SetRow(Player, currentSession.PlayerPosTop + 1);
                            currentSession.PlayerPosTop += 1;
                            break;
                        case "down":
                            Grid.SetRow(Player, currentSession.PlayerPosTop - 1);
                            currentSession.PlayerPosTop -= 1;
                            break;
                        case "left":
                            Grid.SetColumn(Player, currentSession.PlayerPosLeft + 1);
                            currentSession.PlayerPosLeft += 1;
                            break;
                        case "right":
                            Grid.SetColumn(Player, currentSession.PlayerPosLeft - 1);
                            currentSession.PlayerPosLeft -= 1;
                            break;
                    }
                    break;
                default:
                    return; // something went wrong, dont do anything
            }
            Movable = true;
        }
        public void AddConsoleText(string text)
        {
            // add more text to the console
            Console.AppendText(text+"\n");
            Console.ScrollToEnd();
        }
        public void AddConsoleText(int text)
        {
            // add more text to the console
            Console.AppendText(text.ToString() + "\n");
            Console.ScrollToEnd();
        }

        public void AddConsoleColorText(string text, string color)
        {
            // same as AddConsoleText, but in color
            if (Console == null) return;
            AddConsoleText(text);
            SolidColorBrush brush;
            switch (color)
            {
                case "yellow":
                    brush = Brushes.Yellow;
                    break;
                case "red":
                    brush = Brushes.Red;
                    break;
                case "green":
                    brush = Brushes.LimeGreen;
                    break;
                case "blue":
                    brush = Brushes.DodgerBlue;
                    break;
                default:
                    brush = Brushes.Yellow;
                    break;
            }
            for (int i = Console.Document.Blocks.Count - 1; i > 0; i--)
            {
                var paragraph = Console.Document.Blocks.ElementAt(i);
                var text2 = new System.Windows.Documents.TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text;
                if (text2 == text)
                {
                    paragraph.Foreground = brush;
                    break;
                }
            }
        }

        public void ListBoxSelected(Skill skill)
        {
            // receive a listbox item selected and inform the game session
            currentSession.CurrentSelection = skill;
        }
 
        // utility function - get either the last or the second last image from a grid cell
        public Image GetImageFromGrid(int x, int y, bool secondToLast = false)
        {
            if(secondToLast)
            {
                try
                {
                   UIElement el = ItemGrid.Children.Cast<UIElement>().Where(f => Grid.GetColumn(f) == x && Grid.GetRow(f) == y).Reverse().Skip(1).First();
                   if ((el as Image) != null) return (el as Image);
                }
                catch(Exception e)
                {
                    if(e.Message != "Sequence contains no elements") AddConsoleText(e.Message); // silence a useless LINQ warning
                }
            }
            else
            {
                try
                {
                    UIElement el = ItemGrid.Children.Cast<UIElement>().Where(f => Grid.GetColumn(f) == x && Grid.GetRow(f) == y).Reverse().First();
                    if ((el as Image) != null) return (el as Image);
                }
                catch (Exception e)
                {
                    if (e.Message != "Sequence contains no elements") AddConsoleText(e.Message); // silence a useless LINQ warning
                }
            }
            return null;
        }

        // display world monsters
        public void AddMonster(int key, Image monster, int modulo)
        {
            if (monster != null)
            {
                monsterImages.Add(key, monster);
                WorldGrid.Children.Add(monsterImages[key]);
                monsterImages[key].Stretch = Stretch.Fill;
                Grid.SetRow(monsterImages[key], key / modulo);
                Grid.SetColumn(monsterImages[key], key % modulo);
            }
        }
        public void UpdateMonster(int key, Image monster, int modulo)
        {
            if (monsterImages.ContainsKey(key)) WorldGrid.Children.Remove(monsterImages[key]);
            if (monster != null)
            {
                monsterImages[key] = monster;
                WorldGrid.Children.Add(monsterImages[key]);
                monsterImages[key].Stretch = Stretch.Fill;
                Grid.SetRow(monsterImages[key], key / modulo);
                Grid.SetColumn(monsterImages[key], key % modulo);
            }
        }

        // display obstacles
        public void AddObstacle(int x, int y, int number)
        {
            try
            {
                Image img = new Image();
                img.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(("Assets/obstacle" + (number.ToString()).PadLeft(4, '0') + ".png"), UriKind.Relative));
                img.Stretch = Stretch.Fill;
                WorldGrid.Children.Add(img);
                obstacleImages.Add(img);
                Grid.SetColumn(img, x);
                Grid.SetRow(img, y);
            }
            catch(Exception e)
            {
                AddConsoleText(e.Message);
                AddConsoleText("Nie znaleziono obrazka: obstacle" + (number.ToString()).PadLeft(4, '0') + ".png");
            }  
        }

        public void AddPortal(int x, int y)
        {
            int number = 1;
            try
            {
                Image img = new Image();
                img.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(("Assets/portal" + (number.ToString()).PadLeft(4, '0') + ".png"), UriKind.Relative));
                img.Stretch = Stretch.Fill;
                WorldGrid.Children.Add(img);
                portalImages.Add(img);
                Grid.SetColumn(img, x);
                Grid.SetRow(img, y);
            }
            catch (Exception e)
            {
                AddConsoleText(e.Message);
                AddConsoleText("Nie znaleziono obrazka: portal" + (number.ToString()).PadLeft(4, '0') + ".png");
            }

        }
        public void AddInteraction(int x, int y, int number)
        {
            number = number - 3000;
            try
            {
                Image img = new Image();
                img.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(("Assets/Interactions/interaction" + (number.ToString()).PadLeft(4, '0') + ".png"), UriKind.Relative));
                img.Stretch = Stretch.Fill;
                WorldGrid.Children.Add(img);
                interactionImages.Add(img);
                Grid.SetColumn(img, x);
                Grid.SetRow(img, y);
            }
            catch (Exception e)
            {
                AddConsoleText(e.Message);
                AddConsoleText("Nie znaleziono obrazka: interaction" + (number.ToString()).PadLeft(4, '0') + ".png");
            }
        }

        // when changing map, clear map items
        public void ClearMap()
        {
            try
            {
                foreach (KeyValuePair<int, Image> kvp in monsterImages) WorldGrid.Children.Remove(monsterImages[kvp.Key]);
                monsterImages.Clear();
                foreach (Image i in obstacleImages) WorldGrid.Children.Remove(i);
                foreach (Image i in portalImages) WorldGrid.Children.Remove(i);
                foreach (Image i in interactionImages) WorldGrid.Children.Remove(i);
            }
            catch (Exception e)
            {
                AddConsoleText(e.Message);
            }
        }
        // end current game
        public void EndGame()
        {
            CleanAllSoundEngines();
            frameRef.ParentFrame.Navigate(new Display.MenuPage(frameRef));
            var window = Window.GetWindow(this);
            window.IsVisibleChanged -= CleaningAfterPageClosedHandler;
            window.KeyDown -= OnKeyDownHandler; 
        }

        private void CleaningAfterPageClosedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            CleanAllSoundEngines();
        }

        private void CleanAllSoundEngines()
        {
            try 
            {
                soundEngine.StopAllPlayers();
                foreach (var se in currentSession.ChildSoundEngines)
                {
                    se.StopAllPlayers();
                }
                currentSession.ChildSoundEngines = null;
            }
            catch(Exception e)
            {
                AddConsoleText("Nie udalo sie prawidlowo zamknac silnikow dzwiekowych: " + e.Message);
            }
        }


        protected void OnKeyDownHandler(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // this function is called after a keyboard signal from the user
            // first, do everything that base Page class wants to do
            base.OnKeyDown(e);
            currentSession.CurrentKey = e.Key.ToString();
            if (!Movable) return; // sometimes we don't want the player to move (e.g. when battling or selling items)
            if (IgnoreNextKey) // see GameSession.LocationEvents for explanation
            {
                IgnoreNextKey = false;
                return;
            }
            // AvailableMoves[0..3] -> WSAD
            if ((e.Key == Key.Up || e.Key == Key.W) && currentSession.AvailableMoves[0])
            {
                MovePlayer("up");
            }

            if ((e.Key == Key.Down || e.Key == Key.S) && currentSession.AvailableMoves[1])
            {
                MovePlayer("down");
            }

            if ((e.Key == Key.Left || e.Key == Key.A) && currentSession.AvailableMoves[2])
            {
                MovePlayer("left");
            }

            if ((e.Key == Key.Right || e.Key == Key.D) && currentSession.AvailableMoves[3])
            {
                MovePlayer("right");
            }

            if ((e.Key == Key.Escape))
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "savegame"; // Default file name
                dlg.DefaultExt = ".bin"; // Default file extension
                dlg.Filter = "Text documents (.bin)|*.bin"; // Filter files by extension
                Nullable<bool> result = dlg.ShowDialog();
                if (result == true)
                {
                    try
                    {
                        currentSession.SaveGame(dlg.FileName);
                        pageData.CountItems();
                        IFormatter formatter = new BinaryFormatter();
                        Stream stream = new FileStream(dlg.FileName + ".pgd", FileMode.Create, FileAccess.Write, FileShare.None);
                        formatter.Serialize(stream, pageData);
                        stream.Close();
                    }
                    catch (Exception exc)
                    {
                        AddConsoleText(exc.Message);
                    }
                }    
            }

            if (e.Key == Key.I)
            {
                AddConsoleText("Obecnie posiadasz nastepujace itemy:");
                currentSession.ListAllItemsTips();
            }

            if (e.Key == Key.U)
            {
                AddConsoleText("Obecnie znasz nastepujace umiejetnosci:");
                foreach(Skill sk in currentSession.currentPlayer.ListOfSkills)
                {
                    AddConsoleText(sk.PublicName);
                }
            }

            if (e.Key == Key.Q)
            {
                AddConsoleColorText("Informacja o quescie: ukonczono " + currentSession.CurrentlyComplete + "%", "blue");
            }

            if (e.Key == Key.P)
            {
                currentSession.EndGame();
            }
        }

    }
}
