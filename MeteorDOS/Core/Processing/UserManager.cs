using Cosmos.System.FileSystem.VFS;
using MeteorDOS.Core.Encryption;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MeteorDOS.Core.Processing
{
    public struct User
    {
        public string Name;
        public byte[] Password;  // Change to byte array for proper hash handling
    }

    public class UserManager
    {
        public static string CurrentUser = string.Empty;
        public const byte Separator = 0xCF;
        public const string passwd = @"0:\Core\passwd";

        public static void CreateUser(string username, string password)
        {
            try
            {
                if (!VFSManager.FileExists(passwd))
                {
                    VFSManager.CreateFile(passwd);
                }
                if (UserExists(username))
                {
                    Console.WriteLine("User with that name already exists");
                    return;
                }
                List<byte> data = new List<byte>(File.ReadAllBytes(passwd));
                data.AddRange(Encoding.UTF8.GetBytes(username));
                data.Add(Separator);
                data.AddRange(SHA256.ComputeHash(Encoding.UTF8.GetBytes(password)));
                data.Add(Separator);
                File.WriteAllBytes(passwd, data.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                // TODO: Save logs to file
            }
        }

        public static void RemoveUser(string username)
        {
            try
            {
                if (!VFSManager.FileExists(passwd))
                {
                    Console.WriteLine("User database file does not exist.");
                    return;
                }

                List<byte> newData = new List<byte>();
                byte[] hashes = File.ReadAllBytes(passwd);
                List<byte> currentData = new List<byte>();
                bool found = false;

                for (int i = 0; i < hashes.Length; i++)
                {
                    if (hashes[i] == Separator)
                    {
                        if (currentData.Count > 0)
                        {
                            string storedUsername = Encoding.UTF8.GetString(currentData.ToArray());
                            if (storedUsername == username)
                            {
                                found = true;
                                currentData.Clear();
                                while (i < hashes.Length && hashes[i] != Separator)
                                {
                                    i++;
                                }
                                if (i < hashes.Length)
                                {
                                    i++;
                                }
                                continue;
                            }
                            else
                            {
                                newData.AddRange(currentData);
                                newData.Add(Separator);  // Don't forget to add the separator back
                            }
                            currentData.Clear();
                        }
                    }
                    currentData.Add(hashes[i]);
                }

                if (!found)
                {
                    Console.WriteLine("User not found.");
                    return;
                }
                newData.AddRange(currentData);
                File.WriteAllBytes(passwd, newData.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing user: {ex.Message}");
                // TODO: Save logs to file
            }
        }

        public static User[] GetUserHashes()
        {
            List<User> users = new List<User>();

            try
            {
                byte[] hashes = File.ReadAllBytes(passwd);
                List<byte> currentData = new List<byte>();
                User user = new User();

                for (int i = 0; i < hashes.Length; i++)
                {
                    if (hashes[i] == Separator)
                    {
                        if (currentData.Count > 0)
                        {
                            if (user.Name == null)
                            {
                                user.Name = Encoding.UTF8.GetString(currentData.ToArray());
                            }
                            else
                            {
                                user.Password = currentData.ToArray();
                                users.Add(user);
                                user = new User();
                            }
                            currentData.Clear();
                        }
                    }
                    else
                    {
                        currentData.Add(hashes[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading user hashes: {ex.Message}");
                // TODO: Save logs to file
            }

            return users.ToArray();
        }

        public static User GetUser(string username)
        {
            foreach (User user in GetUserHashes())
            {
                if (user.Name == username)
                {
                    return user;
                }
            }
            return new User();
        }

        public static bool UserExists(string username)
        {
            User user = GetUser(username);
            return user.Name != null;
        }

        public static void LoginPrompt()
        {
            while (true)
            {
                Console.Write("Username: ");
                string username = Console.ReadLine();
                Console.Write("Password: ");
                string password = Console.ReadLine();
                User user = GetUser(username);
                if (user.Name != null)
                {
                    byte[] passwordHash = SHA256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    if (CompareHashes(user.Password, passwordHash))
                    {
                        CurrentUser = username;
                        Console.WriteLine($"Successfully logged in as {username}");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid password");
                    }
                }
                else
                {
                    Console.WriteLine("User with that username doesn't exist");
                }
            }
        }

        public static int GetUsersCount()
        {
            return GetUserHashes().Length;
        }

        public static void RegisterPrompt()
        {
            while (true)
            {
                Console.Write("Username: ");
                string username = Console.ReadLine();
                Console.Write("Password: ");
                string password = Console.ReadLine();
                if (UserExists(username))
                {
                    Console.WriteLine("User with that name already exists");
                    continue;
                }
                else if (username.Length < 3)
                {
                    Console.WriteLine("Name length cannot be lower than 3");
                    continue;
                }
                if (password.Length < 4)
                {
                    Console.WriteLine("Password length cannot be lower than 4");
                    continue;
                }
                CreateUser(username, password);
                break;
            }
        }

        private static bool CompareHashes(byte[] hash1, byte[] hash2)
        {
            if (hash1.Length != hash2.Length)
            {
                return false;
            }

            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
