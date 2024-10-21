using MSU.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using SS2.Jobs;
using RoR2;

namespace SS2.Components
{
    //Main behaviour which manages instances of MongerTarTrails, There's a game object with this component that gets added to the Run component of SS2 if clay mongers are enabled.
    public class MongerTarTrailManager : MonoBehaviour
    {
        public static MongerTarTrailManager instance { get; private set; }
        [ConfigureField(SS2Config.ID_MAIN, configDescOverride = "The lifetime of a single tar puddle of a clay monger, lowering this value will increase performance.")]
        public static float pointLifetime = 10;

        public GameObject pointPrefab;
        public float raycastLength = 2f;
        public float timeBetweenTrailUpdates = 0.25f;
        public float timeBetweenPhysicsChecks = 0.3f;
        public float damageCoefficient = 0.2f;

        //We cannot directly reference a tar point's trail owner, but we can reference their instance id, so we have this dictionary to get the owner of a point.
        private Dictionary<int, MongerTarTrail> _instanceIDToMonger = new Dictionary<int, MongerTarTrail>();
        private List<MongerTarTrail> _mongerInstances;
        private List<Transform> _mongerTransforms; //Used for raycast purposes.

        private float _trailUpdateStopwatch;
        private float _physicsCheckStopwatch;

        //Mongers will always have less or equal to this amount of tar points. Dont ask how i got this formula down, i think it has to do with the fact that the fixed time stamp is 0.02.
        public int totalPointsPerMonger => Mathf.CeilToInt(pointLifetime) * 5;

        //Job collections breakdown:
        //A Job can only have "Native" collections, in short a native collection is simply a type of collection that only accepts value types inside, there are a couple of rules, but the most important ones are:
        //* A native collection cannot reference Reference types
        //* A Native collection MUST be disposed manually.

        private List<TarPoolEntry> _tarPoolEntries; //Actual VFX for a point is stored here.
        private TransformAccessArray _tarPoolTransforms; //used to modify a transform in a job.
        private NativeList<TarPoint> _allTarPoints; //Contains all the tar points that have been created in the run.
        private NativeQueue<int> _invalidIndexQueue; //A queue that contains indices in _allTarPoints that are invalid.
        private NativeList<ManagerIndex> _activeTarPoints; //A list of points that are actually in use

        //We cannot use the game's pooling system, so this is a very crude setup of it.
        private int _poolCounts;
        private int _currentPoolChildCount;
        private GameObject _currentPoolObject;
        private List<GameObject> _allPoolObjects = new List<GameObject>();

        private void Awake()
        {
            _mongerTransforms = new List<Transform>();
            _tarPoolEntries = new List<TarPoolEntry>();
            _tarPoolTransforms = new TransformAccessArray(0);
            //Most collections accept a specific allocator type. this is what they mean:
            //Persistent - The allocation can last as long as it needs to, relatively slow but useful for scenarios where its used across multiple frames. This type of collection allocation CAN be used in jobs.
            // Temp - The allocation must be disposed on the same frame, otherwise its considered a leak, this type of collection allocation cannot be used in jobs.
            // TempJob - The allocation must be disposed within 4 frames, otherwise its considered a leak, this type of collection allocation CAN be used in jobs.

            _allTarPoints = new NativeList<TarPoint>(Allocator.Persistent);
            _invalidIndexQueue = new NativeQueue<int>(Allocator.Persistent);
            _activeTarPoints = new NativeList<ManagerIndex>(Allocator.Persistent);
        }
        private void OnEnable()
        {
            instance = this;
        }

        private void OnDisable()
        {
            instance = null;
        }

        public void AddMonger(MongerTarTrail trail)
        {
            _mongerInstances.Add(trail);
            _mongerTransforms.Add(trail.transform);
            _instanceIDToMonger.Add(trail.GetInstanceID(), trail);
        }

