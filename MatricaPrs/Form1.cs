using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MatricaPrs
{
    public partial class Form1 : Form
    {
        long m = 0,m1=0;
        long n = 0, n1 = 0;

        long[,] matricaRezultat;

        long[,] matricaPrva;
        long[,] matricaDruga;

        long[,] matricaBenchmark1, matricaBenchmark2,matricaBenchmarkReultat;

        bool generisane = false;

        string path;
        string filePath1, filePath2,filePath3;


        public Form1()
        {
            InitializeComponent();

            //POZIVANJE METODE ZA BENCHMARK

            IstestirajZaRacunar();

            this.Icon = Properties.Resources.matrixIcon;
            //dio za kreiranje direktorijuma i txt fajlova za matrice
            path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            filePath1 = Path.Combine(path, "MatricaFolder", "matrica1.txt");
            filePath2 = Path.Combine(path, "MatricaFolder", "matrica2.txt");
            filePath3 = Path.Combine(path, "MatricaFolder", "rezultat.txt");

            if (!Directory.Exists(Path.Combine(path, "MatricaFolder")))
                Directory.CreateDirectory(Path.Combine(path, "MatricaFolder"));

            if (!File.Exists(filePath1))
            {
                File.Create(filePath1).Dispose();
            }

            if (!File.Exists(filePath2))
            {
                File.Create(filePath2).Dispose();
            }

            if (!File.Exists(filePath3))
            {
                File.Create(filePath3).Dispose();
            }


            groupBox1.Enabled=false;

            lblPrva.Text = "";
            lblDruga.Text = "";
            lblPuta.Text = "";
            lblSacekajte.Text = "";
        }
        // METODA KOJA SE SAMO JEDNOM POKRECE NA SVAKOM RACUNARU
        //SLUZI DA ISTESTIRA SEKVENCIJALNO I PARALELNO VRIJEME IZVRESNJA ZA RAZLICITE MATRICE I DA SACUVA PODATKE
        private void IstestirajZaRacunar()
        {

            if (!Properties.Settings.Default.prviPut)
            {
                try
                {
                    MessageBox.Show("Sacekajte...Pokrenut je benchmark za vas racunar.");
                    //Kreiranje foldera za skladistenje benchmark matrica na Desktopu
                    string filePathProba = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ProbniFolder");

                    if (!Directory.Exists(filePathProba))
                        Directory.CreateDirectory(filePathProba);

                    //POZIVANJE METODE KOJA TESTIRA MATRICE DIMENZIJA KOE=JE SE PROSLIJEDJUJU I CUVA U SETINGS

                    IstersirajZaOdredjenuMatricuBenchmark(25, filePathProba);
                    IstersirajZaOdredjenuMatricuBenchmark(50, filePathProba);
                    IstersirajZaOdredjenuMatricuBenchmark(100, filePathProba);
                    IstersirajZaOdredjenuMatricuBenchmark(150, filePathProba);
                    IstersirajZaOdredjenuMatricuBenchmark(300, filePathProba);
                    IstersirajZaOdredjenuMatricuBenchmark(600, filePathProba);
                    IstersirajZaOdredjenuMatricuBenchmark(800, filePathProba);
                    IstersirajZaOdredjenuMatricuBenchmark(1200, filePathProba);

                    MessageBox.Show("Zavrseno je testiranje.Mozete nastaviti koristiti aplikaciju");

                    //KADA SE ISTESTIRA PRVI PUT POSTAVLJA ZE NA TRUE DA BI SPRIJECILI PONOVO POKRETANJE BENCHMARKA
                    Properties.Settings.Default.prviPut = true;
                    Properties.Settings.Default.Save();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Greskaa");
                }
            }
        }

        private void IstersirajZaOdredjenuMatricuBenchmark(long n, string path)
        {
            Stopwatch tajmer;
            decimal sekvencijalno = 0;
            decimal paralelno = 0;
            matricaBenchmarkReultat = new long[n, n];

            //MNOZENJE MATRICA RAZLICITIH DIMENZIJA I ISPISIVANJE U ZASEBAN FOLDER NA DOSKTOPU

            //Matrica NXN
            generisiBenchmarkMatrice(n, path);

            // sekvencijalno
            tajmer = new Stopwatch();
            tajmer.Start();
            mnozenjeMatricaSekvencijalno(n, n, n, matricaBenchmark1, matricaBenchmark2, true);
            tajmer.Stop();
            sekvencijalno = decimal.Parse(tajmer.Elapsed.TotalSeconds.ToString());

            //paralelno
            Stopwatch tajmer1 = new Stopwatch();
            tajmer1.Start();
            paralelnoMnozenjePoSegmentima(n, true);
            paralelno = decimal.Parse(tajmer1.Elapsed.TotalSeconds.ToString());
            tajmer1.Stop();

            //Pozivanje metode koja provjerava koje je izvrsenje brze za datu matricu
            uporediVremenaIzvrsenjaISacuvaj(sekvencijalno, paralelno, n);

            // 1 oznacava da je sekvencijalno brze a 2 da je paralelno brze za dati racunar za tu matricu
            //   uporediVremenaIzvrsenjaISacuvaj(sekvencijalno, paralelno, n);
            //ispis rezultata
            string pathRezultat = Path.Combine(path, "Matrica " + n + "x" + n, "rezultat.txt");
            ispisiMatricuUTekstualniFajl(matricaBenchmarkReultat, pathRezultat, n, n, true);
        }

        //METODA KOJA GENERISE BENCHMARK MATRICE I ZA SVAKU PRAVI POSEBAN FOLDER I FAJLOVE NA DESKTOPU RAC U KOJIMA SE ISPISUJE MATR I REZ
        void generisiBenchmarkMatrice(long n,string path)
        { 
            System.Random random = new System.Random();

            matricaBenchmark1 = new long[n, n];
            matricaBenchmark2 = new long[n, n];

            for (long i = 0; i < n; i++)
                for (long j = 0; j < n; j++)
                    matricaBenchmark1[i, j] = random.Next(1, 999);

            for (long i = 0; i < n; i++)
                for (long j = 0; j < n; j++)
                    matricaBenchmark2[i, j] = random.Next(1, 999);

            //Putanja do foldera za svaku od matrice koje se testiraju u benchmarku
            string pathMatricaNth = Path.Combine(path, "Matrica " + n + "x" + n);

            //kreira se direktorijum na desktopu racunara
            if (!Directory.Exists(pathMatricaNth))
                Directory.CreateDirectory(pathMatricaNth);

            //putanje za matrice za svaki direktorium
           string filePath1 = Path.Combine(pathMatricaNth, "matrica1.txt");
           string filePath2 = Path.Combine(pathMatricaNth, "matrica2.txt");

            if (!File.Exists(filePath1))
            {
                File.Create(filePath1).Dispose();
            }

            if (!File.Exists(filePath2))
            {
                File.Create(filePath2).Dispose();
            }

            ispisiMatricuUTekstualniFajl(matricaBenchmark1, filePath1, n, n, false);
            ispisiMatricuUTekstualniFajl(matricaBenchmark2, filePath2, n, n, false);
        }

        //METODA KOJOJ SE PROSLIJEDJUJU SEKV VRIJEME I PARALELNO TE ZA SVAKU DIMENZIJU CUVA U SETTINGS 1 ILI 2
        //AKO JE SEKVENCIJALNO BRZE 1 A PARALELNO 2
        private void uporediVremenaIzvrsenjaISacuvaj(decimal sek,decimal par,long dimenzija)
        {
            switch (dimenzija)
            {
                case 25:
                    if (sek < par)
                    {
                        Properties.Settings.Default.matrica25x25 = 1;
                    }
                    else
                        Properties.Settings.Default.matrica25x25 = 2;
                    break;
                case 50:
                    if (sek < par)
                    {
                        Properties.Settings.Default.matrica50x50 = 1;
                    }
                    else
                        Properties.Settings.Default.matrica50x50 = 2;
                    break;
                case 100:
                    if (sek < par)
                    {
                        Properties.Settings.Default.matrica100x100 = 1;
                    }
                    else
                        Properties.Settings.Default.matrica100x100 = 2;
                    break;
                case 150:
                    if (sek < par)
                    {
                        Properties.Settings.Default.matrica150x150 = 1;
                    }
                    else
                        Properties.Settings.Default.matrica150x150 = 2;
                    break;
                case 300:
                    if (sek < par)
                    {
                        Properties.Settings.Default.matrica300x300 = 1;
                    }
                    else
                        Properties.Settings.Default.matrica300x300 = 2;
                    break;
                case 600:
                    if (sek < par)
                    {
                        Properties.Settings.Default.matrica600x600 = 1;
                    }
                    else
                        Properties.Settings.Default.matrica600x600 = 2;
                    break;
                case 800:
                    if (sek < par)
                    {
                        Properties.Settings.Default.matrica800x800 = 1;
                    }
                    else
                        Properties.Settings.Default.matrica800x800 = 2;
                    break;
                case 1200:
                    if (sek < par)
                    {
                        Properties.Settings.Default.matrica1200x1200 = 1;
                    }
                    else
                        Properties.Settings.Default.matrica1200x1200 = 2;
                    break;
                    
            }
            Properties.Settings.Default.Save();
        }
        //Klik na dugme generisanja matrica u opsegu brojeva od 1-999
        private void btnGenerisi_Click(object sender, EventArgs e)
        {
            try
            {
                n = long.Parse(textBox1.Text);
                m = long.Parse(textBox2.Text);

                n1 = long.Parse(textBox4.Text);
                m1 = long.Parse(textBox3.Text);

                bool ulancana;

                //ukoliko je matrica ulancana poziva se metoda za generisanje matrica
                //Ulancana znaci da je br kolona prve jednak br redova druge
                if (m == n1)
                {
                    ulancana = true;
                }
                else
                {
                    ulancana = false;
                }
                if (ulancana)
                {
                    generisane = true;
                    groupBox1.Enabled = true;

                    matricaPrva = new long[0, 0];
                    matricaDruga = new long[0,0];

                    matricaRezultat = new long[0,0];

                    lblPrva.Text = "A[" + n + "][" + m + "]";
                    lblPuta.Text = "X";
                    lblDruga.Text="B["+n1+ "]["+m1 + "]";

                    if(!rdbSekvencijalno.Enabled && !rdbParalelno.Enabled && !rdbOptimalno.Enabled)
                    btnIzracunaj.Enabled = false;

                    generisimatrice(n, m, n1, m1);
                }
                else
                {
                    MessageBox.Show("Morate unijeti da br. kolona prve matrice bude jednak broju redova druge matrice!!!");
                    groupBox1.Enabled=false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Morate unijeti brojevne vrijednosti u polja");
                groupBox1.Enabled = false;
            }
        }

        //METODA ZA GENERISANJE RADNOM MATRICA, NE KORISTI SE ZA BENCHMARK
        void generisimatrice(long n,long m,long n1,long m1)
        {
            System.Random random = new System.Random();

            matricaPrva = new long[n, m];
            matricaDruga = new long[n1, m1];

            for (long i = 0; i < n; i++)
                for (long j = 0; j < m; j++)
                    matricaPrva[i, j] = random.Next(1, 999);

            for (long i = 0; i < n1; i++)
                for (long j = 0; j < m1; j++)
                    matricaDruga[i, j] = random.Next(1,999);

            //Generisanje matrica u fajlove
            ispisiMatricuUTekstualniFajl(matricaPrva, filePath1, n, m,false);
            ispisiMatricuUTekstualniFajl(matricaDruga, filePath2, n1, m1,false);


        }

        //METODA ZA MNOZENJE MATRICA, PARAMETAR BENCHMARK OZNACAVA DA LI SE RADI O BENCHMARK MATRICI ILI O REGULARNOJ
        //AKO JE REGULARNA MNOZENJE SE SMJESTA U matricaREZULTAT A AKO NIJE U matricaBenchmarkRezultat
        public void mnozenjeMatricaSekvencijalno(long n,long m,long m1,long[,] matricaPrva,long[,] matricaDruga,bool benchmark)
        {
            if (!benchmark)
            {
                matricaRezultat = new long[n, m1];

                for (long i = 0; i < n; ++i)
                {
                    for (long j = 0; j < m1; ++j)
                    {
                        for (long k = 0; k < m; ++k)
                        {
                            matricaRezultat[i, j] += matricaPrva[i, k] * matricaDruga[k, j];
                        }
                    }
                }
            }
            else
            {
                matricaBenchmarkReultat = new long[n, n];
                for (long i = 0; i < n; ++i)
                {
                    for (long j = 0; j < n; ++j)
                    {
                        for (long k = 0; k < n; ++k)
                        {
                            matricaBenchmarkReultat[i, j] += matricaPrva[i, k] * matricaDruga[k, j];
                        }
                    }
                }
            }

        }

        //METODA A PARALELNO MNOZENJE PO SEGMENTIMA, PROSLIJEDJUJE JOJ SE BENCHMARK KOJI OZNACAVA DA LI JE BENCHMARK MATRICA
        public void paralelnoMnozenjePoSegmentima(long n,bool benckmark)
        {
            int maxBrojTredova = Environment.ProcessorCount;

            int workerThreads = 0;
            int completionPortThreads = 0;

            matricaRezultat = new long[n, m1];

            ThreadPool.GetMinThreads(out workerThreads, out completionPortThreads);

            //korak tj broj redova koje ce mnoziti svaki tred
            long korak = n / workerThreads;

            //ukoliko ima ostatka koristi se drugi algoritam za mnozenje
            long ostatak = n % workerThreads;

            Thread[] tred = new Thread[workerThreads];

            //UKOLIKO NIJE BENCHMARK
            if (!benckmark)
            {
                //Ukoliko nema ostatka

                if (ostatak == 0)
                {
                    //Raspodjela posla po maximalnom broju tredova
                    Parallel.For(0, workerThreads, i =>
                    {
                        long pocetak = 0;
                    //Ukoliko nije nulti korak, pocetak se povecava za korak i mnozi sa i
                    if (i >= 1)
                        {
                            pocetak = (pocetak + korak) * i;
                        }

                        tred[i] = new Thread(() => podijeliMatricuNaSegmente(korak, pocetak,0));
                        tred[i].Start();
                    });
                    for (int i = 0; i < workerThreads; i++)
                    {
                        tred[i].Join();
                    }
                }
                else
                {
                    Parallel.For(0, workerThreads, i =>
                    {
                        long pocetak = 0;

                    //Ukoliko je zadnji tred, onda on mnozi do kraja tj korak mu se povecava za ostatk

                    if (i == (workerThreads - 1))
                        {
                            pocetak = (pocetak + korak) * i;
                            tred[i] = new Thread(() => podijeliMatricuNaSegmente(ostatak + korak, pocetak,0));
                        }
                        else
                        {
                            if (i >= 1)
                            {
                                pocetak = (pocetak + korak) * i;
                            }

                            tred[i] = new Thread(() => podijeliMatricuNaSegmente(korak, pocetak,0));
                        }
                        tred[i].Start();
                    });
                    for (int i = 0; i < workerThreads; i++)
                    {
                        tred[i].Join();
                    }
                }
            }
            //UKOLIKO JE BENCHMARK PROSLIJEDJUJE SE N METODI podijeliMatricuNaSegmente
            else
            {
                //Ukoliko nema ostatka

                if (ostatak == 0)
                {
                    //Raspodjela posla po maximalnom broju tredova
                    Parallel.For(0, workerThreads, i =>
                    {
                        long pocetak = 0;
                        //Ukoliko nije nulti korak, pocetak se povecava za korak i mnozi sa i
                        if (i >= 1)
                        {
                            pocetak = (pocetak + korak) * i;
                        }

                        tred[i] = new Thread(() => podijeliMatricuNaSegmente(korak, pocetak,n));
                        tred[i].Start();
                    });
                    for (int i = 0; i < workerThreads; i++)
                    {
                        tred[i].Join();
                    }
                }
                else
                {
                    Parallel.For(0, workerThreads, i =>
                    {
                        long pocetak = 0;

                        //Ukoliko je zadnji tred, onda on mnozi do kraja tj korak mu se povecava za ostatk

                        if (i == (workerThreads - 1))
                        {
                            pocetak = (pocetak + korak) * i;
                            tred[i] = new Thread(() => podijeliMatricuNaSegmente(ostatak + korak, pocetak,n));
                        }
                        else
                        {
                            if (i >= 1)
                            {
                                pocetak = (pocetak + korak) * i;
                            }

                            tred[i] = new Thread(() => podijeliMatricuNaSegmente(korak, pocetak,n));
                        }
                        tred[i].Start();
                    });
                    for (int i = 0; i < workerThreads; i++)
                    {
                        tred[i].Join();
                    }
                }
            }
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            btnIzracunaj.Enabled = true;
        }

        private void btnIzracunaj_Click(object sender, EventArgs e)
        {
            if (rdbSekvencijalno.Checked)
            {
                izracunajSekvencijalno();

            }
            if (rdbParalelno.Checked)
            {
                izracunajParalelno();
            }
            if (rdbOptimalno.Checked)
            {
               int metod= IzracunajOptimalno();

                if (metod == 1)
                {
                    izracunajSekvencijalno();
                }
                if (metod == 2)
                {
                    izracunajParalelno();
                }
            }

        }
        //PROVJERAVA OPTIMALAN METOD PREMA BENCHMARK REZULTATIMA
        private int IzracunajOptimalno()
        {
            int metod = 0;
            if (n < 25)
            {
                metod = 1;
            }
            if(n>=25 && n < 50)
            {
                metod = Properties.Settings.Default.matrica25x25;
            }
            if(n>=50 && n < 100)
            {
                metod = Properties.Settings.Default.matrica50x50;
            }
            if (n >= 100 && n < 150)
            {
                metod = Properties.Settings.Default.matrica100x100;
            }
            if (n >= 150 && n < 300)
            {
                metod = Properties.Settings.Default.matrica150x150;
            }
            if (n >= 300 && n < 600)
            {
                metod = Properties.Settings.Default.matrica300x300;
            }
            if (n >= 600 && n < 800)
            {
                metod = Properties.Settings.Default.matrica600x600;
            }
            if (n >= 800 && n < 1200)
            {
                metod = Properties.Settings.Default.matrica800x800;
            }
            if (n >= 1200)
            {
                //ukoliko je preko 1200 uvijek koristi paralelno
                metod = 2;
            }
            return metod;
        }
        //POZIVA SE UKOLIKO SE ODABERE SEKVENCIJALNO IZVRSENJE
        void izracunajSekvencijalno()
        {
            lblSacekajte.Text = "Sacekajte...";
            lblVrijeme.Text = "";

            Stopwatch tamjer = new Stopwatch();

            groupBox1.Enabled = false;

            tamjer.Start();

            mnozenjeMatricaSekvencijalno(n, m, m1, matricaPrva, matricaDruga,false);

            tamjer.Stop();

            lblSacekajte.Text = "";

            lblSacekajte.Text = "";
            groupBox1.Enabled = true;

            lblVrijeme.Text = tamjer.Elapsed.TotalSeconds.ToString();

            ispisiMatricuUTekstualniFajl(matricaRezultat, filePath3, n, m1, true);
        }

        //POZIVA SE UKOLIKO SE ODABERE PARALELNO IZVRSENJE
        void izracunajParalelno()
        {
            lblSacekajte.Text = "Sacekajte...";
            lblVrijeme.Text = "";

            Stopwatch tajmer = new Stopwatch();

            groupBox1.Enabled = false;

            tajmer.Start();

            paralelnoMnozenjePoSegmentima(n,false);

            tajmer.Stop();

            lblSacekajte.Text = "";

            groupBox1.Enabled= true;

            lblVrijeme.Text = tajmer.Elapsed.TotalSeconds.ToString();

            ispisiMatricuUTekstualniFajl(matricaRezultat, filePath3, n, m1, true);
        }
        //Metoda akoja dijeli matrice na segmente, tj svaki tred ima segment za koji je zaduzen
        public void podijeliMatricuNaSegmente(long segment,long pocetak,long n)
        {
            if (n > 0)
            {
                for (long i = pocetak; i < segment + pocetak; i++)
                {
                    for (long j = 0; j < n; j++)
                    {
                        for (long k = 0; k < n; k++)
                        {
                            matricaBenchmarkReultat[i, j] += matricaBenchmark1[i, k] * matricaBenchmark2[k, j];
                        }
                    }
                }
            }
            else
            {
                for (long i = pocetak; i < segment + pocetak; i++)
                {
                    for (long j = 0; j < m1; j++)
                    {
                        for (long k = 0; k < m; k++)
                        {
                            matricaRezultat[i, j] += matricaPrva[i, k] * matricaDruga[k, j];
                        }
                    }
                }
            }

        }
        //ispis matrice u txt fajl
        void ispisiMatricuUTekstualniFajl(long[,] matrica,string putanja,long n,long m,bool rezultat)
        {
            StreamWriter tw = new StreamWriter(putanja, false) ;

            for (long i = 0; i < n; i++)
            {
                for (long j = 0; j < m; j++)
                {
                    int cifre = 0;
                    long broj = matrica[i, j];

                    while (broj > 0)
                    {
                        broj = broj / 10;
                        cifre++;
                    }
                    if (rezultat)
                    {
                        tw.Write(matrica[i,j].ToString().PadRight(10));
                    }
                    else
                    {
                        if (cifre == 1)
                            tw.Write(matrica[i, j].ToString().PadRight(3) + "  ");

                        if (cifre == 2)
                            tw.Write(matrica[i, j].ToString().PadRight(3) + "  ");

                        if (cifre == 3)
                            tw.Write(matrica[i, j].ToString() + "  ");
                    }

                }
                tw.WriteLine();
            }
            tw.Close();
        }
        
    }
}
