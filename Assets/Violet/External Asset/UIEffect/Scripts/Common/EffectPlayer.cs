﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Coffee.UIExtensions
{
	/// <summary>
	///     Effect player.
	/// </summary>
	[Serializable]
    public class EffectPlayer
    {
        private static List<Action> s_UpdateActions;

        //################################
        // Public Members.
        //################################
        /// <summary>
        ///     Gets or sets a value indicating whether is playing.
        /// </summary>
        [Tooltip("Playing.")] public bool play;

        /// <summary>
        ///     Gets or sets the delay before looping.
        /// </summary>
        [Tooltip("Initial play delay.")] [Range(0f, 10f)]
        public float initialPlayDelay;

        /// <summary>
        ///     Gets or sets a value indicating whether can loop.
        /// </summary>
        [Tooltip("Loop.")] public bool loop;

        /// <summary>
        ///     Gets or sets the duration.
        /// </summary>
        [Tooltip("Duration.")] [Range(0.01f, 10f)]
        public float duration = 1;

        /// <summary>
        ///     Gets or sets the delay before looping.
        /// </summary>
        [Tooltip("Delay before looping.")] [Range(0f, 10f)]
        public float loopDelay;

        /// <summary>
        ///     Gets or sets the update mode.
        /// </summary>
        [Tooltip("Update mode")] public AnimatorUpdateMode updateMode = AnimatorUpdateMode.Normal;

        private Action<float> _callback;

        //################################
        // Private Members.
        //################################
        private float _time;

        /// <summary>
        ///     Register player.
        /// </summary>
        public void OnEnable(Action<float> callback = null)
        {
            if (s_UpdateActions == null)
            {
                s_UpdateActions = new List<Action>();
                Canvas.willRenderCanvases += () =>
                {
                    var count = s_UpdateActions.Count;
                    for (var i = 0; i < count; i++) s_UpdateActions[i].Invoke();
                };
            }

            s_UpdateActions.Add(OnWillRenderCanvases);

            if (play)
                _time = -initialPlayDelay;
            else
                _time = 0;
            _callback = callback;
        }

        /// <summary>
        ///     Unregister player.
        /// </summary>
        public void OnDisable()
        {
            _callback = null;
            s_UpdateActions.Remove(OnWillRenderCanvases);
        }

        /// <summary>
        ///     Start playing.
        /// </summary>
        public void Play(Action<float> callback = null)
        {
            _time = 0;
            play = true;
            if (callback != null) _callback = callback;
        }

        /// <summary>
        ///     Stop playing.
        /// </summary>
        public void Stop()
        {
            play = false;
        }

        private void OnWillRenderCanvases()
        {
            if (!play || !Application.isPlaying || _callback == null) return;

            _time += updateMode == AnimatorUpdateMode.UnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;
            var current = _time / duration;

            if (duration <= _time)
            {
                play = loop;
                _time = loop ? -loopDelay : 0;
            }

            _callback(current);
        }
    }
}