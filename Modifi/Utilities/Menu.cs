using System;
using System.Collections.Generic;
using System.Text;

namespace RobotGryphon.Modifi {

    public class MenuOption<T> {
        public T Item;
        public bool Selectable;
        public bool IsSpace;

        public Action ExecuteAction;

        public MenuOption() {
            this.Selectable = false;
            this.IsSpace = true;
        }

        public MenuOption(T item) {
            this.Item = item;
            this.Selectable = true;
            this.IsSpace = false;
        }
    }

    public class Menu<T> {

        protected LinkedList<MenuOption<T>> Options;
        protected LinkedListNode<MenuOption<T>> CurrentOption;
        public Action<T> OptionFormatter;

        public T SelectedOption;

        public Menu() {
            this.Options = new LinkedList<MenuOption<T>>();
            this.CurrentOption = null;

            this.OptionFormatter = (t) => Console.WriteLine(t);
        }

        public Menu<T> AddItem(T item) {
            MenuOption<T> newOption = new MenuOption<T>(item);
            this.Options.AddLast(newOption);

            if (this.Options.Count == 1) this.CurrentOption = Options.First;
            return this;
        }

        public Menu<T> AddSpacer() {
            MenuOption<T> newOption = new MenuOption<T>() {
                IsSpace = true,
                Selectable = false
            };

            this.Options.AddLast(newOption);
            return this;
        }

        public void Next() {
            if (this.CurrentOption.Next != null) {
                this.CurrentOption = this.CurrentOption.Next;
                if (this.CurrentOption.Value.IsSpace) Next();
            }

            RedrawCurrentOption();
        }

        public void Previous() {
            if (this.CurrentOption.Previous != null) {
                this.CurrentOption = this.CurrentOption.Previous;
                if (this.CurrentOption.Value.IsSpace) Previous();
            }

            RedrawCurrentOption();
        }

        private void RedrawCurrentOption() {
            Console.CursorTop -= 1;
            Console.CursorLeft = 0;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Current Option: ".PadRight(Console.BufferWidth - 10));
            Console.CursorLeft = "Current Option: ".Length;

            Console.ResetColor();

            this.OptionFormatter.Invoke(this.CurrentOption.Value.Item);
        }

        public void DrawMenu() {
            LinkedList<MenuOption<T>>.Enumerator e = this.Options.GetEnumerator();
            while(e.MoveNext()) {
                MenuOption<T> currentOption = e.Current;

                if (currentOption.IsSpace)
                    Console.WriteLine();
                else
                    OptionFormatter.Invoke(currentOption.Item);
            }

            Console.WriteLine();
            Console.WriteLine();

            RedrawCurrentOption();

            DoMenuLoop();
        }

        private void DoMenuLoop() {
            
            bool submitted = false;
            while(!submitted) {
                ConsoleKeyInfo key = Console.ReadKey(false);
                switch (key.Key) {
                    case ConsoleKey.UpArrow:
                        this.Previous();
                        break;

                    case ConsoleKey.DownArrow:
                        this.Next();
                        break;

                    case ConsoleKey.Enter:
                        if (!CurrentOption.Value.Selectable) break;

                        this.SelectedOption = this.CurrentOption.Value.Item;
                        return;
                }
            }
        }
    }
}
