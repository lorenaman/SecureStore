﻿using System;
using System.Collections.Generic;

namespace NeoSmart.SecureStore.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var testSuite = new Dictionary<string, object>
            {
                {"string", "hello"},
                {"int", 42},
                {"guid", Guid.NewGuid()}
            };

            using (var secure = SecretsManager.NewStore())
            {
                secure.LoadKeyFromPassword("test123");

                //Test adding
                foreach (var secret in testSuite)
                {
                    secure.Set(secret.Key, secret.Value);
                }

                //Test exporting the key (also so we can test key <---> password compatibility later)
                secure.SaveKeyFile("keyfile.bin");

                //Test saving
                secure.SaveSecretsToFile("encrypted.bin");
            }

            //Test loading encrypted data from disk
            using (var test1 = SecretsManager.LoadStore("encrypted.bin"))
            {

                //Test decrypting with previously-exported key file
                test1.LoadKeyFromFile("keyfile.bin");

                //Test decryption of basic string
                if (test1.Retrieve<string>("string") != (string) testSuite["string"])
                {
                    throw new Exception("Problem retrieving previously-saved string with key from file!");
                }

                //Test decryption of complex type (GUID)
                if (test1.Retrieve<Guid>("guid") != (Guid) testSuite["guid"])
                {
                    throw new Exception("Problem retrieving previously-saved GUID with key from file!");
                }
            }

            //Test decryption from password
            using (var test2 = SecretsManager.LoadStore("encrypted.bin"))
            {
                test2.LoadKeyFromPassword("test123");

                //Test decryption of basic int
                if (test2.Retrieve<int>("int") != (int) testSuite["int"])
                {
                    throw new Exception("Problem retrieving previously-saved int with key from password!");
                }
            }

            Console.WriteLine("Test passed!");
        }
    }
}
