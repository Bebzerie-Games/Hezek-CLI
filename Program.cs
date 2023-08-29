using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;

namespace HezekREPL {
    public class Program {

        public static string InputString { get; set; }
        public static Input Input => new(InputString.Split(' ')[0], InputString.Split(' '));
        public static ProgramEnvironment ProgramEnvironment => new ProgramEnvironment();
        public static IOEnvironment IOEnvironment => new IOEnvironment();

        public static Command CommandLastRun = null;

        public static Command[] GenericCommands => new Command[4] {
            new Command(Program.Input, "env", "output the current used environment or specific other environment", () => {

                try {
                    if (Input.Arguments[1] != "") {
                        for (int i = 0; i < Environments.Length; i++)
                        {
                            if (Environments[i].EnvironmentName == Input.Arguments[1]) {
                                foreach (var cmd in Environments[i].Commands) {
                                    Console.WriteLine($"{Environments[i].EnvironmentName}'s CommandsName: " + cmd.Name);
                                }

                                return;
                            }
                            else if (Input.Arguments[1] == Environments[int.Parse(Input.Arguments[1])].EnvironmentName) {
                                foreach (var cmd in Environments[i].Commands) {
                                    Console.WriteLine($"{Environments[i].EnvironmentName}'s CommandsName: " + cmd.Name);
                                }

                                return;
                            }
                        }
                    }

                    foreach (var cmd in CurrentCommandsEnvironment.Commands) {
                        Console.WriteLine($"{CurrentCommandsEnvironment}'s CommandsName: " + cmd.Name);
                    }
                }
                catch(Exception ex) {
                    Output.PrintError($"unkown error: {Input.Primary}' {ex.InnerException}");
                }
            }),
            new Command(Program.Input, "envs", "display all environment.", () => {
                try {
                    foreach (var env in Environments) {
                        Console.WriteLine("Environments: " + env.EnvironmentName);
                    }
                }
                catch (Exception ex) {
                Output.PrintError($"unkown error: {Input.Primary}' {ex.InnerException}");
                }
            }),
            new Command(Program.Input, "switchenv", "switch to an other environment.", () => {
                try {
                    for (int i = 0; i < Environments.Length; i++)
                    {   
                        CurrentCommandsEnvironment = Environments[int.Parse(Input.Arguments[1])];
                        Console.WriteLine("CurrentCommandsEnvironment is now " + CurrentCommandsEnvironment.EnvironmentName);

                        if (Input.Arguments[1] == Environments.ElementAt(i).EnvironmentName) {
                            CurrentCommandsEnvironment = Environments[i];
                            Console.WriteLine("CurrentCommandsEnvironment is now " + CurrentCommandsEnvironment.EnvironmentName);

                            return;
                        }

                        return;
                    }
                }
                catch (Exception ex) {
                    Output.PrintError($"error: {Input.Primary}' (from {CurrentCommandsEnvironment.EnvironmentName} : {CommandLastRun.Name})\n {ex.Source} \n cannot find any environnements associated with " + Input.Arguments[1]);
                }
            }, "envname"),
            new Command(Program.Input, "helpenv", "display commands of specific environment.", () => {
                try {
                    for (int i = 0; i < Environments.Length; i++)
                    {
                        if (Input.Arguments[1] == Environments[int.Parse(Input.Arguments[1])].EnvironmentName) {
                            foreach (var env in Environments[i].Commands) {
                                Console.WriteLine($"{Environments[i].EnvironmentName}'s Commands : \n" +
                                    $"{env}");
                            }
                        }
                        else if (Input.Arguments[1] == Environments.ElementAt(i).EnvironmentName) {
                            foreach (var env in Environments[i].Commands) {
                                Console.WriteLine($"{Environments[i].EnvironmentName}'s Commands : \n" +
                                    $"{env}");
                            }
                        }
                        else
                            throw new Exception();
                    }
                }
                catch (Exception ex) {
                    Output.PrintError($"error: {Input.Primary}' (from {CurrentCommandsEnvironment.EnvironmentName} : {CommandLastRun.Name})\n {ex.Source} \n cannot find any environnements associated with " + Input.Arguments[1]);
                }
            }, "envname"),
        };

        public static IEnvironment CurrentCommandsEnvironment { get; set; }
        public static IEnvironment[] Environments = {
            ProgramEnvironment,
            IOEnvironment
        };

