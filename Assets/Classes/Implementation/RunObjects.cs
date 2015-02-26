using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Classes.Core;
using Assets.Classes.Foundation.Enums;
using PathologicalGames;
using UnityEngine;

namespace Assets.Classes.Implementation
{
    public class RunObjects : PoolBase<RunObjects, RunObject>
    {

        #region Objects pipeline & pool

        public const string PipelineBecameEmptyEventName = "PipelineBecameEmpty";

        public Queue<RunObjectBuild> Pipeline { get; private set; }
        public List<RunObject> SpawnedObjects { get; private set; }
 
        public bool IsPipelineIsEmpty
        {
            get { return Pipeline.Count == 0; }
        }

        public void Enqueue(RunObjectBuild build)
        {
            Pipeline.Enqueue(build);
        }

        private RunObject Spawn(Type objType)
        {
            PrefabPool pool;
            var isPrefabPoolExist = Pool.prefabPools.TryGetValue(objType.Name, out pool);

            if (!isPrefabPoolExist)
            {
                ProcessError("RunObjects.Dequeue(): prefab pool for object '" + objType.Name + "' not found");
                return null;
            }

            var spawnedObj1 = Pool.Spawn(objType.Name);

            if (spawnedObj1 == null)
            {
                ProcessError("RunObjects.Dequeue(): error while trying to spawn new instance of '" + objType.Name + "'");
                return null;
            }

            var spawnedObj2 = spawnedObj1.GetComponent<RunObject>();

            if (spawnedObj2 == null)
            {
                ProcessError("RunObjects.Dequeue(): spawned object doesn't contains RunObject component");
                return null;
            }

            return spawnedObj2;
        }
        public override RunObject Spawn()
        {
            if (IsPipelineIsEmpty)
            {
                ProcessError("RunObjects.Dequeue(): Pipeline is empty");
                return null;
            }

            var build = Pipeline.Dequeue();
            var objType = build.GetObjectType();

            var obj = Spawn(objType);

            if (obj == null)
            {
                ProcessError("RunObjects.Dequeue(): Spawn(objType) == null");
                return null;
            }

            build.Build(obj);

            SpawnedObjects.Add(obj);
            obj.OnObjectSpawned(build);

            if (IsPipelineIsEmpty)
            {
                OnPipelineBecameEmpty();
            }

            return obj;
        }
        public override void Despawn(RunObject ro)
        {
            Pool.Despawn(ro.transform);
            SpawnedObjects.Remove(ro);
            ro.OnObjectDespawned();
        }

        private void OnPipelineBecameEmpty()
        {
            GameMessenger.Broadcast(PipelineBecameEmptyEventName);
            GenerateNewObjects();
        }

        public RunObject GetObjectByType<T>(bool isObjectIsSpawned)
        {
            return GetObjectByType(typeof (T), isObjectIsSpawned);
        }
        public RunObject GetObjectByType(Type objectType, bool isObjectIsSpawned)
        {
            foreach (var instance in Pool)
            {
                if (Pool.IsSpawned(instance) == isObjectIsSpawned)
                {
                    var ro = instance.GetComponent<RunObject>();
                    if (ro.GetType() == objectType)
                        return ro;
                }
            }
            return null;
        }
        public RunObject GetDespawnedObjectByType<T>()
        {
            return GetObjectByType(typeof(T), false);
        }
        public RunObject GetSpawnedObjectByType<T>()
        {
            return GetObjectByType(typeof(T), true);
        }
        #endregion

        #region Objects positionation

        public bool ShouldSpawnNewObjects
        {
            get
            {
                var lo = GetLatestObject();

                return lo == null || lo.IsCompletelyOnScreen;
            }
        }

        public RunObject GetLatestObject()
        {
            return SpawnedObjects.LastOrDefault();
        }
        public RunObject GetFirstObject()
        {
            return SpawnedObjects.FirstOrDefault();
        }
        public float GetHightestPoint()
        {
            return CameraSupervisor.Main.CameraViewRect.yMin;

            var latestObject = GetLatestObject();

            if (latestObject == null || !latestObject.IsSpawned)
                return CameraSupervisor.Main.CameraViewRect.yMin;

            return latestObject.WorldRect.yMax;

        }

        public float SpeedMultiplier = 2f;

        public void TranslateSpawnedObjects()
        {
            TranslateSpawnedObjects(-(Run.Instance.Speed * Time.deltaTime * SpeedMultiplier));
        }
        public void TranslateSpawnedObjects(float y)
        {
            foreach (var spawnedObject in SpawnedObjects)
            {
                spawnedObject.transform.Translate(0, y, 0, UnityEngine.Space.World);
            }
        }

        private void CheckForDespawn()
        {
            for (int i = 0; i < SpawnedObjects.Count; i++)
            {
                var spawnedObject = SpawnedObjects[i];
                if (spawnedObject.ShouldDespawnAfterBelowScreen && spawnedObject.CalculatePositionationType() == RunObject.PositionationType.BelowScreen)
                {
                    Despawn(spawnedObject);
                }
            }
        }
        private void CheckForSpawn()
        {
            if (ShouldSpawnNewObjects)
            {
                Spawn();
            }
        }

        #endregion

        #region Objects generation

        public bool IsSpawnProcessIsActive { get; set; }

        public void GenerateNewObjects()
        {

           Enqueue(new RunObjectGenericBuild<Space>(space => space.SpaceHeight = UnityEngine.Random.Range(3.3f, 4f)));
            Enqueue(new RunObjectGenericBuild<HorizontalGate>(obstacle =>
            {
                if (UnityEngine.Random.Range(0, 4) != 2)
                {
                    obstacle.Behaviour =
                        HorizontalGate.HorizontalGateMove.Random();
                }
                else
                {
                    obstacle.Behaviour = HorizontalGate.HorizontalGateOpening.Random();
                }
            }));
            Enqueue(new RunObjectGenericBuild<Space>(space => space.SpaceHeight = UnityEngine.Random.Range(3.3f, 4f)));
            Enqueue(new RunObjectGenericBuild<Dots>(dots =>
                                                    {
                                                        
                                                    }));
        }

        #endregion

        #region Unity callbacks
        private void Awake()
        {
            Pipeline = new Queue<RunObjectBuild>();
            SpawnedObjects = new List<RunObject>();
        }

        private void Start()
        {
            Enqueue(new RunObjectGenericBuild<Space>()
                    {
                        BuildAction = space =>
                                      {
                                          space.SpaceHeight = 0.3f;
                                      }
                    });
            Spawn();
        }

        private void Update()
        {
            TranslateSpawnedObjects();
            CheckForDespawn();
            CheckForSpawn();
        }



        #endregion

    }
}
