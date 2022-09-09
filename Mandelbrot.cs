using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDrawingThing
{
    public class Mandelbrot
    {
        // COMPLEX NUMBER STRUCT
        public struct Complex
        {
            public double re { get; set; }
            public double im { get; set; }
            public double arg() => Math.Atan2(im, re); // ARGUMENT
            public double mod() => Math.Sqrt( (im * im) + (re * re) ); // MODULUS
            public Complex(double real, double imaginary) { this.re = real; this.im = imaginary; }

            public static Complex polar(double mod, double arg) => new Complex(mod * Math.Cos(arg), mod * Math.Sin(arg) );
            
            public override string ToString() => $"({re}, {im})";
        }

        static Complex ZERO = new Complex(0, 0);
        static Complex ONE = new Complex(1, 0);
        static Complex I = new Complex(0, 1);

        static double REAL_START = -2.0;
        static double REAL_END = 1.0;

        static double IMAGINARY_START = -2.0;
        static double IMAGINARY_END = 2.0;
        const int MAX_ITERATIONS = 80;

        private struct thingy { public int steps; public bool in_set; public thingy(int steps, bool does_diverge) { this.steps = steps;  this.in_set = does_diverge; } }

        public struct ShadedColor
        {
            public ConsoleColor fg;
            public ConsoleColor bg;
            public int shadingAmount;

            public ShadedColor(ConsoleColor fg, ConsoleColor bg, int shadingAmount) { this.fg = fg;this.bg = bg;this.shadingAmount = shadingAmount; }
            public void draw()
            {
                ConsoleColor prev_bg = Console.BackgroundColor;
                ConsoleColor prev_fg = Console.ForegroundColor;

                SetConsoleColor(fg, bg);
                char c;
                switch (shadingAmount) {
                    default:
                    case 0:
                        c = '█'; // NO SHADING
                        break;
                    case 1:
                        c = '▓';
                        break;
                    case 2:
                        c = '▒';
                        break;
                    case 3:
                        c = '░'; // LIGHT SHADING
                        break;
                }
                Console.Write(c);
                SetConsoleColor(prev_fg, prev_bg);
            }
        }

        #region Colors
        public static ShadedColor[] ListColors = {
            new ShadedColor(ConsoleColor.Red, ConsoleColor.Yellow, 0),
            new ShadedColor(ConsoleColor.Red, ConsoleColor.Yellow, 1),
            new ShadedColor(ConsoleColor.Red, ConsoleColor.Yellow, 2),
            new ShadedColor(ConsoleColor.Red, ConsoleColor.Yellow, 3),

            new ShadedColor(ConsoleColor.Yellow, ConsoleColor.Green, 0),
            new ShadedColor(ConsoleColor.Yellow, ConsoleColor.Green, 1),
            new ShadedColor(ConsoleColor.Yellow, ConsoleColor.Green, 2),
            new ShadedColor(ConsoleColor.Yellow, ConsoleColor.Green, 3),

            new ShadedColor(ConsoleColor.Green, ConsoleColor.Cyan, 0),
            new ShadedColor(ConsoleColor.Green, ConsoleColor.Cyan, 1),
            new ShadedColor(ConsoleColor.Green, ConsoleColor.Cyan, 2),
            new ShadedColor(ConsoleColor.Green, ConsoleColor.Cyan, 3),

            new ShadedColor(ConsoleColor.Cyan, ConsoleColor.Blue, 0),
            new ShadedColor(ConsoleColor.Cyan, ConsoleColor.Blue, 1),
            new ShadedColor(ConsoleColor.Cyan, ConsoleColor.Blue, 2),
            new ShadedColor(ConsoleColor.Cyan, ConsoleColor.Blue, 3),

            new ShadedColor(ConsoleColor.Blue, ConsoleColor.Magenta, 0),
            new ShadedColor(ConsoleColor.Blue, ConsoleColor.Magenta, 1),
            new ShadedColor(ConsoleColor.Blue, ConsoleColor.Magenta, 2),
            new ShadedColor(ConsoleColor.Blue, ConsoleColor.Magenta, 3),

            new ShadedColor(ConsoleColor.Magenta, ConsoleColor.Red, 0),
            new ShadedColor(ConsoleColor.Magenta, ConsoleColor.Red, 1),
            new ShadedColor(ConsoleColor.Magenta, ConsoleColor.Red, 2),
            new ShadedColor(ConsoleColor.Magenta, ConsoleColor.Red, 3),

        };

        static ShadedColor BLACK = new ShadedColor(ConsoleColor.Black, ConsoleColor.Black, 0);
        #endregion

        #region Complex Mathematics
        bool ceq(Complex z1, Complex z2) => (z1.re == z2.re && z1.im == z2.im);
        // ADDITION
        Complex cadd(Complex z1, Complex z2) => ceq(z1,z2) ? new Complex(z1.re * 2, z1.im * 2) : new Complex(z1.re + z2.re, z1.im + z2.im);
        // SUBTRACTION
        Complex csub(Complex z1, Complex z2) => ceq(z1, z2) ? ZERO : new Complex(z1.re - z2.re, z1.im - z2.im);
        // SQUARE
        Complex csqr(Complex z) => new Complex( (z.re * z.re) - (z.im * z.im), 2 * z.im * z.re );
        // MULTIPLICATION
        Complex cmul(Complex z1, Complex z2) => ceq(z1, z2) ? csqr(z1) : new Complex( ((z1.re * z2.re) - (z1.im * z2.im)), ((z1.re * z2.im) + (z2.re * z1.im)) ); 
        // DIVISION
        Complex cdiv(Complex z1, Complex z2) => ceq(z1, z2) ? ONE : new Complex( ((z1.re * z2.re) + (z1.im * z2.im)) / ((z2.re * z2.re) + (z2.im * z2.im)), ((z1.im * z2.re) - (z1.re * z2.im)) / ((z2.re * z2.re) + (z2.im * z2.im)) );
        // REAL INTEGER EXPONENT
        Complex cpow_real(Complex z, double exp)
        {
            // exponentiation of a complex number with a real, integer exponent
            if (exp == 2) { return csqr(z); }
            if (exp == 1) { return z; }
            if (exp == 0) { return ONE; }
            Complex result = ONE;
            Complex cexp = new Complex(exp, 0);
            for (int j = 0; j < exp; j++)
            {
                result = cmul( result, cexp );
            }
            return result;
        }
        // COMPLEX EXPONENT
        Complex cpow_complex(Complex z, Complex exp)
        {
            // exponentiation of a complex number with a complex exponent
            double mod = exp.mod();
            double arg = exp.arg();

            double new_mod = (exp.re * Math.Log(mod)) - (arg * exp.im);
            double new_arg = (exp.im * Math.Log(mod)) + (arg * exp.re);

            return Complex.polar(new_mod, new_arg);
        }
        // POLYNOMIAL
        Complex cpoly( Complex z, Complex[] coeffs)
        {
            var terms = coeffs.Select((_, n) => cmul(coeffs[n], cpow_real(z, n)));
            return terms.Aggregate( (a,b) => cadd(a, b) );
        }
        #endregion

        private Complex convertCursorXYtoComplex(int x, int y, int w, int h)
        {
            double _h = (double)h;
            double _w = (double)w;
            double _x = (double)x;
            double _y = (double)y;
            double re = REAL_START + ( (_x / _w) * (REAL_END - REAL_START) );
            double im = IMAGINARY_START + ( (_y / _h) * (IMAGINARY_END - IMAGINARY_START) );
            return new Complex(re, im);
        }

        private thingy doMandelbrotIterations(Complex c)
        {
            Complex[] coeffs = {c, ZERO, I};
            Complex z = ZERO;
            int n = 0;
            double d = 0;

            do {
                z = cpoly(z, coeffs);
                d = z.mod();
                n+=1;
            } while (n < MAX_ITERATIONS && d <= 2);

            return new thingy(n, d <= 2);
        }

        static void SetConsoleColor(ConsoleColor fg, ConsoleColor bg) { Console.BackgroundColor = bg; Console.ForegroundColor = fg; }

        static void SetCursorPosition(int x, int y) { Console.CursorLeft = x; Console.CursorTop = y; }

        public void ConsoleDraw(int x, int y, int w, int h) {
            int prev_x = Console.CursorLeft;
            int prev_y = Console.CursorTop;

            Complex p;
            thingy t;
            ShadedColor c;

            // Set console to start of drawing
            SetCursorPosition(x, y);

            int j, k;
            for (k = 0; k < h; k++)
            {
                for (j = 0; j < w; j++)
                {
                    SetCursorPosition(x + j, y + k);
                    p = convertCursorXYtoComplex(j, k, w, h);
                    t = doMandelbrotIterations(p);

                    if (t.in_set) { c = BLACK; } else { c = ListColors[t.steps % ListColors.Length]; }
                    c.draw();
                }
            }

            // Set console to previous
            SetCursorPosition(prev_x, prev_y);
            return;
        }
    }
}
