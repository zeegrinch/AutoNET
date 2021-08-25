using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
using AutoNet.Common;

namespace AutoNet.Utils
{
    /// <summary>
    /// Utility class for interacting with the user via a command line interface (CLI)
    /// Minimal functionality in this version: parsing of multiple parameters, support for quoted-parameters, prompter change
    /// We assume input syntax to follow this basic grammar:
    ///     > VERB param1 param2 "param 3" ...
    ///     > VERB var = value  
    ///     etc.
    /// </summary>
    public class CLIHelper
    {
        private readonly object _padlock = new object();
        private readonly ConsoleColor _foreColor = Console.ForegroundColor;
        private List<string> _tokens = new List<string>();
        private Command _currCommand;

        private const string NOCONTEXTPROMPT = "[No context] >";
        private const string INITPROMPT = "[Initializing...] >";

        public enum Feedback 
        {
            Echo,
            Confirmation,
            Success,
            Warning,
            Error,
            Dull
        }


        public CLIHelper() { 
            Console.Clear();
            Prompter = INITPROMPT;
        }

        public CLIHelper(string title)
        {
            Console.Clear();
            Console.Title = title;
            Prompter = INITPROMPT;
        }
        
        public string Prompter { get; set; }

        
        #region Echo/Display
        
        public void RefreshPrompt(bool withNewLine = false)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(withNewLine?"\n":"\r" + Prompter);
            Console.ForegroundColor = _foreColor;
        }
        
        
        /// <summary>
        /// Display a specific message to the user; switches color/highlighitng based on type
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="style"></param>
        public void ShowMessage(string msg, Feedback style) 
        {
            /*wish to be able to up to C#8 for a switch expression here
            ConsoleColor foreColor = style switch {
                Feedback.Confirmation => ConsoleColor.DarkYellow,
                Feedback.Error => ConsoleColor.DarkRed,
                _ => _foreColor
            };*/

            ConsoleColor foreColor = _foreColor;
            switch (style) {
                case Feedback.Echo:
                    foreColor = ConsoleColor.DarkGreen;
                    break;

                case Feedback.Confirmation:
                    foreColor = ConsoleColor.Blue;
                    break;
                
                case Feedback.Dull:
                    foreColor = ConsoleColor.Gray;
                    break;

                case Feedback.Warning:
                    foreColor = ConsoleColor.DarkYellow;
                    break;
                case Feedback.Error:
                    foreColor = ConsoleColor.DarkRed;
                    break;
                case Feedback.Success:
                    foreColor = ConsoleColor.DarkGreen;
                    break;
            }

            lock (_padlock)
            {
                Console.ForegroundColor = foreColor;
                Console.WriteLine("\t" + msg + Environment.NewLine);
                Console.ForegroundColor = _foreColor;
            }
        }

