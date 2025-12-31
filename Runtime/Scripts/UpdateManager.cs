using System;
using UnityEngine;
using System.Collections.Generic;

namespace ThornDuck.UpdateSystem{
    /// <summary>
    /// Manager for updating <see cref="UpdatableEntity"/> instances.
    /// Handles execution order.
    /// </summary>
    /// <seealso cref="UpdatableEntity"/>
    /// <seealso cref="UpdateProfile"/>
    /// <author>Murilo M. Grosso</author>
    public class UpdateManager : MonoBehaviour
    {
        public static UpdateManager Instance { get; private set; }

        public static event Action OnInstanceInitialized;

        [SerializeField, HideInInspector] 
        private bool isRunning = true;

        private int runningFrameCount;
        private float runningTimeSeconds;
        private bool hasStarted;
        private bool canModifyUpdatableEntities;

        private readonly List<Action> pendingModifications = new();
        private readonly List<UpdatableEntity> updatableEntities = new();

        [SerializeField]
        public UpdateProfile DefaultUpdateProfile;

        private static readonly IComparer<UpdatableEntity> UpdatableEntityPriorityComparer =
            new UpdatableEntityPriorityComparer();

        public void Resume() => isRunning = true;
        public void Pause() => isRunning = false;
        public bool IsRunning => isRunning;
        public int RunningFrameCount => runningFrameCount;
        public float RunningTimeSeconds => runningTimeSeconds;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            OnInstanceInitialized?.Invoke();
        }

        private void Start()
        {
            hasStarted = true;

            if(Time.timeSinceLevelLoad > 0)
            {   // Handles belatedly instanciated Update Manager.
                UpdatableEntity[] updatableEntitiesInScene = 
                    FindObjectsByType<UpdatableEntity>(FindObjectsSortMode.None);
                int updatableEntitiesInSceneLength = updatableEntitiesInScene.Length;
                for(int i = 0; i < updatableEntitiesInSceneLength; i++)
                    RegisterUpdatableEntity(updatableEntitiesInScene[i]);
            }

            for (int i = 0; i < updatableEntities.Count; i++)
                updatableEntities[i].StartEntity();
        }

        private void Update()
        {
            if (isRunning)
            {
                canModifyUpdatableEntities = false;

                int updatableEntitiesCount = updatableEntities.Count;
                for (int i = 0; i < updatableEntitiesCount; i++)
                    updatableEntities[i].TryUpdatingEntity(runningTimeSeconds, runningFrameCount);

                canModifyUpdatableEntities = true;

                for (int i = 0; i < pendingModifications.Count; i++)
                    pendingModifications[i]();
                pendingModifications.Clear();

                runningFrameCount++;
                runningTimeSeconds += Time.deltaTime;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                OnInstanceInitialized = null;
            }
        }

        /// <summary>
        /// Registers an <see cref="UpdatableEntity"/> with the manager,
        /// inserting it in the correct order.
        /// </summary>
        public void RegisterUpdatableEntity(UpdatableEntity updatableEntity)
        {
            if (!canModifyUpdatableEntities)
            {
                pendingModifications.Add(() => RegisterUpdatableEntity(updatableEntity));
                return;
            }

            if (updatableEntity == null)
                return;
            if (updatableEntities.Contains(updatableEntity))
                return;

            InsertUpdatableEntitySorted(updatableEntity);

            updatableEntity.InitializeTiming(runningTimeSeconds, runningFrameCount);
            if (hasStarted)
                updatableEntity.StartEntity();
        }

        /// <summary>
        /// Removes a registered <see cref="UpdatableEntity"/> from the manager.
        /// </summary>
        /// <param name="updatableEntity">The entity to remove.</param>
        public void RemoveUpdatableEntity(UpdatableEntity updatableEntity)
        {
            if (!canModifyUpdatableEntities)
            {
                pendingModifications.Add(() => RemoveUpdatableEntity(updatableEntity));
                return;
            }

            if (updatableEntity != null)
                updatableEntities.Remove(updatableEntity);
        }

        /// <summary>
        /// Repositions a registered <see cref="UpdatableEntity"/> in the sorted list
        /// to maintain order.
        /// </summary>
        /// <param name="updatableEntity">The entity to resort.</param>
        public void ReinsertUpdatableEntitySorted(UpdatableEntity updatableEntity)
        {
            if (!canModifyUpdatableEntities)
            {
                pendingModifications.Add(() => ReinsertUpdatableEntitySorted(updatableEntity));
                return;
            }

            if (updatableEntity == null)
                return;
            if (!updatableEntities.Remove(updatableEntity))
                return;

            InsertUpdatableEntitySorted(updatableEntity);
        }

        private void InsertUpdatableEntitySorted(UpdatableEntity updatableEntity)
        {
            /* If the element is not found in BinarySearch, it returns bitwise complement 
            (negative integer) of the index where the element should be inserted. */
            int index = updatableEntities.BinarySearch(updatableEntity, UpdatableEntityPriorityComparer);
            if (index < 0)
                index = ~index;
            updatableEntities.Insert(index, updatableEntity);
        }

        /// <summary>
        /// Logs detailed information about the current state of the Update Manager.
        /// </summary>
        /// <remarks>
        /// This method is intended for debugging use only.
        /// </remarks>
        public void LogCurrentState()
        {
            if (!Debug.isDebugBuild)
                return;

            System.Text.StringBuilder log = new();

            log.AppendLine($"[UPDATE MANAGER] LOG ({Time.realtimeSinceStartup:F3}):");
            log.AppendLine();
            log.AppendLine($"Currently {(isRunning ? "running" : "paused")}.");
            log.AppendLine($"Running for {runningTimeSeconds:F3}s and {runningFrameCount} frames.");
            log.AppendLine();
            log.AppendLine("Updatable entities queue:");

            int updatableEntitiesCount = updatableEntities.Count;
            for (int i = 0; i < updatableEntitiesCount; i++)
            {
                UpdatableEntity updatableEntity = updatableEntities[i];
                log.Append($"\t{updatableEntity.name}\t\t\t[{updatableEntity.UpdateProfile.Priority}]\t");

                switch (updatableEntity.UpdateProfile.UpdateMode)
                {
                    case UpdateMode.FixedTime:
                        log.AppendLine($"{1f / updatableEntity.UpdateProfile.TargetDeltaTimeSeconds} FPS");
                        break;
                    case UpdateMode.FixedFrame:
                        log.Append($"{updatableEntity.UpdateProfile.DeltaFrame} frame(s)");
                        log.AppendLine($"+ {updatableEntity.UpdateProfile.FrameOffset}");
                        break;
                    default:
                        log.AppendLine();
                        break;
                }
            }

            Debug.Log(log);
        }
    }

    /// <summary>
    /// Compares <see cref="UpdatableEntity"/> instances by descending priority.
    /// Higher priority entities come first.
    /// </summary>
    class UpdatableEntityPriorityComparer : IComparer<UpdatableEntity>
    {
        public int Compare(UpdatableEntity x, UpdatableEntity y)
            => y.UpdateProfile.Priority.CompareTo(x.UpdateProfile.Priority);
    }
}

