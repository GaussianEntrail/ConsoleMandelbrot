using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDrawingThing
{
    class Program
    {
        static void Main(string[] args)
        {
            // Mandelbrot set drawer
            Mandelbrot m = new Mandelbrot();

            Console.Clear();
            Console.OutputEncoding = Encoding.UTF8;
            Console.SetWindowSize(80, 39);
            Console.CursorVisible = false;

            //foreach (Mandelbrot.ShadedColor c in Mandelbrot.ListColors) { c.draw(); }
            //Console.ReadKey();
            
            m.ConsoleDraw(0, 0, 80, 39);

            Console.ReadKey();
        }
    }
}
