using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Dispatcher
{
	public class MessageEventDispatcher : MonoBehaviour
	{
		private static MessageEventDispatcher _instance;
		public static MessageEventDispatcher Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindObjectOfType<MessageEventDispatcher>();
				}
				return _instance;
			}
		}

		public delegate void OnMessage(IDispatchResult result);
		public event OnMessage OnMessageReceive;

        private void OnDestroy()
        {
			CleanUp();
			_instance = null;
        }

        public void InitManager()
		{
			CleanUp();
			StopAllCoroutines();
			StartCoroutine(DequeueMessage());
		}

		public void CleanUp()
		{
			if (OnMessageReceive != null)
			{
				OnMessageReceive = null;
			}
			_messageQueue.Clear();
			lock (_delayedMessage)
			{
				_delayedMessage.Clear();
			}
		}

		private List<IDispatchResult> _messageQueue = new List<IDispatchResult>();
		private List<IDispatchResult> _delayedMessage = new List<IDispatchResult>();

		public void AddMessage(IDispatchResult message)
		{
			//Since this is on another thread so we need to lock it first
			lock (_delayedMessage)
			{
				_delayedMessage.Add(message);
			}
		}

		IEnumerator DequeueMessage()
		{
			do
			{
				if (System.Threading.Monitor.TryEnter(_delayedMessage))
				{
					try
					{
						_messageQueue.AddRange(_delayedMessage);
						_delayedMessage.Clear();
					}
					finally { System.Threading.Monitor.Exit(_delayedMessage); }
				}

				if (_messageQueue.Count > 0)
				{
					foreach (var message in _messageQueue.ToList())
					{
						Dispath(message);
					}
					_messageQueue.Clear();
				}

				yield return new WaitForSeconds(1.0f / 30.0f);
			} while (true);
		}

		private void Dispath(IDispatchResult message)
		{
			if (OnMessageReceive != null && message != null)
			{
				OnMessageReceive(message);
			}
		}
	}
}