        public void RemoveMonger(MongerTarTrail trail)
        {
            _mongerInstances.Remove(trail);
            _instanceIDToMonger.Remove(trail.GetInstanceID());

            //We neeed to return the points used by this trail.
            for (int i = trail.points.Length - 1; i >= 0; i--)
            {
                ReturnTarPoint(trail.points[i]);
            }
            trail.points.Clear(); //Clear the list to prepare for deallocation if needed.

            void ReturnTarPoint(TarPoint tarPoint)
            {
                if (!tarPoint.isValid)
                    return;

                int index = (int)tarPoint.managerIndex;

                TarPoolEntry gameObjectForPoint = _tarPoolEntries[index];
                //This index is being freed, add it to the stash so another monger can use it.
                _invalidIndexQueue.Enqueue(index);
                _allTarPoints[index] = TarPoint.invalid;
                gameObjectForPoint.isInPool = true;
                //Maybe remove at swap back?
                _tarPoolTransforms[index] = null;
                _tarPoolEntries[index] = gameObjectForPoint;
            }
        }

        //This method returns a valid TarPoint alongside it's visual representation.
        public TarPoint RequestTarPoint(MongerTarTrail owner, Vector3 position, Vector3 normalDirection, float yRotation, out TarPoolEntry gameObjectForPoint)
        {
            int index = GetFreeTarPointIndex();
            TarPoint tarPoint = new TarPoint(index)
            {
                pointLifetime = pointLifetime,
                totalLifetime = pointLifetime,
                normalDirection = normalDirection,
                rotation = quaternion.EulerXYZ(0, yRotation, 0),
                pointWidthDepth = new float2(5),
                remappedLifetime0to1 = 1,
                worldPosition = position,
                currentOwnerInstanceID = owner.GetInstanceID(),
                ownerPointIndex = owner.points.Length
            };
            _allTarPoints[index] = tarPoint;

            gameObjectForPoint = GetTarPoolEntryAtIndex(index);
            gameObjectForPoint.isInPool = false;
            return tarPoint;
        }

        private int GetFreeTarPointIndex()
        {
            int index = -1;
            //Use the stash of invalid indices first, should be considerably quicker.
            if (_invalidIndexQueue.TryDequeue(out index))
            {
                return index;
            }

            //Add a new point if needed.
            _allTarPoints.Add(TarPoint.invalid);
            index = _allTarPoints.Length - 1;
            return index;
        }

        //This one returns the visual representation of a TarPoint which is stored at the specified index, it creates a new one if needed.
        private TarPoolEntry GetTarPoolEntryAtIndex(int index)
        {
            if (index >= _tarPoolEntries.Count) //Ensures the index is not invalid.
            {
                _tarPoolEntries.Add(new TarPoolEntry(index));
                index = _tarPoolEntries.Count - 1;
            }

            var entry = _tarPoolEntries[index];
            if (!entry.tiedGameObject) //In case this entry was just created, we need to instantiate a new VFX instance.
            {
                if (_currentPoolChildCount >= totalPointsPerMonger) //Ensures that pool objects remain with a specific amount of children, this allows later for fast processing of the scaling changes for a point in its specified job.
                {
                    _currentPoolChildCount = 0;
                    _currentPoolObject = null;
                }
                if (!_currentPoolObject)
                {
                    //Creates a new pool object, stores it in dont destroy on load.
                    _currentPoolObject = new GameObject($"SplotchContainer{_poolCounts}");
                    UnityEngine.Object.DontDestroyOnLoad(_currentPoolObject);
                    _allPoolObjects.Add(_currentPoolObject); //Store it for destruction when this behaviour is destroyed
                    _poolCounts++;
                }

                var instance = Instantiate(pointPrefab, _currentPoolObject.transform);
                _currentPoolChildCount++;
                _tarPoolTransforms.Add(instance.transform); //Save this pool's transform for later usage.

                entry = new TarPoolEntry(index)
                {
                    tiedGameObject = instance,
                    isInPool = true
                };
                _tarPoolEntries.Insert(index, entry);
            }
            else //Game object already exists, we just need to add it back to the all point transforms.
            {
                _tarPoolTransforms[index] = entry.cachedTransform;
            }
            return entry;
        }