        /// <summary>
        /// displays data in a tabular format, using specified number of columns (up to 7)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="columns"></param>
        public void ShowTabularData<T>(List<T> data, int columns) where T: ITuple
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            var fields = typeof(T).GetFields();
            switch (columns)
            {
                case 2:
                    foreach (T el in data)
                    {
                        Console.WriteLine($"\t{fields[0].GetValue(el)} = {fields[1].GetValue(el)}");
                    }   
                    break;

                default:
                    ShowMessage($"Unable to display data. Unsupported number of columns {columns}.", Feedback.Warning);
                    return;
            }
            Console.Write(Environment.NewLine);
            Console.ForegroundColor = _foreColor;
        }

        #endregion

        #region Input & Parsing
        /// <summary>
        /// Awaits a command, parses and validates input
        /// </summary>
        /// <returns>a Command object</returns>
        public Command GetUserInput()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(Prompter);
            Console.ForegroundColor = _foreColor;

            string userInput = Console.ReadLine();

            try
            {
                //may need to 'sanitize' first 
                Tokenize(userInput);

                //builds a Command object (current command)
                //if(!ParseValidate()): showcase more-specialized exceptions
                ParseValidate();
            }
            catch (ValidationException vex)
            {
                //any additional tweaks here
                ShowMessage(vex.Message, Feedback.Warning);
                return null;
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, Feedback.Error);
                return null;
            }
            return _currCommand;
        }

        /// <summary>
        /// splits input string into tokens
        /// $TODO$: review RegEX - issues with space/separators
        /// </summary>
        /// <param name="input"></param>
        private void Tokenize(string input)
        {
            _tokens.Clear();
            _tokens = Regex.Matches(input, @"\""(\""\""|[^\""])+\""|[^ ]+",
                            RegexOptions.ExplicitCapture).Cast<Match>()
                           .Select(m => m.Value)
                           .ToList();
                           //.ToArray();

        }

        /// <summary>
        /// Parses the current input; if syntactically correct, builds a Command object
        /// Fairly crude logic: establish a 'verb' (command), proper arguments (right-handside param if assignment, etc.)
        /// $TODO$ candidate to extract in its own class, if 'interpreter' logic needs to became more complex (expression trees)
        /// </summary>
        /// <returns>true/false if validation succeeds/fails</returns>
        private bool ParseValidate()
        {
            //establish a VERB
            string cmd = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(_tokens.First().ToLower());
            Verb cmdVerb;

            //is it a Session command ?   $Set | $Refresh
            if(cmd.StartsWith("$"))
            {
                _currCommand = new Command()  { IsSession = true, IsValid = true , SesionCommand = cmd.Trim().Replace("$","").ToUpper() };
                foreach (var el in _tokens.Skip(1))
                {
                    _currCommand.arguments.Push(el);
                }
                return true;
            }

            if (!Enum.TryParse<Verb>( cmd,  out cmdVerb))
            {
                throw new ValidationException($"Invalid command: {cmd}");
            }

            _currCommand = new Command()
            {
                Verb = cmdVerb,
                IsSession = false,
            };

            //is it an assignment ? must include an assign token & right hand operand
            if (cmdVerb.Equals(Verb.Set))
            {
                ConstructSetCommand();
                return true;
            }

            if (cmdVerb.Equals(Verb.Get))
            {
                ConstructGetCommand();
                return true;
            }

            foreach( var el in _tokens.Skip(1) )
            {
                //any 'sanitization' can go in here - like discarding single-char, special character tokens, keywords, etc.
                _currCommand.arguments.Push(el);
            }

            _currCommand.IsValid = true;
            return true;
        }


        /// <summary>
        /// composes a GET command (in principle > Get propName | *  discard all other arguments
        /// $TODO$:perhaps we should display a warning if we discard any arguments (a GET command should have only 2 tokens)
        /// </summary>
        private void ConstructGetCommand()
        {
            var arg = _tokens.Skip(1).FirstOrDefault();
            if(arg == null)
                throw new ValidationException($"Invalid command argument: {_currCommand.Verb}. Provide property-name or '*' ?");
            _currCommand.arguments.Push(arg);
            _currCommand.IsValid = true;
        }


        /// <summary>
        /// composes a SET command (in principle > Set leftOp = rightOp; discard all other arguments
        /// $TODO$:perhaps we should display a warning if we discard any arguments (a SET command should have only 4 tokens)
        /// </summary>
        private void ConstructSetCommand()
        {
            if (!_tokens.Any(el => el.Trim().Equals("=")))
            {
                throw new ValidationException($"Invalid command: {_currCommand.Verb}. Assignment ?");
            }

            try
            {
                var rightOp = _tokens.SkipWhile(p => p != "=").ElementAt(1);
                if (rightOp == null)
                {
                    throw new ValidationException($"Invalid command: {_currCommand.Verb}. No right-hand operand.");
                }
                _currCommand.arguments.Push(rightOp);
            }
            catch (ArgumentOutOfRangeException)
            {

                throw new ValidationException($"Invalid command: {_currCommand.Verb}. No right-hand operand.");
            }

            var idx = _tokens.IndexOf("=");
            if (idx <= 1)
            {
                throw new ValidationException($"Invalid command: {_currCommand.Verb}. No left-hand operand.");
            }
            _currCommand.arguments.Push(_tokens.ElementAt(idx - 1));
            _currCommand.IsValid = true;
        }

    }
    #endregion

}

