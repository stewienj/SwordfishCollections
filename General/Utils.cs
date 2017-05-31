using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Swordfish.NET.General
{
	public static class Utils
	{
		//--------------------------------------------------------------------------------------------

		public static StringCollection ReadMemoryStreamIntoStringCollection(MemoryStream _ms)
		{
			// go to the start of the stream.
			_ms.Seek(0, SeekOrigin.Begin);

			const int maxBytes = 65536;
			StreamReader sr = new StreamReader(_ms);
			StringCollection result = new StringCollection();
			int nBytesRead = 0;
			string nextLine;
			while ((nextLine = sr.ReadLine()) != null)
			{
				nBytesRead += nextLine.Length;
				if (nBytesRead > maxBytes)
					break;
				result.Add(nextLine);
			}
			sr.Close();
			return result;
		}

		//--------------------------------------------------------------------------------------------

		// NOTE: Duplicate values are not ignored
		public static IEnumerable<string> GetListOfNodeAttributes(string _filename, string _nodeName, string _elementName)
		{
			if (File.Exists(_filename))
			{
				List<string> returnedAtts = new List<string>();

				XDocument xDoc = XDocument.Load(_filename);
				XElement root = xDoc.Root;
				if (root != null)
				{
					foreach (XElement node in root.Descendants(_nodeName))
					{
						XElement element = node.Element(_elementName);
						if (element != null)
						{
							string value = element.Value;
							if (returnedAtts.Contains(value) == false)
							{
								returnedAtts.Add(value);
								yield return value;
							}
						}
						else
						{
							XAttribute attribute = node.Attribute(_elementName);
							if (attribute != null)
							{
								string value = attribute.Value;
								if (returnedAtts.Contains(value) == false)
								{
									returnedAtts.Add(value);
									yield return value;
								}
							}
						}
					}
				}
			}
		}
	
		//--------------------------------------------------------------------------------------------------------

		/// <summary>
		///     Throws an <see cref="ArgumentNullException"/> if the
		///     provided string is null.
		///     Throws an <see cref="ArgumentOutOfRangeException"/> if the
		///     provided string is empty.
		/// </summary>
		/// <param name="stringParameter">The object to test for null and empty.</param>
		/// <param name="parameterName">The string for the ArgumentException parameter, if thrown.</param>
		[DebuggerStepThrough]
		public static void RequireNotNullOrEmpty(string stringParameter, string parameterName)
		{
			if (stringParameter == null)
			{
				throw new ArgumentNullException(parameterName);
			}
			else if (stringParameter.Length == 0)
			{
				throw new ArgumentOutOfRangeException(parameterName);
			}
		}
	
		//--------------------------------------------------------------------------------------------

		/// <summary>
		///     Throws an <see cref="ArgumentNullException"/> if the
		///     provided object is null.
		/// </summary>
		/// <param name="obj">The object to test for null.</param>
		/// <param name="parameterName">The string for the ArgumentNullException parameter, if thrown.</param>
		[DebuggerStepThrough]
		public static void RequireNotNull(object obj, string parameterName)
		{
			if (obj == null)
			{
				throw new ArgumentNullException(parameterName);
			}
		}

		//--------------------------------------------------------------------------------------------

		/// <summary>
		///     Throws an <see cref="ArgumentException"/> if the provided truth is false.
		/// </summary>
		/// <param name="truth">The value assumed to be true.</param>
		/// <param name="parameterName">The string for <see cref="ArgumentException"/>, if thrown.</param>
		[DebuggerStepThrough]
		public static void RequireArgument(bool truth, string parameterName)
		{
			RequireNotNullOrEmpty(parameterName, "parameterName");

			if (!truth)
			{
				throw new ArgumentException(parameterName);
			}
		}

		//--------------------------------------------------------------------------------------------

		/// <summary>
		///     Throws an <see cref="ArgumentException"/> if the provided truth is false.
		/// </summary>
		/// <param name="truth">The value assumed to be true.</param>
		/// <param name="paramName">The paramName for the <see cref="ArgumentException"/>, if thrown.</param>
		/// <param name="message">The message for <see cref="ArgumentException"/>, if thrown.</param>
		[DebuggerStepThrough]
		public static void RequireArgument(bool truth, string paramName, string message)
		{
			RequireNotNullOrEmpty(paramName, "paramName");
			RequireNotNullOrEmpty(message, "message");

			if (!truth)
			{
				throw new ArgumentException(message, paramName);
			}
		}

		//--------------------------------------------------------------------------------------------

		/// <summary>
		///     Throws an <see cref="ArgumentOutOfRangeException"/> if the provided truth is false.
		/// </summary>
		/// <param name="truth">The value assumed to be true.</param>
		/// <param name="parameterName">The string for <see cref="ArgumentOutOfRangeException"/>, if thrown.</param>
		[DebuggerStepThrough]
		public static void RequireArgumentRange(bool truth, string parameterName)
		{
			RequireNotNullOrEmpty(parameterName, "parameterName");

			if (!truth)
			{
				throw new ArgumentOutOfRangeException(parameterName);
			}
		}

		//--------------------------------------------------------------------------------------------
		
		/// <summary>
		///     Throws an <see cref="ArgumentOutOfRangeException"/> if the provided truth is false.
		/// </summary>
		/// <param name="truth">The value assumed to be true.</param>
		/// <param name="paramName">The paramName for the <see cref="ArgumentOutOfRangeException"/>, if thrown.</param>
		/// <param name="message">The message for <see cref="ArgumentOutOfRangeException"/>, if thrown.</param>
		[DebuggerStepThrough]
		public static void RequireArgumentRange(bool truth, string paramName, string message)
		{
			RequireNotNullOrEmpty(paramName, "paramName");
			RequireNotNullOrEmpty(message, "message");

			if (!truth)
			{
				throw new ArgumentOutOfRangeException(message, paramName);
			}
		}

	}
}