        //Oh boy this one is long
        private void FixedUpdate()
        {
            if (_mongerInstances.Count == 0)
                return;

            int allTarPoints = 0;
            PhysicsScene physicsScene = default;

            bool shouldTrailUpdate = false;

            //Collections in this method are set either on Temp or TempJob, because theyre only instantiated in a timer, so hogging memory with a persistent allocation is unecesary.
            bool shouldPhysicsCheck = false;
            NativeArray<BoxcastCommand> physicsChecksBoxcastCommands = default;
            NativeArray<RaycastHit> physicsChecksHitBuffer = default;
            NativeList<ManagerIndex> pointsThatCollidedWithSomething = default;

            //Most of the time we need a job to run after anotehr one has completed, this pattern of replacing this field's value allows us to properly and easily chain dependencies.
            JobHandle dependency = default;
            float deltaTime = Time.fixedDeltaTime;

            _physicsCheckStopwatch += deltaTime;
            _trailUpdateStopwatch += deltaTime;

            if(_trailUpdateStopwatch > timeBetweenTrailUpdates)
            {
                _trailUpdateStopwatch -= timeBetweenTrailUpdates;
                shouldTrailUpdate = true;
            }
            if(_physicsCheckStopwatch > timeBetweenPhysicsChecks)
            {
                _physicsCheckStopwatch -= timeBetweenPhysicsChecks;
                shouldPhysicsCheck = true;
            }

            if(shouldTrailUpdate || shouldPhysicsCheck)
            {
                //Normally physics commands use this by default, but that's going to cause issues when we write the commands in parallel since it'll cause an exception. so we store it early.
                physicsScene = Physics.defaultPhysicsScene;
            }

            if(shouldTrailUpdate)
            {
                //Initialize collections related to trail update.
                TransformAccessArray mongerTransformsAccessArray = new TransformAccessArray(0);
                mongerTransformsAccessArray.SetTransforms(_mongerTransforms.ToArray());
                NativeArray<RaycastCommand> mongerRaycastCommands = default;
                NativeArray<RaycastHit> mongerRaycastHitBuffer = default;
                NativeList<TarPoint> pointsToKill = default;

                //Technically speaking we can have both the killing and the begining of adding new trail points separate.
                JobHandle killTrailsHandle = default;
                JobHandle mongerRaycastDependency = default;

                //Filters out points which lifetimes are equal or less than 0.
                pointsToKill = new NativeList<TarPoint>(_allTarPoints.Length, Allocator.TempJob);
                KillPointsJob killTrailsJob = new KillPointsJob()
                {
                    pointsToKill = pointsToKill.AsParallelWriter(),
                    points = _allTarPoints
                };
                killTrailsHandle = killTrailsJob.Schedule(_allTarPoints.Length, 4, killTrailsHandle);

                //Returns the points from the output of the previous job to the queue.
                ReturnKilledPointsJob returnKilledPointsJob = new ReturnKilledPointsJob
                {
                    allPoints = _allTarPoints,
                    invalidPointIndices = _invalidIndexQueue.AsParallelWriter(),
                    killedPointsJob = pointsToKill.AsDeferredJobArray(),
                };
                killTrailsHandle = returnKilledPointsJob.Schedule(pointsToKill, 4, killTrailsHandle);

                //Creates raycast commands for adding new points, Raycastcommands are jobs that can be executed in parallel.
                mongerRaycastDependency = default;
                mongerRaycastCommands = new NativeArray<RaycastCommand>(_mongerInstances.Count, Allocator.TempJob);
                WriteRaycastCommandsJob writeRaycastCommandsJob = new WriteRaycastCommandsJob
                {
                    output = mongerRaycastCommands,
                    physicsScene = physicsScene,
                    raycastLength = raycastLength,
                    raycastMask = LayerIndex.world.mask
                };
                mongerRaycastDependency = writeRaycastCommandsJob.Schedule(mongerTransformsAccessArray, mongerRaycastDependency);

                //Properly executes the raycast commands and stores them in the buffer below.
                mongerRaycastHitBuffer = new NativeArray<RaycastHit>(_mongerInstances.Count, Allocator.TempJob);
                mongerRaycastDependency = RaycastCommand.ScheduleBatch(mongerRaycastCommands, mongerRaycastHitBuffer, 4, mongerRaycastDependency);

                //Before we attempt to read into the modified collections, we need to call complete to make sure the jobs associated with this handle are completed.
                killTrailsHandle.Complete();

                for (int i = 0; i < pointsToKill.Length; i++)
                {
                    //Removes the expired points from the associated monger and disabled them from being modified.
                    var point = pointsToKill[i];
                    int managerIndex = (int)point.managerIndex;

                    _tarPoolTransforms[managerIndex] = null;
                    TarPoolEntry gameObjectForPoint = _tarPoolEntries[managerIndex];
                    gameObjectForPoint.isInPool = true;
                    _tarPoolEntries[managerIndex] = gameObjectForPoint;
                    _instanceIDToMonger[point.currentOwnerInstanceID].points.RemoveAt(point.ownerPointIndex);
                }

                //Create new points where mongers have succesfully raycasted downwards.
                mongerRaycastDependency.Complete();
                for (int i = 0; i < _mongerInstances.Count; i++)
                {
                    var instance = _mongerInstances[i];
                    instance.AddPoint(mongerRaycastHitBuffer[i]);
                }

                //Dispose of collections since they've fulfilled their purposes.
                if (mongerTransformsAccessArray.isCreated)
                    mongerTransformsAccessArray.Dispose();
                if (pointsToKill.IsCreated)
                    pointsToKill.Dispose();
                if (mongerRaycastHitBuffer.IsCreated)
                    mongerRaycastHitBuffer.Dispose();
                if (mongerRaycastCommands.IsCreated)
                    mongerRaycastCommands.Dispose();
            }

            //Once we've obtained the new amount of tar points, we can begin with the rest of the process.
            allTarPoints = _allTarPoints.Length;

            //Our jobs should only affect tar points who's indices are not invalid. these active points are also used on the lifetime and size jobs so we need to run it each fixed update. This is done because some points may be allocated in memory but not used (IE: a monger that spawned them was destroyed)
            //Unlike all other jobs, we need to complete this immediatly, since we need to know the length of the points we should modify prior to scheduling the actual heavy lifting jobs.
            _activeTarPoints.Clear();
            if (_activeTarPoints.Capacity <= allTarPoints) //Ensure we have enough capacity prior to using the parallel writer.
            {
                _activeTarPoints.SetCapacity(allTarPoints);
            }

            new GetActiveTarPointsJob
            {
                activePoints = _activeTarPoints.AsParallelWriter(),
                allTarPoints = _allTarPoints,
            }.Schedule(allTarPoints, totalPointsPerMonger, dependency).Complete();

            //How many entries the following parallel jobs should execute.
            int innerloopBatchCount = _activeTarPoints.Length / _mongerInstances.Count;

            if (shouldPhysicsCheck)
            {
                //We need to check if the active points have overlapping objects
                physicsChecksBoxcastCommands = new NativeArray<BoxcastCommand>(_activeTarPoints.Length, Allocator.TempJob);
                //Write the boxcasts
                dependency = new WriteBoxcastCommandsJob
                {
                    output = physicsChecksBoxcastCommands,
                    physicsScene = Physics.defaultPhysicsScene,
                    physicsCheckMask = LayerIndex.entityPrecise.mask,
                    tarPoints = _allTarPoints
                }.Schedule(_activeTarPoints.Length, innerloopBatchCount, dependency);

                //Check for any colliders the points have collided with.
                physicsChecksHitBuffer = new NativeArray<RaycastHit>(physicsChecksBoxcastCommands.Length, Allocator.TempJob);
                dependency = BoxcastCommand.ScheduleBatch(physicsChecksBoxcastCommands, physicsChecksHitBuffer, innerloopBatchCount, dependency);

                //We should filter out the points that didnt hit anything. this will greatly speed up the actual damaging of entities.
                pointsThatCollidedWithSomething = new NativeList<ManagerIndex>(_activeTarPoints.Capacity / 2, Allocator.TempJob);
                dependency = new FilterManagerIndicesThatDidntCollideWithAnythingJob
                {
                    input = _allTarPoints,
                    output = pointsThatCollidedWithSomething.AsParallelWriter(),
                    raycastHits = physicsChecksHitBuffer
                }.Schedule(_activeTarPoints.Length, innerloopBatchCount, dependency);
            }

            //as long as pool transforms exists, call these jobs.
            if(_tarPoolTransforms.length > 0)
            {
                //Reduces the lifetime of each point and recalculates their remapped 0-1 value.
                TrailPointLifetimeJob lifetimeJob = new TrailPointLifetimeJob
                {
                    deltaTime = deltaTime,
                    tarPoints = _allTarPoints,
                };
                dependency = lifetimeJob.Schedule(allTarPoints, totalPointsPerMonger, dependency);

                //Scales the VFX of the points according to their remapped 0-1 value
                TrailPointVisualJob job = new TrailPointVisualJob
                {
                    maxSize = new float3(1),
                    tarPoints = _allTarPoints,
                    totalLifetime = pointLifetime,
                };
                dependency = job.Schedule(_tarPoolTransforms, dependency);
            }

            //Ensure physics and visual jobs are completed
            dependency.Complete();

            if(shouldPhysicsCheck)
            {
                //From the results of the boxcasts, we shall check for enemies of the monger.
                for (int i = 0; i < pointsThatCollidedWithSomething.Length; i++)
                {
                    var managerIndex = pointsThatCollidedWithSomething[i];
                    var collider = physicsChecksHitBuffer[(int)managerIndex].collider;

                    if (!collider.TryGetComponent<HurtBox>(out var hb))
                        continue;

                    HealthComponent hc = hb.healthComponent;
                    if (!hc)
                        continue;

                    var point = GetPoint(managerIndex);

                    var collidedGameObject = hc.gameObject;
                    //Avoid doing stuff to the owner of the point
                    var mongerTrailThatOwnsTheCurrentPoint = _instanceIDToMonger[point.currentOwnerInstanceID];
                    if (hc.gameObject == mongerTrailThatOwnsTheCurrentPoint.gameObject)
                        continue;

                    //respect friendly fire thing.
                    if (!FriendlyFireManager.ShouldSplashHitProceed(hc, mongerTrailThatOwnsTheCurrentPoint.teamComponent.teamIndex))
                        continue;


                    hc.body.AddTimedBuff(SS2Content.Buffs.bdMongerTar, timeBetweenPhysicsChecks * 2);
                    if(point.isBubbling)
                    {
                        //All checks passed, time to deal damage.
                        DamageInfo damageInfo = new DamageInfo
                        {
                            attacker = mongerTrailThatOwnsTheCurrentPoint.gameObject,
                            inflictor = mongerTrailThatOwnsTheCurrentPoint.gameObject,
                            crit = false,
                            damage = mongerTrailThatOwnsTheCurrentPoint.characterBody.damage * damageCoefficient,
                            damageColorIndex = DamageColorIndex.Item,
                            damageType = DamageType.Generic,
                            force = Vector3.zero,
                            position = point.worldPosition,
                            procCoefficient = 0,
                        };
                        damageInfo.position = collider.transform.position;
                        hc.TakeDamage(damageInfo);
                    }
                }
            }

            //We need to ensure the monger instances have properly updated their internal lists, this does that.
            NativeArray<JobHandle> updateFromManagerHandles = new NativeArray<JobHandle>(_mongerInstances.Count, Allocator.Temp);
            for (int i = 0; i < _mongerInstances.Count; i++)
            {
                var mongerInstance = _mongerInstances[i];
                updateFromManagerHandles[i] = new UpdateFromManagerJob
                {
                    allTarPoints = _allTarPoints,
                    myTarPoints = mongerInstance.points
                }.Schedule(mongerInstance.points.Length, default);
            }
            JobHandle.CompleteAll(updateFromManagerHandles);

            //Free memory
            if (physicsChecksHitBuffer.IsCreated)
                physicsChecksHitBuffer.Dispose();

            if (pointsThatCollidedWithSomething.IsCreated)
                pointsThatCollidedWithSomething.Dispose();

            if (physicsChecksBoxcastCommands.IsCreated)
                physicsChecksBoxcastCommands.Dispose();

            if (updateFromManagerHandles.IsCreated)
                updateFromManagerHandles.Dispose();
        }
        private void OnDestroy()
        {
            //Free persistent memory
            if (_allTarPoints.IsCreated)
                _allTarPoints.Dispose();

            if (_tarPoolTransforms.isCreated)
                _tarPoolTransforms.Dispose();


            if (_activeTarPoints.IsCreated)
                _activeTarPoints.Dispose();

            if (_invalidIndexQueue.IsCreated)
                _invalidIndexQueue.Dispose();

            //Kill the pools
            foreach(var obj in _allPoolObjects)
            {
                Destroy(obj);
            }
        }

