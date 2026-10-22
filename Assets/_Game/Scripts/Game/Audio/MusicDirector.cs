using System;
using System.Collections.Generic;
using Neko.Game.Core;
using UnityEngine;

namespace Neko.Game.Audio
{
    /// <summary>
    /// Two sources, crossfading. The music reports the state of the game.
    /// </summary>
    /// <remarks>
    /// The shift bed changes when the room passes seventy per cent full or the clock drops
    /// under twenty seconds, which means a player can hear that they are behind before they
    /// have finished reading the table they are standing at.
    /// </remarks>
    public sealed class MusicDirector : MonoBehaviour
    {
        public const string ResourceFolder = "Audio/music_";

        private readonly Dictionary<MusicBed, AudioClip> _beds = new Dictionary<MusicBed, AudioClip>();

        private AudioSource _a;
        private AudioSource _b;
        private AudioSource _active;
        private MusicBed? _playing;
        private float _fade;
        private float _fadeSeconds = 1f;
        private bool _ducked;

        public float Volume { get; set; } = 0.45f;

        public bool Muted
        {
            get => _muted;
            set
            {
                _muted = value;
                if (value) Stop();
                else if (_playing.HasValue) Play(_playing.Value, 0.2f);
            }
        }

        private bool _muted;

        public MusicBed? Playing => _playing;

        public void Load()
        {
            foreach (MusicBed bed in Enum.GetValues(typeof(MusicBed)))
            {
                var clip = Resources.Load<AudioClip>(ResourceFolder + bed.ToString().ToLowerInvariant());
                if (clip != null) _beds[bed] = clip;
                else Debug.LogWarning("Trilha faltando: " + bed);
            }

            _a = Source();
            _b = Source();
            _active = _a;
        }

        public bool Has(MusicBed bed) => _beds.ContainsKey(bed);

        public void Play(MusicBed bed, float crossfadeSeconds = 1.5f)
        {
            if (_muted || _playing == bed) return;
            if (!_beds.TryGetValue(bed, out var clip)) return;

            _playing = bed;
            var next = _active == _a ? _b : _a;

            next.clip = clip;
            next.volume = 0f;
            next.Play();

            _active = next;
            _fade = 0f;
            _fadeSeconds = Mathf.Max(0.05f, crossfadeSeconds);
        }

        public void Stop()
        {
            _a?.Stop();
            _b?.Stop();
            _playing = null;
        }

        /// <summary>Pulls the music down under a dialog, and lets it back up after.</summary>
        public void Duck(bool ducked) => _ducked = ducked;

        private void Update()
        {
            if (_a == null) return;

            _fade = Mathf.Min(1f, _fade + Clocks.Ui / _fadeSeconds);
            var target = Volume * (_ducked ? 0.3f : 1f);

            var rising = _active;
            var falling = _active == _a ? _b : _a;

            rising.volume = target * _fade;
            falling.volume = target * (1f - _fade);

            if (_fade >= 1f && falling.isPlaying) falling.Stop();
        }

        private AudioSource Source()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0f;
            return source;
        }
    }
}
