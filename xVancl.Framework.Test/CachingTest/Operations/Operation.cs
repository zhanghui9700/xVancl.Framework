using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xVancl.Framework.Test
{
	abstract class Operation : IDisposable
	{
		private bool isDisposed;
		private bool success;
			

		public void Execute()
		{
			this.success = false;

			try
			{
				if (this.CheckDisposed(false))
					return;

				this.success = this.ExecuteAction();
			}
			catch (NotSupportedException)
			{
				throw;
			}
			catch (Exception)
			{
				// TODO generic catch-all does not seem to be a good idea now. Some errors (like command not supported by server) should be exposed while retaining the fire-and-forget behavior
				throw;
			}
		}

		protected abstract bool ExecuteAction();

		protected bool CheckDisposed(bool throwOnError)
		{
			if (throwOnError && this.isDisposed)
				throw new ObjectDisposedException("Operation");

			return this.isDisposed;
		}

		public bool Success
		{
			get { return this.success; }
		}

		#region IDisposable 成员
		public virtual void Dispose()
		{
			this.isDisposed = true;
		}

		void IDisposable.Dispose()
		{
			this.Dispose();
		}

		#endregion
	}
}
