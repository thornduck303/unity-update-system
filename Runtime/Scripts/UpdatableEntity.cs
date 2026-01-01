using UnityEngine;

namespace ThornDuck.UpdateSystem{
    /// <summary>
    /// Abstract base class for entities that are updated by the <see cref="UpdateManager"/>.
    /// </summary>
    /// <seealso cref="UpdateManager"/>
    /// <seealso cref="UpdateSystem.UpdateProfile"/>
    /// <author>Murilo M. Grosso</author>
    public abstract class UpdatableEntity : MonoBehaviour
    {
        private int lastFrameIndex;
        private bool hasStarted;
        private float deltaTimeSeconds;
        private float lastFrameStartTimeSeconds;

        [SerializeField, HideInInspector]
        private UpdateProfile updateProfile;

        /// <summary>Gets the time interval (in seconds) since the last update.</summary>
        public float DeltaTimeSeconds => deltaTimeSeconds;
        public UpdateProfile UpdateProfile => updateProfile;

        protected virtual void OnEnable()
        {
            TryRegister();
        }

        private void TryRegister()
        {
            if (UpdateManager.Instance != null)
            {
                if(updateProfile == null)
                    updateProfile = UpdateManager.Instance.DefaultUpdateProfile;
                UpdateManager.Instance.RegisterUpdatableEntity(this);
            }
            else
                UpdateManager.OnInstanceInitialized += OnUpdateManagerReady;
        }

        private void OnUpdateManagerReady()
        {
            if(updateProfile == null)
                updateProfile = UpdateManager.Instance.DefaultUpdateProfile;
            UpdateManager.Instance.RegisterUpdatableEntity(this);
            UpdateManager.OnInstanceInitialized -= OnUpdateManagerReady;
        }

        protected virtual void OnDisable()
        {
            if (UpdateManager.Instance != null)
                UpdateManager.Instance.RemoveUpdatableEntity(this);
            else
                UpdateManager.OnInstanceInitialized -= OnUpdateManagerReady;
        }

        /// <summary>
        /// Initializes timing variables used for calculating delta times.
        /// </summary>
        public void InitializeTiming(float startTimeSeconds, int startFrameIndex)
        {
            switch (updateProfile.UpdateMode)
            {
                case UpdateMode.FixedTime:
                    deltaTimeSeconds = updateProfile.TargetDeltaTimeSeconds;
                    break;
                case UpdateMode.FixedFrame:
                    deltaTimeSeconds = Time.deltaTime * updateProfile.DeltaFrame;
                    break;
                default:
                    deltaTimeSeconds = Time.deltaTime;
                    break;
            }

            lastFrameStartTimeSeconds = startTimeSeconds;
            lastFrameIndex = startFrameIndex + updateProfile.FrameOffset;
        }

        /// <summary>
        /// Ensures the entity’s <see cref="OnStartEntity"/> is called once.
        /// </summary>
        public void StartEntity()
        {
            if (hasStarted)
                return;
            hasStarted = true;
            OnStartEntity();
        }

        /// <summary>
        /// Attempts to update the entity if the current timing conditions are met.
        /// </summary>
        /// <returns>True if updated this frame, otherwise false.</returns>
        public bool TryUpdatingEntity(float currentTimeSeconds, int currentFrameIndex)
        {
            if (!ShouldUpdate(currentTimeSeconds, currentFrameIndex))
                return false;

            UpdateEntity(currentTimeSeconds, currentFrameIndex);
            return true;
        }

        /// <summary>
        /// Determines whether the entity is ready to update based on timing or frame count.
        /// </summary>
        public bool ShouldUpdate(float currentTimeSeconds, int currentFrameIndex)
        {
            switch (updateProfile.UpdateMode)
            {
                case UpdateMode.FixedTime:
                    return currentTimeSeconds - lastFrameStartTimeSeconds > updateProfile.TargetDeltaTimeSeconds;
                case UpdateMode.FixedFrame:
                    return currentFrameIndex - lastFrameIndex + 1 > updateProfile.DeltaFrame;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Performs the entity’s update step and refreshes timing state.
        /// </summary>
        public void UpdateEntity(float startTimeSeconds, int startFrameIndex)
        {
            deltaTimeSeconds = startTimeSeconds - lastFrameStartTimeSeconds;

            OnUpdateEntity();

            lastFrameIndex = startFrameIndex;
            lastFrameStartTimeSeconds = startTimeSeconds;
        }

        /// <summary>
        /// Reassigns <see cref="UpdateSystem.UpdateProfile"/>.
        /// </summary>
        public void SetUpdateProfile(UpdateProfile updateProfile)
        {
            if(updateProfile == null)
            {
                if (Debug.isDebugBuild)
                    Debug.LogWarning("[UPDATABLE ENTITY] Update profile cannot be null!");
                return;
            }

            int previousPriority = this.updateProfile.Priority;
            this.updateProfile = updateProfile;
            if(previousPriority != this.updateProfile.Priority)
                OnUpdateProfilePriorityChange();
        }

        public void OnUpdateProfilePriorityChange()
            => UpdateManager.Instance?.ReinsertUpdatableEntitySorted(this);

        /// <summary>Called once when the entity is started by the UpdateManager.</summary>
        protected virtual void OnStartEntity() { }
        /// <summary>Called each time the entity is updated by the UpdateManager.</summary>
        protected virtual void OnUpdateEntity() { }
    }
}