using System.Globalization;
using System.Text.RegularExpressions;

namespace server.Common.Utils
{
    public static class ValidatorHepler
    {
        public static bool IsValidEmail(string email)
        {
            var regex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, regex);
        }

        public static bool EmailValidation(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                static string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static string GenerateRandomNumberList(int length = 6)
        {
            const string validChars = "0123456789";
            var random = new Random();
            var numbers = new char[length];
            for (int i = 0; i < length; i++)
            {
                numbers[i] = validChars[random.Next(validChars.Length)];
            }
            return new string(numbers);
        }
    }
}
