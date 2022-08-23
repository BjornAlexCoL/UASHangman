using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Hangman.Models;
//using System.Text;

namespace Hangman
{
    class Program
    {
        static void Main(string[] args)
        {
            do
            {
                GoHangman();
            }
            while (GetValidLetters("Do you want to play again? (y/n)", "yYnN") == "Y");
        }
        private static void GoHangman()
        {
            List<string> guessedWord = new List<string>();
            string secretWord = GetSecretWord();
            char[] displayWordCharArray = new string('_', secretWord.Length).ToCharArray();
            string guessedLetters = "";
            int guesses = 0;
            bool validGuess = false;
            string inpString;
            do
            {
                DisplayHangman(guesses, displayWordCharArray);

                Console.WriteLine("\nThis is try number " + (guesses + 1));
                if (guessedLetters.Length > 0)
                {
                    DisplayGuessedLetters("\nYou have guessed on letters ", guessedLetters, secretWord);
                }
                DisplayGuessedWord(guessedWord);// Display guessed Word
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                inpString = GetValidLetters("Please, Input a Letter (A-Ö) or a Word",
                                         "abcdefghijklmnopqrstuvwxyzåäöABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ");
                //Console.WriteLine(inpString);
                validGuess = false;
                if (inpString.Length == 1)
                {
                    if (!guessedLetters.Contains(inpString))//Already guessed that letter
                    {
                        validGuess = true;
                        guessedLetters += inpString;
                        for (int testLoop = 0; testLoop < secretWord.Length; testLoop++)//Loop to go through and compare the letter with letters in secret word
                        {
                            if (secretWord[testLoop] == inpString[0])//If letter is in secretword add it to the charray
                            {

                                displayWordCharArray[testLoop] = inpString[0];
                                validGuess = false;//Don't count guesses.
                            }

                        }
                    }
                    else
                    {
                        //Already guessed message. Fix a delay.
                        MessageAndDelay("You already guessed on that letter!");
                        validGuess = false;
                    }
                }
                else
                {
                    if (!guessedWord.Contains(inpString)) //Add the guessed word to the list.
                    {
                        guessedWord.Add(inpString);
                    }

                    if (inpString != secretWord) //Bad guess for the word.
                    {
                        //Sorry failed guess
                        MessageAndDelay("Sorry wrong word but it was a nice try!");
                        validGuess = true; //count the guess
                    }
                    else
                    {
                        displayWordCharArray = inpString.ToCharArray();
                    }
                }
                if (validGuess) //increase guess
                {
                    guesses++;
                }

            }
            while ((secretWord != inpString) && (secretWord != string.Concat(displayWordCharArray)) && (guesses < 10));//End when displayWordCharArray is all letters and equal with secret word or guesses reached 10.
            DisplayHangman(guesses, displayWordCharArray);
            DisplayWinLoss(guesses); //Show the result of the game
            SaveGuessedWordsToWords(guessedWord); // Give option to add words.
        }

        private static void DisplayGuessedLetters(string v, string guessedLetters, string secretWord)
        {
            if (guessedLetters.Length > 0)
            {
                Console.Write(v);
                foreach (char letter in guessedLetters)
                {
                    Console.ForegroundColor = secretWord.Contains(letter) ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;
                    Console.Write(letter);
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write(",");
                }
            }
        }
        private static void MessageAndDelay(string v)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("\n{0}", v);
            Thread.Sleep(3000);
        }

        private static void DisplayGuessedWord(List<string> guessedWord)
        {
            if (guessedWord.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("\nYou have guessed on the following words");
                Console.ForegroundColor = ConsoleColor.DarkRed;

                foreach (string word in guessedWord)
                {
                    Console.WriteLine(word);
                }
            }
            Console.ResetColor();
        }

