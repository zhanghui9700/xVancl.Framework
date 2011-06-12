using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xVancl.Framework.Test
{
	internal abstract class ItemOperation : Operation
	{
		private string key;
		private string hashedKey;

		private PooledSocket socket;

		protected ItemOperation(string key,PooledSocket socket)
			: base()
		{
			this.key = key;
			this.socket = socket;
		}

		protected string Key
		{
			get { return this.key; }
		}

		/// <summary>
		/// Gets the hashed bersion of the key which should be used as key in communication with memcached
		/// </summary>
		protected string HashedKey
		{
			get { return this.key; }
		}

		protected PooledSocket Socket
		{
			get
			{
				if (this.socket == null)
				{
					throw new NullReferenceException();
				}

				return this.socket;
			}
		}

		public override void Dispose()
		{
			GC.SuppressFinalize(this);

			if (this.socket != null)
			{
				((IDisposable)this.socket).Dispose();
				this.socket = null;
			}

			base.Dispose();
		}
	}
}
