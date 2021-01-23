﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Media;
using System.IO;

namespace Game.Sound.Players
{
    class BasicSoundPlayer
    {
        protected MediaPlayer mediaPlayer;
        protected bool playLooped;
        protected bool waiting;
        protected List<Sound> SoundList;

        public double Volume
        {
            get { return mediaPlayer.Volume; }
            set { mediaPlayer.Volume = value; }
        }
        public BasicSoundPlayer()
        {
            mediaPlayer = new MediaPlayer();
            playLooped = false;
            waiting = false;
            SoundList = new List<Sound>();
            mediaPlayer.Volume = 1;

            // allow to loop audio
            mediaPlayer.MediaEnded += new EventHandler(Player_Ended);
        }

        public void Open(Sound sound)
        {
            if(sound != null && File.Exists(sound.FilePath.AbsolutePath))
            {
                mediaPlayer.Open(sound.FilePath);
            }
        }

        public void Play()
        {
            if(mediaPlayer.Source != null)
            {
                mediaPlayer.IsMuted = false;
                mediaPlayer.Play();
            }
        }

        public void Pause()
        {
            mediaPlayer.Pause();
        }

        public void Stop()
        {
            double tempVolume = mediaPlayer.Volume;
            int msTime = 10;
            double sleepFactor = (double)msTime / 1000;

            while (mediaPlayer.Volume > 0)
            {
                mediaPlayer.Volume -= sleepFactor;
                // wait to decrease volume sound slowly
                System.Threading.Thread.Sleep(msTime);
            }

            mediaPlayer.IsMuted = true;
            mediaPlayer.Stop();
            mediaPlayer.Volume = tempVolume;
        }

        public void ForceStop()
        {
            mediaPlayer.IsMuted = true;
            mediaPlayer.Stop();
            mediaPlayer.IsMuted = false;
        }

        public void PlayLooped()
        {
            playLooped = true;
            Play();
        }

        public void WaitAndPlay(Sound sound)
        {
            if (sound == null) return;
            SoundList.Add(sound);

            if (mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan && mediaPlayer.NaturalDuration.TimeSpan > mediaPlayer.Position)
            {
                waiting = true;
            }
            else
            {
                HandleWaitingPlayer();
            }
        }

        private void Player_Ended(object sender, EventArgs e)
        {
            if (playLooped)
            {
                //Set the music back to the beginning
                mediaPlayer.Position = TimeSpan.Zero;
                //Play the music
                mediaPlayer.Play();
            }
            if (waiting && SoundList.Count > 0)
            {
                HandleWaitingPlayer();
            }
        }

        private void HandleWaitingPlayer()
        {
            SoundList.RemoveAll(x => x == null);
            if (SoundList.Count == 0)
            {
                waiting = false;
                return;
            }

            Sound temp = SoundList.First();
            SoundList.RemoveAt(0);
            Open(temp);
            Play();
            if (SoundList.Count == 0)
                waiting = false;
        }

    }
}
