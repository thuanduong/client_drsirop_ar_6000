using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class UISliderDotsIndicator : MonoBehaviour
{
    #region Variables

    [Header("References")]

    /// <summary>
    /// Prefab reference for the PageDot component representing a single dot indicator.
    /// </summary>
    [Tooltip("Prefab reference for the PageDot component representing a single dot indicator")]
    [SerializeField] private UISliderDot _prefab;

    [Header("Children")]

    /// <summary>
    /// List containing references to all currently displayed PageDot instances.
    /// </summary>
    [Tooltip("List containing references to all currently displayed PageDot instances")]
    [SerializeField] private List<UISliderDot> _dots;

    [Header("Events")]

    /// <summary>
    /// UnityEvent that is invoked when a page dot is pressed, passing the index of the pressed dot.
    /// </summary>
    [Tooltip("Invoked when a page dot is pressed, passing the index of the pressed dot")]
    public UnityEvent<int> OnDotPressed;

    /// <summary>
    /// Gets or sets the visibility of the PageDotsIndicator game object.
    /// </summary>
    public bool IsVisible
    {
        get { return gameObject.activeInHierarchy; }
        set { gameObject.SetActive(value); }
    }

    #endregion

    private void Awake()
    {
        if (_prefab != default)
            _prefab.gameObject.SetActive(false);

        if (_dots.Count == 0) return;
        for (int i = 0; i < _dots.Count; i++)
        {
            _dots[i].ChangeActiveState(i == 0);
        }
    }

    /// <summary>
    /// Adds a new page dot indicator to the collection.
    /// </summary>
    public void Add()
    {
        UISliderDot dot = null;

        // If no dot was instantiated in editor mode, use regular Instantiate in play mode.
        if (dot == null)
        {
            dot = Instantiate(_prefab, transform);
            dot.gameObject.SetActive(true);
        }

        dot.Index = _dots.Count;
        dot.ChangeActiveState(_dots.Count == 0); // Activate the first dot.

        _dots.Add(dot);

    }

    /// <summary>
    /// Clears all the page dot indicators from the collection and destroys their game objects.
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < _dots.Count; i++)
        {
            if (_dots[i] == null) { continue; }
            // In play mode, use Destroy for object removal during gameplay.
            Destroy(_dots[i].gameObject);
        }

        _dots.Clear();

    }

    /// <summary>
    /// Changes the active state of the page dots.
    /// It deactivates the dot at the 'fromIndex' and activates the dot at the 'toIndex'.
    /// </summary>
    /// <param name="fromIndex">The index of the dot to deactivate.</param>
    /// <param name="toIndex">The index of the dot to activate.</param>
    public void ChangeActiveDot(int fromIndex, int toIndex)
    {
        _dots[fromIndex].ChangeActiveState(false);
        _dots[toIndex].ChangeActiveState(true);
    }
}
