using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeferredList<T> : List<T>
{
	List<T> mDeferredAdds;
	List<T> mDeferredRemoves;
	
	public DeferredList()
		: base()
	{
		mDeferredAdds = new List<T>();
		mDeferredRemoves = new List<T>();
	}
	
	public DeferredList(int reserve)
		: base(reserve)
	{
		mDeferredAdds = new List<T>(reserve);
		mDeferredRemoves = new List<T>(reserve);
	}
	
	public DeferredList(DeferredList<T> deferredList)
		: base(deferredList)
	{
		mDeferredAdds = new List<T>(deferredList.mDeferredAdds);
		mDeferredRemoves = new List<T>(deferredList.mDeferredRemoves);
	}
	
	public void AddDeferred(T obj)
	{
		mDeferredAdds.Add(obj);
	}
	public void RemoveDeferred(T obj)
	{
		mDeferredRemoves.Add(obj);
	}
	
	public void ProcessDeferred()
	{
		foreach (T obj in mDeferredRemoves)
		{
			Remove(obj);
		}
		mDeferredRemoves.Clear();
		
		foreach (T obj in mDeferredAdds)
		{
			Add(obj);
		}
		mDeferredAdds.Clear();
	}
}