        private TarPoint GetPoint(ManagerIndex managerIndex)
        {
            return _allTarPoints[(int)managerIndex];
        }

        //This actually represents a VFX instance for a point.
        /// <summary>
        /// NOT JOB SAFE
        /// </summary>
        public struct TarPoolEntry : IEquatable<TarPoolEntry>
        {
            public GameObject tiedGameObject
            {
                get => _tiedGameObject;
                set
                {
                    _tiedGameObject = value;
                    cachedTransform = value.transform;
                }
            }
            private GameObject _tiedGameObject;

            public Transform cachedTransform;
            public bool isInPool
            {
                get => _isInPool;
                set
                {
                    if (!tiedGameObject)
                    {
                        _isInPool = false;
                        return;
                    }

                    if (_isInPool != value)
                    {
                        _isInPool = value;
                        tiedGameObject.SetActive(!value);
                    }
                }
            }
            private bool _isInPool;
            public ManagerIndex managerPoolIndex => _managerPoolIndex;
            private ManagerIndex _managerPoolIndex;

            internal TarPoolEntry(int index)
            {
                _tiedGameObject = null;
                cachedTransform = null;
                _isInPool = false;
                _managerPoolIndex = (ManagerIndex)index;
            }

            public bool Equals(TarPoolEntry other)
            {
                return managerPoolIndex == other.managerPoolIndex;
            }
        }

