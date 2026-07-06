using System;
using System.Collections;
using System.Collections.Generic;
using TapKnockout.Ability;
using TapKnockout.Combat;
using TapKnockout.Enemy;
using TapKnockout.Input;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Survivor
{
    public readonly struct ActiveSkillSlotState
    {
        public ActiveSkillSlotState(int slotIndex, ActiveSkillSlot slot)
        {
            SlotIndex = slotIndex;
            Ability = slot != null ? slot.Ability : null;
            HotkeyLabel = slot != null ? slot.HotkeyLabel : string.Empty;
            CooldownRemaining = slot != null ? slot.CooldownRemaining : 0f;
            CooldownDuration = slot != null ? slot.CooldownDuration : 0f;
            NormalizedCooldown = slot != null ? slot.NormalizedCooldown : 0f;
            IsReady = slot != null && slot.IsReady;
            IsCasting = slot != null && slot.IsCasting;
        }

        public int SlotIndex { get; }
        public AbilityDefinition Ability { get; }
        public string HotkeyLabel { get; }
        public float CooldownRemaining { get; }
        public float CooldownDuration { get; }
        public float NormalizedCooldown { get; }
        public bool IsReady { get; }
        public bool IsCasting { get; }
    }

    public readonly struct ActiveSkillFeedbackEventArgs
    {
        public ActiveSkillFeedbackEventArgs(
            GameObject source,
            AbilityDefinition ability,
            ActiveSkillEffectType effectType,
            ActiveSkillFeedbackPhase phase,
            Vector3 position,
            Quaternion rotation,
            float scale,
            float lifetime,
            bool hasDirectPrefab)
        {
            Source = source;
            Ability = ability;
            EffectType = effectType;
            Phase = phase;
            Position = position;
            Rotation = rotation;
            Scale = Mathf.Max(0.05f, scale);
            Lifetime = Mathf.Max(0f, lifetime);
            HasDirectPrefab = hasDirectPrefab;
        }

        public GameObject Source { get; }
        public AbilityDefinition Ability { get; }
        public ActiveSkillEffectType EffectType { get; }
        public ActiveSkillFeedbackPhase Phase { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public float Scale { get; }
        public float Lifetime { get; }
        public bool HasDirectPrefab { get; }
    }

    public static class ActiveSkillFeedbackEvents
    {
        public static event Action<ActiveSkillFeedbackEventArgs> OnFeedbackRequested;

        public static void RaiseFeedbackRequested(ActiveSkillFeedbackEventArgs eventArgs)
        {
            OnFeedbackRequested?.Invoke(eventArgs);
        }
    }

    [DisallowMultipleComponent]
    public sealed class ActiveSkillController : MonoBehaviour
    {
        private const int DefaultSlotCount = 4;

        [Header("References")]
        [SerializeField] private DesktopSurvivorInputBridge inputBridge;
        [SerializeField] private AbilitySelectionController abilitySelectionController;
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerRuntimeStats runtimeStats;
        [SerializeField] private MouseAimController mouseAimController;
        [SerializeField] private PlayerConfig playerConfig;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Transform castOrigin;
        [SerializeField] private SurvivorFeedbackPlayer feedbackPlayer;

        [Header("Targeting")]
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField, Range(8, 128)] private int hitBufferSize = 48;

        [Header("Slots")]
        [SerializeField] private ActiveSkillSlot[] slots =
        {
            new ActiveSkillSlot("Q", ActiveSkillEffectType.ForwardCleave),
            new ActiveSkillSlot("E", ActiveSkillEffectType.GroundImpact),
            new ActiveSkillSlot("R", ActiveSkillEffectType.ForwardCleave),
            new ActiveSkillSlot("F", ActiveSkillEffectType.GroundImpact)
        };
        [SerializeField] private bool allowFallbackSkillsWhenEmpty = true;
        [SerializeField] private bool autoEquipSelectedActiveAbilities = true;

        [Header("Input Buffer")]
        [SerializeField, Min(0f)] private float fallbackSkillInputBufferSeconds = 0.15f;

        [Header("Debug")]
        [SerializeField] private bool logEmptySlotWarnings = true;
        [SerializeField] private bool logCasts;

        private readonly HashSet<GameObject> hitObjects = new HashSet<GameObject>();
        private Collider[] hitBuffer;
        private bool subscribedToInput;
        private bool subscribedToAbilitySelection;
        private float[] bufferedInputRemaining;

        public event Action<ActiveSkillSlotState> OnSlotStateChanged;

        public int SlotCount => slots != null ? slots.Length : 0;

        private float SkillInputBufferSeconds =>
            playerConfig != null ? playerConfig.SkillInputBufferSeconds : fallbackSkillInputBufferSeconds;

        private void Reset()
        {
            ResolveReferences();
            EnsureSlots();
        }

        private void Awake()
        {
            ResolveReferences();
            EnsureSlots();
            EnsureHitBuffer();
            EnsureInputBuffer();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeInput();
            SubscribeAbilitySelection();
            RefreshAllSlotStates();
        }

        private void OnDisable()
        {
            UnsubscribeInput();
            UnsubscribeAbilitySelection();
            if (movementController != null)
            {
                movementController.SetMovementLocked(false);
            }
            ClearAllBufferedInputs();
        }
        private void ClearAllBufferedInputs()
        {
            if (bufferedInputRemaining == null)
            {
                return;
            }

            for (var i = 0; i < bufferedInputRemaining.Length; i++)
            {
                bufferedInputRemaining[i] = 0f;
            }
        }

        private void OnValidate()
        {
            hitBufferSize = Mathf.Clamp(hitBufferSize, 8, 128);
            fallbackSkillInputBufferSeconds = Mathf.Max(0f, fallbackSkillInputBufferSeconds);
            EnsureSlots();
            EnsureInputBuffer();
        }

        private void Update()
        {
            if (slots == null)
            {
                return;
            }

            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null)
                {
                    continue;
                }

                var before = slots[i].NormalizedCooldown;
                slots[i].Tick(Time.deltaTime);
                if (!Mathf.Approximately(before, slots[i].NormalizedCooldown))
                {
                    RaiseSlotChanged(i);
                }
            }

            TickBufferedInputs(Time.deltaTime);
        }

        public void Configure(
            DesktopSurvivorInputBridge bridge,
            AbilitySelectionController selectionController,
            PlayerMovementController playerMovement,
            PlayerRuntimeStats stats,
            MouseAimController aimController,
            Transform origin,
            SurvivorFeedbackPlayer feedback = null)
        {
            UnsubscribeInput();
            UnsubscribeAbilitySelection();
            inputBridge = bridge;
            abilitySelectionController = selectionController;
            movementController = playerMovement;
            runtimeStats = stats;
            mouseAimController = aimController;
            playerConfig = playerMovement != null ? playerMovement.Config : playerConfig;
            playerHealth = GetComponent<PlayerHealth>();
            castOrigin = origin;
            feedbackPlayer = feedback != null ? feedback : feedbackPlayer;
            SubscribeInput();
            SubscribeAbilitySelection();
        }

        public ActiveSkillSlotState GetSlotState(int slotIndex)
        {
            if (!IsValidSlot(slotIndex))
            {
                return new ActiveSkillSlotState(slotIndex, null);
            }

            return new ActiveSkillSlotState(slotIndex, slots[slotIndex]);
        }

        public bool SetSlotAbility(int slotIndex, AbilityDefinition ability)
        {
            if (!IsValidSlot(slotIndex))
            {
                return false;
            }

            slots[slotIndex].SetAbility(ability);
            RaiseSlotChanged(slotIndex);
            return true;
        }

        public bool TryCastSlot(int slotIndex)
        {
            if (!CanCastFromGlobalState())
            {
                return false;
            }

            if (!IsValidSlot(slotIndex))
            {
                return false;
            }

            var slot = slots[slotIndex];
            if (slot.Ability == null && !allowFallbackSkillsWhenEmpty)
            {
                if (logEmptySlotWarnings)
                {
                    Debug.Log($"Active skill slot {slotIndex + 1} is empty.", this);
                }

                return false;
            }

            if (!slot.IsReady)
            {
                return false;
            }

            var effectType = slot.ResolveEffectType();
            if (effectType == ActiveSkillEffectType.None)
            {
                return false;
            }

            slot.BeginCast();
            RaiseSlotChanged(slotIndex);

            var castTime = slot.ResolveCastTime();
            var effectDelay = slot.ResolveEffectDelay();
            var feedbackLeadTime = castTime + effectDelay;
            NotifySkillCastAnimation(feedbackLeadTime);
            PlaySkillFeedback(slot, ActiveSkillFeedbackPhase.Cast, feedbackLeadTime);
            if (feedbackLeadTime > 0f || effectType == ActiveSkillEffectType.GroundImpact)
            {
                PlaySkillFeedback(slot, ActiveSkillFeedbackPhase.Telegraph, feedbackLeadTime);
            }

            if (castTime > 0f || effectDelay > 0f)
            {
                StartCoroutine(CastAfterDelay(slotIndex, castTime, effectDelay));
                return true;
            }

            ResolveCast(slotIndex);
            return true;
        }

        private IEnumerator CastAfterDelay(int slotIndex, float castTime, float effectDelay)
        {
            var slot = slots[slotIndex];
            if (slot.LockMovementDuringCast && movementController != null)
            {
                movementController.SetMovementLocked(true);
            }

            yield return new WaitForSeconds(castTime + effectDelay);

            if (slot.LockMovementDuringCast && movementController != null)
            {
                movementController.SetMovementLocked(false);
            }

            if (!CanCastFromGlobalState())
            {
                slot.CancelCast();
                RaiseSlotChanged(slotIndex);
                yield break;
            }

            ResolveCast(slotIndex);
        }

        private void ResolveCast(int slotIndex)
        {
            if (!IsValidSlot(slotIndex))
            {
                return;
            }

            var slot = slots[slotIndex];
            var effectType = slot.ResolveEffectType();
            var resolved = effectType switch
            {
                ActiveSkillEffectType.ForwardCleave => ResolveForwardCleave(slot),
                ActiveSkillEffectType.GroundImpact => ResolveGroundImpact(slot),
                _ => false
            };

            PlaySkillFeedback(slot, ActiveSkillFeedbackPhase.Impact, 0f);
            slot.CompleteCast();
            RaiseSlotChanged(slotIndex);

            if (logCasts)
            {
                var abilityId = slot.Ability != null ? slot.Ability.AbilityId : $"fallback_slot_{slotIndex + 1}";
                Debug.Log($"{nameof(ActiveSkillController)} cast {abilityId}. Resolved hit: {resolved}.", this);
            }
        }

        private bool ResolveForwardCleave(ActiveSkillSlot slot)
        {
            var origin = ResolveOrigin(slot);
            var direction = ResolveAimDirection(slot, origin);
            var range = slot.ResolveRange();
            var coneAngle = slot.ResolveConeAngle();
            var damage = ResolveDamage(slot);
            var knockbackForce = slot.ResolveKnockbackForce();
            var knockbackDuration = slot.ResolveKnockbackDuration();

            return ResolveAreaHits(
                origin,
                range,
                candidate =>
                {
                    var offset = candidate - origin;
                    offset.y = 0f;
                    if (offset.sqrMagnitude > range * range)
                    {
                        return false;
                    }

                    var candidateDirection = offset.sqrMagnitude > 0.0001f ? offset.normalized : direction;
                    return Vector3.Angle(direction, candidateDirection) <= coneAngle * 0.5f;
                },
                damage,
                direction,
                knockbackForce,
                knockbackDuration,
                slot);
        }

        private bool ResolveGroundImpact(ActiveSkillSlot slot)
        {
            var origin = ResolveOrigin(slot);
            var radius = slot.ResolveRadius();
            var damage = ResolveDamage(slot);
            var knockbackForce = slot.ResolveKnockbackForce();
            var knockbackDuration = slot.ResolveKnockbackDuration();

            return ResolveAreaHits(
                origin,
                radius,
                candidate =>
                {
                    var offset = candidate - origin;
                    offset.y = 0f;
                    return offset.sqrMagnitude <= radius * radius;
                },
                damage,
                Vector3.zero,
                knockbackForce,
                knockbackDuration,
                slot);
        }

        private bool ResolveAreaHits(
            Vector3 origin,
            float queryRadius,
            Func<Vector3, bool> candidateFilter,
            float damage,
            Vector3 fallbackDirection,
            float knockbackForce,
            float knockbackDuration,
            ActiveSkillSlot slot)
        {
            if (targetLayers.value == 0)
            {
                return false;
            }

            EnsureHitBuffer();
            hitObjects.Clear();
            var hitAny = false;
            var hitCount = Physics.OverlapSphereNonAlloc(
                origin,
                Mathf.Max(0.1f, queryRadius),
                hitBuffer,
                targetLayers,
                QueryTriggerInteraction.Collide);

            for (var i = 0; i < hitCount; i++)
            {
                var candidateCollider = hitBuffer[i];
                if (candidateCollider == null || !candidateCollider.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var damageable = candidateCollider.GetComponentInParent<IDamageable>();
                if (damageable == null || !damageable.IsAlive || damageable.GameObject == gameObject)
                {
                    continue;
                }

                var targetObject = damageable.GameObject != null ? damageable.GameObject : candidateCollider.gameObject;
                if (targetObject == null ||
                    !targetObject.activeInHierarchy ||
                    IsSelf(targetObject.transform) ||
                    !hitObjects.Add(targetObject))
                {
                    continue;
                }

                var hitPosition = ResolveHitPosition(candidateCollider, targetObject);
                if (!candidateFilter(hitPosition))
                {
                    continue;
                }

                var hitDirection = hitPosition - origin;
                hitDirection.y = 0f;
                if (hitDirection.sqrMagnitude <= 0.0001f)
                {
                    hitDirection = fallbackDirection.sqrMagnitude > 0.0001f ? fallbackDirection.normalized : transform.forward;
                }
                else
                {
                    hitDirection.Normalize();
                }

                var hitContext = new HitContext(gameObject, targetObject, damage, DamageType.Physical)
                {
                    IsAbilityHit = true,
                    AbilityId = slot.Ability != null ? slot.Ability.AbilityId : slot.ResolveEffectType().ToString(),
                    HitPoint = hitPosition,
                    HitDirection = hitDirection,
                    Knockback = knockbackForce > 0f && knockbackDuration > 0f
                        ? new KnockbackData(hitDirection, knockbackForce, knockbackDuration)
                        : KnockbackData.None
                };

                CombatHitModifierUtility.ApplySourceModifiers(hitContext);
                damageable.ReceiveHit(hitContext);
                RaiseDamageEvents(hitContext);
                hitAny = true;
            }

            return hitAny;
        }

        private float ResolveDamage(ActiveSkillSlot slot)
        {
            var damage = slot.ResolveDamage();
            if (runtimeStats != null)
            {
                damage *= runtimeStats.AttackDamageMultiplier;
            }

            return Mathf.Max(0f, damage);
        }

        private Vector3 ResolveOrigin()
        {
            return castOrigin != null ? castOrigin.position : transform.position;
        }

        private Vector3 ResolveOrigin(ActiveSkillSlot slot)
        {
            if (slot == null)
            {
                return ResolveOrigin();
            }

            if (slot.TargetMode == ActiveSkillTargetMode.MouseWorldPoint ||
                slot.OriginMode == ActiveSkillOriginMode.MouseWorldPoint)
            {
                if (mouseAimController != null && mouseAimController.TryGetAimPoint(out var aimPoint))
                {
                    return aimPoint;
                }
            }

            if (slot.TargetMode == ActiveSkillTargetMode.NearestTarget ||
                slot.OriginMode == ActiveSkillOriginMode.NearestTarget)
            {
                if (TryFindNearestTarget(ResolveOrigin(), slot.ResolveRange(), out var targetPosition))
                {
                    return targetPosition;
                }
            }

            return slot.OriginMode switch
            {
                ActiveSkillOriginMode.CastOrigin => ResolveOrigin(),
                ActiveSkillOriginMode.Player => transform.position,
                _ => ResolveOrigin()
            };
        }

        private Vector3 ResolveAimDirection()
        {
            return ResolveAimDirection(null, ResolveOrigin());
        }

        private Vector3 ResolveAimDirection(ActiveSkillSlot slot, Vector3 origin)
        {
            var aimMode = slot != null ? slot.AimMode : ActiveSkillAimMode.MouseAim;

            switch (aimMode)
            {
                case ActiveSkillAimMode.NearestTarget:
                    if (TryFindNearestTarget(origin, slot != null ? slot.ResolveRange() : 8f, out var targetPosition))
                    {
                        var toTarget = targetPosition - origin;
                        toTarget.y = 0f;
                        if (toTarget.sqrMagnitude > 0.0001f)
                        {
                            return toTarget.normalized;
                        }
                    }

                    return ResolveFacingDirection();

                case ActiveSkillAimMode.MovementDirection:
                    if (movementController != null &&
                        movementController.CurrentMoveDirection.sqrMagnitude > 0.0001f)
                    {
                        return movementController.CurrentMoveDirection.normalized;
                    }

                    return ResolveFacingDirection();

                case ActiveSkillAimMode.FacingDirection:
                    return ResolveFacingDirection();

                case ActiveSkillAimMode.HybridMouseAimThenTarget:
                    if (TryResolveMouseAimDirection(out var hybridMouseAim))
                    {
                        return hybridMouseAim;
                    }

                    if (TryFindNearestTarget(origin, slot != null ? slot.ResolveRange() : 8f, out var hybridTargetPosition))
                    {
                        var toHybridTarget = hybridTargetPosition - origin;
                        toHybridTarget.y = 0f;
                        if (toHybridTarget.sqrMagnitude > 0.0001f)
                        {
                            return toHybridTarget.normalized;
                        }
                    }

                    return ResolveFacingDirection();

                case ActiveSkillAimMode.MouseAim:
                default:
                    if (TryResolveMouseAimDirection(out var mouseAim))
                    {
                        return mouseAim;
                    }

                    return ResolveFacingDirection();
            }
        }
        private bool TryResolveMouseAimDirection(out Vector3 direction)
        {
            if (mouseAimController != null &&
                mouseAimController.TryGetAimDirection(out direction) &&
                direction.sqrMagnitude > 0.0001f)
            {
                direction.y = 0f;
                direction.Normalize();
                return true;
            }

            direction = Vector3.zero;
            return false;
        }

        private Vector3 ResolveFacingDirection()
        {
            var forward = transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }

        private bool TryFindNearestTarget(Vector3 origin, float range, out Vector3 targetPosition)
        {
            targetPosition = Vector3.zero;

            if (targetLayers.value == 0)
            {
                return false;
            }

            EnsureHitBuffer();

            var hitCount = Physics.OverlapSphereNonAlloc(
                origin,
                Mathf.Max(0.1f, range),
                hitBuffer,
                targetLayers,
                QueryTriggerInteraction.Collide);

            var bestSqrDistance = float.PositiveInfinity;
            var found = false;

            for (var i = 0; i < hitCount; i++)
            {
                var candidateCollider = hitBuffer[i];
                if (candidateCollider == null ||
                    !candidateCollider.gameObject.activeInHierarchy ||
                    IsSelf(candidateCollider.transform))
                {
                    continue;
                }

                var damageable = candidateCollider.GetComponentInParent<IDamageable>();
                if (damageable == null || !damageable.IsAlive)
                {
                    continue;
                }

                var targetObject = damageable.GameObject != null ? damageable.GameObject : candidateCollider.gameObject;
                if (targetObject == null || !targetObject.activeInHierarchy || IsSelf(targetObject.transform))
                {
                    continue;
                }

                var candidatePosition = ResolveHitPosition(candidateCollider, targetObject);
                var offset = candidatePosition - origin;
                offset.y = 0f;

                var sqrDistance = offset.sqrMagnitude;
                if (sqrDistance >= bestSqrDistance)
                {
                    continue;
                }

                bestSqrDistance = sqrDistance;
                targetPosition = candidatePosition;
                found = true;
            }

            return found;
        }
        private Vector3 ResolveHitPosition(Collider candidateCollider, GameObject targetObject)
        {
            if (targetObject != null)
            {
                return targetObject.transform.position;
            }

            return candidateCollider != null ? candidateCollider.ClosestPoint(ResolveOrigin()) : ResolveOrigin();
        }

        private bool IsSelf(Transform candidate)
        {
            return candidate == transform || candidate != null && candidate.IsChildOf(transform);
        }

        private void ResolveReferences()
        {
            if (inputBridge == null)
            {
                inputBridge = GetComponent<DesktopSurvivorInputBridge>();
            }

            if (movementController == null)
            {
                movementController = GetComponent<PlayerMovementController>();
            }

            if (runtimeStats == null)
            {
                runtimeStats = GetComponent<PlayerRuntimeStats>();
            }

            if (mouseAimController == null)
            {
                mouseAimController = GetComponent<MouseAimController>();
            }

            if (playerConfig == null && movementController != null)
            {
                playerConfig = movementController.Config;
            }

            if (playerHealth == null)
            {
                playerHealth = GetComponent<PlayerHealth>();
            }

            if (castOrigin == null)
            {
                castOrigin = transform;
            }

            if (feedbackPlayer == null)
            {
                feedbackPlayer = GetComponent<SurvivorFeedbackPlayer>();
            }
        }

        private void SubscribeInput()
        {
            if (subscribedToInput || inputBridge == null)
            {
                return;
            }

            inputBridge.OnActiveSkillPressed += HandleActiveSkillPressed;
            subscribedToInput = true;
        }

        private void UnsubscribeInput()
        {
            if (!subscribedToInput || inputBridge == null)
            {
                return;
            }

            inputBridge.OnActiveSkillPressed -= HandleActiveSkillPressed;
            subscribedToInput = false;
        }

        private void SubscribeAbilitySelection()
        {
            if (subscribedToAbilitySelection || abilitySelectionController == null)
            {
                return;
            }

            abilitySelectionController.OnAbilitySelected += HandleAbilitySelected;
            subscribedToAbilitySelection = true;
        }

        private void UnsubscribeAbilitySelection()
        {
            if (!subscribedToAbilitySelection || abilitySelectionController == null)
            {
                return;
            }

            abilitySelectionController.OnAbilitySelected -= HandleAbilitySelected;
            subscribedToAbilitySelection = false;
        }

        private void HandleActiveSkillPressed(int slotIndex)
        {
            if (TryCastSlot(slotIndex))
            {
                ClearBufferedInput(slotIndex);
                return;
            }

            TryBufferSlotInput(slotIndex);
        }

        private void HandleAbilitySelected(AbilitySelectedEventArgs eventArgs)
        {
            if (!autoEquipSelectedActiveAbilities ||
                eventArgs.SelectedAbility == null ||
                !LooksLikeActiveSkill(eventArgs.SelectedAbility))
            {
                return;
            }

            var slotIndex = FindSlotForAbility(eventArgs.SelectedAbility);
            if (slotIndex >= 0)
            {
                SetSlotAbility(slotIndex, eventArgs.SelectedAbility);
            }
        }

        private int FindSlotForAbility(AbilityDefinition ability)
        {
            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null && slots[i].Ability == ability)
                {
                    return i;
                }
            }

            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null && slots[i].Ability == null)
                {
                    return i;
                }
            }

            return slots.Length > 0 ? 0 : -1;
        }

        private static bool LooksLikeActiveSkill(AbilityDefinition ability)
        {
            if (ability.Cooldown > 0f)
            {
                return true;
            }

            switch (ability.EffectType)
            {
                case AbilityEffectType.EnergyRing:
                case AbilityEffectType.EnergyBeam:
                case AbilityEffectType.DashBeam:
                case AbilityEffectType.ChargedShot:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsValidSlot(int slotIndex)
        {
            return slots != null && slotIndex >= 0 && slotIndex < slots.Length && slots[slotIndex] != null;
        }

        private void EnsureSlots()
        {
            if (slots == null || slots.Length != DefaultSlotCount)
            {
                var previous = slots;
                slots = new ActiveSkillSlot[DefaultSlotCount];
                if (previous != null)
                {
                    for (var i = 0; i < Mathf.Min(previous.Length, slots.Length); i++)
                    {
                        slots[i] = previous[i];
                    }
                }
            }

            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null)
                {
                    slots[i] = i switch
                    {
                        1 => new ActiveSkillSlot("E", ActiveSkillEffectType.GroundImpact),
                        2 => new ActiveSkillSlot("R", ActiveSkillEffectType.ForwardCleave),
                        3 => new ActiveSkillSlot("F", ActiveSkillEffectType.GroundImpact),
                        _ => new ActiveSkillSlot("Q", ActiveSkillEffectType.ForwardCleave)
                    };
                }

                slots[i].EnsureFeedbackConfig();
            }

            slots[0].SetFallbackDefaults("Q", ActiveSkillEffectType.ForwardCleave);
            slots[1].SetFallbackDefaults("E", ActiveSkillEffectType.GroundImpact);
            slots[2].SetFallbackDefaults("R", ActiveSkillEffectType.ForwardCleave);
            slots[3].SetFallbackDefaults("F", ActiveSkillEffectType.GroundImpact);
        }

        private void PlaySkillFeedback(ActiveSkillSlot slot, ActiveSkillFeedbackPhase phase, float requestedLifetime)
        {
            if (slot == null || slot.Feedback == null)
            {
                return;
            }

            var effectType = slot.ResolveEffectType();
            var origin = ResolveOrigin(slot);
            var direction = ResolveAimDirection(slot, origin);
            var position = ResolveFeedbackPosition(effectType, phase, origin, direction, slot);
            var rotation = direction.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(direction, Vector3.up)
                : transform.rotation;
            var scale = ResolveFeedbackScale(effectType, phase, slot);
            var lifetime = requestedLifetime > 0f ? requestedLifetime : slot.Feedback.ResolveVFXLifetime(slot.Ability);
            var hasDirectPrefab = slot.Feedback.ResolveVFXPrefab(slot.Ability, phase) != null;

            ActiveSkillFeedbackEvents.RaiseFeedbackRequested(new ActiveSkillFeedbackEventArgs(
                gameObject,
                slot.Ability,
                effectType,
                phase,
                position,
                rotation,
                scale,
                lifetime,
                hasDirectPrefab));

            var player = ResolveFeedbackPlayer(slot);
            if (player == null)
            {
                return;
            }

            player.Play(slot.Feedback, phase, position, rotation, null, scale, lifetime, gameObject, slot.Ability);
        }

        private SurvivorFeedbackPlayer ResolveFeedbackPlayer(ActiveSkillSlot slot)
        {
            if (feedbackPlayer != null)
            {
                return feedbackPlayer;
            }

            if (slot == null || slot.Feedback == null)
            {
                return null;
            }

            if (slot.Feedback.ResolveVFXPrefab(slot.Ability, ActiveSkillFeedbackPhase.Cast) == null &&
                slot.Feedback.ResolveVFXPrefab(slot.Ability, ActiveSkillFeedbackPhase.Telegraph) == null &&
                slot.Feedback.ResolveVFXPrefab(slot.Ability, ActiveSkillFeedbackPhase.Impact) == null &&
                slot.Feedback.ResolveSFX(slot.Ability, ActiveSkillFeedbackPhase.Cast) == null &&
                slot.Feedback.ResolveSFX(slot.Ability, ActiveSkillFeedbackPhase.Impact) == null &&
                slot.Feedback.ResolveLoopSFX(slot.Ability) == null &&
                slot.Feedback.ResolveCameraShakeIntensity(slot.Ability) <= 0f)
            {
                return null;
            }

            feedbackPlayer = SurvivorFeedbackPlayer.Shared;
            return feedbackPlayer;
        }

        private Vector3 ResolveFeedbackPosition(
            ActiveSkillEffectType effectType,
            ActiveSkillFeedbackPhase phase,
            Vector3 origin,
            Vector3 direction,
            ActiveSkillSlot slot)
        {
            if (effectType == ActiveSkillEffectType.ForwardCleave && phase != ActiveSkillFeedbackPhase.Cast)
            {
                return origin + direction * Mathf.Max(0.5f, slot.ResolveRange() * 0.5f);
            }

            return origin;
        }

        private static float ResolveFeedbackScale(ActiveSkillEffectType effectType, ActiveSkillFeedbackPhase phase, ActiveSkillSlot slot)
        {
            if (phase == ActiveSkillFeedbackPhase.Telegraph || phase == ActiveSkillFeedbackPhase.Impact)
            {
                return effectType == ActiveSkillEffectType.GroundImpact
                    ? Mathf.Max(0.1f, slot.ResolveRadius())
                    : Mathf.Max(0.1f, slot.ResolveRange());
            }

            return 1f;
        }

        private void EnsureHitBuffer()
        {
            if (hitBuffer == null || hitBuffer.Length != hitBufferSize)
            {
                hitBuffer = new Collider[hitBufferSize];
            }
        }

        private void RaiseSlotChanged(int slotIndex)
        {
            OnSlotStateChanged?.Invoke(GetSlotState(slotIndex));
        }

        private void RefreshAllSlotStates()
        {
            for (var i = 0; i < SlotCount; i++)
            {
                RaiseSlotChanged(i);
            }
        }

        private static void RaiseDamageEvents(HitContext hitContext)
        {
            CombatEvents.RaiseHitResolved(hitContext);
            if (hitContext.WasIgnored)
            {
                return;
            }

            var damageEvent = new DamageEvent(
                hitContext.Source,
                hitContext.Target,
                hitContext.DamageAmount,
                hitContext.DamageType,
                hitContext);
            CombatEvents.RaiseDamageDealt(damageEvent);
            CombatEvents.RaiseDamageReceived(damageEvent);
        }
        private bool CanCastFromGlobalState()
        {
            if (!isActiveAndEnabled)
            {
                return false;
            }

            if (playerHealth != null && !playerHealth.IsAlive)
            {
                return false;
            }

            if (Time.timeScale <= 0f)
            {
                return false;
            }

            return true;
        }

        private void EnsureInputBuffer()
        {
            var count = slots != null ? slots.Length : DefaultSlotCount;
            if (bufferedInputRemaining == null || bufferedInputRemaining.Length != count)
            {
                bufferedInputRemaining = new float[count];
            }
        }

        private void TickBufferedInputs(float deltaTime)
        {
            if (bufferedInputRemaining == null || slots == null)
            {
                return;
            }

            for (var i = 0; i < Mathf.Min(bufferedInputRemaining.Length, slots.Length); i++)
            {
                if (bufferedInputRemaining[i] <= 0f)
                {
                    continue;
                }

                bufferedInputRemaining[i] = Mathf.Max(0f, bufferedInputRemaining[i] - Mathf.Max(0f, deltaTime));

                if (bufferedInputRemaining[i] <= 0f)
                {
                    continue;
                }

                if (slots[i] != null && slots[i].IsReady && CanCastFromGlobalState())
                {
                    bufferedInputRemaining[i] = 0f;
                    TryCastSlot(i);
                }
            }
        }

        private bool TryBufferSlotInput(int slotIndex)
        {
            if (!IsValidSlot(slotIndex) || bufferedInputRemaining == null)
            {
                return false;
            }

            var bufferSeconds = SkillInputBufferSeconds;
            if (bufferSeconds <= 0f)
            {
                return false;
            }

            var slot = slots[slotIndex];
            if (slot == null || slot.IsCasting || slot.CooldownRemaining > bufferSeconds)
            {
                return false;
            }

            bufferedInputRemaining[slotIndex] = bufferSeconds;
            return true;
        }

        private void ClearBufferedInput(int slotIndex)
        {
            if (bufferedInputRemaining == null ||
                slotIndex < 0 ||
                slotIndex >= bufferedInputRemaining.Length)
            {
                return;
            }

            bufferedInputRemaining[slotIndex] = 0f;
        }

        private void NotifySkillCastAnimation(float duration)
        {
            var visualDuration = Mathf.Max(0.05f, duration);
            gameObject.BroadcastMessage(
                "TriggerSkillCastAnimation",
                visualDuration,
                SendMessageOptions.DontRequireReceiver);
        }
    }
}
