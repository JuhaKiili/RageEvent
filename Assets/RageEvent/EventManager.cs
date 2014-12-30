using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = System.Object;

namespace RageEvent
{
	[AttributeUsage(AttributeTargets.Method)]
	public class Listen : Attribute
	{
		private string m_Name;
		public string name { get { return m_Name; } }
		public Listen(string name)
		{
			m_Name = name;
		}
	}

	class EventManager
	{
		private const float k_GCFrequency = 1f;
		private static float m_TimeSinceLastGC = 0f;
		private static Dictionary<string, Dictionary<int, Action<object[]>>> s_Listeners;
		
		public static void Initialize ()
		{
			s_Listeners = new Dictionary<string, Dictionary<int, Action<object[]>>> ();
		}

		public static void Initialize (Object target)
		{
			if (s_Listeners == null)
				Initialize();

			MethodInfo[] methods = target.GetType().GetMethods();
			foreach (MethodInfo methodInfo in methods)
				if (methodInfo.GetCustomAttributes (typeof (Listen), true).Length > 0)
					Listen (target, methodInfo.Name);
		}

		public static void Tick (float deltaTime)
		{
			m_TimeSinceLastGC += deltaTime;
			if (m_TimeSinceLastGC > k_GCFrequency)
			{
				m_TimeSinceLastGC = 0f;
				GarbageCollect();
			}
		}

		public static void Listen (Object target, string eventName)
		{
			if (target == null || eventName == null || eventName.Length == 0)
			{
				Debug.LogError("Invalid EventManager.Listen(). Caller: " + target + ", eventName: " + eventName);
				return;
			}
		
			MethodInfo methodInfo = GetMethodInfo (target, eventName);
			if (methodInfo == null)
			{
				Debug.LogError("Can't find methodInfo. Caller: " + target + ", eventName: " + eventName);
				return;
			}

			Delegate newDelegate = Delegate.CreateDelegate (typeof(Action<object[]>), target, methodInfo);

			if (!s_Listeners.ContainsKey (eventName))
				s_Listeners.Add (eventName, new Dictionary<int, Action<object[]>> ());

			Dictionary<int, Action<object[]>> methodCache = s_Listeners[eventName];

			if (!methodCache.ContainsKey(target.GetHashCode()))
				methodCache.Add (target.GetHashCode(), (Action<object[]>)newDelegate);
			else
				Debug.LogError ("Trying to listen when already listening. Caller: " + target + ", eventName: " + eventName);
		}

		public static void Trigger (string eventName, params object[] parameters)
		{
			if (!s_Listeners.ContainsKey (eventName))
				return;

			List<int> nullTargets = new List<int> ();
			Dictionary<int, Action<object[]>> methodCache = s_Listeners[eventName];
			foreach (KeyValuePair<int, Action<object[]>> item in methodCache)
			{
				if (item.Value.Target != null)
				{
					item.Value.Invoke (parameters);
				}
				else
				{
					Debug.LogWarning("Trying to trigger on null target. EventName: " + eventName);
					nullTargets.Add (item.Key);
				}
			}

			foreach (int key in nullTargets)
				methodCache.Remove (key);
		}

		public static void GarbageCollect ()
		{
			List<int> nullTargets = new List<int> ();
			foreach (KeyValuePair<string, Dictionary<int, Action<object[]>>> listener in s_Listeners)
			{
				foreach (KeyValuePair<int, Action<object[]>> item in listener.Value)
					if (item.Value.Target == null)
						nullTargets.Add (item.Key);

				foreach (int key in nullTargets)
					listener.Value.Remove (key);
			}
		}

		private static MethodInfo GetMethodInfo(Object target, string method)
		{
			Type type = target.GetType ();
			return type.GetMethod (method);
		}
	}
}
