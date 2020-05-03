using System;
using System.Collections.Generic;
using System.Linq;

/* Grammar:
 * E -> T E'
 * E' -> + T E' | - T E' | ε
 * T -> F T'
 * T' -> * F T' | / F T' | ε
 * F -> V | - V
 * V -> num | var
 */

namespace синтаксический_анализатор
{
    class Program
    {
        class syntax
        {
            public List<Lexem> Lexems; //Входная строка
            Stack<string> results;
            public List<string> Results; 

            public syntax(List<Lexem> lexems)
            {
                Lexems = new List<Lexem>();
                Lexems = lexems;

                results = new Stack<string>();
                Results = new List<string>();
            }

            public void tree(string type, string str)
            {
                if (type == "Operator")
                {
                    if (results.FirstOrDefault() == null || prior(results.Peek()) < prior(str))
                        results.Push(str);
                    else
                    {
                        while (results.FirstOrDefault() != null && prior(results.Peek()) >= prior(str))
                        {
                            Results.Add(results.Peek());
                            results.Pop();
                        }
                        results.Push(str);
                    }
                }
                else
                {
                    if (str == "(") 
                        results.Push(str);
                    else
                    {
                        while (results.Peek() != "(") 
                        {
                            Results.Add(results.Peek());
                            results.Pop();
                        }
                        results.Pop();
                    }
                }
            }

            public void output()
            {
                while (results.FirstOrDefault() != null)
                {
                    Results.Add(results.Peek());
                    results.Pop();
                }
                string result = "";

                foreach (string k in Results)
                {
                   result += "  " + k + "  ";
                }

                Console.WriteLine("Вывод: ");
                Console.WriteLine(result);
            }
        }

        public static int prior(string v)//приоритет различных знаков выражения
        {
            switch (v)
            {
                case "(": return 1;
                case "+": return 2;
                case "-": return 2;
                case "*": return 3;
                case "/": return 3;
                case "^": return 4;
            }
            return -1;
        }

        public static bool is_operator(string op) //Проверка символа, оператор ли это;
        {
            return op == "*" || op == "+" || op == "-" || op == "/" || op == "^";
        }

        class SyntaxAnalyzer
        {
            public static string getParse(List<Lexem> lexems) 
            {
                syntax parsStr = new syntax(lexems);
                parseE(parsStr).output();
                return null;
            }

            //Правила
            public static syntax parseE(syntax str) //E -> TE'
            {
                return parseE1(parseT(str));
            }

            public static syntax parseT(syntax str) // T -> FT'
            {
                return parseT1(parseF(str));
            }

            public static syntax parseE1(syntax str) //E' -> +TE' | -TE' | пусто
            {
                switch (str.Lexems.FirstOrDefault().Value)
                {
                    case "+":
                        str.tree(str.Lexems.FirstOrDefault().TokType, str.Lexems.FirstOrDefault().Value);
                        str.Lexems.RemoveAt(0);
                        return parseE1(parseT(str));
                    case "-":
                        str.tree(str.Lexems.FirstOrDefault().TokType, str.Lexems.FirstOrDefault().Value);
                        str.Lexems.RemoveAt(0);
                        return parseE1(parseT(str));
                    default:
                        return str; //Возвращаем то же самое выражение
                }

            }

            public static syntax parseT1(syntax str) //T' -> *FT' | /FT' | пусто
            {
                switch (str.Lexems.FirstOrDefault().Value)
                {
                    case "*":
                        str.tree(str.Lexems.FirstOrDefault().TokType, str.Lexems.FirstOrDefault().Value);
                        str.Lexems.RemoveAt(0);
                        return parseT1(parseF(str));
                    case "/":
                        str.tree(str.Lexems.FirstOrDefault().TokType, str.Lexems.FirstOrDefault().Value);
                        str.Lexems.RemoveAt(0); 
                        return parseT1(parseF(str));
                    default:
                        return str; //Возвращаем то же самое выражение
                }
            }

