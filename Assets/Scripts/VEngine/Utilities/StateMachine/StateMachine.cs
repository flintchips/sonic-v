using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
	public abstract class StateMachine<EState, SelfType> where EState : Enum
	{
		protected Dictionary<EState, BaseState<EState, SelfType>> States = new Dictionary<EState, BaseState<EState, SelfType>>();
		public SelfType self { get; private set; }
		public StateMachine(SelfType self) => this.self = self;
		public void Run(EState state)
		{
			BaseState<EState, SelfType> baseState = States[state];
			baseState.self = self;
			baseState.HandleState();
		}

		// Adds the enum and state to the dictionary
		protected void InitState(EState enumState, BaseState<EState, SelfType> baseState)
		{
			States.Add(enumState, baseState);
		}
	}
}