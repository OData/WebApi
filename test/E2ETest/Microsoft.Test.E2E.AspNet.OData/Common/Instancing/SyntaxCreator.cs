//-----------------------------------------------------------------------------
// <copyright file="SyntaxCreator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Text;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Instancing
{
    public interface ISyntax
    {
        double Weight { get; }
        string Generate(Random random, CreatorSettings settings);
    }

    public abstract class Syntax : ISyntax
    {
        private bool recursive = false;
        private int depth = 0;
        private double? weight = null;

        public Syntax()
        {
            this.Random = new Random();
            this.Settings = new CreatorSettings();
        }

        public Random Random { get; set; }
        public CreatorSettings Settings { get; set; }

        public double Weight
        {
            get
            {
                if (recursive)
                {
                    return 0;
                }

                this.recursive = true;
                if (!weight.HasValue)
                {
                    weight = CalcWeight();
                }
                this.recursive = false;

                return weight.Value * ((double)this.Settings.MaxGraphDepth - depth) / (double)this.Settings.MaxGraphDepth;
            }
        }

        protected virtual double CalcWeight()
        {
            return 1;
        }

        public static Syntax operator +(Syntax left, ISyntax right)
        {
            return new AppendSyntax(left, right);
        }

        public static Syntax operator |(Syntax left, ISyntax right)
        {
            return new OrSyntax(left, right);
        }

        public static Syntax operator +(Syntax left, string right)
        {
            return new AppendSyntax(left, new Literal(right));
        }

        public static Syntax operator |(Syntax left, string right)
        {
            return new OrSyntax(left, new Literal(right));
        }

        public Syntax OrEmpty()
        {
            return new OrSyntax(this, new Literal(string.Empty));
        }

        public Syntax Occurs(int min, int max)
        {
            return new OccursSyntax(this, min, max);
        }

        public NonTerminalSyntax AsNonTerminal()
        {
            return new NonTerminalSyntax(this);
        }

        public virtual string Generate(Random random, CreatorSettings settings)
        {
            this.Random = random;
            this.Settings = settings;

            depth++;
            var result = GenerateInternal();
            depth--;
            return result;
        }

        protected abstract string GenerateInternal();
    }

    public class NonTerminalSyntax : Syntax
    {
        public NonTerminalSyntax()
        {
        }

        public NonTerminalSyntax(Syntax syntax)
        {
            this.Syntax = syntax;
        }

        public Syntax Syntax
        {
            get;
            set;
        }

        protected override double CalcWeight()
        {
            return this.Syntax.Weight;
        }

        protected override string GenerateInternal()
        {
            return this.Syntax.Generate(this.Random, this.Settings);
        }
    }

    public class Token : Syntax
    {
        /// <summary>
        /// Create a Token which generates string randomly
        /// </summary>
        /// <param name="charToUse">a string contains all the charactors can be used in this token.</param>
        /// <param name="minSize">minimal length of the string to be generated</param>
        /// <param name="maxSize">maximal length of the string to be generated</param>
        public Token(string charToUse, int? minSize = null, int? maxSize = null)
        {
            this.CharToUse = charToUse;
            this.MinSize = minSize ?? this.Settings.MinStringLength;
            this.MaxSize = maxSize ?? this.Settings.MaxStringLength;
        }

        public string CharToUse
        {
            get;
            set;
        }

        public int MinSize
        {
            get;
            set;
        }

        public int MaxSize
        {
            get;
            set;
        }

        protected override string GenerateInternal()
        {
            return PrimitiveCreator.CreateRandomString(
                this.Random,
                -1,
                this.CharToUse,
                new CreatorSettings(this.Settings)
                {
                    MinStringLength = this.MinSize,
                    MaxStringLength = this.MaxSize
                });
        }
    }

    public class Literal : Syntax
    {
        public static readonly Literal Empty = new Literal(string.Empty);

        public Literal(string literal)
        {
            this.Text = literal;
        }

        public string Text
        {
            get;
            set;
        }

        protected override string GenerateInternal()
        {
            return this.Text;
        }
    }

    public abstract class BinaryOperandsSyntax : Syntax
    {
        protected BinaryOperandsSyntax(ISyntax left, ISyntax right)
        {
            this.Left = left;
            this.Right = right;
        }

        public ISyntax Left
        {
            get;
            set;
        }

        public ISyntax Right
        {
            get;
            set;
        }

        protected override double CalcWeight()
        {
            return Left.Weight + Right.Weight;
        }
    }

    public class AppendSyntax : BinaryOperandsSyntax
    {
        public AppendSyntax(ISyntax left, ISyntax right)
            : base(left, right)
        {
        }

        protected override string GenerateInternal()
        {
            return Left.Generate(this.Random, this.Settings)
                + Right.Generate(this.Random, this.Settings);
        }
    }

    public class OrSyntax : BinaryOperandsSyntax
    {
        public OrSyntax(ISyntax left, ISyntax right)
            : base(left, right)
        {
        }

        protected override string GenerateInternal()
        {
            if (this.Random.NextDouble() < ((double)Left.Weight / this.Weight))
            {
                return Left.Generate(this.Random, this.Settings);
            }
            else
            {
                return Right.Generate(this.Random, this.Settings);
            }
        }
    }

    public class OccursSyntax : NonTerminalSyntax
    {
        public OccursSyntax(Syntax syntax, int? min = null, int? max = null)
            : base(syntax)
        {
            this.Min = min ?? 0;
            this.Max = max ?? this.Settings.MaxListLength;
        }

        public int Min
        {
            get;
            set;
        }

        public int Max
        {
            get;
            set;
        }

        protected override string GenerateInternal()
        {
            StringBuilder sb = new StringBuilder();
            double randomValue = this.Random.NextDouble();
            int size;
            if (Max > Min)
            {
                size = (int)Math.Pow(this.Max - this.Min, randomValue) - 1;
                size += this.Min;
            }
            else
            {
                size = Max;
            }

            for (int i = 0; i < size; i++)
            {
                sb.Append(this.Syntax.Generate(this.Random, this.Settings));
            }

            return sb.ToString();
        }
    }
}
