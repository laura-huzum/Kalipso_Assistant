using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kalipso
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
			try
			{
				Translator trans = new Translator();
				Form1 form = new Form1();
				form.translator = trans;
				Application.EnableVisualStyles();
				//Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(form);
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
			}
        }
    }
}
