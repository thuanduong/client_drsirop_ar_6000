using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dispatcher
{
	public interface IDispatchResult
	{
		int Command { get; }
		string Message { get; }
		int ErrorCode { get; }
	}

	public interface IConnectResult : IDispatchResult
	{
		bool IsSuccess { get; }
	}

	public interface IErrorResult : IDispatchResult
	{
	}

	public interface IMessageResult : IDispatchResult
	{
		IMessageBaseModel Model { get; }
	}

	public abstract class BaseResult : IDispatchResult
	{

		public BaseResult(string message, int code)
		{
			this.Message = message;
			this.ErrorCode = code;
		}

		#region IDispatchResult implementation

		public int Command
		{
			get;
			set;
		}

		public string Message
		{
			get;
			private set;
		}

		public int ErrorCode
		{
			get;
			private set;
		}

		#endregion
	}

	internal sealed class ConnectResult : BaseResult, IConnectResult
	{
		public ConnectResult(bool isSuccess, string message, int code) :
		base(message, code)
		{
			this.IsSuccess = isSuccess;
		}

		#region IConnectResult implementation

		public bool IsSuccess
		{
			get;
			private set;
		}

		#endregion
	}

	internal sealed class ErrorResult : BaseResult, IErrorResult
	{
		public ErrorResult(string message, int code) :
		base(message, code)
		{ }
	}
}

public interface IMessageBaseModel
{
}