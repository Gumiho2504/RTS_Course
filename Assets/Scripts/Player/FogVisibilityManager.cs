
using System;
using System.Collections.Generic;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Gumiho_Rts.Units;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Gumiho_Rts.Player
{
    [RequireComponent(typeof(Camera))]
    public class FogVisibilityManager : MonoBehaviour
    {
        public static FogVisibilityManager Instance { get; private set; }
        private Camera fogOfWarCamera;

        private Texture2D visionTexture;
        private Rect textureRect;

        private HashSet<IHideable> hideables = new(1000);
        void Awake()
        {

            IntailizationSingleton();

            fogOfWarCamera = GetComponent<Camera>();
            visionTexture = new Texture2D(fogOfWarCamera.targetTexture.width, fogOfWarCamera.targetTexture.height);
            textureRect = new Rect(0, 0, visionTexture.width, visionTexture.height);

            Bus<UnitSpawnEvent>.RegisterForAll(HandleUnitSpawn);
            Bus<UnitDeathEvent>.RegisterForAll(HandleUnitDeath);

            Bus<BuildingSpawnEvent>.RegisterForAll(HandleBuildingSpawn);
            Bus<BuildingDeathEvent>.RegisterForAll(HandleBuildingDeath);

            Bus<SupplySpawnEvent>.OnEvent[Owner.Unowned] += HandleSupplySpawn;
            Bus<SupplyDepletedEvent>.OnEvent[Owner.Unowned] += HandleSupplyDepleted;

            Bus<PlaceholderSpawnEvent>.RegisterForAll(HandlePlaceholderSpawn);
            Bus<PlaceholderDestroyEvent>.RegisterForAll(HandlePlaceholderDestroy);
        }

        private void IntailizationSingleton()
        {
            if (Instance != null)
            {
                Debug.LogError("FogVisibilityManager already exists!");
                enabled = false;
                return;
            }
            Instance = this;
        }


        void OnDestroy()
        {
            Bus<UnitSpawnEvent>.UnregisterForAll(HandleUnitSpawn);
            Bus<UnitDeathEvent>.UnregisterForAll(HandleUnitDeath);

            Bus<BuildingSpawnEvent>.UnregisterForAll(HandleBuildingSpawn);
            Bus<BuildingDeathEvent>.UnregisterForAll(HandleBuildingDeath);

            Bus<SupplySpawnEvent>.OnEvent[Owner.Unowned] += HandleSupplySpawn;
            Bus<SupplyDepletedEvent>.OnEvent[Owner.Unowned] += HandleSupplyDepleted;

            Bus<PlaceholderSpawnEvent>.UnregisterForAll(HandlePlaceholderSpawn);
            Bus<PlaceholderDestroyEvent>.UnregisterForAll(HandlePlaceholderDestroy);
        }


        void LateUpdate()
        {

            if (hideables.Count == 0) return;

            ReadPixelsToVisionTexture();

            hideables.RemoveWhere(unit => unit == null);

            foreach (IHideable hideable in hideables)
            {
                SetUnitVisibilityStatus(hideable);
            }
        }

        public bool IsVisible(Vector3 position)
        {
            Vector3 screenPoint = fogOfWarCamera.WorldToScreenPoint(position);
            Color visibilityColor = visionTexture.GetPixel((int)screenPoint.x, (int)screenPoint.y);
            return visibilityColor.r > 0.9f;
        }

        private void HandleUnitDeath(UnitDeathEvent args)
        {
            if (args.Unit != null)
            {
                hideables.Remove(args.Unit);
            }
        }

        private void HandleUnitSpawn(UnitSpawnEvent args)
        {
            if (args.Unit != null && args.Unit.Owner != Owner.Player1)
            {
                hideables.Add(args.Unit);
            }
        }

        private void HandleBuildingSpawn(BuildingSpawnEvent args)
        {
            if (args.Unit != null && args.Unit.Owner != Owner.Player1)
            {
                hideables.Add(args.Unit);
            }
        }

        private void HandleBuildingDeath(BuildingDeathEvent args)
        {
            if (args.Unit != null)
            {
                hideables.Remove(args.Unit);
            }
        }

        private void HandleSupplySpawn(SupplySpawnEvent args)
        {
            hideables.Add(args.Supply);
        }
        private void HandleSupplyDepleted(SupplyDepletedEvent args)
        {
            hideables.Remove(args.Supply);
        }

        private void HandlePlaceholderSpawn(PlaceholderSpawnEvent args)
        {
            hideables.Add(args.Placeholder);
        }

        private void HandlePlaceholderDestroy(PlaceholderDestroyEvent args)
        {
            hideables.Remove(args.Placeholder);
        }


        private void ReadPixelsToVisionTexture()
        {
            RenderTexture previousRenderTexture = RenderTexture.active;
            RenderTexture.active = fogOfWarCamera.targetTexture;
            visionTexture.ReadPixels(textureRect, 0, 0);
            RenderTexture.active = previousRenderTexture;
        }

        private void SetUnitVisibilityStatus(IHideable hideable)
        {

            hideable.SetVisitable(IsVisible(hideable.Transform.position));
        }
    }
}