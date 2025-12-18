# Update System
Instance update handler for Unity.

Custom update manager for Unity that replaces `MonoBehaviour.Update` with a centralized pipeline. 
Handles execution order based on priority, fixed time or fixed frame updates, and per-entity timing via ScriptableObject update profiles.
