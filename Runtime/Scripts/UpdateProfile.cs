using UnityEngine;

namespace ThornDuck.UpdateSystem{
    /// <summary>
    /// Information on how to <see cref="UpdateManager"/> should update <see cref="UpdatableEntity"/>.
    /// </summary>
    /// <seealso cref="UpdateManager"/>
    /// <seealso cref="UpdatableEntity"/>
    /// <seealso cref="UpdateSystem.UpdateMode"/>
    /// <author>Murilo M. Grosso</author>
    [CreateAssetMenu(fileName = "new_updateprofile", menuName = "ThornDuck/Update System/Update Profile")]
    public class UpdateProfile : ScriptableObject
    {
        public const int DEFAULT_DELTA_FRAME = 1;
        public const float MAX_DELTA_TIME_SECONDS = 1000f;
        public const float MIN_DELTA_TIME_SECONDS = 0.0001f;
        public const float DEFAULT_DELTA_TIME_SECONDS = 1f / 60f;

        [SerializeField, HideInInspector] private int priority;
        [SerializeField, HideInInspector] private int frameOffset;
        [SerializeField, HideInInspector] private int deltaFrame = UpdateSettings.DEFAULT_DELTA_FRAME;
        [SerializeField, HideInInspector] private float targetDeltaTimeSeconds = UpdateSettings.DEFAULT_DELTA_TIME_SECONDS;
        [SerializeField, HideInInspector] private UpdateMode updateMode;

        public int Priority => priority;
        /// <summary>Gets the frame interval between update calls.</summary>
        /// <remarks>Only used for <see cref="UpdateMode.FixedFrame"/>.</remarks>
        public int DeltaFrame => deltaFrame;
        /// <summary>Gets the offset used to stagger updates between frames.</summary>
        /// <remarks>Only used for <see cref="UpdateMode.FixedFrame"/>.</remarks>
        public int FrameOffset => frameOffset;
        /// <remarks>Only used for <see cref="UpdateMode.FixedTime"/>.</remarks>
        public float TargetDeltaTimeSeconds => targetDeltaTimeSeconds;
        public UpdateMode UpdateMode => updateMode;
    }

    /// <summary>
    /// Describes how <see cref="UpdatableEntity"/> managed by the <see cref="UpdateManager"/> should be updated.
    /// </summary>
    public enum UpdateMode
    {
        /// <summary>
        /// Updates every frame.
        /// </summary>
        Default,
        /// <summary>
        /// Updates at a fixed time step, regardless of frame rate variability.
        /// </summary>
        FixedTime,
        /// <summary>
        /// Updates once every specified number of frames.
        /// </summary>
        FixedFrame
    }

    /// <summary>
    /// Holds constant values for the Update System.
    /// </summary>
    public static class UpdateSettings
    {
        public const int DEFAULT_DELTA_FRAME = 1;
        public const float MAX_DELTA_TIME_SECONDS = 1000f;
        public const float MIN_DELTA_TIME_SECONDS = 0.0001f;
        public const float DEFAULT_DELTA_TIME_SECONDS = 1f / 60f;
    }
}