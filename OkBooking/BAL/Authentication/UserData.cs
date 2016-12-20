using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace BAL.Authentication {
	public interface IUserData {
		ExchangeVersion Version { get; }
		string EmailAddress { get; }
		SecureString Password { get; }
		Uri AutodiscoverUrl { get; set; }
	}

	public class UserData : IUserData {
		public ExchangeVersion Version { get { return ExchangeVersion.Exchange2013_SP1; } }

		public string EmailAddress {
			get;
			private set;
		}

		public SecureString Password {
			get;
			private set;
		}

		public Uri AutodiscoverUrl {
			get;
			set;
		}

		public UserData(string email, string password) {
			EmailAddress = email;
			Password = new SecureString();
			foreach (char c in password) { Password.AppendChar(c); }
		}
	}

	public class UserDataFromConsole : IUserData {
		private static UserDataFromConsole userData;

		public static IUserData GetUserData() {
			if (userData == null) {
				GetUserDataFromConsole();
			}

			return userData;
		}

		private static void GetUserDataFromConsole() {
			userData = new UserDataFromConsole();

			Console.Write("Enter email address: ");
			userData.EmailAddress = Console.ReadLine();

			userData.Password = new SecureString();

			Console.Write("Enter password: ");

			while (true) {
				ConsoleKeyInfo userInput = Console.ReadKey(true);
				if (userInput.Key == ConsoleKey.Enter) {
					break;
				} else if (userInput.Key == ConsoleKey.Escape) {
					return;
				} else if (userInput.Key == ConsoleKey.Backspace) {
					if (userData.Password.Length != 0) {
						userData.Password.RemoveAt(userData.Password.Length - 1);
					}
				} else {
					userData.Password.AppendChar(userInput.KeyChar);
					Console.Write("*");
				}
			}

			Console.WriteLine();

			userData.Password.MakeReadOnly();
		}

		public ExchangeVersion Version { get { return ExchangeVersion.Exchange2013_SP1; } }

		public string EmailAddress {
			get;
			private set;
		}

		public SecureString Password {
			get;
			private set;
		}

		public Uri AutodiscoverUrl {
			get;
			set;
		}
	}
}
