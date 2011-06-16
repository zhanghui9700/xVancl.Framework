using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xVancl.Framework.Test
{
	/// <summary>
	/// memcached set command:
	/// Q:set {key} {flag} {expires} {dataLength}\r\n
	///   data\r\n
	/// R:STORED\r\n(if stored success)
	///   CLIENT_ERROR(if bed data chunk)
	/// </summary>
	/// <returns></returns>
	class SetOperation : ItemOperation
	{
		object value;
		public SetOperation(String key, Object value, PooledSocket socket)
			: base(key, socket)
		{
			this.value = value;
		}
		
		protected override bool ExecuteAction()
		{
			ITranscoder transcode = new DefaultTranscoder();
			CacheItem item = transcode.Serialize(this.value);
			
			String command = String.Format("set {0} 0 0 {1}",this.Key,item.Data.Count - item.Data.Offset);
			ArraySegment<byte> commandByte = PooledSocket.GetCommandBuffer(command);
			this.Socket.Write(new ArraySegment<byte>[]{commandByte,item.Data,new ArraySegment<byte>(new byte[2] { (byte)'\r', (byte)'\n' })});


			return "STORED".Equals(this.Socket.ReadResponse(),StringComparison.Ordinal);
		}
	}
}