        //A tar point is just a position in 3D space that represents an actual puddle from a monger, only contains value types since that allows the struct to be used in Jobs
        public struct TarPoint : IEquatable<TarPoint>
        {
            public static readonly TarPoint invalid = new TarPoint(-1);

            public float3 worldPosition;
            public float3 normalDirection;
            public quaternion rotation;
            public float2 pointWidthDepth;
            public float pointLifetime;
            public float totalLifetime;
            public float remappedLifetime0to1;
            public int ownerPointIndex;
            public int currentOwnerInstanceID;

            public float halfLifetime => totalLifetime / 2;
            public bool isBubbling => pointLifetime > halfLifetime;
            public ManagerIndex managerIndex => _managerIndex;
            private ManagerIndex _managerIndex;
            public bool isValid => !Equals(invalid);

            public bool Equals(TarPoint other)
            {
                return _managerIndex == other._managerIndex;
            }

            internal TarPoint(int index)
            {
                _managerIndex = (ManagerIndex)index;
                worldPosition = float3.zero;
                normalDirection = float3.zero;
                rotation = quaternion.identity;
                pointWidthDepth = float2.zero;
                pointLifetime = 0;
                totalLifetime = 0;
                remappedLifetime0to1 = 0;
                ownerPointIndex = -1;
                currentOwnerInstanceID = 0;
            }
        }

        //An index for this manager, used mainly to keep proper indices on Tar Points.
        public enum ManagerIndex : int
        {
            Invalid = -1
        }
    }
}
