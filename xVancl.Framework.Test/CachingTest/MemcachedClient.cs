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

		public Boolean Set(String key,object value)
		{
			using (SetOperation s = new SetOperation(key,value,this.socket))
			{
				s.Execute();
				return s.Success;
			}
		}

		public void Delete(String key)
		{
			using (DeleteOperation d = new DeleteOperation(key,this.socket))
			{
				d.Execute();
			}
		}
	}
}
