using System;
using System.Collections.Generic;
using Neko.Sim.Maids;
using Neko.Sim.Shift;
using UnityEngine;

namespace Neko.Game.Audio
{
    /// <summary>
    /// Plays the sound effects, including the one each maid answers with.
    /// </summary>
    /// <remarks>
    /// A pool of sources with a little pitch jitter on every play. Without the jitter a busy
    /// café machine-guns the same sample a dozen times a second and the whole mix turns to
    /// gravel; it is the cheapest fix in audio and the one most often left out.
    /// </remarks>
    public sealed class AudioDirector : MonoBehaviour
    {
        private const int Voices = 10;
        public const string ResourceFolder = "Audio/";

        private readonly Dictionary<SoundId, AudioClip> _clips = new Dictionary<SoundId, AudioClip>();
        private readonly List<AudioSource> _sources = new List<AudioSource>();
        private int _next;

        public float Volume { get; set; } = 0.8f;

        public bool Muted { get; set; }

        public void Load()
        {
            foreach (SoundId id in Enum.GetValues(typeof(SoundId)))
            {
                var clip = Resources.Load<AudioClip>(ResourceFolder + FileNameOf(id));
                if (clip != null) _clips[id] = clip;
                else Debug.LogWarning("Som faltando: " + FileNameOf(id));
            }

            for (var i = 0; i < Voices; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                _sources.Add(source);
            }
        }

        /// <summary>Lower-case with underscores: the name the synthesis script writes.</summary>
        public static string FileNameOf(SoundId id)
        {
            var name = id.ToString();
            var text = new System.Text.StringBuilder();

            for (var i = 0; i < name.Length; i++)
            {
                if (char.IsUpper(name[i]) && i > 0) text.Append('_');
                text.Append(char.ToLowerInvariant(name[i]));
            }

            return text.ToString();
        }

        public bool Has(SoundId id) => _clips.ContainsKey(id);

        public void Play(SoundId id, float volume = 1f, float pitchJitter = 0.045f)
        {
            if (Muted || !_clips.TryGetValue(id, out var clip)) return;

            var source = _sources[_next];
            _next = (_next + 1) % _sources.Count;

            source.pitch = 1f + UnityEngine.Random.Range(-pitchJitter, pitchJitter);
            source.PlayOneShot(clip, Mathf.Clamp01(volume) * Volume);
        }

        /// <summary>Her own voice. The whole point of having nine of them.</summary>
        public void Nya(Archetype archetype) => Play(NyaFor(archetype), 0.7f, 0.03f);

        public static SoundId NyaFor(Archetype archetype)
        {
            switch (archetype)
            {
                case Archetype.Genki: return SoundId.NyaGenki;
                case Archetype.Kuudere: return SoundId.NyaKuudere;
                case Archetype.Tsundere: return SoundId.NyaTsundere;
                case Archetype.Dojikko: return SoundId.NyaDojikko;
                case Archetype.Ojousama: return SoundId.NyaOjousama;
                case Archetype.Oneesan: return SoundId.NyaOneesan;
                case Archetype.Idol: return SoundId.NyaIdol;
                case Archetype.Gourmand: return SoundId.NyaGourmand;
                default: return SoundId.NyaMukuchi;
            }
        }

        /// <summary>
        /// What each thing that happens sounds like.
        /// </summary>
        /// <remarks>
        /// Exhaustive over the event enum, and a test checks that. A simulation event with no
        /// sound is a thing the player is told about only if they happen to be looking at it.
        /// </remarks>
        public void Handle(ShiftEvent e)
        {
            switch (e.Kind)
            {
                case ShiftEventKind.CustomerArrived:
                    Play(SoundId.DoorBell, 0.55f);
                    break;

                case ShiftEventKind.TurnedAway:
                    Play(SoundId.AngryPuff, 0.5f);
                    break;

                case ShiftEventKind.Seated:
                    Play(SoundId.OrderPop, 0.4f);
                    break;

                case ShiftEventKind.Ordered:
                    Play(SoundId.OrderPop, 0.7f);
                    break;

                case ShiftEventKind.TicketQueued:
                    Play(SoundId.TicketSlap, 0.45f);
                    break;

                case ShiftEventKind.CookStarted:
                    Play(SoundId.Sizzle, 0.5f);
                    break;

                case ShiftEventKind.Fumbled:
                    Play(SoundId.Drop, 0.6f);
                    break;

                case ShiftEventKind.PlateReady:
                    Play(SoundId.PlateChime, 0.65f);
                    break;

                case ShiftEventKind.PlateDropped:
                    Play(SoundId.Drop, 0.9f);
                    break;

                case ShiftEventKind.Served:
                    Play(SoundId.PlateChime, 0.5f);
                    break;

                case ShiftEventKind.Paid:
                    Play(SoundId.Register, 0.8f);
                    Play(SoundId.Coin, 0.5f);
                    break;

                case ShiftEventKind.LeftHappy:
                    Play(SoundId.Heart, 0.5f);
                    break;

                case ShiftEventKind.LeftAngry:
                    Play(SoundId.AngryPuff, 0.85f);
                    break;

                case ShiftEventKind.Bussed:
                    Play(SoundId.UiTick, 0.35f);
                    break;

                case ShiftEventKind.MoodEmpty:
                    Play(SoundId.AngryPuff, 0.35f);
                    break;

                case ShiftEventKind.SecondWind:
                case ShiftEventKind.BackFromBreak:
                    Play(SoundId.LevelUp, 0.4f);
                    break;

                case ShiftEventKind.WrongOrder:
                    Play(SoundId.AngryPuff, 0.7f);
                    Play(SoundId.Drop, 0.4f);
                    break;

                case ShiftEventKind.EncoreUsed:
                    Play(SoundId.LevelUp, 0.9f);
                    break;

                case ShiftEventKind.ClosingTime:
                    Play(SoundId.DoorBell, 0.9f);
                    break;

                case ShiftEventKind.ShiftEnded:
                    Play(SoundId.UiConfirm, 0.8f);
                    break;
            }
        }
    }
}
