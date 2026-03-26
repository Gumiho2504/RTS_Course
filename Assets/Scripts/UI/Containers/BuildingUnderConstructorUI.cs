
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
            StartCoroutine(AnimateBuildingProgress(building as BaseBuilding));

        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        private IEnumerator AnimateBuildingProgress(BaseBuilding building)
        {
            while (enabled && building.Progress.State == BuildingProgress.BuildingState.Building)
            {
                if (building.Progress.State != BuildingProgress.BuildingState.Building)
                {
                    yield return null;
                    continue;
                }
                float startTime = building.Progress.StartTime;
                float endTime = startTime + building.BuildingUnit.BuildTime;

                progressBar.SetProgress(Mathf.Clamp01((Time.time - startTime) / (endTime - startTime)));
                yield return null;
            }

        }


    }
}