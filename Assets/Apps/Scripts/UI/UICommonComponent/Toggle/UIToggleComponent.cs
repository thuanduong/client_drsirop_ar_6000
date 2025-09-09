using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIToggleComponent : UIComponent<UIToggleComponent.Entity>
{
	[System.Serializable]
    public class Entity
    {
	    public bool isOn;
        public bool isDisable;
        public int index;
		public Action<bool> onActiveToggle = ActionUtility.EmptyAction<bool>.Instance;
        public Action<bool, int> onActiveToggleIndex = ActionUtility.EmptyAction<bool, int>.Instance;
    }

    public Toggle toggle;
    private Action<bool> onActiveToggleInternal = ActionUtility.EmptyAction<bool>.Instance;
    private Action<bool, int> onActiveToggleIndexInternal = ActionUtility.EmptyAction<bool, int>.Instance;

    [SerializeField] private UnityEvent onActiveToggle;
    [SerializeField] private UnityEvent onDeActiveToggle;

    private void Awake()
    {
	    toggle.onValueChanged.AddListener(val => OnActiveToggleInternal(val));
    }

    protected override void OnSetEntity()
    {
	    toggle.isOn = this.entity.isOn;
        toggle.interactable = !this.entity.isDisable;
        onActiveToggleInternal = ActionUtility.EmptyAction<bool>.Instance;
	    onActiveToggleInternal += this.entity.onActiveToggle;
        onActiveToggleIndexInternal = this.entity.onActiveToggleIndex;
    }

    private void Reset()
    {
	    toggle = GetComponent<Toggle>();
    }

    private void OnActiveToggleInternal(bool val)
    {
        if (val) {
            onActiveToggle?.Invoke();
        }
        else
        {
            onDeActiveToggle?.Invoke();
        }
        onActiveToggleInternal(val);
        if (entity != default)
            onActiveToggleIndexInternal(val, entity.index );
    }
}	