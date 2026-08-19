
using System.Collections;
using Gumiho_Rts.Units;
using TMPro;
using UnityEngine;
using Gumiho_Rts.UI.Components;

namespace Gumiho_Rts.UI.Containers
{
    public class BuildingUnderConstructorUI : MonoBehaviour, IUIElement<AbstractCommandable>
    {
        [SerializeField] private TextMeshProUGUI unitNameText;
        [SerializeField] private ProgressBar progressBar;
        public void EnableFor(AbstractCommandable building)
        {
            gameObject.SetActive(true);
            unitNameText.SetText(building.UnitSO.Name);

            if (building is BaseBuilding baseBuilding)
            {
                Debug.Log("Enabling building UI for " + building.UnitSO.Name);
                StartCoroutine(AnimateBuildingProgress(baseBuilding));
            }

        }

        public void Disable()
        {
            if (gameObject != null)
                gameObject.SetActive(false);
        }

        private IEnumerator AnimateBuildingProgress(BaseBuilding building)
        {
            if (building == null)
            {
                Debug.LogError("Building is null in AnimateBuildingProgress");
                yield break;

            }
            while (enabled && building.Progress.State == BuildingProgress.BuildingState.Building)
            {





                if (building.Progress.State != BuildingProgress.BuildingState.Building)
                {
                    yield return null;
                    continue;
                }
                float startTime = building.Progress.StartTime;

                float endTime = startTime + building.SOBeingBuilt.BuildTime;

                progressBar.SetProgress(Mathf.Clamp01((Time.time - startTime) / (endTime - startTime)));
                yield return null;
            }

        }


    }
}