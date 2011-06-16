using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;

namespace xVancl.Framework.Test
{
	class Program
	{
		static void Main(string[] args)
		{
			//MemcachedClient_Get_Test();
			//MemcachedClient_Set_Test();
			MemcachedClient_Delete_Test();

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
			
			Person p = client.Get("foo1") as Person;
			if (p == null)
				Console.WriteLine("---------foo1 null-----------");
			else
			{
				Console.WriteLine("---------foo1-----------");
				Console.WriteLine(p.ToString());
				Console.WriteLine("---------foo1-----------");
			}
		}

		private static void MemcachedClient_Set_Test()
		{
			MemcachedClient client = new MemcachedClient();
			client.Set("foo1", new Person { Name="zhanghui",Age=27 });

		}

		private static void MemcachedClient_Delete_Test()
		{
			MemcachedClient client = new MemcachedClient();
			client.Delete("foo1");
		}
	}

	[Serializable]
	public class Person
	{
		public String Name { get; set; }
		public Int32 Age { get; set; }

		public override string ToString()
		{
			return String.Format("Name:{0},Age:{1}",this.Name,this.Age);
		}
	}
}