        static void Main(string[] args) {
            Console.WriteLine("Hezek REPL\n\t1.0");
            Console.WriteLine("-----------------");
            Console.WriteLine("Color code :");

            CurrentCommandsEnvironment = Environments[0];

            Console.WriteLine("\t [");
            while (true) {
                SwitchEnv(CurrentCommandsEnvironment);

                InputString = Console.ReadLine().Trim();
                
                if (!string.IsNullOrEmpty(InputString) || !string.IsNullOrWhiteSpace(InputString))
                    Evaluate(Input);

                if (CommandLastRun != null)
                    AnsiConsole.Markup($"${CommandLastRun.Name}({CurrentCommandsEnvironment.EnvironmentName})* {PrefixHandler(Input)} >> ");
                else {
                    AnsiConsole.Markup($"({CurrentCommandsEnvironment.EnvironmentName})* {PrefixHandler(Input)} >> ");
                }
            }
        }

        static string PrefixHandler(Input input) {
            if (input.Arguments.Length > 1) {
                foreach (var args in input.Arguments) {

                    string arguments = string.Empty;

                    for (int i = 1; i < input.Arguments.Length; i++) {
                        arguments += $" {input.Arguments[i]}";
                    }

                    return $"[yellow]{arguments}[/]" + $"[purple]({args})[/]";
                }
            }

            else {
                return $"[green]{input.Primary}[/]";
            }

            return "null";
        }

        static void SwitchEnv(IEnvironment env) {
            CurrentCommandsEnvironment = env;
        }

        static void Evaluate(Input input) {
            try {
                for (int j = 0; j < GenericCommands.Length; j++) {
                    if (Input.Primary == GenericCommands[j].Name) {

                        CommandLastRun = GenericCommands[j];

                        CommandLastRun.Action.Invoke();

                        return;
                    }
                }

                for (int i = 0; i < CurrentCommandsEnvironment.Commands.Length; i++) {
                    if (Input.Primary == GenericCommands[i].Name) {
                        if (Input.Arguments.Length > 1 && Input.Arguments[1] == "?") {
                            Console.WriteLine($"{CurrentCommandsEnvironment.Commands[i].Name}'s {CurrentCommandsEnvironment.Commands[i].Description}");

                            return;
                        }
                        else if (Input.Arguments.Length <= 0) {
                            CommandLastRun = CurrentCommandsEnvironment.Commands[i];

                            CommandLastRun.Action.Invoke();

                            return;
                        }
                    }
                    else throw new Exception();
                }
            }
            catch(Exception ex) {
                Interop(Input, ex);
            }
        }

        static void Interop(Input input, Exception ex) {
            if (CommandLastRun == null) {
                Output.PrintError("unkown command !");
                return;
            }
            else {
                if (Input.Arguments[1] != "") {
                    CommandLastRun.Action.Invoke();
                    return;
                }
            }
            Output.PrintError($"error: {input.Primary}' (from {CurrentCommandsEnvironment.EnvironmentName} : {CommandLastRun.Name})\n {ex.Source} \n command maybe missing argument ?");

            foreach (var cmd in CommandLastRun.Arguments) {
                if (CommandLastRun.Arguments.Length > 0) {

                    Output.PrintError("--> command require argument(s). : " + cmd);

                    return;
                }
            }

        }
    }

    public sealed class ProgramEnvironment : IEnvironment {
        public Command[] Commands => new Command[1] {
            new Command(Program.Input, "print", "output the argument", () => {
                Console.WriteLine($"{Program.Input.Arguments[1]}");
            }, "arg"),
        };
        public string EnvironmentName => nameof(ProgramEnvironment);
    }

    public sealed class IOEnvironment : IEnvironment {
        public Command[] Commands => new Command[2] {
            new Command(Program.Input, "template3", "template for testing purpose", () => {
                Console.WriteLine("working");
            }),
            new Command(Program.Input, "template4", "template for testing purpose", () => {
                Console.WriteLine("working");
            }),
        };
        public string EnvironmentName => nameof(IOEnvironment);
    }

    public interface IEnvironment {
        public Command[] Commands { get; }
        public string EnvironmentName { get; }
    }

    public sealed class Command {

        public Input Input { get; }
        public Action Action { get; }

        public string Name { get; }
        public string[] Arguments { get; }

        public string Description { get; }

        public Command(Input input, string name, string desc, Action output) {
            Input = input;
            Name = name;
            Action = output;
            Description = desc;
        }

        public Command(Input input, string name, string desc, Action output, params string[] args) {
            Input = input;
            Name = name;
            Action = output;
            Arguments = args;
            Description = desc;
        }
    }

    public struct Input
    {
        public string Primary;
        public string[] Arguments;

        public Input(string primary, string[] arguments) {
            Primary = primary;
            Arguments = arguments;
        }
    }

    public sealed class Output {
        public static void Print(string arg) {
            AnsiConsole.MarkupLine(arg);
        }

        public static void PrintError(string arg) {
            AnsiConsole.MarkupLine($"[red]{arg}[/]");
        }

        public static void PrintWarning(string arg) {
            AnsiConsole.MarkupLine($"[yellow]{arg}[/]");
        }
    }
}