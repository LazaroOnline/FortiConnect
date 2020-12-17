using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FortiConnect.Services
{
	// TODO: create a NuGet package with this Retry class.
	public class Retry
	{
		/// <summary>
		/// Retry an action that returns an object.
		/// </summary>
		/// <typeparam name="T">Type of the object returned by the action.</typeparam>
		/// <param name="action">action to retry</param>
		/// <param name="whileIs">condition to keep retrying, if null it wi</param>
		/// <param name="maxRetries">Maximum number of retries, if null it will retry forever until the whileIs condition is met.</param>
		/// <param name="every">TimeSpan to wait between retries, if null it will not add any delay.</param>
		/// <param name="catchErrors">If true it will wrap the action in a try-catch.</param>
		/// <param name="untilNoException">If true it will keep retrying until the action doesn't throw any exception, or the maxRetries limit is reached.</param>
		/// <returns></returns>
		public static T Get<T>(Func<int, T> action, Func<int, bool> whileIs = null, int? maxRetries = null, TimeSpan? every = null, bool catchErrors = true, bool untilNoException = true)
		{
			T result = default(T);
			int retryNumber = 0;
			while (true) {
				retryNumber++;
				Exception exceptionThrown = null;
				if (catchErrors) {
					try {
						result = action(retryNumber);
					} catch(Exception ex) {
						exceptionThrown = ex;
					}
				} else {
					result = action(retryNumber);
				}
				var maxRetriesReached = maxRetries != null && retryNumber >= maxRetries;
				var tryAgainCondition = whileIs == null || whileIs(retryNumber);
				var tryAgainDueToExceptions = untilNoException && exceptionThrown != null;
				if ((maxRetriesReached || !tryAgainCondition) && !tryAgainDueToExceptions) {
					break;
				}
				if (every != null) {
					System.Threading.Thread.Sleep(every.Value);
				}
			}
			return result;
		}
		
		/// <summary>
		/// Retry an action.
		/// </summary>
		/// <param name="action">action to retry</param>
		/// <param name="whileIs">condition to keep retrying, if null it wi</param>
		/// <param name="maxRetries">Maximum number of retries, if null it will retry forever until the whileIs condition is met.</param>
		/// <param name="every">TimeSpan to wait between retries, if null it will not add any delay.</param>
		/// <param name="catchErrors">If true it will wrap the action in a try-catch.</param>
		/// <param name="untilNoException">If true it will keep retrying until the action doesn't throw any exception, or the maxRetries limit is reached.</param>
		public static void Action(Action<int> action, Func<int, bool> whileIs = null, int? maxRetries = null, TimeSpan? every = null, bool catchErrors = true, bool untilNoException = true)
		{
			Retry.Get<int>(r => { action(r); return 1; }, whileIs, maxRetries, every, catchErrors, untilNoException);
		}

	}
}