            public static syntax parseF(syntax str) //F -> VF'
            {
                return parseF1(parseV(str));
            }

            public static syntax parseF1(syntax str) //F' -> ^F | пусто
            {
                switch (str.Lexems.FirstOrDefault().Value)
                {
                    case "^":
                        str.tree(str.Lexems.FirstOrDefault().TokType, str.Lexems.FirstOrDefault().Value);
                        str.Lexems.RemoveAt(0);
                        return parseF(str);
                    default:
                        return str;
                }
            }

            public static syntax parseV(syntax str) //V -> (E) | id | numb
            {
                switch (str.Lexems.FirstOrDefault().TokType)
                {
                    case "Lparen":
                        str.tree(str.Lexems.FirstOrDefault().TokType, str.Lexems.FirstOrDefault().Value);
                        str.Lexems.RemoveAt(0);
                        syntax resLp = parseE(str); 

                        switch (resLp.Lexems.FirstOrDefault().TokType)
                        {
                            case "Rparen":
                                str.tree(str.Lexems.FirstOrDefault().TokType, str.Lexems.FirstOrDefault().Value);
                                resLp.Lexems.RemoveAt(0);
                                return resLp;
                            default:
                                Console.WriteLine(" Error - nonexistent )");
                                break;
                        }
                        return resLp;
                    case "Identifier":
                        str.Results.Add(str.Lexems.FirstOrDefault().Value);
                        str.Lexems.RemoveAt(0);
                        return str;
                    case "Number":
                        str.Results.Add(str.Lexems.FirstOrDefault().Value);
                        str.Lexems.RemoveAt(0);
                        return str;
                    case "Operator":
                        if (str.Lexems.First().Value == "-")
                        {
                            str.Results.Add("-" + str.Lexems.FirstOrDefault().Value);
                            str.Lexems.RemoveAt(0);
                            return str;
                        }
                        return str;
                    default:
                        break;
                }
                return str;
            }
        }


        static void Main(string[] args)
        {

            do
            {
                Console.Write("Введите выражение: ");
                string str = Console.ReadLine();
               
                bool flag = false;
                string buf1 = str;
                int k = 0;
                for (int i = 0; i < buf1.Length; i++)
                {
                    if (buf1[i] == ',' || buf1[i] == ' ')
                    {
                        string buf;
                        buf = buf1.Substring(0, i);
                        if (buf.EndsWith("."))
                        {
                            Console.WriteLine("Лексема не должна заканчиваться точкой, исправьте");
                            buf1 = "";
                            flag = true;
                            break;
                        }
                        buf = buf + str[i];
                        buf1 = buf1.Replace(buf, "");
                        i = -1;
                    }

                    k++;
                    if (k == str.Length)
                        break;
                }


                if (flag == false)
                {
                    //Console.WriteLine("Lexer: ");

                    //foreach (var lexem in lexems) //Цикл по полученному списку
                    //{
                    //    Console.WriteLine(lexem.TokType + " " + lexem.Value);
                    //}
                    var lexems = LexicalAnalyzer.getLex(str); //Запуск лексического анализатора
                    var parseStr = SyntaxAnalyzer.getParse(lexems);

                }

            } while (end_enter() != "Да");
        }

        static string end_enter() //Параметр для завершения работы
        {
            Console.WriteLine("Закончить? Да/Нет");
            string str = Console.ReadLine();
            return str;
        }


        struct Lexem
        {
            public string TokType;
            public string Value;

            public Lexem(string type, string value)
            {
                TokType = type;
                Value = value;
            }
        }

