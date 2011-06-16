using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xVancl.Framework.Test
{
	/// <summary>
	/// memcached delete command:
	/// Q:delete key\r\n
	/// R:DELETED|NOT_FOUND
	/// </summary>
	/// <returns></returns>
	class DeleteOperation : ItemOperation
	{
		public DeleteOperation(String key, PooledSocket socket) 
			: base(key, socket) 
		{ }
		
		protected override bool ExecuteAction()
		{
			String commend = String.Format("delete {0}", this.Key);
			this.Socket.SendCommand(commend);

			return String.Equals("DELETED", this.Socket.ReadResponse(), StringComparison.Ordinal);
		}
	}
}
