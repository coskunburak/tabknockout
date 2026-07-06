using System.IO;
using TapKnockout.UI.HUD;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TapKnockout.Editor.Tools
{
    /// <summary>
    /// Idempotent editor builder for the Player Health HUD top-left prefab.
    ///
    /// Menu: Tools/Tap Knockout/UI/Build Player Health HUD
    ///
    /// What it does:
    ///  1. Copies selected GabrielaTot health meter sprites to the project-owned art folder
    ///     and reconfigures their import settings as Sprites (without touching the originals).
    ///  2. Creates/updates PlayerHealthHudConfig.asset with correct sprite references.
    ///  3. Creates/updates PF_PlayerHealthHUD_TopLeft.prefab with the full child hierarchy,
    ///     correct anchors, images, text labels, and serialized component wiring.
    ///  4. Optionally places the prefab instance in the active scene's HUD Canvas
    ///     (idempotent — will not create duplicates).
    ///
    /// Run again at any time to repair missing children or update wiring.
    /// </summary>
    public static class PlayerHealthHudBuilder
    {
        // ──────────────────────────────────────────────────────────
        // Paths — source pack
        // ──────────────────────────────────────────────────────────

        private const string SourcePackFolder =
            "Assets/Assets/game asset packs/HEALTH METERS";

        // Primary selection: gold ornamental bar with red heart (health meter-28)
        private const string SrcFrameSprite       = "health meter-28.png";
        // HP fill sprite — designed to fit inside the meter-28 frame (parallelogram red bar)
        private const string SrcHpFillSprite      = "health meter-31.png";
        // Portrait frame: gold heart full (health meter-48)
        private const string SrcPortraitSprite    = "health meter-48.png";
        // Secondary bar frame: blue-heart gold frame (meter-29)
        private const string SrcSecondarySprite   = "health meter-29.png";

        // ──────────────────────────────────────────────────────────
        // Paths — project-owned destinations
        // ──────────────────────────────────────────────────────────

        private const string ArtFolder      = "Assets/_Project/Art/UI/HUD/PlayerHealthHUD";
        private const string GeneratedFolder = "Assets/_Project/Generated/UI/HUD";
        private const string PrefabFolder   = "Assets/_Project/Prefabs/UI";

        private const string DstFrameSprite     = "HM_GoldBar_Frame.png";
        private const string DstHpFillSprite    = "HM_HpFill_Red.png";
        private const string DstPortraitSprite  = "HM_PortraitFrame.png";
        private const string DstSecondarySprite = "HM_SecondaryBar_Frame.png";

        private const string ConfigAssetPath  = GeneratedFolder + "/PlayerHealthHudConfig.asset";
        private const string PrefabAssetPath  = PrefabFolder    + "/PF_PlayerHealthHUD_TopLeft.prefab";

        // HUD Canvas name searched in scene
        private const string HudCanvasName    = "HUD_Canvas";
        private const string PrefabInstanceName = "PF_PlayerHealthHUD_TopLeft";

        // ──────────────────────────────────────────────────────────
        // Menu Entry
        // ──────────────────────────────────────────────────────────

        [MenuItem("Tools/Tap Knockout/UI/Build Player Health HUD")]
        public static void BuildPlayerHealthHud()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Player Health HUD",
                    "Exit Play Mode before running the HUD builder.", "OK");
                return;
            }

            Debug.Log("[PlayerHealthHudBuilder] === Starting build ===");

            // Step 1: Copy and configure sprites
            var frameSprite    = EnsureSpriteCopied(SrcFrameSprite,     DstFrameSprite);
            var hpFillSprite   = EnsureSpriteCopied(SrcHpFillSprite,    DstHpFillSprite);
            var portraitSprite = EnsureSpriteCopied(SrcPortraitSprite,  DstPortraitSprite);
            var secondarySprite = EnsureSpriteCopied(SrcSecondarySprite, DstSecondarySprite);

            // Step 2: Create/update config asset
            var config = EnsureConfig(frameSprite, hpFillSprite, portraitSprite, secondarySprite);

            // Step 3: Create/update prefab
            var prefabGo = BuildOrUpdatePrefab(config);

            // Step 4: Place in scene (idempotent)
            if (prefabGo != null)
            {
                PlacePrefabInScene(prefabGo);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[PlayerHealthHudBuilder] === Done. Prefab: {PrefabAssetPath} | Config: {ConfigAssetPath} ===");

            EditorUtility.DisplayDialog("Player Health HUD",
                $"Build complete!\n\nPrefab: {PrefabAssetPath}\nConfig: {ConfigAssetPath}\n\nSee Console for details.",
                "OK");
        }

        // ──────────────────────────────────────────────────────────
        // Step 1 — Sprite Copy + Import Configuration
        // ──────────────────────────────────────────────────────────

        private static Sprite EnsureSpriteCopied(string srcFilename, string dstFilename)
        {
            EnsureFolder(ArtFolder);

            var srcPath = $"{SourcePackFolder}/{srcFilename}";
            var dstPath = $"{ArtFolder}/{dstFilename}";

            // Only copy if destination doesn't already exist
            if (!File.Exists(Path.Combine(Application.dataPath.Replace("Assets", ""), dstPath)))
            {
                if (!File.Exists(Path.Combine(Application.dataPath.Replace("Assets", ""), srcPath)))
                {
                    Debug.LogWarning($"[PlayerHealthHudBuilder] Source sprite not found: {srcPath}. Skipping copy.");
                    return null;
                }

                AssetDatabase.CopyAsset(srcPath, dstPath);
                Debug.Log($"[PlayerHealthHudBuilder] Copied {srcFilename} → {dstPath}");
            }

            // Configure import settings as Sprite (only modify dst, never src)
            ConfigureSpriteImport(dstPath);

            return AssetDatabase.LoadAssetAtPath<Sprite>(dstPath);
        }

        private static void ConfigureSpriteImport(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            var changed = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            // High quality pixels per unit for UI
            if ((int)importer.spritePixelsPerUnit != 100)
            {
                importer.spritePixelsPerUnit = 100;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
                Debug.Log($"[PlayerHealthHudBuilder] Configured sprite import: {assetPath}");
            }
        }

        // ──────────────────────────────────────────────────────────
        // Step 2 — Config ScriptableObject
        // ──────────────────────────────────────────────────────────

        private static PlayerHealthHudConfig EnsureConfig(
            Sprite frame, Sprite hpFill, Sprite portrait, Sprite secondary)
        {
            EnsureFolder(GeneratedFolder);

            var config = AssetDatabase.LoadAssetAtPath<PlayerHealthHudConfig>(ConfigAssetPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<PlayerHealthHudConfig>();
                AssetDatabase.CreateAsset(config, ConfigAssetPath);
                Debug.Log($"[PlayerHealthHudBuilder] Created config: {ConfigAssetPath}");
            }

            // Wire sprites via SerializedObject to respect Unity's serialization
            var so = new SerializedObject(config);
            SetSprite(so, "frameSprite",          frame);
            SetSprite(so, "fillSprite",           hpFill);   // meter-31: red parallelogram fill
            SetSprite(so, "portraitFrameSprite",  portrait);
            SetSprite(so, "secondaryFrameSprite", secondary); // meter-29: blue-heart frame
            SetSprite(so, "defaultPortraitSprite", null);     // placeholder, user assigns
            SetSprite(so, "buffSlotFrameSprite",   null);     // placeholder, user assigns
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(config);
            Debug.Log($"[PlayerHealthHudBuilder] Config sprites wired.");
            return config;
        }

        // ──────────────────────────────────────────────────────────
        // Step 3 — Prefab Build
        // ──────────────────────────────────────────────────────────

        private static GameObject BuildOrUpdatePrefab(PlayerHealthHudConfig config)
        {
            EnsureFolder(PrefabFolder);

            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabAssetPath);
            var isUpdate = existing != null;

            GameObject root;
            if (isUpdate)
            {
                root = PrefabUtility.LoadPrefabContents(PrefabAssetPath);
            }
            else
            {
                root = new GameObject(PrefabInstanceName, typeof(RectTransform));
            }

            try
            {
                RepairPrefab(root, config);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabAssetPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[PlayerHealthHudBuilder] {(isUpdate ? "Updated" : "Created")} prefab: {PrefabAssetPath}");
                return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabAssetPath);
            }
            finally
            {
                if (isUpdate)
                    PrefabUtility.UnloadPrefabContents(root);
                else
                    Object.DestroyImmediate(root);
            }
        }

        private static void RepairPrefab(GameObject root, PlayerHealthHudConfig config)
        {
            // ── Root RectTransform (anchored top-left, full-stretch as Canvas child)
            var rootRect = root.GetComponent<RectTransform>()
                           ?? root.AddComponent<RectTransform>();
            rootRect.anchorMin  = Vector2.zero;
            rootRect.anchorMax  = Vector2.one;
            rootRect.offsetMin  = Vector2.zero;
            rootRect.offsetMax  = Vector2.zero;

            // ── SafeAreaRoot (child of root)
            var safeAreaRoot = EnsureChild(root.transform, "SafeAreaRoot");
            var safeRect = safeAreaRoot.GetComponent<RectTransform>()
                           ?? safeAreaRoot.gameObject.AddComponent<RectTransform>();
            safeRect.anchorMin  = Vector2.zero;
            safeRect.anchorMax  = Vector2.one;
            safeRect.offsetMin  = Vector2.zero;
            safeRect.offsetMax  = Vector2.zero;

            // ── TopLeftAnchor (child of SafeAreaRoot)
            var topLeft = EnsureChild(safeAreaRoot, "TopLeftAnchor");
            var tlRect = topLeft.GetComponent<RectTransform>()
                         ?? topLeft.gameObject.AddComponent<RectTransform>();
            // Anchored to top-left corner
            tlRect.anchorMin  = new Vector2(0f, 1f);
            tlRect.anchorMax  = new Vector2(0f, 1f);
            tlRect.pivot      = new Vector2(0f, 1f);
            tlRect.anchoredPosition = new Vector2(12f, -12f);  // 12px margin from corner
            tlRect.sizeDelta  = new Vector2(480f, 140f);

            // ─────────────────────────────────────────────
            // Portrait Group
            // ─────────────────────────────────────────────
            var portraitGroup = EnsureChild(topLeft, "PortraitGroup");
            var pgRect = GetOrAddRect(portraitGroup);
            pgRect.anchorMin        = new Vector2(0f, 0f);
            pgRect.anchorMax        = new Vector2(0f, 1f);
            pgRect.pivot            = new Vector2(0f, 0.5f);
            pgRect.anchoredPosition = Vector2.zero;
            pgRect.sizeDelta        = new Vector2(110f, 0f);

            // Portrait Frame image (ornamental gold heart border)
            var portraitFrameImg = EnsureImage(portraitGroup, "PortraitFrame",
                new Color(1f, 1f, 1f, 1f));
            var pfRect = GetOrAddRect(portraitFrameImg.transform);
            pfRect.anchorMin  = Vector2.zero;
            pfRect.anchorMax  = Vector2.one;
            pfRect.offsetMin  = Vector2.zero;
            pfRect.offsetMax  = Vector2.zero;
            if (config.PortraitFrameSprite != null)
                portraitFrameImg.sprite = config.PortraitFrameSprite;
            portraitFrameImg.preserveAspect = true;

            // Portrait Image (character art)
            var portraitImg = EnsureImage(portraitGroup, "PortraitImage",
                new Color(0.5f, 0.5f, 0.5f, 0.9f));
            var piRect = GetOrAddRect(portraitImg.transform);
            piRect.anchorMin  = new Vector2(0.1f, 0.1f);
            piRect.anchorMax  = new Vector2(0.9f, 0.9f);
            piRect.offsetMin  = Vector2.zero;
            piRect.offsetMax  = Vector2.zero;
            portraitImg.preserveAspect = true;

            // Level Badge
            var levelBadge = EnsureChild(portraitGroup, "LevelBadge");
            var lbRect = GetOrAddRect(levelBadge);
            lbRect.anchorMin        = new Vector2(0.6f, 0f);
            lbRect.anchorMax        = new Vector2(1f, 0.35f);
            lbRect.offsetMin        = Vector2.zero;
            lbRect.offsetMax        = Vector2.zero;

            var badgeFrameImg = EnsureImage(levelBadge, "BadgeFrame",
                new Color(0.12f, 0.10f, 0.05f, 0.90f));
            var bfRect = GetOrAddRect(badgeFrameImg.transform);
            bfRect.anchorMin  = Vector2.zero;
            bfRect.anchorMax  = Vector2.one;
            bfRect.offsetMin  = Vector2.zero;
            bfRect.offsetMax  = Vector2.zero;

            var levelText = EnsureText(levelBadge, "LevelText", "1", 14, TextAnchor.MiddleCenter);
            var ltRect = GetOrAddRect(levelText.transform);
            ltRect.anchorMin  = Vector2.zero;
            ltRect.anchorMax  = Vector2.one;
            ltRect.offsetMin  = Vector2.zero;
            ltRect.offsetMax  = Vector2.zero;

            // ─────────────────────────────────────────────
            // Health Bar Group
            // ─────────────────────────────────────────────
            var healthBarGroup = EnsureChild(topLeft, "HealthBarGroup");
            var hbgRect = GetOrAddRect(healthBarGroup);
            hbgRect.anchorMin        = new Vector2(0f, 0.45f);
            hbgRect.anchorMax        = new Vector2(1f, 1f);
            hbgRect.offsetMin        = new Vector2(105f, 0f);
            hbgRect.offsetMax        = Vector2.zero;

            // Health Bar Frame (GabrielaTot ornamental gold frame — the selected asset)
            var healthBarFrameImg = EnsureImage(healthBarGroup, "HealthBarFrame",
                Color.white);
            var fiRect = GetOrAddRect(healthBarFrameImg.transform);
            fiRect.anchorMin  = Vector2.zero;
            fiRect.anchorMax  = Vector2.one;
            fiRect.offsetMin  = Vector2.zero;
            fiRect.offsetMax  = Vector2.zero;
            if (config.FrameSprite != null)
                healthBarFrameImg.sprite = config.FrameSprite;
            healthBarFrameImg.type           = Image.Type.Sliced;
            healthBarFrameImg.preserveAspect = false;

            // Health Bar Background (dark fill base \u2014 matches fill area of meter-28 frame)
            var bgImg = EnsureImage(healthBarGroup, "HealthBarBackground",
                new Color(0.06f, 0.04f, 0.04f, 0.65f));
            var bgRect = GetOrAddRect(bgImg.transform);
            bgRect.anchorMin  = new Vector2(0.22f, 0.10f);
            bgRect.anchorMax  = new Vector2(0.97f, 0.88f);
            bgRect.offsetMin  = Vector2.zero;
            bgRect.offsetMax  = Vector2.zero;

            // Damage Delay Fill (trails behind main fill — same sprite, tinted darker)
            var delayImg = EnsureImage(healthBarGroup, "HealthDamageDelayFill",
                config != null ? config.DamageDelayColor : new Color(0.55f, 0.18f, 0.05f, 0.90f));
            var delayRect = GetOrAddRect(delayImg.transform);
            // Fill area inside the meter-28 frame: starts after the heart icon (~22%), ends at right lip (~97%)
            delayRect.anchorMin  = new Vector2(0.22f, 0.10f);
            delayRect.anchorMax  = new Vector2(0.97f, 0.88f);
            delayRect.offsetMin  = Vector2.zero;
            delayRect.offsetMax  = Vector2.zero;
            if (config?.FillSprite != null)
            {
                delayImg.sprite = config.FillSprite;
                delayImg.color  = config.DamageDelayColor;
            }
            delayImg.type        = Image.Type.Filled;
            delayImg.fillMethod  = Image.FillMethod.Horizontal;
            delayImg.fillOrigin  = (int)Image.OriginHorizontal.Left;
            delayImg.fillAmount  = 1f;
            delayImg.preserveAspect = false;

            // Main Health Fill (meter-31 red parallelogram sprite)
            var fillImg = EnsureImage(healthBarGroup, "HealthFill", Color.white);
            var fillRect = GetOrAddRect(fillImg.transform);
            fillRect.anchorMin  = new Vector2(0.22f, 0.10f);
            fillRect.anchorMax  = new Vector2(0.97f, 0.88f);
            fillRect.offsetMin  = Vector2.zero;
            fillRect.offsetMax  = Vector2.zero;
            if (config?.FillSprite != null)
            {
                fillImg.sprite = config.FillSprite;
                fillImg.color  = Color.white; // use sprite's own color, no tint
            }
            else
            {
                fillImg.color = config != null ? config.HpFillColor : new Color(0.80f, 0.10f, 0.08f, 1f);
            }
            fillImg.type        = Image.Type.Filled;
            fillImg.fillMethod  = Image.FillMethod.Horizontal;
            fillImg.fillOrigin  = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount  = 1f;
            fillImg.preserveAspect = false;

            // Health Glow (heal/low-health overlay, starts invisible)
            var glowImg = EnsureImage(healthBarGroup, "HealthGlow",
                new Color(0.40f, 1f, 0.50f, 0f));
            var glowRect = GetOrAddRect(glowImg.transform);
            glowRect.anchorMin  = new Vector2(0.18f, 0.15f);
            glowRect.anchorMax  = new Vector2(0.96f, 0.82f);
            glowRect.offsetMin  = new Vector2(-6f, -4f);
            glowRect.offsetMax  = new Vector2(6f, 4f);
            glowImg.type        = Image.Type.Filled;
            glowImg.fillMethod  = Image.FillMethod.Horizontal;
            glowImg.fillOrigin  = (int)Image.OriginHorizontal.Left;
            glowImg.fillAmount  = 1f;
            glowImg.raycastTarget = false;

            // HP Text (centered over the bar)
            var hpText = EnsureText(healthBarGroup, "HealthText", "100 / 100", 16, TextAnchor.MiddleCenter);
            var htRect = GetOrAddRect(hpText.transform);
            htRect.anchorMin  = new Vector2(0.18f, 0.12f);
            htRect.anchorMax  = new Vector2(0.96f, 0.88f);
            htRect.offsetMin  = Vector2.zero;
            htRect.offsetMax  = Vector2.zero;
            hpText.fontStyle  = FontStyle.Bold;
            hpText.color      = new Color(0.97f, 0.94f, 0.88f, 1f);  // warm white

            // ─────────────────────────────────────────────
            // Secondary Bar Group
            // ─────────────────────────────────────────────
            var secondaryBarGroup = EnsureChild(topLeft, "SecondaryBarGroup");
            var sbgRect = GetOrAddRect(secondaryBarGroup);
            sbgRect.anchorMin        = new Vector2(0f, 0.05f);
            sbgRect.anchorMax        = new Vector2(1f, 0.45f);
            sbgRect.offsetMin        = new Vector2(105f, 6f);
            sbgRect.offsetMax        = new Vector2(0f, 0f);
            secondaryBarGroup.gameObject.SetActive(false); // hidden until needed

            var secBgImg = EnsureImage(secondaryBarGroup, "SecondaryBarBackground",
                new Color(0.05f, 0.05f, 0.05f, 0.70f));
            var sbbRect = GetOrAddRect(secBgImg.transform);
            sbbRect.anchorMin  = Vector2.zero;
            sbbRect.anchorMax  = Vector2.one;
            sbbRect.offsetMin  = Vector2.zero;
            sbbRect.offsetMax  = Vector2.zero;

            var secFillImg = EnsureImage(secondaryBarGroup, "SecondaryBarFill",
                config != null ? config.SecondaryBarColor : new Color(0.18f, 0.72f, 0.22f, 1f));
            var sfRect = GetOrAddRect(secFillImg.transform);
            sfRect.anchorMin  = Vector2.zero;
            sfRect.anchorMax  = Vector2.one;
            sfRect.offsetMin  = new Vector2(2f, 2f);
            sfRect.offsetMax  = new Vector2(-2f, -2f);
            secFillImg.type        = Image.Type.Filled;
            secFillImg.fillMethod  = Image.FillMethod.Horizontal;
            secFillImg.fillOrigin  = (int)Image.OriginHorizontal.Left;
            secFillImg.fillAmount  = 1f;

            // ─────────────────────────────────────────────
            // Buff Icon Row (4 slots)
            // ─────────────────────────────────────────────
            const int buffSlotCount = 4;
            const float slotSize    = 28f;
            const float slotSpacing = 4f;

            var buffRow = EnsureChild(topLeft, "BuffIconRow");
            var brRect = GetOrAddRect(buffRow);
            brRect.anchorMin        = new Vector2(0f, 0f);
            brRect.anchorMax        = new Vector2(0f, 0.05f);
            brRect.pivot            = new Vector2(0f, 0f);
            brRect.anchoredPosition = new Vector2(105f, 0f);
            brRect.sizeDelta        = new Vector2((slotSize + slotSpacing) * buffSlotCount, slotSize);

            for (var i = 0; i < buffSlotCount; i++)
            {
                var slotName = $"BuffSlot_{(i + 1):00}";
                var slot     = EnsureChild(buffRow, slotName);
                var slotRect = GetOrAddRect(slot);
                slotRect.anchorMin        = new Vector2(0f, 0f);
                slotRect.anchorMax        = new Vector2(0f, 1f);
                slotRect.pivot            = new Vector2(0f, 0.5f);
                slotRect.anchoredPosition = new Vector2(i * (slotSize + slotSpacing), 0f);
                slotRect.sizeDelta        = new Vector2(slotSize, 0f);

                // Slot background
                var slotBg = EnsureImage(slot, "SlotBackground",
                    new Color(0.08f, 0.08f, 0.08f, 0.85f));
                var sbRect = GetOrAddRect(slotBg.transform);
                sbRect.anchorMin  = Vector2.zero;
                sbRect.anchorMax  = Vector2.one;
                sbRect.offsetMin  = Vector2.zero;
                sbRect.offsetMax  = Vector2.zero;

                // Slot icon image
                var slotIcon = EnsureImage(slot, "SlotIcon", Color.white);
                var siRect = GetOrAddRect(slotIcon.transform);
                siRect.anchorMin  = new Vector2(0.1f, 0.1f);
                siRect.anchorMax  = new Vector2(0.9f, 0.9f);
                siRect.offsetMin  = Vector2.zero;
                siRect.offsetMax  = Vector2.zero;
                slotIcon.preserveAspect = true;
                slotIcon.enabled  = false;  // hidden until set

                // Stack count label
                var stackLabel = EnsureText(slot, "StackCount", string.Empty, 9, TextAnchor.LowerRight);
                var scRect = GetOrAddRect(stackLabel.transform);
                scRect.anchorMin  = new Vector2(0.5f, 0f);
                scRect.anchorMax  = new Vector2(1f, 0.5f);
                scRect.offsetMin  = Vector2.zero;
                scRect.offsetMax  = Vector2.zero;
                stackLabel.fontStyle = FontStyle.Bold;
                stackLabel.enabled   = false;

                slot.gameObject.SetActive(false);

                // Add/configure PlayerHealthHudBuffSlot component
                var buffSlotComp = slot.gameObject.GetComponent<PlayerHealthHudBuffSlot>()
                                   ?? slot.gameObject.AddComponent<PlayerHealthHudBuffSlot>();

                // Wire internal refs via SerializedObject
                var bsSo = new SerializedObject(buffSlotComp);
                SetObject(bsSo, "iconImage",       slotIcon);
                SetObject(bsSo, "frameImage",      slotBg);
                SetObject(bsSo, "stackCountLabel", stackLabel);
                bsSo.ApplyModifiedPropertiesWithoutUndo();
            }

            // ── Wire PlayerHealthHudView ──────────────────────────
            var view = root.GetComponent<PlayerHealthHudView>()
                       ?? root.AddComponent<PlayerHealthHudView>();

            // Collect buff slots
            var slots = new PlayerHealthHudBuffSlot[buffSlotCount];
            for (var i = 0; i < buffSlotCount; i++)
            {
                var slotName = $"BuffSlot_{(i + 1):00}";
                var buffRowTransform = topLeft.Find("BuffIconRow");
                var slotTransform    = buffRowTransform != null ? buffRowTransform.Find(slotName) : null;
                if (slotTransform != null)
                {
                    slots[i] = slotTransform.GetComponent<PlayerHealthHudBuffSlot>();
                }
            }

            var viewSo = new SerializedObject(view);
            SetObject(viewSo, "config",                config);
            SetObject(viewSo, "portraitFrameImage",    frameImg(portraitGroup, "PortraitFrame"));
            SetObject(viewSo, "portraitImage",         frameImg(portraitGroup, "PortraitImage"));
            SetObject(viewSo, "levelText",             levelText);
            SetObject(viewSo, "healthBarFrame",        frameImg(healthBarGroup, "HealthBarFrame"));
            SetObject(viewSo, "healthBarBackground",   frameImg(healthBarGroup, "HealthBarBackground"));
            SetObject(viewSo, "healthDamageDelayFill", frameImg(healthBarGroup, "HealthDamageDelayFill"));
            SetObject(viewSo, "healthFill",            frameImg(healthBarGroup, "HealthFill"));
            SetObject(viewSo, "healthGlow",            frameImg(healthBarGroup, "HealthGlow"));
            SetObject(viewSo, "healthText",            hpText);
            SetObject(viewSo, "healthBarGroup",        hbgRect);
            SetObject(viewSo, "secondaryBarBackground", frameImg(secondaryBarGroup, "SecondaryBarBackground"));
            SetObject(viewSo, "secondaryBarFill",      frameImg(secondaryBarGroup, "SecondaryBarFill"));
            SetObject(viewSo, "secondaryBarGroup",     secondaryBarGroup.gameObject);

            // Buff slot array
            var buffSlotsProp = viewSo.FindProperty("buffSlots");
            if (buffSlotsProp != null)
            {
                buffSlotsProp.arraySize = buffSlotCount;
                for (var i = 0; i < buffSlotCount; i++)
                {
                    buffSlotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
                }
            }

            viewSo.ApplyModifiedPropertiesWithoutUndo();

            // ── Wire PlayerHealthHudPresenter ─────────────────────
            var presenter = root.GetComponent<PlayerHealthHudPresenter>()
                            ?? root.AddComponent<PlayerHealthHudPresenter>();

            var presenterSo = new SerializedObject(presenter);
            SetObject(presenterSo, "view", view);
            presenterSo.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[PlayerHealthHudBuilder] Prefab hierarchy built and wired.");
        }

        // ──────────────────────────────────────────────────────────
        // Step 4 — Scene Placement (idempotent)
        // ──────────────────────────────────────────────────────────

        private static void PlacePrefabInScene(GameObject prefabAsset)
        {
            // Find or create HUD Canvas
            var canvas = FindOrCreateHudCanvas();
            if (canvas == null)
            {
                Debug.LogWarning("[PlayerHealthHudBuilder] No Canvas found/created. Skipping scene placement.");
                return;
            }

            // Check if already in scene
            var existing = canvas.transform.Find(PrefabInstanceName);
            if (existing != null)
            {
                Debug.Log("[PlayerHealthHudBuilder] HUD instance already exists in scene — skipping placement.");
                return;
            }

            var instance = PrefabUtility.InstantiatePrefab(prefabAsset, canvas.transform) as GameObject;
            if (instance != null)
            {
                var rect = instance.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin  = Vector2.zero;
                    rect.anchorMax  = Vector2.one;
                    rect.offsetMin  = Vector2.zero;
                    rect.offsetMax  = Vector2.zero;
                }

                Undo.RegisterCreatedObjectUndo(instance, "Place Player Health HUD");
                Debug.Log($"[PlayerHealthHudBuilder] Placed HUD in scene under '{canvas.name}'.");
            }
        }

        private static Canvas FindOrCreateHudCanvas()
        {
            // Look for a canvas named HUD_Canvas or any canvas with "HUD" in name
            var allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var c in allCanvases)
            {
                if (c.name.Contains("HUD") || c.name.Contains("Hud"))
                    return c;
            }

            // Fallback: any Screen Space Overlay canvas
            foreach (var c in allCanvases)
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay)
                    return c;
            }

            // Create a new HUD Canvas
            Debug.Log("[PlayerHealthHudBuilder] No HUD Canvas found in scene. Creating one.");
            var canvasGo = new GameObject(HudCanvasName, typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution  = new Vector2(1920f, 1080f);
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0.5f;

            Undo.RegisterCreatedObjectUndo(canvasGo, "Create HUD Canvas");
            return canvas;
        }

        // ──────────────────────────────────────────────────────────
        // Shared Helpers (mirrors BossHealthBarSetupBuilder pattern)
        // ──────────────────────────────────────────────────────────

        private static Transform EnsureChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null) return child;

            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static Image EnsureImage(Transform parent, string name, Color color)
        {
            var child = EnsureChild(parent, name);
            var image = child.GetComponent<Image>() ?? child.gameObject.AddComponent<Image>();
            image.color        = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text EnsureText(Transform parent, string name, string defaultText,
            int fontSize, TextAnchor alignment)
        {
            var child = EnsureChild(parent, name);
            var label = child.GetComponent<Text>() ?? child.gameObject.AddComponent<Text>();
            label.text      = defaultText;
            label.fontSize  = fontSize;
            label.alignment = alignment;
            label.color     = Color.white;
            label.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.raycastTarget = false;
            return label;
        }

        private static RectTransform GetOrAddRect(Transform t)
        {
            return t.GetComponent<RectTransform>() ?? t.gameObject.AddComponent<RectTransform>();
        }

        private static void SetObject(SerializedObject so, string propName, Object value)
        {
            var prop = so.FindProperty(propName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
            }
            else
            {
                Debug.LogWarning($"[PlayerHealthHudBuilder] Property not found: '{propName}' on {so.targetObject?.GetType().Name}");
            }
        }

        private static void SetSprite(SerializedObject so, string propName, Sprite value)
        {
            SetObject(so, propName, value);
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;

            var parent    = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            var childName = Path.GetFileName(folderPath);

            if (!string.IsNullOrEmpty(parent))
                EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent ?? "Assets", childName);
        }

        // Helper: find Image component in a child of a given transform by name
        private static Image frameImg(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            return child != null ? child.GetComponent<Image>() : null;
        }
    }
}
