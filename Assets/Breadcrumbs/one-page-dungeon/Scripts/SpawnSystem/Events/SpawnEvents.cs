using Breadcrumbs.EventSystem;
using UnityEngine;

namespace Breadcrumbs.SpawnSystem.Events
{
    /// <summary>
    /// Event dispatched when an object is spawned
    /// </summary>
    public class SpawnEvent : IEvent
    {
        public GameObject SpawnedObject { get; private set; }
        public SpawnPoint SpawnPoint { get; private set; }
        
        public SpawnEvent(GameObject spawnedObject, SpawnPoint spawnPoint)
        {
            SpawnedObject = spawnedObject;
            SpawnPoint = spawnPoint;
        }
    }

    /// <summary>
    /// Event dispatched when an object is despawned
    /// </summary>
    public class DespawnEvent : IEvent
    {
        public GameObject DespawnedObject { get; private set; }
        
        public DespawnEvent(GameObject despawnedObject)
        {
            DespawnedObject = despawnedObject;
        }
    }

    /// <summary>
    /// Event dispatched when the game difficulty changes
    /// </summary>
    public class DifficultyChangedEvent : IEvent
    {
        public DifficultyLevel NewDifficulty { get; private set; }
        public DifficultySettings Settings { get; private set; }
        
        public DifficultyChangedEvent(DifficultyLevel newDifficulty, DifficultySettings settings)
        {
            NewDifficulty = newDifficulty;
            Settings = settings;
        }
    }

    /// <summary>
    /// Event dispatched when the game starts
    /// </summary>
    public class GameStartEvent : IEvent
    {
        public GameStartEvent()
        {
        }
    }

    /// <summary>
    /// Event dispatched when a spawn group is activated
    /// </summary>
    public class SpawnGroupActivatedEvent : IEvent
    {
        public string GroupId { get; private set; }
        
        public SpawnGroupActivatedEvent(string groupId)
        {
            GroupId = groupId;
        }
    }

    /// <summary>
    /// Event dispatched when a spawn group is deactivated
    /// </summary>
    public class SpawnGroupDeactivatedEvent : IEvent
    {
        public string GroupId { get; private set; }
        
        public SpawnGroupDeactivatedEvent(string groupId)
        {
            GroupId = groupId;
        }
    }
}
