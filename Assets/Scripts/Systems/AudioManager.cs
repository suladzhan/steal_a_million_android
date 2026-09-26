using System;
using System.Collections.Generic;
using StealAMillion.Core;
using UnityEngine;

namespace StealAMillion
{
    public enum SoundCue { Click, Safe, Risk, Win, Loss, Jackpot, Level, Victory, GameOver, Cash, Police }

    public sealed class AudioManager : MonoBehaviour
    {
        private readonly Dictionary<SoundCue, AudioClip> clips = new Dictionary<SoundCue, AudioClip>();
        private AudioSource effects;
        private AudioSource music;
        private SaveData settings;
        private float lastCash=-10;private int cashPitch;private bool generatedMusic;

        public void Initialize(SaveData data)
        {
            settings = data;
            effects = gameObject.AddComponent<AudioSource>();
            effects.playOnAwake = false;
            effects.volume = .28f;
            music = gameObject.AddComponent<AudioSource>();
            music.playOnAwake = false;
            music.loop = true;
            music.volume = .14f;
            clips[SoundCue.Click] = Tone("click", new[] { 700f }, .04f);
            clips[SoundCue.Cash] = Tone("cash", new[] { 1100f, 1400f }, .04f);
            clips[SoundCue.Police] = Tone("police", new[] { 680f, 420f, 680f }, .1f);
            clips[SoundCue.Safe] = Tone("safe", new[] { 392f, 523f }, .11f);
            clips[SoundCue.Risk] = Tone("risk", new[] { 220f, 247f, 262f }, .14f);
            clips[SoundCue.Win] = Tone("win", new[] { 523f, 659f, 784f }, .10f);
            clips[SoundCue.Loss] = Tone("loss", new[] { 294f, 220f, 147f }, .12f);
            clips[SoundCue.Jackpot] = Tone("jackpot", new[] { 523f, 659f, 784f, 1047f }, .13f);
            clips[SoundCue.Level] = Tone("level", new[] { 659f, 784f }, .12f);
            clips[SoundCue.Victory] = Tone("victory", new[] { 523f, 659f, 784f, 1047f, 784f, 1047f }, .17f);
            clips[SoundCue.GameOver] = Tone("game over", new[] { 262f, 220f, 175f, 131f }, .16f);
            music.clip=Resources.Load<AudioClip>("Runner/Audio/music-v002");generatedMusic=music.clip==null;
            if(generatedMusic)music.clip = Tone("night shift", new[] { 130.81f, 164.81f, 196f, 164.81f, 110f, 146.83f, 174.61f, 146.83f }, .75f);
            Refresh();
        }

        public void Refresh()
        {
            effects.mute = !settings.soundEnabled;
            if (settings.musicEnabled && !music.isPlaying) music.Play();
            if (!settings.musicEnabled) music.Stop();
        }

        public void SetSettings(SaveData data) { settings = data; Refresh(); }
        public void Duck(bool value) { if (music != null) music.volume = value ? .035f : .14f; }

        public void Play(SoundCue cue)
        {
            if(cue==SoundCue.Cash){if(Time.unscaledTime-lastCash<.055f)return;lastCash=Time.unscaledTime;effects.pitch=.96f+(cashPitch++%4)*.025f;}else effects.pitch=1;
            AudioClip clip;
            if (settings != null && settings.soundEnabled && clips.TryGetValue(cue, out clip) && clip != null)
                effects.PlayOneShot(clip);
        }

        private static AudioClip Tone(string name, float[] notes, float noteDuration)
        {
            const int rate = 22050;
            int noteSamples = Mathf.RoundToInt(rate * noteDuration);
            var samples = new float[noteSamples * notes.Length];
            for (int n = 0; n < notes.Length; n++)
                for (int i = 0; i < noteSamples; i++)
                {
                    float t = (float)i / noteSamples;
                    float envelope = Mathf.Min(1, t * 30) * Mathf.Pow(1 - t, 2);
                    double phase = 2 * Math.PI * notes[n] * i / rate;
                    samples[n * noteSamples + i] = (float)(Math.Sin(phase) + .15 * Math.Sin(phase * 2)) * envelope * .6f;
                }
            var clip = AudioClip.Create(name, samples.Length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnDestroy()
        {
            foreach (var clip in clips.Values) if (clip != null) Destroy(clip);
            if (generatedMusic && music != null && music.clip != null) Destroy(music.clip);
        }
    }
}
