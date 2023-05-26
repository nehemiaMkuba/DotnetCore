using System;
using System.Net;
using System.Reflection;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

using static System.Math;

using Core.Domain.Exceptions;
using Core.Management.Extensions;

namespace Core.Management.Repositories
{
    public class HelperRepository
    {
        public static (bool IsValid, string Msisdn, string ErrorMessage) NormalizeMsisdn(string incomingMsidn, bool throwException = false)
        {
            string mobileNumber = incomingMsidn.ToDigits();
            bool isValid = Regex.IsMatch(mobileNumber, @"^(07|01|\+?2547|\+?2541|7|1)[\d]{8}$", RegexOptions.IgnoreCase);

            if (!isValid)
            {
                string exception = mobileNumber.Length < 1 ? "" : $" : {incomingMsidn}";
                if (throwException)
                {
                    throw new GenericException($"Invalid phone number{exception}", "AN005", HttpStatusCode.PreconditionFailed);
                }

                return (false, incomingMsidn, $"Invalid phone number{exception}");
            }

            mobileNumber = mobileNumber.TrimStart(new char[] { '0', '+' });
            return (true, mobileNumber.StartsWith("254") ? mobileNumber : $"254{mobileNumber}", null);
        }

        public static (string EmailAddress, bool IsValid) ValidateEmailAddress(string emailAddress, bool throwException = false)
        {
            emailAddress ??= string.Empty;
            const string emailValidationRegexString = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";

            if (string.IsNullOrEmpty(emailAddress) || !Regex.IsMatch(emailAddress, emailValidationRegexString, RegexOptions.IgnoreCase))
            {
                if (throwException) throw new GenericException($"The email address provided is invalid", "AN006", HttpStatusCode.BadRequest);

                return (EmailAddress: emailAddress.Trim().ToLower(), IsValid: false);
            }

            return (EmailAddress: emailAddress.Trim().ToLower(), IsValid: true);
        }

        public static void ValidatedParameter(string parameter, string value, out string result, bool throwException = false)
        {
            result = value?.Trim() ?? string.Empty;
            if (result.Length < 1 && throwException)
                throw new GenericException($"{parameter} must be provided to complete this request", "AN007", HttpStatusCode.Forbidden);
        }

        public static string ToStandardNumericFormat(long amount)
        {
            if (amount == 0) { return "0"; }

            if (amount < 100) { return (amount * 0.01).ToString(); }

            long quotient = DivRem(amount, 100, out long remainder);

            return $"{quotient + (remainder * 0.01):#,###.##}";
        }    

        public static string GenerateActivationCode()
        {
            using RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buffer = new byte[sizeof(uint)];
            rng.GetBytes(buffer);
            uint number = BitConverter.ToUInt32(buffer, 0);
            uint pin = number % 10000;
            return pin.ToString("D4");
        }

        public static string DescriptionAttr<T>(T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            if (fi != null)
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes != null && attributes.Length > 0) return attributes[0].Description;
                else return source.ToString();
            }

            return string.Empty;
        }

        public static long GenerateReference(long floor = 100000000000, long ceiling = 999999999999)
        {
            //Generate 12 digit reference
            //https://www.geeksforgeeks.org/random-vs-secure-random-numbers-java/
            //secure random numbers - must produce non-deterministic output - unpredictable and cryptographically strong machine entropy(os data eg keystrokes)
            using RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            ulong scale = ulong.MaxValue;
            while (scale == ulong.MaxValue)
            {
                // Get eight random bytes.
                byte[] eight_bytes = new byte[8];
                rng.GetBytes(eight_bytes);

                // Convert that into an uint.
                scale = BitConverter.ToUInt64(eight_bytes, 0);
            }

            // Add min to the scaled difference between max and min.
            return (long)(floor + (ceiling - floor) * (scale / (double)ulong.MaxValue));

        }

        public static int GenerateAccountNumber(int floor = 10000000, int ceiling = 99999999)
        {
            //generate 8 digit account suffix
            using RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            uint scale = uint.MaxValue;
            while (scale == uint.MaxValue)
            {
                // Get four random bytes.
                byte[] four_bytes = new byte[4];
                rng.GetBytes(four_bytes);

                // Convert that into an uint.
                scale = BitConverter.ToUInt32(four_bytes, 0);
            }

            // Add min to the scaled difference between max and min.
            // 2,147,483,647
            //  4,294,967,295
            return (int)(floor + (ceiling - floor) * (scale / (double)uint.MaxValue));

        }

        public static void EvaluatePinStrength(string pin)
        {
            bool valid = Regex.IsMatch(pin, @"^[0-9]{4}$", RegexOptions.IgnoreCase);
            if (!valid) throw new GenericException($"PIN must be 4 digits long", "AN024", HttpStatusCode.PreconditionFailed);
        }

        public static void EvaluatePasswordStrength(string password)
        {
            password ??= string.Empty;
            if (password.Length < 6) throw new GenericException($"Password must be at least 6 characters long", "AN025", HttpStatusCode.PreconditionFailed);
        }

     
    }
}
