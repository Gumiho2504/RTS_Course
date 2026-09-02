using System.Collections.Generic;
namespace Gumiho_Rts.UI.Components
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Gumiho_Rts.EventBus;
    using Gumiho_Rts.Events;
    using Gumiho_Rts.Units;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.InputSystem;
    using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public class ControlGroup : MonoBehaviour, IUIElement<
                HashSet<AbstractCommandable>,
                Key,
                UnityAction<HashSet<AbstractCommandable>>
                >
    {
        [SerializeField] private Image unitIcon;
        [SerializeField] private TextMeshProUGUI groupText;
        [SerializeField] private TextMeshProUGUI unitCountText;

        [SerializeField] private Button button;
        private Key hotKey;
        private UnityAction<HashSet<AbstractCommandable>> onActivate;

        private HashSet<AbstractCommandable> unitsInGroup;

        private void Awake() => button = GetComponent<Button>();

        private void OnEnable()
        {
            Bus<UnitDeathEvent>.OnEvent[Owner.Player1] += HandleUnitDeath;
        }

        private void OnDestroy()
        {
            Bus<UnitDeathEvent>.OnEvent[Owner.Player1] -= HandleUnitDeath;
        }


        private void Update()
        {
            if (Keyboard.current[hotKey].wasPressedThisFrame)
            {
                onActivate?.Invoke(unitsInGroup);
            }
        }

        public void Disable()
        {
            button.onClick.RemoveAllListeners();
            Bus<UnitDeathEvent>.OnEvent[Owner.Player1] -= HandleUnitDeath;
            gameObject.SetActive(false);
        }

        public void EnableFor(HashSet<AbstractCommandable> items, Key hotKey, UnityAction<HashSet<AbstractCommandable>> callback)
        {
            unitsInGroup = items.ToHashSet();
            this.hotKey = hotKey;
            onActivate = callback;



            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => callback(unitsInGroup));

            SetIconAndUnitCountText();

            gameObject.SetActive(true);

        }

        private void SetIconAndUnitCountText()
        {
            unitCountText.SetText(unitsInGroup.Count.ToString());
            unitIcon.sprite = unitsInGroup.First().UnitSO.Icon;
         //   groupText.SetText();
        }


        private void HandleUnitDeath(UnitDeathEvent evt)
        {
            unitsInGroup.Remove(evt.Unit);
            if (unitsInGroup.Count == 0)
            {
                Disable();
                return;
            }

            SetIconAndUnitCountText();
        }

    }
}