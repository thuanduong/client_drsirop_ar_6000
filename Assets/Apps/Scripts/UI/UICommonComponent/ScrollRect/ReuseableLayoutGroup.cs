using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ReuseableLayoutGroup : MonoBehaviour
{
    public enum TypeOfLayout
    {
        Horizontal,
        Vertical,
        Grid,
    }

    public TypeOfLayout typeOfLayout;

	int _numContentChild;
	[SerializeField] int _headIndex;
	[SerializeField] int _tailIndex;
	[SerializeField] int _realIndex;
	[SerializeField] int _realHeadIndex;

	[SerializeField] Vector2 cellSize;
	[SerializeField] Vector2 spacing;
	[SerializeField] float PaddingLeft;
	[SerializeField] float PaddingRight;
	[SerializeField] float PaddingTop;
	[SerializeField] float PaddingBot;

	public Vector2 CellSize => cellSize;
	public Vector2 Spacing => spacing;

	RectTransform _headChild;
	RectTransform _tailChild;
	float _maxZone;
	float _minZone;
	float _minSize = -1;

	Vector2 _lastDelta = Vector2.zero;
	Vector2 _lastPosition = Vector2.zero;
	ScrollDirection _currentMoveDir = ScrollDirection.None;

	Canvas _parentCanvas;

	private bool _isDirty;

	public IPoolList PoolList { get; set; }

	public System.Action<ScrollRect.MovementType> MoveType = ActionUtility.EmptyAction<ScrollRect.MovementType>.Instance;
	public System.Action<Vector2> MoveTo = ActionUtility.EmptyAction<Vector2>.Instance;

	public enum ScrollDirection
	{
		None,
		Top,
		Bot,
	}

	public void Initialize(IPoolList pool)
	{
		PoolList = pool;
		PoolList.OnFinishUpdate += FinishValid;
		_lastPosition = new Vector2(1, 1);
		_lastDelta = transform.position;
		_parentCanvas = GetComponentInParent<Canvas>();
	}

	void FinishValid()
    {
		var numChild = PoolList.NumChild;

		var scrollRect = this.GetComponent<RectTransform>();
		_numContentChild = (int)(scrollRect.rect.height / (cellSize.y + spacing.y)) + 1;
		_numContentChild += 2;

		_headChild = default;
		_tailChild = default;

		_headIndex = PoolList.HeadIndex;
		_tailIndex = PoolList.TailIndex;
		_realIndex = _tailIndex;
		_realHeadIndex = _headIndex;
		ResizeAndPositionContent();
		_lastDelta = transform.position;
		MoveTo(_lastPosition);

	}

    private void RefreshList()
	{
		PoolList.UpdateIndex(0);
		_headIndex = PoolList.HeadIndex;
		_tailIndex = PoolList.TailIndex;
		_realIndex = _tailIndex;
		_realHeadIndex = _headIndex;
	}

	public void ResizeAndPositionContent()
    {
		if (PoolList == default) return;
		int numChild = PoolList.NumChild;
		if (numChild <= -1)
		{
			// if infinity scroll then we need to make it scroll forever
			MoveType(ScrollRect.MovementType.Unrestricted);
			numChild = _numContentChild;
		}
		else
		{
			//restricted movement when we have limited content
			MoveType(ScrollRect.MovementType.Elastic);
		}

		// Get content rect
		var rect = GetComponent<RectTransform>();
		var parenRect = transform.parent.GetComponent<RectTransform>();
		if (_minSize == -1)
		{
			_minSize = parenRect.rect.height;
		}

		var size = Mathf.Max(_minSize, numChild * (CellSize.y + Spacing.y));
		var horizontal = parenRect.rect.width - PaddingLeft - PaddingRight;
		if (size != _minSize)
		{
			rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, size);
		}
		else
		{
			rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, _minSize);
		}

		float delta = (cellSize.y + spacing.y);
		var startPosY = PaddingTop + delta * PoolList.HeadIndex;
		
		var l = PoolList.InstanceList;
		int childCount = l.Count;
		for (int i = 0; i < numChild; ++i)
		{
			if (i >= childCount) continue;
			var item = l[i];
			var childRect = item.transform.AsRectTransform();
			childRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, startPosY, cellSize.y);
			childRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.width);
			childRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, PaddingLeft, horizontal);
			startPosY += delta;
		}

		if (numChild == 0) return;

		_headChild = l[0].GetComponent<RectTransform>();
		int t = l.Count - 1;
		if (t >= 0)
			_tailChild = l[t].GetComponent<RectTransform>();
		if (_headChild == default || _tailChild == default) return;
		float delRev = delta * PoolList.HeadIndex;
		//_maxZone = _parentCanvas.transform.InverseTransformPoint(_headChild.position).y + delta * 1.5f;
		//_minZone = _parentCanvas.transform.InverseTransformPoint(_tailChild.position).y + delRev;
		var firstPoint = rect.TransformPoint(Vector3.zero);
		var finalPoint = firstPoint;
		finalPoint.y -= parenRect.rect.height;

		_maxZone = _parentCanvas.transform.InverseTransformPoint(firstPoint).y + delta * 1.5f;
		_minZone = _parentCanvas.transform.InverseTransformPoint(finalPoint).y;// + delRev;
		 
	}

	public void OnScroll(Vector2 value)
    {
		_lastPosition = value;
		if (PoolList.NumChild < 2) return;
		if (_headChild == default || _tailChild == default)
		{
			var l = PoolList.InstanceList;
			var eee = l.Count - 1;

			_headChild = l[0].transform.AsRectTransform();
			_tailChild = l[eee].transform.AsRectTransform();
			
			float delta = (cellSize.y + spacing.y);
			float delRev = delta * PoolList.HeadIndex;

			//_maxZone = _parentCanvas.transform.InverseTransformPoint(_headChild.position).y + delta * 1.5f;
			//_minZone = _parentCanvas.transform.InverseTransformPoint(_tailChild.position).y + delRev;

			var rect = GetComponent<RectTransform>();
			var parenRect = transform.parent.GetComponent<RectTransform>();
			var firstPoint = rect.TransformPoint(Vector3.zero);
			var finalPoint = firstPoint;
			finalPoint.y -= parenRect.rect.height;

			_maxZone = _parentCanvas.transform.InverseTransformPoint(firstPoint).y + delta * 1.5f;
			_minZone = _parentCanvas.transform.InverseTransformPoint(finalPoint).y;// + delRev;
		}
		
		if (_lastDelta.y < transform.position.y)
		{
			_currentMoveDir = ScrollDirection.Top;
		}
		else if (_lastDelta.y > transform.position.y)
		{
			_currentMoveDir = ScrollDirection.Bot;
		}
        else
            _currentMoveDir = ScrollDirection.None;

		_lastDelta = transform.position;

		if (_currentMoveDir == ScrollDirection.Top)
        {
			if (PoolList.TailIndex + 1 < PoolList.NumChild)
			{
				var headChildPosY = _parentCanvas.transform.InverseTransformPoint(_headChild.position).y;
				if (headChildPosY > _maxZone)
				{
					if (PoolList.MoveNext()) {
						var ll = PoolList.InstanceList;
						var eee = ll.Count - 1;
						_headChild.position = _tailChild.position - new Vector3(0, cellSize.y + spacing.y, 0);
						_headIndex = PoolList.HeadIndex;
						_tailIndex = PoolList.TailIndex;
						_tailChild = ll[eee].transform.AsRectTransform();
						_headChild = ll[0].transform.AsRectTransform();
					}
				}
			}
        }
		else if (_currentMoveDir == ScrollDirection.Bot)
        {
			if (PoolList.HeadIndex  > 0)
			{
				var tailChildPosY = _parentCanvas.transform.InverseTransformPoint(_tailChild.position).y;
				if (tailChildPosY < _minZone)
				{
					if (PoolList.MovePrev())
                    {
						var ll = PoolList.InstanceList;
						var eee = ll.Count - 1;
						_tailChild.position = _headChild.position + new Vector3(0, cellSize.y + spacing.y, 0);
						_headIndex = PoolList.HeadIndex;
						_tailIndex = PoolList.TailIndex;
						_headChild = ll[0].transform.AsRectTransform();
						_tailChild = ll[eee].transform.AsRectTransform();
					}
				}
			}
		}
	}
}
