using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi {
    public class ConsoleMenu {

        public struct Option {
            public int highlightIndex;
            public string text;
        }

        protected Dictionary<char, Option> options;
        protected ConsoleColor highlightColor;
        protected ConsoleColor keyColor;

        private ConsoleMenu() {
            this.options = new Dictionary<char, Option>();
            this.highlightColor = ConsoleColor.Yellow;
            this.keyColor = ConsoleColor.Cyan;
        }

        public static ConsoleMenu Create() {
            ConsoleMenu c = new ConsoleMenu();
            return c;
        }

        public ConsoleMenu AddOption(char key, string text, int highlightKeyIndex = -1) {
            if (this.options.ContainsKey(key)) throw new Exception("Key already exists.");
            this.options.Add(key, new Option() { text = text, highlightIndex = highlightKeyIndex });
            return this;
        }

        public ConsoleMenu AddOption(char key, Option o) {
            if (this.options.ContainsKey(key)) throw new Exception("Key already exists.");
            this.options.Add(key, o);
            return this;
        }

        public ConsoleMenu SetHighlightColor(ConsoleColor c) {
            this.highlightColor = c;
            return this;
        }

        public char Display(int indent = 1) {

            int optionAreaTop = Console.CursorTop;
            bool fo = true;

            // Write out all the options, with the first one highlighted
            foreach (Option opt in options.Values) {
                WriteOptionLine(Console.CursorTop, indent, opt, fo ? this.highlightColor : ConsoleColor.White);
                if (fo) fo = false;
            }


            int answerIndex = 0;
            int currentAnswerTop = optionAreaTop;
            int originalCursorPosition = Console.CursorTop;

            char finalAnswer = '*';
            bool answerSelected = false;

            while (!answerSelected) {

                ConsoleKeyInfo kin = Console.ReadKey(true);
                ConsoleKey ki = kin.Key;

                switch (ki) {
                    case ConsoleKey.UpArrow:
                        if (currentAnswerTop - 1 >= optionAreaTop) {
                            // Rewrite selection in white
                            WriteOptionLine(currentAnswerTop, indent, options.Values.ElementAt(answerIndex), ConsoleColor.White);
                            WriteOptionLine(currentAnswerTop - 1, indent, options.Values.ElementAt(answerIndex - 1), this.highlightColor);
                            currentAnswerTop -= 1;
                            answerIndex -= 1;
                        }
                        break;

                    case ConsoleKey.DownArrow:
                        if (answerIndex + 1 < options.Count) {
                            // Rewrite selection in white
                            WriteOptionLine(currentAnswerTop, indent, options.Values.ElementAt(answerIndex), ConsoleColor.White);
                            WriteOptionLine(currentAnswerTop + 1, indent, options.Values.ElementAt(answerIndex + 1), this.highlightColor);
                            currentAnswerTop += 1;
                            answerIndex += 1;
                        }

                        break;

                    case ConsoleKey.Enter:
                        finalAnswer = options.Keys.ElementAt(answerIndex);
                        answerSelected = true;
                        break;

                    default:
                        // Retry
                        if(this.options.ContainsKey(kin.KeyChar)) {
                            finalAnswer = kin.KeyChar;
                            answerSelected = true;
                        }
                        break;
                }

                Console.CursorTop = originalCursorPosition;
            }

            return finalAnswer;
        }

        /// <summary>
        /// Outputs an option at a specified line on the console.
        /// </summary>
        /// <param name="topPosition">Line to output on.</param>
        /// <param name="indent">How far to indent it on the left</param>
        /// <param name="option">The option to write out</param>
        /// <param name="color">The color to write it as</param>
        void WriteOptionLine(int topPosition, int indent, Option option, ConsoleColor color) {

            // Indent to left position
            Console.SetCursorPosition(0, topPosition);
            Console.Write("".PadLeft(indent, '\t'));

            for (int i = 0; i < option.text.Length; i++) {
                Console.ForegroundColor = (option.highlightIndex > -1 && i == option.highlightIndex) ? keyColor : color;
                Console.Write(option.text[i]);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(0, topPosition + 1);
        }
    }
}
