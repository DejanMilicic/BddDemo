using System;
using System.Security.Authentication;

namespace Digitalis.Infrastructure.Guards
{
    public static class AuthenticationGuard
    {
        public static void AgainstNull(object argument)
        {
            if (argument == null)
                throw new AuthenticationException();
        }

        public static void AgainstNullOrEmpty(string argument)
        {
            if (String.IsNullOrEmpty(argument))
                throw new AuthenticationException();
        }

        public static void Affirm(bool argument)
        {
            if (argument == false)
                throw new AuthenticationException();
        }

        public static void Affirm(bool? argument)
        {
            if (argument == null)
                throw new AuthenticationException();

            if (argument == false)
                throw new AuthenticationException();
        }
    }
}
