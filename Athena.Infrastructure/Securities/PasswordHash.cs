/* 
 * Password Hashing With PBKDF2 (http://crackstation.net/hashing-security.htm).
 * Copyright (c) 2013, Taylor Hornby
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System.Security.Cryptography;

namespace Athena.Infrastructure.Securities;

/// <summary>
/// Salted password hashing with PBKDF2-SHA1.
/// Author: havoc AT defuse.ca
/// www: http://crackstation.net/hashing-security.htm
/// Compatibility: .NET 3.0 and later.
/// </summary>
public static class PasswordHash
{
    // The following constants may be changed without breaking existing hashes.
    private const int SaltByteSize = 24;
    private const int HashByteSize = 24;
    private const int Pbkdf2Iterations = 3412;

    private const int IterationIndex = 0;
    private const int SaltIndex = 1;
    private const int Pbkdf2Index = 2;

    /// <summary>
    /// Creates a salted PBKDF2 hash of the password.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hash of the password.</returns>
    public static string CreateHash(string password)
    {
        // Generate a random salt
        // var cspRng = new RNGCryptoServiceProvider();
        var cspRng = RandomNumberGenerator.Create();
        var salt = new byte[SaltByteSize];
        cspRng.GetBytes(salt);

        // Hash the password and encode the parameters
        var hash = Pbkdf2(password, salt, Pbkdf2Iterations, HashByteSize);
        return Pbkdf2Iterations + ":" +
               Convert.ToBase64String(salt) + ":" +
               Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Creates a salted PBKDF2 hash of the password.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <param name="pbkdf2Iterations">pbkdf2 iterations</param>
    /// <returns>The hash of the password.</returns>
    public static string CreateHash(string password, int pbkdf2Iterations)
    {
        // Generate a random salt
        // var cspRng = new RNGCryptoServiceProvider();
        var cspRng = RandomNumberGenerator.Create();
        var salt = new byte[SaltByteSize];
        cspRng.GetBytes(salt);

        // Hash the password and encode the parameters
        var hash = Pbkdf2(password, salt, pbkdf2Iterations, HashByteSize);
        return pbkdf2Iterations + ":" +
               Convert.ToBase64String(salt) + ":" +
               Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Validates a password given a hash of the correct one.
    /// </summary>
    /// <param name="password">The password to check.</param>
    /// <param name="correctHash">A hash of the correct password.</param>
    /// <returns>True if the password is correct. False otherwise.</returns>
    public static bool ValidatePassword(string password, string correctHash)
    {
        // Extract the parameters from the hash
        char[] delimiter = {':'};
        var split = correctHash.Split(delimiter);
        var iterations = int.Parse(split[IterationIndex]);
        var salt = Convert.FromBase64String(split[SaltIndex]);
        var hash = Convert.FromBase64String(split[Pbkdf2Index]);

        var testHash = Pbkdf2(password, salt, iterations, hash.Length);
        return SlowEquals(hash, testHash);
    }

    /// <summary>
    /// Compares two byte arrays in length-constant time. This comparison
    /// method is used so that password hashes cannot be extracted from
    /// on-line systems using a timing attack and then attacked off-line.
    /// </summary>
    /// <param name="a">The first byte array.</param>
    /// <param name="b">The second byte array.</param>
    /// <returns>True if both byte arrays are equal. False otherwise.</returns>
    private static bool SlowEquals(IReadOnlyList<byte> a, IReadOnlyList<byte> b)
    {
        var diff = (uint) a.Count ^ (uint) b.Count;
        for (var i = 0; i < a.Count && i < b.Count; i++)
            diff |= (uint) (a[i] ^ b[i]);
        return diff == 0;
    }

    /// <summary>
    /// Computes the PBKDF2-SHA1 hash of a password.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="iterations">The PBKDF2 iteration count.</param>
    /// <param name="outputBytes">The length of the hash to generate, in bytes.</param>
    /// <returns>A hash of the password.</returns>
    private static byte[] Pbkdf2(string password, byte[] salt, int iterations, int outputBytes)
    {
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 1000, HashAlgorithmName.SHA1);
        pbkdf2.IterationCount = iterations;
        return pbkdf2.GetBytes(outputBytes);
    }
}