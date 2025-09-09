using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public interface IPoolList 
{
	int NumChild { get; }
	int HeadIndex { get; }
	int TailIndex { get; }
	System.Action OnFinishUpdate { get; set; }
	List<GameObject> InstanceList { get; } 
	void UpdateIndex(int Start);
	bool MoveNext();
	bool MovePrev();
	
}