        private static void SaveGuessedWordsToWords(List<string> guessedWord)
        {
            HangmanEntities db = new HangmanEntities();
            if (guessedWord.Count > 0)
            {
                foreach (string word in guessedWord)
                {
                    if (!db.Words.Any(w=>w.Word==word))//If guessed word isn't in list of word ask to add them
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        if (GetValidLetters($"{word} isn't in the list of word. Do you want to add it (y/n)", "yYnN") == "Y")
                        {
                            db.Words.Add(new Words { Word = word });
                        }
                    }

                }
            }
            db.SaveChanges();
        }
        private static string GetSecretWord()
        {
            HangmanEntities db = new HangmanEntities();
            if (db.Words.Count() == 0) //If no words in list additems.
            {
                db.Words.Add(new Words { Word = "ANANAS" });
                db.Words.Add(new Words { Word = "AX" });
                db.Words.Add(new Words { Word = "SWIXWAX" });
                db.Words.Add(new Words { Word = "MISSISSIPI" });
                db.SaveChanges();
            }
            Random rand = new Random();
            string secretWord = db.Words.Find(rand.Next(1, db.Words.Count())).Word;
            return secretWord;
        }

        private static void DisplayWinLoss(int guesses)
        {

            if (guesses > 9)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Sorry, This didn't go that well.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Congratulation, You did it with {0} fail{1}", guesses, (guesses > 1) ? "s" : ". Did you cheat?");
            }
            Console.ResetColor();
        }

        private static string GetValidLetters(string message, string validLetters)
        {
            char inpKey;
            string inpString = "";
            Console.Write("\n{0}: ", message);
            do
            {
                inpKey = Console.ReadKey(true).KeyChar;
                if (validLetters.Contains(inpKey)) //Check for valid input
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(inpKey);
                    inpString = inpString + inpKey;
                }
                else
                {
                    if (inpKey != '\r') //Check for Enter in inkey otherwise wrong input
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nDon't think '{0}' is a letter. Try Again", inpKey);
                        Console.ResetColor();
                    }
                }
            } while (inpKey != '\r' || inpString == "");//End when valid code otherwise retry.

            return inpString.ToUpper(); //Return Uppercase input
        }

        private static void DisplayHangman(int guesses, char[] displayWordCharArray) //Not needed but more fun ;-)
        {

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            switch (guesses)
            {
                case 0:
                    Console.WriteLine("\n\n\n\n\n");
                    break;
                case 1:
                    Console.WriteLine("\n\n\n\n\n├──────┐");
                    break;
                case 2:
                    Console.WriteLine("\n|");
                    Console.WriteLine("|");
                    Console.WriteLine("|");
                    Console.WriteLine("|");
                    Console.WriteLine("├──────┐");
                    break;
                case 3:
                    Console.WriteLine("______");
                    Console.WriteLine("|/");
                    Console.WriteLine("|");
                    Console.WriteLine("|");
                    Console.WriteLine("|");
                    Console.WriteLine("├──────┐");
                    break;
                case 4:
                    Console.WriteLine("______");
                    Console.WriteLine("|/   |");
                    Console.WriteLine("|");
                    Console.WriteLine("|");
                    Console.WriteLine("|");
                    Console.WriteLine("├──────┐");
                    break;
                case 5:
                    Console.WriteLine("______");
                    Console.WriteLine("|/   |");
                    Console.WriteLine("|    O");
                    Console.WriteLine("|");
                    Console.WriteLine("|");
                    Console.WriteLine("├──────┐");
                    break;
                case 6:
                    Console.WriteLine("______");
                    Console.WriteLine("|/   |");
                    Console.WriteLine("|    O");
                    Console.WriteLine("|    |");
                    Console.WriteLine("|");
                    Console.WriteLine("├──────┐");
                    break;
                case 7:
                    Console.WriteLine("______");
                    Console.WriteLine("|/   |");
                    Console.WriteLine("|    O");
                    Console.WriteLine("|    |");
                    Console.WriteLine("|   /");
                    Console.WriteLine("├──────┐");
                    break;
                case 8:
                    Console.WriteLine("______");
                    Console.WriteLine("|/   |");
                    Console.WriteLine("|    O");
                    Console.WriteLine("|    |");
                    Console.WriteLine("|   / \\");
                    Console.WriteLine("├──────┐");
                    break;
                case 9:
                    Console.WriteLine("______");
                    Console.WriteLine("|/   |");
                    Console.WriteLine("|    O");
                    Console.WriteLine("|   /|");
                    Console.WriteLine("|   / \\");
                    Console.WriteLine("├──────┐");
                    break;
                case 10:
                    Console.WriteLine("______");
                    Console.WriteLine("|/   |");
                    Console.WriteLine("|    O");
                    Console.WriteLine("|   /|\\");
                    Console.WriteLine("|   / \\");
                    Console.WriteLine("├──────┐");
                    break;
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(displayWordCharArray);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
        }

    }
}