        static class LexicalAnalyzer
        {
            public static int transTable(char sym, int state) //Шаблоны
            {
                switch (state)
                {
                    case 0:
                        if (char.IsDigit(sym))
                            return 1;
                        else if (sym == '+' || sym == '*' || sym == '/' || sym == '-' || sym == '^')
                            return 6;
                        else if (char.IsLetter(sym) || sym == '_')
                            return 7;
                        else if (sym == '(')
                            return 8;
                        else if (sym == ')')
                            return 9;
                        else if (sym == ',')
                            return 10;
                        else if (sym == ' ' || sym == '\t' || sym == '\n' || sym == '\r')
                            return 0;
                        break;
                    case 1:
                        if (char.IsDigit(sym))
                            return 1;
                        else if (sym == '.')
                            return 2;
                        else if (sym == 'e' || sym == 'E')
                            return 3;
                        break;
                    case 2:
                        if (char.IsDigit(sym))
                            return 2;
                        else if (sym == 'e' || sym == 'E')
                            return 3;
                        break;
                    case 3:
                        if (sym >= '0' && sym <= '9')
                            return 5;
                        else if (sym == '+' || sym == '-')
                            return 4;
                        break;
                    case 4:
                        if (sym >= '0' && sym <= '9')
                            return 5;
                        break;
                    case 5:
                        if (sym >= '0' && sym <= '9')
                            return 5;
                        break;
                    case 6:
                        break;
                    case 7:
                        if (char.IsLetter(sym) || char.IsDigit(sym) || sym == '_')
                            return 7;
                        break;
                }
                return -1;
            }

            public static string getTokenType(int stId) //Распознавание типа токена
            {
                string tokenType = "Unknown";

                if (stId == 0)
                {
                    tokenType = "";
                }
                else if (stId == 1 || stId == 2 || stId == 5)
                {
                    tokenType = "Number";
                }
                else if (stId == 6)
                {
                    tokenType = "Operator";
                }
                else if (stId == 7)
                {
                    tokenType = "Identifier";
                }
                else if (stId == 8)
                {
                    tokenType = "Lparen";
                }
                else if (stId == 9)
                {
                    tokenType = "Rparen";
                }
                else if (stId == 10)
                {
                    tokenType = "Comma";
                }
                return tokenType;
            }

            public static List<Lexem> getLex(string text)
            {
                List<Lexem> lexems = new List<Lexem>(); //Список распознанных лексем

                int st_id = 0;
                string lex1 = ""; //Конкретная лексема в выражении
                int buff = 0;

                for (int i = 0; i < text.Length; i++) //Цикл по символам строки
                {
                    Lexem lex = new Lexem("Unknown", ""); //Пока не начали распознавание очередной лексемы

                    while (i < text.Length && transTable(text[i], st_id) != -1) //Начинаем распознавание очередной лексемы
                    {
                        lex1 += Convert.ToString(text[i]);
                        buff = st_id;
                        st_id = transTable(text[i], buff);
                        i++;
                    }


                    if (st_id == 4)
                    {
                        i--;
                        string numb_buf = "";
                        string let_buf = "";
                        string oper_buf = "";
                        int j;

                        for (j = 0; j < lex1.Length; j++)
                        {
                            if (char.IsDigit(lex1[j]))
                                numb_buf += Convert.ToString(lex1[j]);
                            else if (char.IsLetter(lex1[j]))
                                let_buf += Convert.ToString(lex1[j]);
                            else if (lex1[j] == '+' || lex1[j] == '-')
                                oper_buf = Convert.ToString(lex1[j]);

                        }
                        lex.Value = numb_buf; lex.TokType = "Number"; lexems.Add(lex);
                        lex.Value = let_buf; lex.TokType = "Identifier"; lexems.Add(lex);
                        lex.Value = oper_buf; lex.TokType = "Operator"; lexems.Add(lex);
                        st_id = 0;
                        lex1 = "";

                    }
                    else
                    {
                        i--;
                        lex.Value = lex1;
                        lex.TokType = getTokenType(transTable(text[i], buff));
                        st_id = 0;
                        lex1 = "";
                        lexems.Add(lex);
                    }


                }


                return lexems;
            }
        }


    }
}
