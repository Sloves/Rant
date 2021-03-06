﻿using Rant.Engine.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Rant.Engine.Delegates;
using Rant.IO;

namespace Rant.Engine
{
	/// <summary>
	/// Contains information for associating a delegate with a Rant function.
	/// </summary>
	internal class RantFunctionInfo
	{
		private readonly Witchcraft _delegate;
		private readonly RantParameter[] _params;

		public RantParameter[] Parameters => _params;

		public string Name { get; }
		public string Description { get; }

		public RantFunctionInfo(string name, string description, MethodInfo method)
		{
			// Sanity checks
			if (method == null) throw new ArgumentNullException(nameof(method));
			if (!method.IsStatic) throw new ArgumentException($"({method.Name}) Method is not static.");

			var parameters = method.GetParameters();
			if (!parameters.Any())
				throw new ArgumentException($"({method.Name}) Cannot use a parameter-less method for a function.");
			if (parameters[0].ParameterType != typeof(Sandbox))
				throw new ArgumentException($"({method.Name}) The first parameter must be of type '{typeof(Sandbox)}'.");

			// Sort out the parameter types for the function
			_params = new RantParameter[parameters.Length - 1];
			Type type;
			RantParameterType rantType;
			for (int i = 1; i < parameters.Length; i++)
			{
				// Resolve Rant parameter type from .NET type
				type = parameters[i].ParameterType;
				if (type == typeof(RantAction) || type.IsSubclassOf(typeof(RantAction)))
				{
					rantType = RantParameterType.Pattern;
				}
				else if (type == typeof(string))
				{
					rantType = RantParameterType.String;
				}
				else if (type.IsEnum)
				{
					rantType = type.GetCustomAttributes(typeof(FlagsAttribute)).Any()
						? RantParameterType.Flags
						: RantParameterType.Mode;
				}
				else if (IOUtil.IsNumericType(type))
				{
					rantType = RantParameterType.Number;
				}
				else
				{
					throw new ArgumentException($"({method.Name}) Unsupported type '{type}' for parameter '{parameters[i].Name}'. Must be a string, number, or RantAction.");
				}

				// Create Rant parameter
				_params[i - 1] = new RantParameter(parameters[i].Name, type, rantType);
			}
			_delegate = Witchcraft.Create(method);
			Name = name;
			Description = description;
		}

		public IEnumerator<RantAction> Invoke(Sandbox sb, object[] arguments)
		{
			return _delegate.Invoke(sb, arguments) as IEnumerator<RantAction> ?? CreateEmptyIterator();
		}

		private static IEnumerator<RantAction> CreateEmptyIterator()
		{
			yield break;
		}
	}
}