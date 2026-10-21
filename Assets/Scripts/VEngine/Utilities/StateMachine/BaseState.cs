using UnityEngine;
using System;

namespace Utilities
{
	public abstract class BaseState<EState, SelfType> where EState : Enum
	{
		public BaseState(EState key)
		{
			StateKey = key;
		}

		public EState StateKey { get; private set; }
		public SelfType self;

		public abstract void HandleState();
	}
}

