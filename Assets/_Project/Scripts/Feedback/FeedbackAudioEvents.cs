using System;
using UnityEngine;

namespace TapKnockout.Feedback
{
    public readonly struct FeedbackAudioEventArgs
    {
        public FeedbackAudioEventArgs(
            FeedbackAudioEventType eventType,
            Vector3 position,
            GameObject source = null,
            GameObject target = null,
            float intensity = 1f)
        {
            EventType = eventType;
            Position = position;
            Source = source;
            Target = target;
            Intensity = Mathf.Max(0f, intensity);
        }

        public FeedbackAudioEventType EventType { get; }
        public Vector3 Position { get; }
        public GameObject Source { get; }
        public GameObject Target { get; }
        public float Intensity { get; }
    }

    public static class FeedbackAudioEvents
    {
        public static event Action<FeedbackAudioEventArgs> OnFeedbackAudioRequested;

        public static void RaiseFeedbackAudioRequested(FeedbackAudioEventArgs eventArgs)
        {
            OnFeedbackAudioRequested?.Invoke(eventArgs);
        }
    }
}
