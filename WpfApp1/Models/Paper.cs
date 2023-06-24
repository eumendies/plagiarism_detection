using System;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class Paper : BindableBase
    {
        private string id;

        public string ID
        {
            get { return id; }
            set { id = value; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string content;

        public string Content
        {
            get { return content; }
            set { content = value; }
        }

        private string director;

        public string Director 
        {
            get { return director; }
            set { director = value; }
        }

    }
}
