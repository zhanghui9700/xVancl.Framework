using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace xVancl.Framework.Test
{
	/// <summary>
	/// memcached get command:
	/// Q:get key\r\n
	/// R:VALUE {key} {flag} {dataLength}\r\n
	///   data\r\n
	///   END\r\n
	///	Or:END\r\n(not key's vlaue)
	/// </summary>
	/// <returns></returns>
	class GetOperation : ItemOperation
	{
		public GetOperation(String key, PooledSocket socket)
			: base(key, socket)
		{

		}

		private object _result;
		public object Result
		{ 
			get { return _result; } 
		}

		protected override bool ExecuteAction()
		{
			PooledSocket socket = this.Socket;

			socket.SendCommand("get " + this.Key);
			GetResponse r = GetHelper.ReadItem(socket);

			if (r != null)
			{
				ITranscoder trandcoder = new DefaultTranscoder();

				this._result = trandcoder.Deserialize(r.Item);
				GetHelper.FinishCurrent(this.Socket);
			}

			return true;
		}
	}

	internal static class GetHelper
	{
		private static LOG log = new LOG();

		public static void FinishCurrent(PooledSocket socket)
		{
			string response = socket.ReadResponse();

			if (String.Compare(response, "END", StringComparison.Ordinal) != 0)
				throw new Exception("No END was received.");
		}

		public static GetResponse ReadItem(PooledSocket socket)
		{
			string description = socket.ReadResponse();

			if (String.Compare(description, "END", StringComparison.Ordinal) == 0)
				return null;

			if (description.Length < 6 || String.Compare(description, 0, "VALUE ", 0, 6, StringComparison.Ordinal) != 0)
				throw new Exception("No VALUE response received.\r\n" + description);

			ulong cas = 0;
			string[] parts = description.Split(' ');

			// response is:
			// VALUE <key> <flags> <bytes> [<cas unique>]
			// 0     1     2       3       4
			//
			// cas only exists in 1.2.4+
			//
			if (parts.Length == 5)
			{
				if (!UInt64.TryParse(parts[4], out cas))
					throw new Exception("Invalid CAS VALUE received.");

			}
			else if (parts.Length < 4)
			{
				throw new Exception("Invalid VALUE response received: " + description);
			}

			ushort flags = UInt16.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
			int length = Int32.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);

			byte[] allData = new byte[length];
			byte[] eod = new byte[2];

			socket.Read(allData, 0, length);
			socket.Read(eod, 0, 2); // data is terminated by \r\n

			GetResponse retval = new GetResponse(parts[1], flags, cas, allData);

			if (log.IsDebugEnabled)
				log.DebugFormat("Received value. Data type: {0}, size: {1}.", retval.Item.Flag, retval.Item.Data.Count);

			return retval;
		}
	}

	internal class GetResponse
	{
		private GetResponse() { }
		public GetResponse(string key, ushort flags, ulong casValue, byte[] data) : 
			this(key, flags, casValue, data, 0, data.Length) { }

		public GetResponse(string key, ushort flags, ulong casValue, byte[] data, int offset, int count)
		{
			this.Key = key;
			this.CasValue = casValue;

			this.Item = new CacheItem(flags, new ArraySegment<byte>(data, offset, count));
		}

		public readonly string Key;
		public readonly ulong CasValue;
		public readonly CacheItem Item;
	}

	public struct CacheItem
	{
		private ArraySegment<byte> data;
		private ushort flags;

		/// <summary>
		/// Initializes a new instance of <see cref="T:CacheItem"/>.
		/// </summary>
		/// <param name="flags">Custom item data.</param>
		/// <param name="data">The serialized item.</param>
		public CacheItem(ushort flags, ArraySegment<byte> data)
		{
			this.data = data;
			this.flags = flags;
		}

		/// <summary>
		/// The data representing the item being stored/retireved.
		/// </summary>
		public ArraySegment<byte> Data
		{
			get { return this.data; }
			set { this.data = value; }
		}

		/// <summary>
		/// Flags set for this instance.
		/// </summary>
		public ushort Flag
		{
			get { return this.flags; }
			set { this.flags = value; }
		}
	}

	public interface ITranscoder
	{
		/// <summary>
		/// Serializes an object for storing in the cache.
		/// </summary>
		/// <param name="o">The object to serialize</param>
		/// <returns>The serialized object</returns>
		CacheItem Serialize(Object o);

		/// <summary>
		/// Deserializes the <see cref="T:CacheItem"/> into an object.
		/// </summary>
		/// <param name="item">The stream that contains the data to deserialize.</param>
		/// <returns>The deserialized object</returns>
		object Deserialize(CacheItem item);
	}

	public sealed class DefaultTranscoder : ITranscoder
	{
		internal const ushort RawDataFlag = 0xfa52;
		internal static readonly byte[] EmptyArray = new byte[0];

		CacheItem ITranscoder.Serialize(object value)
		{
			// raw data is a special case when some1 passes in a buffer (byte[] or ArraySegment<byte>)
			if (value is ArraySegment<byte>)
			{
				// ArraySegment<byte> is only passed in when a part of buffer is being 
				// serialized, usually from a MemoryStream (To avoid duplicating arrays 
				// the byte[] returned by MemoryStream.GetBuffer is placed into an ArraySegment.)
				// 
				return new CacheItem(RawDataFlag, (ArraySegment<byte>)value);
			}

			byte[] tmpByteArray = value as byte[];

			// - or we just received a byte[]. No further processing is needed.
			if (tmpByteArray != null)
			{
				return new CacheItem(RawDataFlag, new ArraySegment<byte>(tmpByteArray));
			}

			TypeCode code = value == null ? TypeCode.Empty : Type.GetTypeCode(value.GetType());

			byte[] data;
			int length = -1;

			switch (code)
			{
				case TypeCode.Empty:
					data = DefaultTranscoder.EmptyArray;
					length = 0;
					break;

				case TypeCode.String:
					data = Encoding.UTF8.GetBytes((string)value);
					break;

				case TypeCode.Boolean:
					data = BitConverter.GetBytes((bool)value);
					break;

				case TypeCode.Int16:
					data = BitConverter.GetBytes((short)value);
					break;

				case TypeCode.Int32:
					data = BitConverter.GetBytes((int)value);
					break;

				case TypeCode.Int64:
					data = BitConverter.GetBytes((long)value);
					break;

				case TypeCode.UInt16:
					data = BitConverter.GetBytes((ushort)value);
					break;

				case TypeCode.UInt32:
					data = BitConverter.GetBytes((uint)value);
					break;

				case TypeCode.UInt64:
					data = BitConverter.GetBytes((ulong)value);
					break;

				case TypeCode.Char:
					data = BitConverter.GetBytes((char)value);
					break;

				case TypeCode.DateTime:
					data = BitConverter.GetBytes(((DateTime)value).ToBinary());
					break;

				case TypeCode.Double:
					data = BitConverter.GetBytes((double)value);
					break;

				case TypeCode.Single:
					data = BitConverter.GetBytes((float)value);
					break;

				default:
					using (MemoryStream ms = new MemoryStream())
					{
						new BinaryFormatter().Serialize(ms, value);

						code = TypeCode.Object;
						data = ms.GetBuffer();
						length = (int)ms.Length;
					}
					break;
			}

			if (length < 0)
				length = data.Length;

			return new CacheItem((ushort)((ushort)code | 0x0100), new ArraySegment<byte>(data, 0, length));
		}

		object ITranscoder.Deserialize(CacheItem item)
		{
			if (item.Flag == RawDataFlag)
			{
				ArraySegment<byte> tmp = item.Data;

				if (tmp.Count == tmp.Array.Length)
					return tmp.Array;

				// we should never arrive here, but it's better to be safe than sorry
				byte[] retval = new byte[tmp.Count];

				Array.Copy(tmp.Array, tmp.Offset, retval, 0, tmp.Count);

				return retval;
			}

			TypeCode code = (TypeCode)(item.Flag & 0x00ff);

			if (code == TypeCode.Empty)
				return null;

			byte[] data = item.Data.Array;
			int offset = item.Data.Offset;
			int count = item.Data.Count;

			switch (code)
			{
				case TypeCode.String:
					return Encoding.UTF8.GetString(data, offset, count);

				case TypeCode.Boolean:
					return BitConverter.ToBoolean(data, offset);

				case TypeCode.Int16:
					return BitConverter.ToInt16(data, offset);

				case TypeCode.Int32:
					return BitConverter.ToInt32(data, offset);

				case TypeCode.Int64:
					return BitConverter.ToInt64(data, offset);

				case TypeCode.UInt16:
					return BitConverter.ToUInt64(data, offset);

				case TypeCode.UInt32:
					return BitConverter.ToUInt32(data, offset);

				case TypeCode.UInt64:
					return BitConverter.ToUInt64(data, offset);

				case TypeCode.Char:
					return BitConverter.ToChar(data, offset);

				case TypeCode.DateTime:
					return DateTime.FromBinary(BitConverter.ToInt64(data, offset));

				case TypeCode.Double:
					return BitConverter.ToDouble(data, offset);

				case TypeCode.Single:
					return BitConverter.ToSingle(data, offset);

				case TypeCode.Object:
					using (MemoryStream ms = new MemoryStream(data, offset, count))
					{
						return new BinaryFormatter().Deserialize(ms);
					}

				default: throw new InvalidOperationException("Unknown TypeCode was returned: " + code);
			}
		}
	}
}
