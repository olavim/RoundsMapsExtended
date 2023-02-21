namespace MapsExt.Test
{
	public class ExecutionResult
	{
		public static ExecutionResult Pass()
		{
			return new ExecutionResult(true);
		}

		public static ExecutionResult Fail(string failReason)
		{
			return new ExecutionResult(false, failReason);
		}

		public readonly bool pass;
		public readonly string failReason;

		private ExecutionResult(bool pass) : this(pass, null) { }

		private ExecutionResult(bool pass, string failReason)
		{
			this.pass = pass;
			this.failReason = failReason;
		}
	}
}
