using System;
using System.Collections;
using System.Collections.Generic;

namespace WestBay
{
	/// <summary>
	/// Notification Center
	/// </summary>
	public class NotificationCenter : Singleton<NotificationCenter>
	{
		private readonly Hashtable _hashtable;

		public NotificationCenter()
		{
			_hashtable = new Hashtable();
		}

		/// <summary>
		/// Adds an entry to the receiver’s dispatch table with an observer, a notification Delegate and notification name.
		/// </summary>
		/// <param name="notificationDelegate">Delegate  that specifies the message the receiver sends notificationObserver to notify it of the notification posting</param>
		/// <param name="notificationName">The name of the notification for which to register the observer; that is, only notifications with this name are delivered to the observer</param>
		public bool AddObserver(Action<Notification> notificationDelegate, string notificationName)
		{
			bool result = false;
			if (string.IsNullOrEmpty(notificationName)) return result;
			if (notificationDelegate == null) return result;

			var delegatesCollection = (List<Action<Notification>>)_hashtable[notificationName];
			if (delegatesCollection == null)
			{
				delegatesCollection = new List<Action<Notification>>();
				_hashtable.Add(notificationName, delegatesCollection);
			}
			delegatesCollection.Add(notificationDelegate);

			return result;
		}

		/// <summary>
		/// Removes matching entries from the receiver’s dispatch table.
		/// </summary>
		/// <param name="notificationDelegate">Delegate  that specifies the message the receiver sends notificationObserver to notify it of the notification posting</param>
		/// <param name="notificationName">The name of the notification for which to register the observer; that is, only notifications with this name are delivered to the observer</param>
		public bool RemoveObserver(Action<Notification> notificationDelegate, string notificationName)
		{
			bool result = false;
			if (string.IsNullOrEmpty(notificationName)) return result;
			if (notificationDelegate == null) return result;

			var delegatesCollection = (List<Action<Notification>>)_hashtable[notificationName];
			if (delegatesCollection != null)
			{
				delegatesCollection.Remove(notificationDelegate);
			}

			return result;
		}

		/// <summary>
		/// Creates a notification with a given name and sender and posts it to the receiver.
		/// </summary>
		/// <param name="notificationName">The name of the notification for which to register the observer; that is, only notifications with this name are delivered to the observer.</param>
		/// <param name="notification">The notification includinf the sender and a message.</param>
		public bool PostNotification(string notificationName, Notification notification)
		{
			bool result = false;
			if (string.IsNullOrEmpty(notificationName)) return result;
			if (notification == null) return result;

			var delegatesCollection = (List<Action<Notification>>)_hashtable[notificationName];
			if (delegatesCollection != null)
			{
				foreach (var notificationDelegate in delegatesCollection)
				{
					notificationDelegate(notification);
				}
			}

			return result;
		}

		/// <summary>
		/// Creates a notification with a given name and sender and posts it to the receiver with Empty notification.
		/// </summary>
		/// <param name="notificationName">The name of the notification for which to register the observer; that is, only notifications with this name are delivered to the observer.</param>
		public bool PostNotification(string notificationName)
		{
			bool result = false;
			if (string.IsNullOrEmpty(notificationName)) return result;

			var delegatesCollection = (List<Action<Notification>>)_hashtable[notificationName];
			if (delegatesCollection != null)
			{
				foreach (var notificationDelegate in delegatesCollection)
				{
					try
					{
						notificationDelegate(Notification.Empty);
					}
					catch (Exception e)
					{
						Debug.LogError($"Error fire Notification:{e}");
					}
				}
			}

			return result;
		}
	}

	public class Notification
	{
		private readonly object _sender;
		private readonly object _message;
		private object[] _args;

		public Notification(object p_sender, object p_message)
		{
			_sender = p_sender;
			_message = p_message;
		}

		public Notification(object p_message)
		{
			_message = p_message;
		}

		public Notification(params object[] args)
		{
			_args = args;
		}

		public object Sender
		{
			get { return _sender; }
		}

		public object Message
		{
			get { return _message; }
		}

		public object[] Args
		{
			get { return _args; }
		}

		public static Notification Empty
		{
			get
			{
				return new Notification(null, null);
			}
		}
	}

	public class NotificationEvent
	{
		public static string PATIENT_LOGON = "PATIENT_LOGON";
		public static string ACHIVE_UPDATEDATA = "ACHIVE_UPDATEDATA";
		public static string DOC_LOGON = "DOC_LOGON";
	}
}