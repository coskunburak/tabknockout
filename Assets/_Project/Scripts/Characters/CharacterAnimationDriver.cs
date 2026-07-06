using System.Collections.Generic;
using TapKnockout.Combat;
using TapKnockout.Enemy;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Characters
{
    [DisallowMultipleComponent]
    public sealed class CharacterAnimationDriver : MonoBehaviour, IPoolLifecycle
    {
        public const string MoveSpeedParameter = "MoveSpeed";
        public const string IsMovingParameter = "IsMoving";
        public const string IsDashingParameter = "IsDashing";
        public const string IsAttackingParameter = "IsAttacking";
        public const string AttackTrigger = "Attack";
        public const string SkillCastTrigger = "SkillCast";
        public const string DashTrigger = "Dash";
        public const string HitTrigger = "Hit";
        public const string DeathTrigger = "Death";
        public const string IdleState = "Idle";
        public const string MoveState = "Move";
        public const string AttackState = "Attack";
        public const string DashState = "Dash";
        public const string HitState = "Hit";
        public const string DeathState = "Death";
        private const string BaseLayerName = "Base Layer";
        private const float EnemyAttackEventDedupWindow = 0.08f;
        private const float EnemyKnockbackHitDedupWindow = 0.08f;

        private static readonly int MoveSpeedHash = Animator.StringToHash(MoveSpeedParameter);
        private static readonly int IsMovingHash = Animator.StringToHash(IsMovingParameter);
        private static readonly int IsDashingHash = Animator.StringToHash(IsDashingParameter);
        private static readonly int IsAttackingHash = Animator.StringToHash(IsAttackingParameter);
        private static readonly int AttackHash = Animator.StringToHash(AttackTrigger);
        private static readonly int SkillCastHash = Animator.StringToHash(SkillCastTrigger);
        private static readonly int DashHash = Animator.StringToHash(DashTrigger);
        private static readonly int HitHash = Animator.StringToHash(HitTrigger);
        private static readonly int DeathHash = Animator.StringToHash(DeathTrigger);
        private static readonly int IdleStateHash = Animator.StringToHash($"{BaseLayerName}.{IdleState}");
        private static readonly int MoveStateHash = Animator.StringToHash($"{BaseLayerName}.{MoveState}");
        private static readonly int AttackStateHash = Animator.StringToHash($"{BaseLayerName}.{AttackState}");
        private static readonly int DashStateHash = Animator.StringToHash($"{BaseLayerName}.{DashState}");
        private static readonly int HitStateHash = Animator.StringToHash($"{BaseLayerName}.{HitState}");
        private static readonly int DeathStateHash = Animator.StringToHash($"{BaseLayerName}.{DeathState}");

        [Header("Animator")]
        [SerializeField] private Animator animator;
        [SerializeField] private bool autoResolveAnimator = true;
        [SerializeField] private bool directStatePlayback = false;
        [SerializeField, Min(0f)] private float speedDampTime = 0.08f;
        [SerializeField, Min(0f)] private float stateCrossFadeDuration = 0.08f;
        [SerializeField, Min(0f)] private float attackLockDuration = 0.35f;
        [SerializeField, Min(0f)] private float playerAttackBoolDuration = 0.18f;
        [SerializeField, Min(0f)] private float skillCastBoolDuration = 0.2f;
        [SerializeField, Min(0f)] private float hitLockDuration = 0.22f;
        [SerializeField] private bool playerAttackLocksDirectState;

        [Header("Debug")]
        [SerializeField] private bool logDebug;

        [Header("Role")]
        [SerializeField] private bool isPlayer;

        [Header("Player References")]
        [SerializeField] private PlayerMovementController playerMovement;
        [SerializeField] private PlayerAttackController playerAttack;
        [SerializeField] private PlayerDashController playerDash;
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Enemy References")]
        [SerializeField] private EnemyMovement enemyMovement;
        [SerializeField] private EnemyAttackController enemyAttack;
        [SerializeField] private EnemyHealth enemyHealth;
        [SerializeField] private KnockbackReceiver enemyKnockbackReceiver;

        private readonly HashSet<int> animatorParameterHashes = new HashSet<int>();
        private RuntimeAnimatorController cachedAnimatorController;
        private Rigidbody cachedRigidbody;
        private bool wasPlayerAttackReady;
        private bool wasEnemyAttackReady;
        private bool wasDashing;
        private bool isDead;
        private bool isSuppressedByParentDriver;
        private int currentStateHash;
        private float stateLockUntil;
        private float nextDebugLogTime;
        private float playerAttackVisualUntil;
        private float skillCastVisualUntil;
        private float lastEnemyAttackAnimationTime = -999f;
        private float lastHitAnimationTime = -999f;
        private PlayerAttackController subscribedPlayerAttack;
        private KnockbackReceiver subscribedKnockbackReceiver;

        public Animator Animator => animator;
        public bool IsPlayer => isPlayer;

        private void Reset()
        {
            ResolveReferences();
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            isSuppressedByParentDriver = IsSuppressedByParentDriver();
            if (isSuppressedByParentDriver)
            {
                return;
            }

            InitializeAnimatorForPlayback();
            SubscribeHealthEvents();
            SubscribePlayerRuntimeEvents();
            SubscribeEnemyRuntimeEvents();

            wasPlayerAttackReady = playerAttack == null || playerAttack.IsCooldownReady;
            wasEnemyAttackReady = enemyAttack == null || enemyAttack.IsCooldownReady;
            wasDashing = playerDash != null && playerDash.IsDashing;
            isDead = IsHealthDead();
            playerAttackVisualUntil = 0f;
            skillCastVisualUntil = 0f;
        }

        private void OnDisable()
        {
            if (isSuppressedByParentDriver)
            {
                isSuppressedByParentDriver = false;
                return;
            }

            UnsubscribeHealthEvents();
            UnsubscribePlayerRuntimeEvents();
            UnsubscribeEnemyRuntimeEvents();
        }

        private void LateUpdate()
        {
            if (IsSuppressedByParentDriver())
            {
                return;
            }

            RefreshAnimationState(Time.deltaTime);
        }

        public void SetAnimator(Animator targetAnimator)
        {
            if (animator == targetAnimator)
            {
                return;
            }

            animator = targetAnimator;
            cachedAnimatorController = null;
            animatorParameterHashes.Clear();
            InitializeAnimatorForPlayback();
        }

        public void SetIsPlayer(bool value)
        {
            isPlayer = value;
        }

        public void OnBeforeSpawnFromPool()
        {
            ResetRuntimeAnimationState();
        }

        public void OnSpawnedFromPool()
        {
            ResetRuntimeAnimationState();
        }

        public void OnBeforeDespawnToPool()
        {
            ResetRuntimeAnimationState();
        }

        public void ResetForPool()
        {
            ResetRuntimeAnimationState();
        }

        public void ResolveReferences()
        {
            if (autoResolveAnimator)
            {
                animator = ResolveAnimator(animator);
            }

            if (cachedRigidbody == null)
            {
                cachedRigidbody = ResolveComponentInSelfOrParent<Rigidbody>(cachedRigidbody);
            }

            playerMovement = ResolveComponentInSelfOrParent(playerMovement);
            playerAttack = ResolveComponentInSelfOrParent(playerAttack);
            playerDash = ResolveComponentInSelfOrParent(playerDash);
            playerHealth = ResolveComponentInSelfOrParent(playerHealth);
            enemyMovement = ResolveComponentInSelfOrParent(enemyMovement);
            enemyAttack = ResolveComponentInSelfOrParent(enemyAttack);
            enemyHealth = ResolveComponentInSelfOrParent(enemyHealth);
            enemyKnockbackReceiver = ResolveComponentInSelfOrParent(enemyKnockbackReceiver);

            if (!isPlayer && (playerMovement != null || playerAttack != null || playerDash != null || playerHealth != null))
            {
                isPlayer = true;
            }
        }

        private Animator ResolveAnimator(Animator current)
        {
            var animators = GetComponentsInChildren<Animator>(true);
            if (animators.Length <= 0)
            {
#if UNITY_EDITOR
                current = EnsureEditorFallbackAnimator(current);
#endif
                return current;
            }

            var preferred = SelectPreferredAnimator(animators, requireController: true, preferNonRoot: true)
                ?? SelectPreferredAnimator(animators, requireController: true, preferNonRoot: false)
                ?? SelectPreferredAnimator(animators, requireController: false, preferNonRoot: true)
                ?? current
                ?? animators[0];

#if UNITY_EDITOR
            EnsureEditorAnimatorAssets(preferred);
#endif
            return preferred;
        }

        private Animator SelectPreferredAnimator(Animator[] animators, bool requireController, bool preferNonRoot)
        {
            for (var i = 0; i < animators.Length; i++)
            {
                var candidate = animators[i];
                if (candidate == null)
                {
                    continue;
                }

                if (requireController && candidate.runtimeAnimatorController == null)
                {
                    continue;
                }

                if (preferNonRoot && candidate.transform == transform)
                {
                    continue;
                }

                return candidate;
            }

            return null;
        }

#if UNITY_EDITOR
        private Animator EnsureEditorFallbackAnimator(Animator current)
        {
            if (current != null)
            {
                return current;
            }

            var visualTarget = ResolveVisualAnimatorTarget();
            if (visualTarget == null)
            {
                return null;
            }

            var createdAnimator = visualTarget.GetComponent<Animator>();
            if (createdAnimator == null)
            {
                createdAnimator = visualTarget.gameObject.AddComponent<Animator>();
            }

            EnsureEditorAnimatorAssets(createdAnimator);
            createdAnimator.applyRootMotion = false;
            createdAnimator.updateMode = AnimatorUpdateMode.Normal;
            createdAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            return createdAnimator;
        }

        private void EnsureEditorAnimatorAssets(Animator targetAnimator)
        {
            if (targetAnimator == null || !isPlayer)
            {
                return;
            }

            targetAnimator.applyRootMotion = false;
            targetAnimator.updateMode = AnimatorUpdateMode.Normal;
            targetAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            if (targetAnimator.runtimeAnimatorController == null)
            {
                targetAnimator.runtimeAnimatorController = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                    "Assets/_Project/Animation/Controllers/AC_Player_Rogue.controller");
            }

            if (targetAnimator.avatar == null)
            {
                var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(
                    "Assets/Assets/game asset packs/RPG Characters - Nov 2020/FBX/Ranger.fbx");
                for (var i = 0; i < assets.Length; i++)
                {
                    if (assets[i] is Avatar avatar)
                    {
                        targetAnimator.avatar = avatar;
                        break;
                    }
                }
            }
        }

        private Transform ResolveVisualAnimatorTarget()
        {
            var visualRoot = transform.Find("VisualRoot");
            if (visualRoot == null)
            {
                return transform;
            }

            return visualRoot.childCount > 0 ? visualRoot.GetChild(0) : visualRoot;
        }
#endif

        private T ResolveComponentInSelfOrParent<T>(T current) where T : Component
        {
            if (current != null)
            {
                return current;
            }

            if (TryGetComponent<T>(out var localComponent))
            {
                return localComponent;
            }

            return GetComponentInParent<T>(true);
        }

        private bool IsSuppressedByParentDriver()
        {
            if (animator == null)
            {
                return false;
            }

            var parentDrivers = GetComponentsInParent<CharacterAnimationDriver>(true);
            for (var i = 0; i < parentDrivers.Length; i++)
            {
                var parentDriver = parentDrivers[i];
                if (parentDriver == null || parentDriver == this || !parentDriver.isActiveAndEnabled)
                {
                    continue;
                }

                if (parentDriver.animator == null)
                {
                    parentDriver.ResolveReferences();
                }

                if (parentDriver.animator == animator)
                {
                    return true;
                }
            }

            return false;
        }

        private void InitializeAnimatorForPlayback()
        {
            currentStateHash = 0;
            stateLockUntil = 0f;

            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return;
            }

            animator.enabled = true;
            animator.Rebind();
            animator.Update(0f);
            cachedAnimatorController = null;
            animatorParameterHashes.Clear();
        }

        public void RefreshAnimationState(float deltaTime)
        {
            ResolveReferences();

            if (animator == null)
            {
                LogDebugState("missing animator", 0f, false, false);
                return;
            }

            RefreshAnimatorParameterCacheIfNeeded();
            isDead = IsHealthDead();

            var dashing = isPlayer && playerDash != null && playerDash.IsDashing;
            if (dashing && !wasDashing)
            {
                TrySetTrigger(DashHash);
                PlayState(DashStateHash, false);
            }

            TrySetBool(IsDashingHash, dashing);
            wasDashing = dashing;

            var moveSpeed = ResolveMoveSpeed();
            var moving = !isDead && !dashing && IsMoving(moveSpeed);
            TrySetFloat(MoveSpeedHash, moveSpeed, Mathf.Max(0f, deltaTime));
            TrySetBool(IsMovingHash, moving);
            DetectAttackTriggers();
            TrySetBool(IsAttackingHash, !isDead && IsAttackVisualActive());
            RefreshDirectLocomotionState(dashing, moving);
            LogDebugState("state refreshed", moveSpeed, moving, dashing);
        }

        private void SubscribeHealthEvents()
        {
            UnsubscribeHealthEvents();

            if (playerHealth != null)
            {
                playerHealth.OnDamaged += HandleDamaged;
                playerHealth.OnPlayerDied += HandleDied;
            }

            if (enemyHealth != null)
            {
                enemyHealth.OnDamaged += HandleDamaged;
                enemyHealth.OnDied += HandleDied;
            }
        }

        private void UnsubscribeHealthEvents()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDamaged -= HandleDamaged;
                playerHealth.OnPlayerDied -= HandleDied;
            }

            if (enemyHealth != null)
            {
                enemyHealth.OnDamaged -= HandleDamaged;
                enemyHealth.OnDied -= HandleDied;
            }
        }

        private void SubscribePlayerRuntimeEvents()
        {
            UnsubscribePlayerRuntimeEvents();

            if (!isPlayer || playerAttack == null)
            {
                return;
            }

            subscribedPlayerAttack = playerAttack;
            subscribedPlayerAttack.OnPrimaryAttackFired += HandlePlayerPrimaryAttackFired;
        }

        private void UnsubscribePlayerRuntimeEvents()
        {
            if (subscribedPlayerAttack == null)
            {
                return;
            }

            subscribedPlayerAttack.OnPrimaryAttackFired -= HandlePlayerPrimaryAttackFired;
            subscribedPlayerAttack = null;
        }

        private void SubscribeEnemyRuntimeEvents()
        {
            UnsubscribeEnemyRuntimeEvents();

            if (isPlayer)
            {
                return;
            }

            EnemyAttackEvents.OnTelegraphStarted += HandleEnemyTelegraphStarted;
            EnemyAttackEvents.OnAttackReleased += HandleEnemyAttackReleased;

            if (enemyKnockbackReceiver != null)
            {
                subscribedKnockbackReceiver = enemyKnockbackReceiver;
                subscribedKnockbackReceiver.OnKnockbackReceived += HandleEnemyKnockbackReceived;
            }
        }

        private void UnsubscribeEnemyRuntimeEvents()
        {
            EnemyAttackEvents.OnTelegraphStarted -= HandleEnemyTelegraphStarted;
            EnemyAttackEvents.OnAttackReleased -= HandleEnemyAttackReleased;

            if (subscribedKnockbackReceiver != null)
            {
                subscribedKnockbackReceiver.OnKnockbackReceived -= HandleEnemyKnockbackReceived;
                subscribedKnockbackReceiver = null;
            }
        }

        private void HandleDamaged(HitContext hitContext)
        {
            if ((hitContext != null && hitContext.WasIgnored) || isDead)
            {
                return;
            }

            TrySetTrigger(HitHash);
            PlayState(HitStateHash, true, hitLockDuration);
            lastHitAnimationTime = Time.time;
        }

        private void HandleDied(HitContext hitContext)
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            TrySetBool(IsMovingHash, false);
            TrySetBool(IsDashingHash, false);
            TrySetBool(IsAttackingHash, false);
            TrySetTrigger(DeathHash);
            PlayState(DeathStateHash, true);
        }

        private void DetectAttackTriggers()
        {
            if (isDead)
            {
                return;
            }

            if (playerAttack != null)
            {
                if (subscribedPlayerAttack == null)
                {
                    DetectAttackReadyTransition(
                        ref wasPlayerAttackReady,
                        playerAttack.IsCooldownReady,
                        playerAttackLocksDirectState,
                        playerAttackLocksDirectState ? attackLockDuration : 0f);
                }
                else
                {
                    wasPlayerAttackReady = playerAttack.IsCooldownReady;
                }
            }

            if (enemyAttack != null)
            {
                DetectEnemyAttackCooldownFallback();
            }
        }

        private void DetectAttackReadyTransition(ref bool wasReady, bool isReady, bool playDirectState, float lockDuration)
        {
            if (wasReady && !isReady)
            {
                TriggerAttackAnimation(lockDuration, playDirectState);
            }

            wasReady = isReady;
        }

        private void DetectEnemyAttackCooldownFallback()
        {
            var isReady = enemyAttack.IsCooldownReady;
            if (wasEnemyAttackReady &&
                !isReady &&
                Time.time - lastEnemyAttackAnimationTime > EnemyAttackEventDedupWindow)
            {
                TriggerAttackAnimation(attackLockDuration);
            }

            wasEnemyAttackReady = isReady;
        }

        private void HandleEnemyTelegraphStarted(EnemyAttackEventArgs eventArgs)
        {
            if (!IsLocalEnemyAttackEvent(eventArgs) || isDead)
            {
                return;
            }

            TriggerAttackAnimation(Mathf.Max(attackLockDuration, eventArgs.Duration));
        }

        private void HandleEnemyAttackReleased(EnemyAttackEventArgs eventArgs)
        {
            if (!IsLocalEnemyAttackEvent(eventArgs) || isDead)
            {
                return;
            }

            if (currentStateHash == AttackStateHash && Time.time < stateLockUntil)
            {
                return;
            }

            if (Time.time - lastEnemyAttackAnimationTime <= EnemyAttackEventDedupWindow)
            {
                return;
            }

            TriggerAttackAnimation(attackLockDuration);
        }

        private void HandleEnemyKnockbackReceived(KnockbackData knockbackData)
        {
            if (isPlayer || isDead || Time.time - lastHitAnimationTime <= EnemyKnockbackHitDedupWindow)
            {
                return;
            }

            TrySetTrigger(HitHash);
            PlayState(HitStateHash, true, hitLockDuration);
            lastHitAnimationTime = Time.time;
        }

        private void HandlePlayerPrimaryAttackFired(Vector3 direction)
        {
            if (isDead)
            {
                return;
            }

            TriggerAttackAnimation(
                playerAttackLocksDirectState ? attackLockDuration : 0f,
                playerAttackLocksDirectState);
            playerAttackVisualUntil = Time.time + Mathf.Max(0.01f, playerAttackBoolDuration);
        }

        public void TriggerSkillCastAnimation(float visualDuration)
        {
            if (isDead)
            {
                return;
            }

            TrySetTrigger(SkillCastHash);
            skillCastVisualUntil = Time.time + Mathf.Max(0.01f, Mathf.Max(skillCastBoolDuration, visualDuration));
        }

        private bool IsAttackVisualActive()
        {
            return Time.time < playerAttackVisualUntil || Time.time < skillCastVisualUntil;
        }

        private bool IsLocalEnemyAttackEvent(EnemyAttackEventArgs eventArgs)
        {
            return !isPlayer &&
                enemyAttack != null &&
                eventArgs.Source == enemyAttack.gameObject;
        }

        private void TriggerAttackAnimation(float lockDuration, bool playDirectState = true)
        {
            TrySetTrigger(AttackHash);
            if (playDirectState)
            {
                PlayState(AttackStateHash, true, lockDuration);
            }

            if (!isPlayer)
            {
                lastEnemyAttackAnimationTime = Time.time;
            }
        }

        private void RefreshDirectLocomotionState(bool dashing, bool moving)
        {
            if (!directStatePlayback || animator == null || isDead)
            {
                return;
            }

            if (Time.time < stateLockUntil)
            {
                return;
            }

            if (dashing)
            {
                PlayState(DashStateHash, false);
                return;
            }

            PlayState(moving ? MoveStateHash : IdleStateHash, false);
        }

        private void PlayState(int stateHash, bool forceRestart, float lockDuration = 0f)
        {
            if (!directStatePlayback || animator == null || animator.runtimeAnimatorController == null)
            {
                return;
            }

            if (!forceRestart && currentStateHash == stateHash)
            {
                return;
            }

            if (!animator.HasState(0, stateHash))
            {
                return;
            }

            currentStateHash = stateHash;
            if (lockDuration > 0f)
            {
                stateLockUntil = Time.time + lockDuration;
            }

            if (stateCrossFadeDuration > 0f)
            {
                animator.CrossFadeInFixedTime(stateHash, stateCrossFadeDuration, 0, 0f);
                return;
            }

            animator.Play(stateHash, 0, 0f);
        }

        private float ResolveMoveSpeed()
        {
            if (isPlayer && playerMovement != null)
            {
                return playerMovement.IsMoving ? 1f : 0f;
            }

            if (!isPlayer && enemyMovement != null)
            {
                if (enemyMovement.IsMoving)
                {
                    return Mathf.Max(0.01f, enemyMovement.NormalizedMoveSpeed);
                }

                return IsEnemyMovementActive() ? 1f : 0f;
            }

            if (cachedRigidbody != null)
            {
                var velocity = cachedRigidbody.linearVelocity;
                velocity.y = 0f;
                return velocity.magnitude;
            }

            return 0f;
        }

        private bool IsMoving(float moveSpeed)
        {
            if (!isPlayer && enemyMovement != null)
            {
                return IsEnemyMovementActive();
            }

            if (moveSpeed > 0.05f)
            {
                return true;
            }

            if (isPlayer && playerMovement != null)
            {
                return playerMovement.IsMoving;
            }

            return false;
        }

        private bool IsEnemyMovementActive()
        {
            return enemyMovement != null &&
                (enemyMovement.IsMoving ||
                    enemyMovement.CanMove &&
                    enemyMovement.HasTarget &&
                    enemyMovement.Target != null &&
                    !enemyMovement.IsWithinStoppingDistanceToTarget);
        }

        private bool IsHealthDead()
        {
            if (playerHealth != null)
            {
                return !playerHealth.IsAlive;
            }

            if (enemyHealth != null)
            {
                return !enemyHealth.IsAlive;
            }

            return false;
        }

        private void RefreshAnimatorParameterCacheIfNeeded()
        {
            var controller = animator != null ? animator.runtimeAnimatorController : null;
            if (controller == cachedAnimatorController && animatorParameterHashes.Count > 0)
            {
                return;
            }

            cachedAnimatorController = controller;
            animatorParameterHashes.Clear();
            if (animator == null || controller == null)
            {
                return;
            }

            var parameters = animator.parameters;
            for (var i = 0; i < parameters.Length; i++)
            {
                animatorParameterHashes.Add(parameters[i].nameHash);
            }
        }

        private bool HasParameter(int parameterHash)
        {
            RefreshAnimatorParameterCacheIfNeeded();
            return animatorParameterHashes.Contains(parameterHash);
        }

        private void TrySetBool(int parameterHash, bool value)
        {
            if (animator != null && HasParameter(parameterHash))
            {
                animator.SetBool(parameterHash, value);
            }
        }

        private void TrySetFloat(int parameterHash, float value, float deltaTime)
        {
            if (animator != null && HasParameter(parameterHash))
            {
                animator.SetFloat(parameterHash, value, speedDampTime, deltaTime);
            }
        }

        private void TrySetTrigger(int parameterHash)
        {
            if (animator != null && HasParameter(parameterHash))
            {
                animator.SetTrigger(parameterHash);
            }
        }

        private void TryResetTrigger(int parameterHash)
        {
            if (animator != null && HasParameter(parameterHash))
            {
                animator.ResetTrigger(parameterHash);
            }
        }

        private void ResetRuntimeAnimationState()
        {
            ResolveReferences();
            isDead = false;
            stateLockUntil = 0f;
            playerAttackVisualUntil = 0f;
            skillCastVisualUntil = 0f;
            lastEnemyAttackAnimationTime = -999f;
            lastHitAnimationTime = -999f;
            currentStateHash = 0;
            wasPlayerAttackReady = playerAttack == null || playerAttack.IsCooldownReady;
            wasEnemyAttackReady = enemyAttack == null || enemyAttack.IsCooldownReady;
            wasDashing = false;

            if (animator == null)
            {
                return;
            }

            RefreshAnimatorParameterCacheIfNeeded();
            TryResetTrigger(AttackHash);
            TryResetTrigger(SkillCastHash);
            TryResetTrigger(DashHash);
            TryResetTrigger(HitHash);
            TryResetTrigger(DeathHash);
            TrySetBool(IsMovingHash, false);
            TrySetBool(IsDashingHash, false);
            TrySetBool(IsAttackingHash, false);
            TrySetFloat(MoveSpeedHash, 0f, 0f);

            if (animator.runtimeAnimatorController == null)
            {
                return;
            }

            if (!animator.isActiveAndEnabled || !animator.gameObject.activeInHierarchy)
            {
                return;
            }

            animator.Rebind();
            animator.Update(0f);
            PlayState(IdleStateHash, true);
        }

        private void LogDebugState(string reason, float moveSpeed, bool moving, bool dashing)
        {
            if (!logDebug || Time.unscaledTime < nextDebugLogTime)
            {
                return;
            }

            nextDebugLogTime = Time.unscaledTime + 0.5f;
            Debug.Log(
                $"{nameof(CharacterAnimationDriver)} on {name}: {reason}. " +
                $"Animator={(animator != null ? animator.name : "None")}, " +
                $"Controller={(animator != null && animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "None")}, " +
                $"IsPlayer={isPlayer}, PlayerMovement={(playerMovement != null ? playerMovement.name : "None")}, " +
                $"PlayerAttack={(playerAttack != null ? playerAttack.name : "None")}, " +
                $"PlayerDash={(playerDash != null ? playerDash.name : "None")}, " +
                $"PlayerHealthAlive={(playerHealth != null ? playerHealth.IsAlive.ToString() : "None")}, " +
                $"EnemyMovement={(enemyMovement != null ? enemyMovement.name : "None")}, " +
                $"EnemyAttack={(enemyAttack != null ? enemyAttack.name : "None")}, " +
                $"EnemyHealthAlive={(enemyHealth != null ? enemyHealth.IsAlive.ToString() : "None")}, " +
                $"EnemyKnockback={(enemyKnockbackReceiver != null ? enemyKnockbackReceiver.name : "None")}, " +
                $"MoveSpeed={moveSpeed:0.###}, IsMoving={moving}, IsDashing={dashing}, IsDead={isDead}.",
                this);
        }
    }
}
