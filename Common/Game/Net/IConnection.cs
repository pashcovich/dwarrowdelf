﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dwarrowdelf.Messages;

namespace Dwarrowdelf
{
	public interface IConnection : IDisposable
	{
		int SentMessages { get; }
		int SentBytes { get; }
		int ReceivedMessages { get; }
		int ReceivedBytes { get; }
		void ResetStats();

		bool IsConnected { get; }

		bool TryGetMessage(out Message msg);

		void Send(Message msg);

		void Disconnect();

		event Action NewMessageEvent;
	}
}
