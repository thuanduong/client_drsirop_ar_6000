using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dispatcher;
using Singleton;
using System.Linq;

namespace MessageEvent
{
	public interface IMessageEventListener
	{

	}

	public delegate void IMessageEventFunction<T>(T listener);

	public interface IConnectEvent : IMessageEventListener
	{
		void OnConnectCallback(IConnectResult result);
	}
	public interface IErrorEvent : IMessageEventListener
	{
		void OnErrorCallback(IErrorResult result);
	}

	public interface IMessageBroadCaster
	{
		void OnBroadCastMessage(IDispatchResult result);
	}

	public sealed class MessageEventMgr : DefaultInstance<MessageEventMgr>
	{
		#region implemented abstract members of IInitializer
		protected override void Init()
		{

		}
		#endregion

		List<IMessageBroadCaster> _handleOverride = new List<IMessageBroadCaster>();

		List<IMessageEventListener> _messageList = new List<IMessageEventListener>();

		public void Register(IMessageEventListener message, bool isHighPriority = false)
		{
			if (!_messageList.Contains(message))
			{
				if (isHighPriority)
				{
					_messageList.Insert(0, message);
				}
				else
				{
					_messageList.Add(message);
				}
			}
		}

		public void UnRegister(IMessageEventListener message)
		{
			_messageList.Remove(message);
		}

		public void Initialize()
		{
			MessageEventDispatcher.Instance.OnMessageReceive += OnMessageReceive;
		}

		public void CleanUp()
		{
			if (MessageEventDispatcher.Instance == null)
				return;
			MessageEventDispatcher.Instance.OnMessageReceive -= OnMessageReceive;
		}

		public void AddMessageBroadCaster(IMessageBroadCaster handler)
		{
			if (!_handleOverride.Contains(handler))
			{
				_handleOverride.Add(handler);
			}
		}

		public void RemoveMessageBroadCaster(IMessageBroadCaster handler)
		{
			_handleOverride.Remove(handler);
		}

		private void OnMessageReceive(IDispatchResult result)
		{
			foreach (var item in _handleOverride.ToList())
			{
				item.OnBroadCastMessage(result);
			}
		}

		public void BroadcastEvent<T>(IMessageEventFunction<T> functor) where T : class
		{	

			foreach (var item in _messageList.ToList())
			{
				var listener = item as T;
				if (listener != null)
				{
					functor.Invoke(listener);
				}
			}
		}

	}
}