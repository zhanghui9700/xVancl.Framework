using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xVancl.Framework.Test
{
	public class MemcachedClient
	{
		private PooledSocket socket = new PooledSocket(null);
		public object Get(String key)
		{
			using (GetOperation g = new GetOperation(key,this.socket))
			{
				g.Execute();

				return g.Result;
			}
		}
	}
}
