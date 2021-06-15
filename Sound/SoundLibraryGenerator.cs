using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Sound
{
    /// <summary>
    /// Contains constant strings of prepared music and sound names. 
    /// </summary>
    public static class SoundNames
    {
        public const string BACKGROUND_MUSIC = "BackgroundMusic";
        public const string BACKGROUND_MUSIC_MENU = "BackgroundMusicMenu";
        public const string BACKGROUND_MUSIC_GAME = "BackgroundMusicGame";
        public const string BACKGROUND_MUSIC_BATTLE = "BackgroundMusicBattle";

        public const string MOUSE_HOVER_MENU = "MouseHoverMenu";
        public const string MOUSE_CLICK_MENU = "MouseClickMenu";

        public const string MOUSE_HOVER_GAME = "MouseHoverGame";
        public const string MOUSE_CLICK_GAME_1 = "MouseClickGame_1";
        public const string MOUSE_CLICK_GAME_2 = "MouseClickGame_2";

        public const string PLAYER_DEATH = "PlayerDeath";
        public const string PLAYER_WIN = "PlayerWin";

    }
    /// <summary>
    /// Generates list (library) of game sounds. 
    /// </summary>
    internal static class SoundLibraryGenerator
    {
        /// <summary>
        /// Contains list of all sounds linked to monsters, items, player, background music etc.
        /// </summary>
        /// <returns>List of sounds used by game app.</returns>
        internal static List<Sound> CreateLibrary()
        {
            List<Sound> soundLibrary = new List<Sound>();

            // Add here list of sounds by pattern
            // soundLibrary.Add(new Sound(null, NAME, "FILE_NAME.mp3", SoundContext, SoundType, Resource1.RESOURCE_NAME));
            // mp3 files have to be added to Sound/Assets, then ALSO dragged from there to Resources window

            // BACKGROUND
            //soundLibrary.Add(new Sound(null, SoundNames.BACKGROUND_MUSIC_GAME, "background_game.mp3", SoundContext.GamePage, SoundType.Background, Resource1.background_game));

            // MOUSE
            soundLibrary.Add(new Sound(null, SoundNames.MOUSE_CLICK_MENU, string.Concat(SoundNames.MOUSE_CLICK_MENU, ".mp3"), SoundContext.MenuPage, SoundType.MouseSound, Resource1.mouse_click));
            soundLibrary.Add(new Sound(null, SoundNames.MOUSE_HOVER_MENU, string.Concat(SoundNames.MOUSE_HOVER_MENU, ".mp3"), SoundContext.MenuPage, SoundType.MouseSound, Resource1.mouse_hover));
            soundLibrary.Add(new Sound(null, SoundNames.MOUSE_CLICK_GAME_1, string.Concat(SoundNames.MOUSE_CLICK_GAME_1, ".mp3"), SoundContext.MenuPage, SoundType.MouseSound, Resource1.mouse_drop));
            soundLibrary.Add(new Sound(null, SoundNames.MOUSE_CLICK_GAME_2, string.Concat(SoundNames.MOUSE_CLICK_GAME_2, ".mp3"), SoundContext.GamePage, SoundType.MouseSound, Resource1.mouse_click));

            // ITEMS
            string item = Engine.RequiredItem.Axe.ToString();
            soundLibrary.Add(new Sound(null, item, string.Concat(item, "_item.mp3"), SoundContext.Battle, SoundType.BattleRequiredItem, Resource1.item_axe));
            item = Engine.RequiredItem.Spear.ToString();
            soundLibrary.Add(new Sound(null, item, string.Concat(item, "_item.mp3"), SoundContext.Battle, SoundType.BattleRequiredItem, Resource1.item_spear));
            item = Engine.RequiredItem.Staff.ToString();
            soundLibrary.Add(new Sound(null, item, string.Concat(item, "_item.mp3"), SoundContext.Battle, SoundType.BattleRequiredItem, Resource1.item_staff));
            item = Engine.RequiredItem.Sword.ToString();
            soundLibrary.Add(new Sound(null, item, string.Concat(item, "_item.mp3"), SoundContext.Battle, SoundType.BattleRequiredItem, Resource1.item_sword));

            // PLAYER 
            soundLibrary.Add(new Sound(null, SoundNames.PLAYER_DEATH, "player_death.mp3", SoundContext.Battle, SoundType.Player, Resource1.player_death));
            // soundLibrary.Add(new Sound(null, SoundNames.PLAYER_WIN, "PlayerWin.mp3", SoundContext.Battle, SoundType.Player, Resource1.player_win));

            // MONSTER
            List<string> monsters = new List<string>();
            // Rat / Evolved Rat
            monsters.Add("monster0001");
            monsters.Add("monster0002");
            foreach (var m in monsters)
            {
                soundLibrary.Add(new Sound(null, m, "rat_init.mp3", SoundContext.Battle, SoundType.MonsterInit, Resource1.rat_init));
                soundLibrary.Add(new Sound(null, m, "rat_death.mp3", SoundContext.Battle, SoundType.MonsterDeath, Resource1.rat_death));
            }
            monsters.Clear();
            // Spider / Tarantula
            monsters.Add("monster0003");
            monsters.Add("monster0004");
            foreach (var m in monsters)
            {
                soundLibrary.Add(new Sound(null, m, "spider_init.mp3", SoundContext.Battle, SoundType.MonsterInit, Resource1.spider_init));
                soundLibrary.Add(new Sound(null, m, "spider_death.mp3", SoundContext.Battle, SoundType.MonsterDeath, Resource1.spider_death));
            }
            monsters.Clear();
            // Vampire Knight
            soundLibrary.Add(new Sound(null, "monster0006", "vampire_knight_init.mp3", SoundContext.Battle, SoundType.MonsterInit, Resource1.vampire_knight_init));
            soundLibrary.Add(new Sound(null, "monster0006", "vampire_knight_death.mp3", SoundContext.Battle, SoundType.MonsterDeath, Resource1.vampire_knight_death));

            // Bat
            soundLibrary.Add(new Sound(null, "monster0005", "bat_init.mp3", SoundContext.Battle, SoundType.MonsterInit, Resource1.bat_init));
            soundLibrary.Add(new Sound(null, "monster0005", "bat_death.mp3", SoundContext.Battle, SoundType.MonsterDeath, Resource1.bat_death));

            // BACKGROUND MUSIC
            // Battle Music
            soundLibrary.Add(new Sound(null, SoundNames.BACKGROUND_MUSIC_BATTLE, "backround_music_cld.mp3", SoundContext.Battle, SoundType.Background, Resource1.background_music_cld));


            return soundLibrary;
        }
    }
}
