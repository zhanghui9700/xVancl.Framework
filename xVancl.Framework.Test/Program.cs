using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xVancl.Framework.Test
{
	class Program
	{
		static void Main(string[] args)
		{
			MemcachedClient_Get_Test();

			Console.WriteLine("done...");
			Console.ReadKey();
		}

		private static void MemcachedClient_Get_Test()
		{
			MemcachedClient client = new MemcachedClient();
			var foo = client.Get("foo");
			Console.WriteLine("---------foo-----------");
			Console.WriteLine(foo);
			Console.WriteLine("---------foo-----------");
			
			var foo1 = client.Get("foo1");
			Console.WriteLine("---------foo1-----------");
			Console.WriteLine(foo1);
			Console.WriteLine("---------foo1-----------");
		}
	}
}
