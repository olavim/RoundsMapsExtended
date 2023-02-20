namespace MapsExt.Test
{
	public class TestResult
	{
		public static TestResult Pass()
		{
			return new TestResult(true);
		}

		public static TestResult Fail(string failReason)
		{
			return new TestResult(false, failReason);
		}

		public readonly bool pass;
		public readonly string failReason;

		private TestResult(bool pass) : this(pass, null) { }

		private TestResult(bool pass, string failReason)
		{
			this.pass = pass;
			this.failReason = failReason;
		}
	}
}
