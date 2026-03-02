
using System;
using System.Collections;
using Gumiho_Rts.UI.Components;
using Gumiho_Rts.Units;
using UnityEngine;



namespace Gumiho_Rts.UI.Containers
{
    public class BuildingBuildingUI : MonoBehaviour, IUIElement<BaseBuilding>
    {
        [SerializeField] private Progressbar progressBar;
        [SerializeField] private UIBuildQueueButton[] unitButtons;
        private Coroutine buildCoroutine;
        private BaseBuilding building;




        public void EnableFor(BaseBuilding item)
        {
            progressBar.SetProgress(0);
            gameObject.SetActive(true);
            building = item;
            building.OnQueueUpdated += HandleQueueUpdated;
            SetupUnitButton();

            buildCoroutine = StartCoroutine(UpdateUnitProgress());
        }

        private void SetupUnitButton()
        {
            int i = 0;
            // size = 3 = 0,1,2
            for (; i < building.QueueSize; i++)
            {
                int index = i;
                unitButtons[i].EnableFor(building.Queue[i], () => building.CancelBuildUnit(index));
            }// exit loop => i = 2
            // this start from 2 , 3,4.  example unit button length = 5
            for (; i < unitButtons.Length; i++)
            {
                unitButtons[i].Disable();
            }
        }

        private void HandleQueueUpdated(UnitSO[] unitsInQueue)
        {
            if (unitsInQueue.Length == 1 && buildCoroutine == null)
            {
                buildCoroutine = StartCoroutine(UpdateUnitProgress());
            }
            SetupUnitButton();
        }

        public void Disable()
        {
            if (building != null)
            {
                building.OnQueueUpdated -= HandleQueueUpdated;
            }
            gameObject.SetActive(false);
            building = null;
            buildCoroutine = null;

        }

        private IEnumerator UpdateUnitProgress()
        {
            while (building != null && building.QueueSize > 0)
            {
                /// <summary>
                /// first Start at Time 5.  And buildTime = 5
                //  => startTime = 5
                // => endTime = 15

                // when Time.time = 5 => progress for  5-5/5 = 0
                // when Time.time = 10 => progress for  10-5/5 = 1

                /// The building is start at 10mn 20s and the buildTime 7s . Now Time.time = 10mn 24s; 
                ///  now we find the  because  the building need 7s to build endtime  = 10mn 27s
                ///  => how many second that building has been start =  now - start = 10mn 29 - 10mn 24s = 4s
                ///  => progress = 4/7 = 0.57
                /// </summary>


                float startTime = building.CurrentQueueStartTime;
                float endTime = startTime + building.BuildingUnit.BuildTime;
                float progress = Mathf.Clamp01((Time.time - startTime) / (endTime - startTime));
                progressBar.SetProgress(progress);
                yield return null;
            }
            buildCoroutine = null;
        }


    }
}