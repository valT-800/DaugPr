using ArffTools;
using java.io;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Uzd_N3
{
    public partial class Form1 : Form
    {

        private int progress = 0;

        private ManualResetEvent resetEvent = new ManualResetEvent(true);
        public Form1()
        {
            InitializeComponent();
        }
        
        private void attributesButton_Click(object sender, EventArgs e)
        {
            StreamWriter sw = new StreamWriter("attributes.txt", true);
            sw.WriteLine(textBox2.Text);
            sw.Close();
            MessageBox.Show("Attribute is added");
        }
        private void fillButton_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(Method)); //sukuriame parametrizuotą giją
            thread.IsBackground = true;
            thread.Start(textBox1.Text); // per parametrus perduodame path

            int folderLength2 = Directory.GetFiles(textBox1.Text).Length;

            Thread thread3 = new Thread(new ParameterizedThreadStart(Method3)); // per parametrus perduodame kiek iš viso yra šifruojamų failų --> koks max progresas
            thread3.IsBackground = true;
            thread3.Start(folderLength2);
        }
        void Method(object obj)
        {
            string folderPath = Convert.ToString(obj);

            string[] allfiles = Directory.GetFiles(folderPath);

            try
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    WriteArffFile(allfiles); 
                });
                resetEvent.WaitOne();
                Thread.Sleep(100);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }            
            
        }
        private void WriteArffFile(string[] allfiles)
        {
            using (ArffWriter arffWriter = new ArffWriter(textBox3.Text + ".arff"))
            {
                bool relation = true;
                object[] inst = null;
                object[][] insts = new object[allfiles.Length][];
                int n = 0;

                foreach (string file in allfiles)
                {
                    FileInfo fi = new FileInfo(file);

                    ArffReader datafile = new ArffReader(file);

                    ArffHeader header = datafile.ReadHeader();

                    var instances = datafile.ReadAllInstances();

                    //var instance = datafile.ReadInstance();

                    
                    object[] instance = null;

                    foreach (var ins in instances) instance = ins;

                    if (header != null)
                    {

                        var attributes = header.Attributes;

                        string line;

                        StreamReader attrfile = new StreamReader("attributes.txt");

                        int i = 0;
                        int j = 0;
                        while ((line = attrfile.ReadLine()) != null)
                        {
                            foreach (var attribute in attributes)
                            {
                                if (attribute.Name == line)
                                {
                                    if (relation == true)
                                    {
                                        arffWriter.WriteRelationName(header.RelationName);
                                        relation = false;
                                    }

                                    arffWriter.WriteAttribute(new ArffAttribute(attribute.Name, attribute.Type));

                                    j++;
                                }
                                i++;
                            }
                        }
                        
                        inst = new object[j];
                        j = 0;
                        i = 0;

                        while ((line = attrfile.ReadLine()) != null)
                        {
                            
                            foreach (var attribute in attributes)
                            {
                                if (attribute.Name == line)
                                {
                                    inst.SetValue(instance.GetValue(i), j);
                                    j++;
                                }
                                i++;
                            }
                        }
                        
                        insts.SetValue(inst,n);
                        
                        attrfile.Close();
                    }
                    datafile.Dispose();
                    n++;
                    progress++;
                }
                foreach(var ins in insts) arffWriter.WriteInstance( new object[] { ins });                
            }
        }
        void Method3(object obj)
        {
            //kiek iš viso yra katalogų + failų perduodame per parametrus, darbo pradžioje
            // pagal tai koks yra progress, apskaičiuojame procentaliai progressBar'o reikšmę
            // IšViso - 100
            // progress - x
            // progress * 100 / išViso

            int folderLength = Convert.ToInt32(obj);
            while (progress <= folderLength)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    progressBar1.Value = progress * 100 / folderLength;
                });
                Thread.Sleep(100);
            }
            this.BeginInvoke((MethodInvoker)delegate { progressBar1.Value = 100; });
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            resetEvent.Reset();
        }

        private void resumeButton_Click(object sender, EventArgs e)
        {
            resetEvent.Set();
        }
        private void stopButton_Click(object sender, EventArgs e)
        {
            resetEvent.Close();
        }
    }
}
